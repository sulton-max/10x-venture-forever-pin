import { flushPromises, mount } from '@vue/test-utils';
import { afterEach, describe, expect, it } from 'vitest';

import { CodeRuleType, ContentMode, ContentType, RuleConditionType, type CodeDto } from '@/domain/codes';
import CopyCodeModal from '@/presentation/codes/list/CopyCodeModal.vue';
import DeleteCodeModal from '@/presentation/codes/list/DeleteCodeModal.vue';

function code(mode: ContentMode, ruleCount: number): CodeDto {
  const rules = Array.from({ length: ruleCount }, (_, index) => ({
    type: CodeRuleType.Conditional,
    order: index + 1,
    condition: RuleConditionType.Device,
    conditionValue: 'Ios',
    content: { type: ContentType.Url, url: `https://example.com/${index}` },
  })) as CodeDto['rules'];
  return { id: 'c1', name: 'Menu', mode, rules } as CodeDto;
}

/** Finds the radio of the choice card labelled `label`; the card does not mirror its value onto the input. */
const radio = (label: string) =>
  [...document.body.querySelectorAll('label')]
    .find((element) => element.textContent?.trim().startsWith(label))
    ?.querySelector<HTMLInputElement>('input[type="radio"]');
const button = (text: string) =>
  [...document.body.querySelectorAll<HTMLButtonElement>('button')].find((element) =>
    element.textContent?.includes(text),
  );

describe('CopyCodeModal', () => {
  afterEach(() => {
    document.body.innerHTML = '';
  });

  it('offers a static copy of a single-rule code and confirms the chosen mode', async () => {
    const wrapper = mount(CopyCodeModal, { props: { code: code(ContentMode.Dynamic, 1) }, attachTo: document.body });
    await flushPromises();

    expect(radio('Static')?.disabled).toBe(false);
    expect(radio('Static')?.checked).toBe(true);
    button('Copy as static')?.click();

    expect(wrapper.emitted('confirm')).toEqual([[ContentMode.Static]]);
    wrapper.unmount();
  });

  it('copies a multi-rule code as dynamic and says why static is unavailable', async () => {
    const wrapper = mount(CopyCodeModal, { props: { code: code(ContentMode.Dynamic, 2) }, attachTo: document.body });
    await flushPromises();

    expect(radio('Static')?.disabled).toBe(true);
    expect(radio('Dynamic')?.checked).toBe(true);
    expect(document.body.textContent).toContain('A static copy holds one rule');
    button('Copy as dynamic')?.click();

    expect(wrapper.emitted('confirm')).toEqual([[ContentMode.Dynamic]]);
    wrapper.unmount();
  });
});

describe('DeleteCodeModal', () => {
  afterEach(() => {
    document.body.innerHTML = '';
  });

  it.each([
    [ContentMode.Dynamic, 'printed copies stop working'],
    [ContentMode.Static, 'Printed copies keep working'],
  ])('states what happens to printed %s codes', async (mode, consequence) => {
    const wrapper = mount(DeleteCodeModal, {
      props: { code: code(mode, 1), isDeleting: false },
      attachTo: document.body,
    });
    await flushPromises();

    expect(document.body.textContent).toContain(consequence);
    wrapper.unmount();
  });
});
