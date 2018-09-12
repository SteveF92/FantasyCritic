import Vue from 'vue';
import BootstrapVue from 'bootstrap-vue';
import axios from 'axios';
import router from './router/index';
import store from './store';
import { sync } from 'vuex-router-sync';
import App from 'components/app-root';
import { FontAwesomeIcon } from './icons';
import Toasted from 'vue-toasted';

import "./filters";

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

axios.interceptors.response.use(function (response) {
  return response;
}, function (error) {
  const originalRequest = error.config;
  if (error.response.status === 401 && !originalRequest._retry) {
    originalRequest._retry = true;

    var oldToken = localStorage.getItem("jwt_token");
    var refreshToken = localStorage.getItem("refresh_token");
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
      });
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
