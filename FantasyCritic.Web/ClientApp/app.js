import Vue from 'vue';
import BootstrapVue from 'bootstrap-vue';
import axios from 'axios';
import router from './router/index';
import store from './store';
import { sync } from 'vuex-router-sync';
import App from 'components/app-root';
import { FontAwesomeIcon } from './icons';
import Toasted from 'vue-toasted';
import VeeValidate from 'vee-validate';

import "./filters";

import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap-vue/dist/bootstrap-vue.css';
import 'bootstrap/dist/js/bootstrap.bundle.js';

Vue.use(BootstrapVue);
Vue.use(Toasted);
Vue.use(VeeValidate);

// Registration of global components
Vue.component('icon', FontAwesomeIcon);
Vue.prototype.$http = axios;
sync(store, router);

axios.interceptors.response.use(function (response) {
  return response;
}, function(error) {
  const originalRequest = error.config;
  if (error.code !== "ECONNABORTED" && error.response.status === 401) {
    if (!originalRequest._retry) {
      originalRequest._retry = true;
      return axios.post("/api/token/refresh", refreshRequest)
        .then((res) => {
          store.commit("setTokenInfo", res.data);
          store.commit("setRefreshToken", res.data.refreshToken);
          var newBearer = "Bearer " + res.data.token;
          originalRequest.headers.Authorization = newBearer;
          return axios(originalRequest);
        });
    } else {
      store.commit("clearUserAndToken");
      router.push({ name: 'login' });
      return Promise.reject(error);
    }
  }
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
