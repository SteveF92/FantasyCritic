import { beforeEach, describe, expect, it, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { accountClient, ApiException } from '@/api/clients';
import type { FantasyCriticUserViewModel } from '@/api/clients';
import { useAuthStore } from '@/store/authStore';

const admin = { userID: '00000000-0000-0000-0000-000000000000', roles: ['Admin'], showDecimalPoints: false } as unknown as FantasyCriticUserViewModel;

function unauthorized() {
  return new ApiException('Unauthorized', 401, '', {}, null);
}

describe('authStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.restoreAllMocks();
  });

  it('loads the signed-in user once', async () => {
    const currentUserCall = vi.spyOn(accountClient, 'currentUser').mockResolvedValue(admin);
    const store = useAuthStore();

    await Promise.all([store.ensureUserLoaded(), store.ensureUserLoaded()]);
    await store.ensureUserLoaded();

    expect(currentUserCall).toHaveBeenCalledTimes(1);
    expect(store.userInfoLoaded).toBe(true);
    expect(store.isAuth).toBe(true);
    expect(store.isAdmin).toBe(true);
    expect(store.isFactChecker).toBe(false);
  });

  it('treats a 401 as signed out and does not ask again', async () => {
    const currentUserCall = vi.spyOn(accountClient, 'currentUser').mockRejectedValue(unauthorized());
    const store = useAuthStore();

    await store.ensureUserLoaded();
    await store.ensureUserLoaded();

    expect(currentUserCall).toHaveBeenCalledTimes(1);
    expect(store.userInfoLoaded).toBe(true);
    expect(store.isAuth).toBe(false);
    expect(store.isAdmin).toBe(false);
  });

  it('rejects on other failures and retries on the next load', async () => {
    const currentUserCall = vi.spyOn(accountClient, 'currentUser').mockRejectedValueOnce(new Error('API down')).mockResolvedValueOnce(admin);
    const store = useAuthStore();

    await expect(store.ensureUserLoaded()).rejects.toThrow('API down');
    expect(store.userInfoLoaded).toBe(false);

    await store.ensureUserLoaded();

    expect(currentUserCall).toHaveBeenCalledTimes(2);
    expect(store.isAuth).toBe(true);
  });
});
