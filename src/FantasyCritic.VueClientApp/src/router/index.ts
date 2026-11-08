import { createRouter, createWebHistory, RouterScrollBehavior } from 'vue-router';

import { useAuthStore } from '@/store/authStore';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import { useLeagueStore } from '@/store/leagueStore';
import { pinia } from '@/store/pinia';
import { usePublisherStore } from '@/store/publisherStore';

import { routes } from './routes.js';

const scrollBehavior: RouterScrollBehavior = (to, from, savedPosition) => {
  if (savedPosition) {
    return savedPosition;
  }

  if (!to.hash) {
    return { left: 0, top: 0 };
  }

  const anchor: ScrollToOptions & { el: string } = { el: to.hash, behavior: 'smooth' };
  if (!to.meta.delayScroll || to.path === from.path) {
    return anchor;
  }

  return new Promise((resolve) => {
    setTimeout(() => resolve(anchor), 500);
  });
};

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

  await Promise.all([interLeagueStore.ensureLoaded(), authStore.ensureUserLoaded()]);

  if (authStore.isAuth) {
    if (toRoute.meta.publicOnly) {
      next({ path: '/home' });
      return;
    }

    if (toRoute.meta.adminOnly && !authStore.canAccessAdminPages) {
      next({ path: '/home' });
      return;
    }

    next();
    return;
  }

  if (toRoute.meta.isPublic) {
    next();
    return;
  }

  window.location.href = '/Account/Login';
});

router.afterEach((toRoute) => {
  if (toRoute.meta.title) {
    document.title = toRoute.meta.title + ' - Fantasy Critic';
  }
});

export default router;
