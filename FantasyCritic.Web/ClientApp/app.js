import Vue from 'vue';
import BootstrapVue from 'bootstrap-vue';
import axios from 'axios';
import router from './router/index';
import store from './store';
import { sync } from 'vuex-router-sync';
import App from 'components/app-root';

import { FontAwesomeIcon, FontAwesomeLayers, FontAwesomeLayersText } from './icons';
import Toasted from 'vue-toasted';
import { ValidationProvider, ValidationObserver, extend } from 'vee-validate';
import { required, email, min_value, max_value } from 'vee-validate/dist/rules';
import { messages } from 'vee-validate/dist/locale/en.json';
import VueAnalytics from 'vue-analytics';
import VueClipboard from 'vue-clipboard2';
import VueDatePicker from '@mathieustan/vue-datepicker';

import "./filters";
import 'bootstrap-vue/dist/bootstrap-vue.css';
import 'bootstrap/dist/js/bootstrap.bundle.js';

VueClipboard.config.autoSetContainer = true;
Vue.use(VueClipboard);
Vue.use(BootstrapVue);
Vue.use(Toasted);

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
extend('min_value', {
  ...min_value,
  message: messages['min_value']
});
extend('max_value', {
  ...max_value,
  message: messages['max_value']
});

// Options: You can set lang by default
Vue.use(VueDatePicker, {
  lang: 'en'
});

Vue.use(VueAnalytics, {
  id: 'UA-131370681-1',
  router
});

// Registration of global components
Vue.component('font-awesome-icon', FontAwesomeIcon);
Vue.component('font-awesome-layers', FontAwesomeLayers);
Vue.component('font-awesome-layers-text', FontAwesomeLayersText);
sync(store, router);

axios.interceptors.response.use(function (response) {
  return response;
}, function (error) {
  const originalRequest = error.config;
  if (error.code !== "ECONNABORTED" && error.response.status === 401) {
    if (!originalRequest._retry) {
      originalRequest._retry = true;
      var oldToken = localStorage.getItem("jwt_token");
      var refreshToken = localStorage.getItem("refresh_token");
      if (oldToken && refreshToken) {
        var refreshRequest = {
          token: oldToken,
          refreshToken: refreshToken
        };
        return axios.post("/api/token/refresh", refreshRequest)
          .then((res) => {
            store.commit("setTokenInfo", res.data);
            store.commit("setRefreshToken", res.data.refreshToken);
            var newBearer = "Bearer " + res.data.token;
            originalRequest.headers.Authorization = newBearer;
            return axios(originalRequest);
          })
          .catch((error) => {
            store.commit("clearUserAndToken");
            router.push({ name: 'login' });
            return Promise.reject(error);
          });
      } else {
        store.commit("clearUserAndToken");
        router.push({ name: 'login' });
        return Promise.reject(error);
      }
    } else {
      store.commit("clearUserAndToken");
      router.push({ name: 'login' });
      return Promise.reject(error);
    }
  }
  return Promise.reject(error);
});

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
