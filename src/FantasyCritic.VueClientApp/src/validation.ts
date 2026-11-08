import { localize } from '@vee-validate/i18n';
import en from '@vee-validate/i18n/dist/locale/en.json';
import { email, integer, max_value, min, min_value, required } from '@vee-validate/rules';
import { configure, defineRule, Field, Form } from 'vee-validate';

defineRule('required', required);
defineRule('email', email);
defineRule('min', min);
defineRule('min_value', min_value);
defineRule('max_value', max_value);
defineRule('integer', integer);

configure({
  generateMessage: localize('en', en)
});

export { Field as ValidationField, Form as ValidationForm };
