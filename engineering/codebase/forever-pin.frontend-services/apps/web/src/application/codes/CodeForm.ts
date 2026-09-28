// Builder schema, initial values and request mapping.

import { defaultMapFieldPath } from '@wow-two-beta/ui-vue/forms-engine';
import { Temporal } from 'temporal-polyfill';
import { z } from 'zod';

import {
  BarcodeFormat,
  CodeRuleType,
  ContentMode,
  ContentType,
  EccLevel,
  FinderShape,
  MobileAppStoreType,
  ModuleShape,
  RuleConditionType,
  WifiEncryption,
  defaultCodeStyle,
  emptyContent,
  type CodeDto,
  type CodeEmojiDto,
  type CodeGradientDto,
  type CodeLogoDto,
  type CodeRuleDto,
  type CodeStyleDto,
  type ConditionalRuleDto,
  type DefaultRuleDto,
} from '@/domain/codes';
import type { CodeCreateUpdateApiRequest, CodeUpdateApiRequest } from '@/integration/codes';

import type { CodeFormModel } from './models/CodeFormModel';

/** Builds a schema accepting one of an enum's string values. */
function oneOf<T extends Record<string, string>>(source: T) {
  return z.enum(Object.values(source) as [T[keyof T], ...T[keyof T][]]);
}

// ── Schema ──────────────────────────────────────────────────────────────────────
// Shapes the control values; the API validates content semantics and returns leaf paths.

const contentSchema = z.discriminatedUnion('type', [
  z.object({ type: z.literal(ContentType.Url), url: z.string() }),
  z.object({ type: z.literal(ContentType.MobileApp), store: oneOf(MobileAppStoreType), url: z.string() }),
  z.object({ type: z.literal(ContentType.Text), text: z.string() }),
  z.object({
    type: z.literal(ContentType.Email),
    to: z.string(),
    subject: z.string().optional(),
    body: z.string().optional(),
  }),
  z.object({ type: z.literal(ContentType.Sms), phone: z.string(), message: z.string().optional() }),
  z.object({ type: z.literal(ContentType.Phone), phone: z.string() }),
  z.object({
    type: z.literal(ContentType.Geo),
    latitude: z.number({ message: 'Enter a latitude.' }).min(-90).max(90),
    longitude: z.number({ message: 'Enter a longitude.' }).min(-180).max(180),
  }),
  z.object({
    type: z.literal(ContentType.Wifi),
    ssid: z.string(),
    password: z.string().optional(),
    encryption: oneOf(WifiEncryption),
    hidden: z.boolean(),
  }),
  z.object({
    type: z.literal(ContentType.VCard),
    firstName: z.string(),
    lastName: z.string().optional(),
    org: z.string().optional(),
    title: z.string().optional(),
    phone: z.string().optional(),
    email: z.string().optional(),
    url: z.string().optional(),
    address: z.string().optional(),
    note: z.string().optional(),
  }),
  z.object({
    type: z.literal(ContentType.Calendar),
    title: z.string(),
    start: z.custom<Temporal.PlainDateTime>((value) => value instanceof Temporal.PlainDateTime, 'Choose a start.'),
    end: z.custom<Temporal.PlainDateTime>((value) => value instanceof Temporal.PlainDateTime).optional(),
    location: z.string().optional(),
    description: z.string().optional(),
  }),
]);

// A rule is its role plus the content it serves — the catch-all is a role, never a condition.
const codeRuleSchema = z.discriminatedUnion('type', [
  z.object({
    type: z.literal(CodeRuleType.Conditional),
    order: z.number(),
    condition: oneOf(RuleConditionType),
    conditionValue: z.string(),
    content: contentSchema,
  }),
  z.object({ type: z.literal(CodeRuleType.Default), content: contentSchema }),
  z.object({ type: z.literal(CodeRuleType.DefaultPointer), targetOrder: z.number() }),
]);

const codeStyleSchema = z.object({
  foregroundColor: z.string(),
  backgroundColor: z.string(),
  transparentBackground: z.boolean(),
  eccLevel: oneOf(EccLevel),
  quietZoneModules: z.number(),
  // Color, emoji and logo models are validated by their own controls, not re-parsed here.
  logo: z.custom<CodeLogoDto>().optional(),
  moduleShape: oneOf(ModuleShape),
  finderShape: oneOf(FinderShape),
  finderDotShape: oneOf(FinderShape),
  gradient: z.custom<CodeGradientDto>().optional(),
  emoji: z.custom<CodeEmojiDto>().optional(),
});

/** Defines the builder form schema. */
export const CodeFormSchema = z
  .object({
    name: z.string(),
    barcodeFormat: oneOf(BarcodeFormat),
    mode: oneOf(ContentMode),
    contentType: oneOf(ContentType),
    style: codeStyleSchema,
    rules: z.array(codeRuleSchema).min(1, 'Add at least one rule.'),
  })
  .superRefine((values, context) => {
    const orders = new Set(
      values.rules.flatMap((rule) => (rule.type === CodeRuleType.Conditional ? [rule.order] : [])),
    );
    values.rules.forEach((rule, index) => {
      if (rule.type === CodeRuleType.DefaultPointer && !orders.has(rule.targetOrder)) {
        context.addIssue({
          code: 'custom',
          path: ['rules', index, 'targetOrder'],
          message: 'Choose the rule everyone else should follow.',
        });
      }
    });
  });

// ── Factories ───────────────────────────────────────────────────────────────────

/** Creates a static URL code with one catch-all rule. */
export function emptyCodeForm(): CodeFormModel {
  return {
    name: '',
    barcodeFormat: BarcodeFormat.QrCode,
    mode: ContentMode.Static,
    contentType: ContentType.Url,
    style: cloneStyle(defaultCodeStyle),
    rules: [{ type: CodeRuleType.Default, content: { type: ContentType.Url, url: 'https://example.com' } }],
  };
}

/** Returns the next free conditional identity; orders are renumbered only on submission. */
export function nextRuleOrder(rules: ReadonlyArray<CodeRuleDto>): number {
  return rules.reduce((max, rule) => (rule.type === CodeRuleType.Conditional ? Math.max(max, rule.order) : max), 0) + 1;
}

/** Creates an empty conditional rule carrying the next free identity. */
export function emptyConditionalRule(contentType: ContentType, rules: ReadonlyArray<CodeRuleDto>): ConditionalRuleDto {
  return {
    type: CodeRuleType.Conditional,
    order: nextRuleOrder(rules),
    condition: RuleConditionType.Device,
    conditionValue: '',
    content: emptyContent(contentType),
  };
}

/** Creates an empty catch-all rule; a code has at most one. */
export function emptyDefaultRule(contentType: ContentType): DefaultRuleDto {
  return { type: CodeRuleType.Default, content: emptyContent(contentType) };
}

/** Returns the opposite content mode. */
export function oppositeMode(mode: ContentMode): ContentMode {
  return mode === ContentMode.Static ? ContentMode.Dynamic : ContentMode.Static;
}

/**
 * Maps a saved code to edit values, preserving its mode. The catch-all moves last — the router evaluates it
 * after every conditional wherever it is stored, so the editor shows it where it acts.
 */
export function toCodeForm(code: CodeDto): CodeFormModel {
  const isConditional = (rule: CodeRuleDto) => rule.type === CodeRuleType.Conditional;
  return {
    name: code.name,
    barcodeFormat: code.barcodeFormat,
    mode: code.mode,
    contentType: code.contentType,
    style: cloneStyle(code.style),
    rules: [...code.rules.filter(isConditional), ...code.rules.filter((rule) => !isConditional(rule))].map(cloneRule),
  };
}

/** Maps a saved code to copy values in the chosen mode. */
export function toCopyCodeForm(code: CodeDto, mode: ContentMode): CodeFormModel {
  return { ...toCodeForm(code), name: `${code.name} copy`, mode };
}

/**
 * Copies a style field by field. Cached server data can arrive as reactive proxies, which `structuredClone`
 * rejects, so nested values are copied explicitly.
 */
function cloneStyle(style: CodeStyleDto): CodeStyleDto {
  return {
    ...style,
    logo: style.logo ? { ...style.logo } : undefined,
    emoji: style.emoji ? { ...style.emoji } : undefined,
    gradient: style.gradient
      ? { ...style.gradient, stops: style.gradient.stops.map((stop) => ({ ...stop })) }
      : undefined,
  };
}

/** Clones a rule without losing Temporal values, which `structuredClone` cannot copy. */
function cloneRule(rule: CodeRuleDto): CodeRuleDto {
  return rule.type === CodeRuleType.DefaultPointer ? { ...rule } : { ...rule, content: { ...rule.content } };
}

// ── Requests ────────────────────────────────────────────────────────────────────

/** Rewrites a server error path onto a form path; leaf content paths bind to their own controls. */
export function mapCodeFieldPath(serverPath: string): string {
  return defaultMapFieldPath(serverPath);
}

/**
 * Normalizes the name and renumbers conditional rules by position, remapping the fallback pointer
 * to the same logical rule, so reordering never changes where unmatched scans go.
 */
export function toCreateCodeRequest(values: CodeFormModel): CodeCreateUpdateApiRequest {
  const renumbered = new Map<number, number>();
  let order = 0;
  for (const rule of values.rules) if (rule.type === CodeRuleType.Conditional) renumbered.set(rule.order, ++order);

  const rules: CodeRuleDto[] = values.rules.map((rule) => {
    if (rule.type === CodeRuleType.Conditional) return { ...rule, order: renumbered.get(rule.order) ?? rule.order };
    if (rule.type === CodeRuleType.DefaultPointer)
      return { ...rule, targetOrder: renumbered.get(rule.targetOrder) ?? rule.targetOrder };
    return { ...rule };
  });

  return {
    ...values,
    name: values.name.trim() || 'Untitled code',
    // Multiple rules require resolution at scan time.
    mode: rules.length > 1 ? ContentMode.Dynamic : values.mode,
    rules,
  };
}

/** Normalizes an update request and omits the immutable mode. */
export function toUpdateCodeRequest(values: CodeFormModel): CodeUpdateApiRequest {
  const { mode: _mode, ...request } = toCreateCodeRequest(values);
  void _mode;
  return request;
}
