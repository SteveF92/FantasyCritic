import Vue from 'vue';
import BootstrapVue from 'bootstrap-vue';
import { ValidationProvider, ValidationObserver, extend, localize } from 'vee-validate';
import * as rules from 'vee-validate/dist/rules';
import en from 'vee-validate/dist/locale/en.json';
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
import vueAwesomeCountdown from 'vue-awesome-countdown';

import BasicMixin from './mixins/basicMixin';

VueClipboard.config.autoSetContainer = true;
Vue.use(VueClipboard);
Vue.use(BootstrapVue);
Vue.use(VueFlatPickr);
Vue.use(vueAwesomeCountdown, 'vac');

//Vee-validate registration
Vue.component('ValidationProvider', ValidationProvider);
Vue.component('ValidationObserver', ValidationObserver);

Vue.mixin(BasicMixin);

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
extend('password', {
  params: ['target'],
  validate(value, { target }) {
    return value === target;
  },
  message: 'Entered passwords do not match'
});

localize({
  en
});

Vue.use(
  VueGtag,
  {
    config: { id: 'UA-131370681-1' }
  },
  router
);

// Registration of global components
Vue.component('FontAwesomeIcon', FontAwesomeIcon);
Vue.component('FontAwesomeLayers', FontAwesomeLayers);
Vue.component('FontAwesomeLayersText', FontAwesomeLayersText);
sync(store, router);

Vue.config.productionTip = false;

new Vue({
  router,
  store,
  render: (h) => h(App)
}).$mount('#app');
