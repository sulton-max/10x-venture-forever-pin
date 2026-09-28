import { ExactNumber } from '@wow-two-beta/ui-vue/foundation/numbers';
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

import { exactCount } from './CountSchemas';

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

/**
 * Decodes a measurement — a coordinate, an order, a ratio. Lossless reads deliver every number exactly, so a
 * measurement is converted to the nearest double, which is what native JSON would have produced.
 */
const measurement = z.preprocess((value) => {
  if (!ExactNumber.isExactNumber(value)) return value;
  const approximate = value.toApproximateNumber();
  return approximate.ok ? approximate.value : Number.NaN;
}, z.number());

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
  z.object({ type: z.literal(ContentType.Geo), latitude: measurement, longitude: measurement }),
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
    order: measurement,
    condition: oneOf(RuleConditionType),
    conditionValue: z.string(),
    content: CodeContentDtoSchema,
  }),
  z.object({ type: z.literal(CodeRuleType.Default), content: CodeContentDtoSchema }),
  z.object({ type: z.literal(CodeRuleType.DefaultPointer), targetOrder: measurement }),
]);

const gradientStop = z.object({ color: z.string(), offset: measurement });

/** Validates a wire visual style. */
export const CodeStyleDtoSchema: z.ZodType<CodeStyleDto> = z.object({
  foregroundColor: z.string(),
  backgroundColor: z.string(),
  transparentBackground: z.boolean(),
  eccLevel: oneOf(EccLevel),
  quietZoneModules: measurement,
  logo: z
    .object({ dataUrl: z.string(), sizeRatio: measurement })
    .nullish()
    .transform((value) => value ?? undefined),
  moduleShape: oneOf(ModuleShape),
  finderShape: oneOf(FinderShape),
  finderDotShape: oneOf(FinderShape),
  gradient: z
    .discriminatedUnion('type', [
      z.object({ type: z.literal('linear'), angle: measurement, stops: z.array(gradientStop) }),
      z.object({ type: z.literal('radial'), radius: measurement, stops: z.array(gradientStop) }),
    ])
    .nullish()
    .transform((value) => value ?? undefined),
  emoji: z
    .object({ glyph: z.string(), sizeRatio: measurement })
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
  scanCount: exactCount,
  createdAt: instant,
  rules: z.array(CodeRuleDtoSchema),
  style: CodeStyleDtoSchema,
});
