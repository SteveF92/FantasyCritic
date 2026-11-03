import { createRouter, createWebHistory } from 'vue-router';

import { pinia } from '@/store/pinia';
import { useAuthStore } from '@/store/authStore';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import { useLeagueStore } from '@/store/leagueStore';
import { usePublisherStore } from '@/store/publisherStore';

import { routes } from './routes.js';

function scrollBehavior(to, from) {
  if (to.hash) {
    const smoothParams = {
      el: to.hash,
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
  return { left: 0, top: 0 }; // Go to the top of the page if no hash
}

const router = createRouter({
  scrollBehavior,
  history: createWebHistory(),
  routes
});

router.beforeEach(async function (toRoute, fromRoute, next) {
  const authStore = useAuthStore(pinia);
  const interLeagueStore = useInterLeagueStore(pinia);
  const leagueStore = useLeagueStore(pinia);
  const publisherStore = usePublisherStore(pinia);

  if (toRoute.path !== fromRoute.path) {
    publisherStore.clearPublisherStoreData();
    leagueStore.clearLeagueStoreData();
  }

  if (!interLeagueStore.interLeagueDataLoaded && !interLeagueStore.interLeagueIsBusy) {
    await interLeagueStore.fetchBasicData();
  }

  //If we are current, we're good to go
  if (authStore.isAuth) {
    if (toRoute.meta.publicOnly) {
      next({ path: '/home' });
      return;
    }
    next();
    return;
  }

  try {
    await authStore.getUserInfo();
    if (authStore.isAuth) {
      if (toRoute.meta.publicOnly) {
        next({ path: '/home' });
        return;
      }
      return next();
    }

    if (toRoute.meta.isPublic) {
      return next();
    } else {
      window.location.href = '/Account/Login';
      return;
    }
  } catch {
    console.log('Router error');
    window.location.href = '/Account/Login';
  }
});

router.afterEach((toRoute) => {
  if (toRoute.meta.title) {
    document.title = toRoute.meta.title + ' - Fantasy Critic';
  }
});

export default router;
