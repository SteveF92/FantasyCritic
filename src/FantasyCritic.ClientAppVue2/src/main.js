import { createApp } from 'vue';

import BootstrapVue from 'bootstrap-vue';
import { ValidationProvider, ValidationObserver, extend, localize } from 'vee-validate';
import * as rules from 'vee-validate/dist/rules';
import en from 'vee-validate/dist/locale/en.json';

import { Chart as ChartJS, Title, Tooltip, Legend, LineElement, LinearScale, CategoryScale, PointElement, Filler } from 'chart.js';
import annotationPlugin from 'chartjs-plugin-annotation';

import App from './App.vue';
import router from './router/index';

import { pinia } from '@/store/pinia';

import { FontAwesomeIcon, FontAwesomeLayers, FontAwesomeLayersText } from './icons';

import 'bootstrap-vue/dist/bootstrap-vue.css';

const app = createApp(App);

app.use(pinia);
app.use(router);

app.use(BootstrapVue);

//Vee-validate registration
app.component('ValidationProvider', ValidationProvider);
app.component('ValidationObserver', ValidationObserver);

extend('required', {
  ...rules.required
});
extend('email', {
  ...rules.email
});
extend('min', {
  ...rules.min
});
extend('max', {
  ...rules.max
});
extend('min_value', {
  ...rules.min_value
});
extend('max_value', {
  ...rules.max_value
});
extend('integer', {
  ...rules.integer
});

localize({
  en
});

// Registration of global components
app.component('FontAwesomeIcon', FontAwesomeIcon);
app.component('FontAwesomeLayers', FontAwesomeLayers);
app.component('FontAwesomeLayersText', FontAwesomeLayersText);

ChartJS.register(Title, Tooltip, Legend, LineElement, LinearScale, CategoryScale, PointElement, Filler, annotationPlugin);

app.mount('#app');
