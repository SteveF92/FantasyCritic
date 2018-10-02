import Vue from 'vue';
import VueRouter from 'vue-router';

import store from '../store';
import { routes } from './routes';

Vue.use(VueRouter);

let router = new VueRouter({
  mode: 'history',
  routes
});

router.beforeEach(function (toRoute, fromRoute, next) {
  if (toRoute.meta.title) {
    document.title = toRoute.meta.title + " - Fantasy Critic";
  }
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
      })
      .catch(() => {
        console.log("Router error");
        context.commit("clearUserAndToken");
        next({ name: 'login' });
      });
  }

  if (store.getters.tokenIsCurrent(new Date())) {
    store.dispatch("getUserInfo")
      .then(() => { next() });
  }
});

export default router;
