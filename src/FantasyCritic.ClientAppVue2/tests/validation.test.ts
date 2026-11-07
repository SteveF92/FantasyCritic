import { describe, expect, it } from 'vitest';
import { defineComponent } from 'vue';
import { flushPromises, mount } from '@vue/test-utils';
import { ValidationField, ValidationForm } from '@/validation';

const RequestForm = {
  components: { ValidationField, ValidationForm },
  template: `
    <ValidationForm v-slot="{ meta }" as="">
      <form>
        <ValidationField v-slot="{ errors }" :model-value="gameName" name="Game Name" rules="required">
          <input id="gameName" v-model="gameName" type="text" />
          <span id="gameNameError">{{ errors[0] }}</span>
        </ValidationField>
        <ValidationField v-slot="{ errors }" :model-value="inviteEmail" name="Email Address" rules="email">
          <input id="inviteEmail" v-model="inviteEmail" type="text" />
          <span id="inviteEmailError">{{ errors[0] }}</span>
        </ValidationField>
        <input id="submit" type="submit" :disabled="!meta.valid" />
      </form>
    </ValidationForm>
  `,
  data: () => ({ gameName: '', inviteEmail: '' })
};

async function mountRequestForm() {
  const wrapper = mount(RequestForm);
  await flushPromises();
  return wrapper;
}

async function typeInto(wrapper, selector: string, value: string) {
  await wrapper.find(selector).setValue(value);
  await flushPromises();
}

async function messageFor(rules: string, value: unknown) {
  const Single = defineComponent({
    components: { ValidationField },
    template: `
      <ValidationField v-slot="{ errors }" :model-value="value" name="Total Number of Games" :rules="rules">
        <span id="error">{{ errors[0] }}</span>
      </ValidationField>
    `,
    data: () => ({ value: 'placeholder' as unknown, rules })
  });
  const wrapper = mount(Single);
  await flushPromises();
  wrapper.vm.value = value;
  await flushPromises();
  return wrapper.find('#error').text();
}

describe('validation', () => {
  it('shows no errors on mount but starts the form invalid', async () => {
    const wrapper = await mountRequestForm();
    expect(wrapper.find('#gameNameError').text()).toBe('');
    expect(wrapper.find('#submit').attributes('disabled')).toBeDefined();
  });

  it('enables submit once the required field is filled', async () => {
    const wrapper = await mountRequestForm();
    await typeInto(wrapper, '#gameName', 'Hollow Knight: Silksong');
    expect(wrapper.find('#submit').attributes('disabled')).toBeUndefined();
  });

  it('shows the required message when a filled field is cleared', async () => {
    const wrapper = await mountRequestForm();
    await typeInto(wrapper, '#gameName', 'Silksong');
    await typeInto(wrapper, '#gameName', '');
    expect(wrapper.find('#gameNameError').text()).toBe('The Game Name field is required');
    expect(wrapper.find('#submit').attributes('disabled')).toBeDefined();
  });

  it('lets an optional email stay empty but rejects a bad one', async () => {
    const wrapper = await mountRequestForm();
    await typeInto(wrapper, '#gameName', 'Silksong');
    expect(wrapper.find('#submit').attributes('disabled')).toBeUndefined();

    await typeInto(wrapper, '#inviteEmail', 'not-an-email');
    expect(wrapper.find('#inviteEmailError').text()).toBe('The Email Address field must be a valid email');
    expect(wrapper.find('#submit').attributes('disabled')).toBeDefined();
  });

  it('uses the same messages as vee-validate 3', async () => {
    expect(await messageFor('min:30', 'short')).toBe('The Total Number of Games field must be at least 30 characters');
    expect(await messageFor('min_value:1', '0')).toBe('The Total Number of Games field must be 1 or more');
    expect(await messageFor('max_value:50', '51')).toBe('The Total Number of Games field must be 50 or less');
    expect(await messageFor('integer', '1.5')).toBe('The Total Number of Games field must be an integer');
  });
});
