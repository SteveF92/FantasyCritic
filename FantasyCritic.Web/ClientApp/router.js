import Vue from "vue";
import VueRouter from "vue-router";

import store from "./stores/store";

import Home from "components/pages/home";
import Login from "components/pages/login";
import Register from "components/pages/register";
import About from "components/pages/about";
import Contact from "components/pages/contact";
import ManageUser from "components/pages/manageUser";
import CreateLeague from "components/pages/createLeague";
import League from "components/pages/league";
import Publisher from "components/pages/publisher";

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
    },
    {
        path: "/createLeague",
        component: CreateLeague,
        name: "createLeague"
    },
    {
        path: "/league/:leagueid/:year",
        component: League,
        name: "league",
        props: (route) => ({
            leagueid: route.params.leagueid,
            year: route.params.year
        })
    },
    {
        path: "/publisher/:publisherid",
        component: Publisher,
        name: "publisher",
        props: (route) => ({
            publisherid: route.params.publisherid
        })
    }
];

let theRouter = new VueRouter({ routes });

theRouter.beforeEach(function (toRoute, fromRoute, next) {
    if (toRoute.name === "login" && store.getters.tokenIsCurrent(new Date())) {
        next({ path: "/" });
        return;
    }
    if (toRoute.meta.isPublic) {
        next();
        return;
    }

    if (!store.getters.hasToken) {
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

    if (!store.getters.hasToken) {
        store.commit("setRedirect", toRoute.path);
        next({ name: 'login' });
        return;
    }

    if (!store.getters.tokenIsCurrent(new Date())) {
        var oldToken = localStorage.getItem("jwt_token");
        var refreshToken = localStorage.getItem("refresh_token");
        var refreshRequest = {
            token: oldToken,
            refreshToken: refreshToken
        };

        store.dispatch("refreshToken", refreshRequest)
            .then(() => {
                if (store.getters.tokenIsCurrent(new Date())) {
                    next();
                } else {
                    store.commit("setRedirect", toRoute.path);
                    next({ name: 'login' });
                }
            });
    }

    if (store.getters.tokenIsCurrent(new Date())) {
        store.dispatch("getUserInfo")
            .then(() => { next() });
    }
});

export default theRouter;
