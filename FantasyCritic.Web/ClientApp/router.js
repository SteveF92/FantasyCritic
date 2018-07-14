import Vue from "vue";
import VueRouter from "vue-router";

import store from "./stores/store";

import Home from "components/pages/home";
import Login from "components/pages/login";
import Register from "components/pages/register";
import About from "components/pages/about";
import Contact from "components/pages/contact";
import ManageUser from "components/pages/manageUser";

Vue.use(VueRouter);

let routes = [
    {
        path: "/",
        component: Home,
        name: "home"
    },
    {
        path: "/login",
        component: Login,
        name: "login",
        meta: {
            isPublic: true
        }
    },
    {
        path: "/register",
        component: Register,
        name: "register",
        meta: {
            isPublic: true
        }
    },
    {
        path: "/about",
        component: About,
        name: "about",
        meta: {
            isPublic: true
        }
    },
    {
        path: "/contact",
        component: Contact,
        name: "contact",
        meta: {
            isPublic: true
        }
    },
    {
        path: "/manageUser",
        component: ManageUser,
        name: "manageUser"
    }
];

let theRouter = new VueRouter({ routes });

theRouter.beforeEach(function (toRoute, fromRoute, next) {
    if (toRoute.name === "login" && store.getters.isAuthenticated) {
        next({ path: "/" });
        return;
    }
    if (toRoute.meta.isPublic) {
        next();
        return;
    }
    if (!store.getters.isAuthenticated) {
        var token = localStorage.getItem("jwt_token");
        if (token) {
            var expiration = localStorage.getItem("jwt_expiration");
            var tokenInfo = {
                "token": token,
                "expiration": expiration
            };
            store.commit("setTokenInfo", tokenInfo);
        }
    }

    if (store.getters.isAuthenticated) {
        store.dispatch("getUserInfo")
            .then(() => { next(); });
    }
    else {
        store.commit("setRedirect", toRoute.path);
        next({ name: 'login' });
        return;
    }
});

export default theRouter;
