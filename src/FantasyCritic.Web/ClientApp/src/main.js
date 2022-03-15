import Vue from 'vue';
import BootstrapVue from 'bootstrap-vue';
import Toasted from 'vue-toasted';
import { ValidationProvider, ValidationObserver, extend } from 'vee-validate';
import { required, email, min_value, max_value, min, max, integer } from 'vee-validate/dist/rules';
import { messages } from 'vee-validate/dist/locale/en.json';
import VueGtag from 'vue-gtag';
import VueClipboard from 'vue-clipboard2';

import App from './App.vue';
import router from './router/index';
import store from './store';
import { sync } from 'vuex-router-sync';
import { FontAwesomeIcon, FontAwesomeLayers, FontAwesomeLayersText } from './icons';

import './filters';
import 'bootstrap-vue/dist/bootstrap-vue.css';
import 'bootstrap/dist/js/bootstrap.bundle.js';
import VueFlatPickr from 'vue-flatpickr-component';
import 'flatpickr/dist/flatpickr.css';

import './registerServiceWorker';

VueClipboard.config.autoSetContainer = true;
Vue.use(VueClipboard);
Vue.use(BootstrapVue);
Vue.use(Toasted);
Vue.use(VueFlatPickr);

//Vee-validate registration
Vue.component('ValidationProvider', ValidationProvider);
Vue.component('ValidationObserver', ValidationObserver);

extend('required', {
  ...required,
  message: messages['required']
});
extend('email', {
  ...email,
  message: messages['email']
});
extend('min', {
  ...min,
  message: messages['min']
});
extend('max', {
  ...max,
  message: messages['max']
});
extend('min_value', {
  ...min_value,
  message: messages['min_value']
});
extend('max_value', {
  ...max_value,
  message: messages['max_value']
});
extend('integer', {
  ...integer,
  message: messages['integer']
});
extend('password', {
  params: ['target'],
  validate(value, { target }) {
    return value === target;
  },
  message: 'Entered passwords do not match'
});

Vue.use(
  VueGtag,
  {
    config: { id: 'UA-131370681-1' }
  },
  router
);

// Registration of global components
Vue.component('font-awesome-icon', FontAwesomeIcon);
Vue.component('font-awesome-layers', FontAwesomeLayers);
Vue.component('font-awesome-layers-text', FontAwesomeLayersText);
sync(store, router);

Vue.config.productionTip = false;

new Vue({
  router,
  store,
  render: (h) => h(App)
}).$mount('#app');
