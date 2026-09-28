import { describe, expect, it } from 'vitest';

import { CodeFormSchema, emptyCodeForm, toCodeForm } from '@/application/codes';
import { toCreateCodeRequest, toUpdateCodeRequest } from '@/application/codes/CodeForm';
import type { CodeFormModel } from '@/application/codes';
import {
  CodeRuleType,
  ContentMode,
  ContentType,
  RuleConditionType,
  type CodeDto,
  type CodeRuleDto,
} from '@/domain/codes';

function conditional(order: number, url: string): CodeRuleDto {
  return {
    type: CodeRuleType.Conditional,
    order,
    condition: RuleConditionType.Country,
    conditionValue: 'US',
    content: { type: ContentType.Url, url },
  };
}

function form(rules: CodeRuleDto[]): CodeFormModel {
  return { ...emptyCodeForm(), mode: ContentMode.Dynamic, rules };
}

describe('toCreateCodeRequest', () => {
  it('keeps the fallback on the same rule when conditionals are reordered', () => {
    // Rule "b" (identity 2) moved above rule "a" (identity 1); the fallback follows "a".
    const request = toCreateCodeRequest(
      form([
        conditional(2, 'https://b.test'),
        conditional(1, 'https://a.test'),
        { type: CodeRuleType.DefaultPointer, targetOrder: 1 },
      ]),
    );
    const target = request.rules.find((rule) => rule.type === CodeRuleType.DefaultPointer);
    const followed = request.rules.find(
      (rule) =>
        rule.type === CodeRuleType.Conditional &&
        target?.type === CodeRuleType.DefaultPointer &&
        rule.order === target.targetOrder,
    );
    expect(request.rules.map((rule) => (rule.type === CodeRuleType.Conditional ? rule.order : null))).toEqual([
      1,
      2,
      null,
    ]);
    expect(followed?.type === CodeRuleType.Conditional && followed.content).toEqual({
      type: ContentType.Url,
      url: 'https://a.test',
    });
  });

  it('resolves more than one rule at scan time and names an unnamed code', () => {
    const request = toCreateCodeRequest({
      ...form([conditional(1, 'https://a.test'), conditional(2, 'https://b.test')]),
      mode: ContentMode.Static,
      name: '  ',
    });
    expect(request.mode).toBe(ContentMode.Dynamic);
    expect(request.name).toBe('Untitled code');
  });

  it('omits the immutable mode from an update', () => {
    expect(toUpdateCodeRequest(form([conditional(1, 'https://a.test')]))).not.toHaveProperty('mode');
  });
});

describe('toCodeForm', () => {
  it('moves the catch-all after every conditional', () => {
    const code = {
      id: 'c1',
      name: 'Menu',
      mode: ContentMode.Dynamic,
      contentType: ContentType.Url,
      rules: [
        { type: CodeRuleType.Default, content: { type: ContentType.Url, url: 'https://all.test' } },
        conditional(1, 'https://a.test'),
      ],
      style: emptyCodeForm().style,
    } as unknown as CodeDto;
    expect(toCodeForm(code).rules.map((rule) => rule.type)).toEqual([CodeRuleType.Conditional, CodeRuleType.Default]);
  });
});

describe('CodeFormSchema', () => {
  it('asks for a new target when the followed rule was removed', () => {
    const result = CodeFormSchema.safeParse(
      form([conditional(2, 'https://b.test'), { type: CodeRuleType.DefaultPointer, targetOrder: 1 }]),
    );
    expect(result.success).toBe(false);
    expect(result.error?.issues[0]?.path).toEqual(['rules', 1, 'targetOrder']);
  });
});
