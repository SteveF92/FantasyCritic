import Vue from 'vue';
import VueRouter from 'vue-router';

import store from '../store';
import { routes } from './routes';

Vue.use(VueRouter);

let router = new VueRouter({
  scrollBehavior(to, from) {
    if (to.hash) {
      const smoothParams = {
        selector: to.hash,
        behavior: 'smooth'
      };

      if (!to.meta.delayScroll || to.path === from.path) {
        return smoothParams;
      }

      return new Promise((resolve) => {
        setTimeout(() => {
          resolve(smoothParams);
        }, 500);
      });
    }
    return { x: 0, y: 0 }; // Go to the top of the page if no hash
  },
  mode: 'history',
  routes
});

router.beforeEach(async function (toRoute, fromRoute, next) {
  if (toRoute.meta.title) {
    document.title = toRoute.meta.title + ' - Fantasy Critic';
  }

  if (toRoute.path !== fromRoute.path) {
    store.commit('clearPublisherStoreData');
    store.commit('clearLeagueStoreData');
  }

  if (!store.getters.allTags && !store.getters.masterGamesIsBusy) {
    await store.dispatch('getAllTags');
  }

  if (!store.getters.bidTimes && !store.getters.bidTimesIsBusy) {
    await store.dispatch('getBidTimes');
  }

  //If we are current, we're good to go
  if (store.getters.isAuth) {
    if (toRoute.meta.publicOnly) {
      next({ path: '/home' });
      return;
    }
    next();
    return;
  }

  try {
    await store.dispatch('getUserInfo');
    if (store.getters.isAuth) {
      if (toRoute.meta.publicOnly) {
        next({ path: '/home' });
        return;
      } else {
        next();
        return;
      }
    } else {
      if (toRoute.meta.isPublic) {
        next();
        return;
      } else {
        store.commit('clearUserInfo');
        window.location.href = '/Identity/Account/Login';
        return;
      }
    }
  } catch (error) {
    console.log('Router error');
    store.commit('clearUserInfo');
    window.location.href = '/Identity/Account/Login';
  }
});

export default router;
