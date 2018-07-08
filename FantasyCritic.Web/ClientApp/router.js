import Vue from "vue";
import VueRouter from "vue-router";

import HomePage from "components/pages/homePage";

Vue.use(VueRouter);

let routes = [
    {
        path: "/",
        component: HomePage,
        name: "homePage"
    }
];

let theRouter = new VueRouter({ routes });

export default theRouter;
