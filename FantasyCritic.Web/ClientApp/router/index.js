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

  //Attempt to get local token if we don't have it in memory
  if (!store.getters.tokenIsCurrent()) {
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

  //If we are current, we're good to go
  if (store.getters.tokenIsCurrent()) {
    if (toRoute.meta.publicOnly) {
      next({ path: "/home" });
      return;
    }
    next();
    return;
  }

  //If not current and not public, attempt refresh token.
  var oldToken = localStorage.getItem("jwt_token");
  var refreshToken = localStorage.getItem("refresh_token");
  if (oldToken && refreshToken) {
    var refreshRequest = {
      token: oldToken,
      refreshToken: refreshToken
    };

    store.dispatch("refreshToken", refreshRequest)
      .then(() => {
        if (store.getters.tokenIsCurrent()) {
          if (toRoute.meta.publicOnly) {
            next({ path: "/home" });
            return;
          }
          next();
          return;
        } else {
          store.commit("setRedirect", toRoute.path);
          next({ name: 'login' });
          return;
        }
      })
      .catch(() => {
        console.log("Router error");
        store.commit("clearUserAndToken");
        next({ name: 'login' });
        return;
      });
  } else {
    if (toRoute.meta.isPublic) {
      next();
      return;
    } else {
      store.commit("clearUserAndToken");
      next({ name: 'login' });
      return;
    }
  }
});

export default router;
