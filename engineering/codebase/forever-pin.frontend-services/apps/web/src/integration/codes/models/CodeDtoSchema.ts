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
  type CodeContent,
  type CodeDto,
  type CodeRuleDto,
  type CodeStyleDto,
} from '@/domain/codes';

/** Builds a schema accepting one of an enum's string values. */
function oneOf<T extends Record<string, string>>(source: T) {
  return z.enum(Object.values(source) as [T[keyof T], ...T[keyof T][]]);
}

/** Decodes an ISO local date-time into a `Temporal.PlainDateTime`. */
const plainDateTime = z.string().transform((value, context) => {
  try {
    return Temporal.PlainDateTime.from(value);
  } catch {
    context.addIssue({ code: 'custom', message: 'Invalid local date-time.' });
    return z.NEVER;
  }
});

/** Decodes an ISO instant into a `Temporal.Instant`. */
const instant = z.string().transform((value, context) => {
  try {
    return Temporal.Instant.from(value);
  } catch {
    context.addIssue({ code: 'custom', message: 'Invalid instant.' });
    return z.NEVER;
  }
});

const optionalText = z
  .string()
  .nullish()
  .transform((value) => value ?? undefined);

/** Validates wire content, reviving calendar date-times. */
export const CodeContentDtoSchema: z.ZodType<CodeContent> = z.discriminatedUnion('type', [
  z.object({ type: z.literal(ContentType.Url), url: z.string() }),
  z.object({ type: z.literal(ContentType.MobileApp), store: oneOf(MobileAppStoreType), url: z.string() }),
  z.object({ type: z.literal(ContentType.Text), text: z.string() }),
  z.object({ type: z.literal(ContentType.Email), to: z.string(), subject: optionalText, body: optionalText }),
  z.object({ type: z.literal(ContentType.Sms), phone: z.string(), message: optionalText }),
  z.object({ type: z.literal(ContentType.Phone), phone: z.string() }),
  z.object({ type: z.literal(ContentType.Geo), latitude: z.number(), longitude: z.number() }),
  z.object({
    type: z.literal(ContentType.Wifi),
    ssid: z.string(),
    password: optionalText,
    encryption: oneOf(WifiEncryption),
    hidden: z.boolean(),
  }),
  z.object({
    type: z.literal(ContentType.VCard),
    firstName: z.string(),
    lastName: optionalText,
    org: optionalText,
    title: optionalText,
    phone: optionalText,
    email: optionalText,
    url: optionalText,
    address: optionalText,
    note: optionalText,
  }),
  z.object({
    type: z.literal(ContentType.Calendar),
    title: z.string(),
    start: plainDateTime,
    end: plainDateTime.nullish().transform((value) => value ?? undefined),
    location: optionalText,
    description: optionalText,
  }),
]);

/** Validates a wire routing rule. */
export const CodeRuleDtoSchema: z.ZodType<CodeRuleDto> = z.discriminatedUnion('type', [
  z.object({
    type: z.literal(CodeRuleType.Conditional),
    order: z.number(),
    condition: oneOf(RuleConditionType),
    conditionValue: z.string(),
    content: CodeContentDtoSchema,
  }),
  z.object({ type: z.literal(CodeRuleType.Default), content: CodeContentDtoSchema }),
  z.object({ type: z.literal(CodeRuleType.DefaultPointer), targetOrder: z.number() }),
]);

const gradientStop = z.object({ color: z.string(), offset: z.number() });

/** Validates a wire visual style. */
export const CodeStyleDtoSchema: z.ZodType<CodeStyleDto> = z.object({
  foregroundColor: z.string(),
  backgroundColor: z.string(),
  transparentBackground: z.boolean(),
  eccLevel: oneOf(EccLevel),
  quietZoneModules: z.number(),
  logo: z
    .object({ dataUrl: z.string(), sizeRatio: z.number() })
    .nullish()
    .transform((value) => value ?? undefined),
  moduleShape: oneOf(ModuleShape),
  finderShape: oneOf(FinderShape),
  finderDotShape: oneOf(FinderShape),
  gradient: z
    .discriminatedUnion('type', [
      z.object({ type: z.literal('linear'), angle: z.number(), stops: z.array(gradientStop) }),
      z.object({ type: z.literal('radial'), radius: z.number(), stops: z.array(gradientStop) }),
    ])
    .nullish()
    .transform((value) => value ?? undefined),
  emoji: z
    .object({ glyph: z.string(), sizeRatio: z.number() })
    .nullish()
    .transform((value) => value ?? undefined),
});

/** Validates a wire code, reviving its creation instant. */
export const CodeDtoSchema: z.ZodType<CodeDto> = z.object({
  id: z.string(),
  slug: optionalText,
  shortUrl: optionalText,
  name: z.string(),
  barcodeFormat: oneOf(BarcodeFormat),
  mode: oneOf(ContentMode),
  contentType: oneOf(ContentType),
  isActive: z.boolean(),
  scanCount: z.number(),
  createdAt: instant,
  rules: z.array(CodeRuleDtoSchema),
  style: CodeStyleDtoSchema,
});
