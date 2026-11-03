import { Field, Form, configure, defineRule } from 'vee-validate';
import { email, integer, max_value, min, min_value, required } from '@vee-validate/rules';
import { localize } from '@vee-validate/i18n';
import en from '@vee-validate/i18n/dist/locale/en.json';

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
