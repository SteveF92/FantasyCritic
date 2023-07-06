import { createRouter, createWebHistory } from 'vue-router';

import store from '../store';
import { routes } from './routes';

let router = createRouter({
  history: createWebHistory(),
  scrollBehavior(to) {
    if (to.hash) {
      return {
        el: to.hash,
        behavior: 'smooth'
      };
    }
  },
  routes
});

router.beforeEach(async function (toRoute, fromRoute, next) {
  if (toRoute.path !== fromRoute.path) {
    store.commit('publisher/clearPublisherStoreData');
    store.commit('league/clearLeagueStoreData');
  }

  if (!store.getters['interLeague/interLeagueDataLoaded'] && !store.getters['interLeague/interLeagueIsBusy']) {
    await store.dispatch('interLeague/fetchInterLeagueData');
  }

  //If we are current, we're good to go
  if (store.getters['auth/isAuth']) {
    if (toRoute.meta.publicOnly) {
      next({ path: '/home' });
      return;
    }
    next();
    return;
  }

  try {
    await store.dispatch('auth/getUserInfo');
    if (store.getters['auth/isAuth']) {
      if (toRoute.meta.publicOnly) {
        next({ path: '/home' });
        return;
      }
      return next();
    }

    if (toRoute.meta.isPublic) {
      return next();
    } else {
      store.commit('auth/clearUserInfo');
      window.location.href = '/Account/Login';
      return;
    }
  } catch (error) {
    console.log('Router error');
    store.commit('auth/clearUserInfo');
    window.location.href = '/Account/Login';
  }
});

router.afterEach((toRoute) => {
  if (toRoute.meta.title) {
    document.title = toRoute.meta.title + ' - Fantasy Critic';
  }
});

export default router;
