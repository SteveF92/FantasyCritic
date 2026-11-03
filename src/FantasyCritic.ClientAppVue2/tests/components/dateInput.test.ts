import { describe, expect, it } from 'vitest';
import { mount } from '@vue/test-utils';
import DateInput from '@/components/dateInput.vue';

function inputValue(wrapper) {
  return (wrapper.element as HTMLInputElement).value;
}

describe('dateInput', () => {
  it('shows its value', () => {
    const wrapper = mount(DateInput, { propsData: { value: '2026-06-10' } });
    expect(inputValue(wrapper)).toBe('2026-06-10');
  });

  it('follows a new value from the model', async () => {
    const wrapper = mount(DateInput, { propsData: { value: '2026-06-10' } });
    await wrapper.setProps({ value: '2026-07-01' });
    expect(inputValue(wrapper)).toBe('2026-07-01');
  });

  it('emits the entered date', async () => {
    const wrapper = mount(DateInput);
    await wrapper.setValue('2026-06-10');
    expect(wrapper.emitted('input')).toEqual([['2026-06-10']]);
  });

  it('emits null, not an empty string, when cleared', async () => {
    const wrapper = mount(DateInput, { propsData: { value: '2026-06-10' } });
    await wrapper.setValue('');
    expect(wrapper.emitted('input')).toEqual([[null]]);
  });
});
