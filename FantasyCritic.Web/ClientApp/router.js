import Vue from "vue";
import VueRouter from "vue-router";

import HomePage from "components/pages/homePage";
import Login from "components/pages/login";
import AboutPage from "components/pages/aboutPage";
import ContactPage from "components/pages/contactPage";

Vue.use(VueRouter);

let routes = [
    {
        path: "/",
        component: HomePage,
        name: "homePage"
    },
    {
        path: "/login",
        component: Login,
        name: "login"
    },
    {
        path: "/about",
        component: AboutPage,
        name: "aboutPage"
    },
    {
        path: "/contact",
        component: ContactPage,
        name: "contactPage"
    },
];

let theRouter = new VueRouter({ routes });

export default theRouter;
