import Vue from 'vue'
import BootstrapVue from 'bootstrap-vue';
import axios from 'axios'
import router from './router'
import App from 'components/application'

import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap-vue/dist/bootstrap-vue.css';
import 'bootswatch/dist/superhero/bootstrap.css';

import 'bootstrap/dist/js/bootstrap.bundle.js';
import 'jquery/dist/jquery.slim.js';

Vue.use(BootstrapVue);

const app = new Vue({
    router,
    ...App
});

export {
    app,
    router
}
