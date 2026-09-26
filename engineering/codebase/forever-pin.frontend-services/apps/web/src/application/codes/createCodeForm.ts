// Code-builder schema, initial values, and request mapping.

import { z } from "zod";
import { Temporal } from "temporal-polyfill";
import { defaultMapFieldPath } from "@wow-two-beta/ui/forms-engine";

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
  type CodeLogoDto,
  type CodeRuleDto,
  type ConditionalRuleDto,
  type DefaultRuleDto,
  type Gradient,
} from "@/domain/codes";
import type { CodeCreateUpdateApiRequest } from "@/integration/codes";

/** Builds a schema accepting the enum's string values. */
function enumOf<T extends Record<string, string>>(source: T) {
  const values = Object.values(source);
  return z.custom<T[keyof T]>((value) => typeof value === "string" && values.includes(value));
}

// ── Schema ──────────────────────────────────────────────────────────────────────
// Shape the control values; the backend validates content semantics.

const contentSchema = z.discriminatedUnion("type", [
  z.object({ type: z.literal(ContentType.Url), url: z.string() }),
  z.object({
    type: z.literal(ContentType.MobileApp),
    store: enumOf(MobileAppStoreType),
    url: z.string(),
  }),
  z.object({ type: z.literal(ContentType.Text), text: z.string() }),
  z.object({
    type: z.literal(ContentType.Email),
    to: z.string(),
    subject: z.string().optional(),
    body: z.string().optional(),
  }),
  z.object({ type: z.literal(ContentType.Sms), phone: z.string(), message: z.string().optional() }),
  z.object({ type: z.literal(ContentType.Phone), phone: z.string() }),
  z.object({ type: z.literal(ContentType.Geo), latitude: z.number(), longitude: z.number() }),
  z.object({
    type: z.literal(ContentType.Wifi),
    ssid: z.string(),
    password: z.string().optional(),
    encryption: enumOf(WifiEncryption),
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
    start: z.custom<Temporal.PlainDateTime>((value) => value instanceof Temporal.PlainDateTime),
    end: z.custom<Temporal.PlainDateTime>((value) => value instanceof Temporal.PlainDateTime).optional(),
    location: z.string().optional(),
    description: z.string().optional(),
  }),
]);

// A rule is its role plus the content it serves — the catch-all is a role, never a condition.
const codeRuleSchema = z.discriminatedUnion("type", [
  z.object({
    type: z.literal(CodeRuleType.Conditional),
    order: z.number(),
    condition: enumOf(RuleConditionType),
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
  eccLevel: enumOf(EccLevel),
  quietZoneModules: z.number(),
  // Complex external color/emoji/logo models — carried by type (validated by their own controls), not re-parsed.
  logo: z.custom<CodeLogoDto>().optional(),
  moduleShape: enumOf(ModuleShape),
  finderShape: enumOf(FinderShape),
  finderDotShape: enumOf(FinderShape),
  gradient: z.custom<Gradient>().optional(),
  emoji: z.custom<CodeEmojiDto>().optional(),
});

/** Defines the code-builder form schema. */
export const CreateCodeSchema = z.object({
  name: z.string(),
  barcodeFormat: enumOf(BarcodeFormat),
  mode: enumOf(ContentMode),
  contentType: enumOf(ContentType),
  style: codeStyleSchema,
  rules: z.array(codeRuleSchema).min(1),
});

// ── Factories + mappers ─────────────────────────────────────────────────────────

/** Creates a static URL request with one default rule. */
export function emptyCodeCreateUpdateApiRequest(): CodeCreateUpdateApiRequest {
  return {
    name: "",
    barcodeFormat: BarcodeFormat.QrCode,
    mode: ContentMode.Static,
    contentType: ContentType.Url,
    style: { ...defaultCodeStyle },
    rules: [{ type: CodeRuleType.Default, content: { type: ContentType.Url, url: "https://example.com" } }],
  };
}

/** Creates an empty conditional rule; assign its order before submission. */
export function emptyConditionalRule(contentType: ContentType): ConditionalRuleDto {
  return {
    type: CodeRuleType.Conditional,
    order: 0,
    condition: RuleConditionType.Device,
    conditionValue: "",
    content: emptyContent(contentType),
  };
}

/** Creates an empty catch-all rule; allow at most one per code. */
export function emptyDefaultRule(contentType: ContentType): DefaultRuleDto {
  return { type: CodeRuleType.Default, content: emptyContent(contentType) };
}

/** Returns the opposite content mode. */
export function oppositeMode(mode: ContentMode): ContentMode {
  return mode === ContentMode.Static ? ContentMode.Dynamic : ContentMode.Static;
}

/** Maps a saved code to edit-form values, preserving its mode. */
export function toCodeCreateUpdateApiRequest(code: CodeDto): CodeCreateUpdateApiRequest {
  return {
    name: code.name,
    barcodeFormat: code.barcodeFormat,
    mode: code.mode,
    contentType: code.contentType,
    style: { ...code.style },
    rules: code.rules.map((rule) => ({ ...rule })),
  };
}

/** Maps a saved code to copy-form values in the selected mode. */
export function toCopyCodeCreateUpdateApiRequest(code: CodeDto, mode: ContentMode): CodeCreateUpdateApiRequest {
  return {
    name: `${code.name} copy`,
    barcodeFormat: code.barcodeFormat,
    mode,
    contentType: code.contentType,
    style: { ...code.style },
    rules: code.rules.map((rule) => ({ ...rule })),
  };
}

// Content errors bind to the whole content group, not individual leaf fields.
const ContentLeafPath = /^(rules\[\d+]\.content)\..+$/;

/** Normalizes server error paths and maps content leaves to their bound content group. */
export function mapCodeFieldPath(serverPath: string): string {
  return defaultMapFieldPath(serverPath).replace(ContentLeafPath, "$1");
}

/** Normalizes the name and rule order, forcing dynamic mode for multiple rules. */
export function toCreateCodeRequest(values: CodeCreateUpdateApiRequest): CodeCreateUpdateApiRequest {
  let order = 0;
  const rules: CodeRuleDto[] = values.rules.map((rule) =>
    rule.type === CodeRuleType.Conditional ? { ...rule, order: ++order } : { ...rule },
  );

  return {
    ...values,
    name: values.name.trim() || "Untitled code",
    // Multiple rules require resolution at scan time.
    mode: rules.length > 1 ? ContentMode.Dynamic : values.mode,
    rules,
  };
}

/** Normalizes an update request and omits the immutable mode. */
export function toUpdateCodeRequest(values: CodeCreateUpdateApiRequest): Omit<CodeCreateUpdateApiRequest, "mode"> {
  const { mode: _mode, ...request } = toCreateCodeRequest(values);
  return request;
}
