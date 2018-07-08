import Vue from 'vue'
import axios from 'axios'
import router from './router'
import App from 'components/application'

const app = new Vue({
    router,
    ...App
});

export {
    app,
    router
}
