import { describe, expect, it } from 'vitest';
import { mount } from '@vue/test-utils';
import DateInput from '@/components/dateInput.vue';

function inputValue(wrapper) {
  return (wrapper.element as HTMLInputElement).value;
}

describe('dateInput', () => {
  it('shows its value', () => {
    const wrapper = mount(DateInput, { props: { modelValue: '2026-06-10' } });
    expect(inputValue(wrapper)).toBe('2026-06-10');
  });

  it('follows a new value from the model', async () => {
    const wrapper = mount(DateInput, { props: { modelValue: '2026-06-10' } });
    await wrapper.setProps({ modelValue: '2026-07-01' });
    expect(inputValue(wrapper)).toBe('2026-07-01');
  });

  it('emits the entered date', async () => {
    const wrapper = mount(DateInput);
    await wrapper.find('input').setValue('2026-06-10');
    expect(wrapper.emitted('update:modelValue')).toEqual([['2026-06-10']]);
  });

  it('emits null, not an empty string, when cleared', async () => {
    const wrapper = mount(DateInput, { props: { modelValue: '2026-06-10' } });
    await wrapper.find('input').setValue('');
    expect(wrapper.emitted('update:modelValue')).toEqual([[null]]);
  });
});
