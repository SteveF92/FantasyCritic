import Vue from 'vue'
import BootstrapVue from 'bootstrap-vue';
import axios from 'axios'
import store from "./stores/store"
import router from './router'
import App from 'components/application'

import { library } from '@fortawesome/fontawesome-svg-core';
import { faCoffee, faBell } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon, FontAwesomeLayers, FontAwesomeLayersText } from '@fortawesome/vue-fontawesome';

import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap-vue/dist/bootstrap-vue.css';
import 'bootswatch/dist/superhero/bootstrap.css';

import 'bootstrap/dist/js/bootstrap.bundle.js';
import 'jquery/dist/jquery.slim.js';

library.add(faCoffee);
library.add(faBell);
Vue.component('font-awesome-icon', FontAwesomeIcon);
Vue.component('font-awesome-layers', FontAwesomeLayers)
Vue.component('font-awesome-layers-text', FontAwesomeLayersText)
Vue.use(BootstrapVue);

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
}
