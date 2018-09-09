import Vue from 'vue';
import BootstrapVue from 'bootstrap-vue';
import axios from 'axios';
import router from './router/index';
import store from './store';
import { sync } from 'vuex-router-sync';
import App from 'components/app-root';
import { FontAwesomeIcon } from './icons';
import Toasted from 'vue-toasted';

import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap-vue/dist/bootstrap-vue.css';
import 'bootswatch/dist/superhero/bootstrap.css';

import 'bootstrap/dist/js/bootstrap.bundle.js';
Vue.use(BootstrapVue);
Vue.use(Toasted);

// Registration of global components
Vue.component('icon', FontAwesomeIcon);
Vue.prototype.$http = axios;
sync(store, router);

const app = new Vue({
  store,
  router,
  ...App
});

export {
  app,
  router,
  store
  };
