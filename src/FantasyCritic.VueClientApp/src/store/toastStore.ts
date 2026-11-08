import { defineStore } from 'pinia';

interface Toast {
  id: number;
  message: string;
  variant: string;
}

interface ToastStoreState {
  toasts: Toast[];
  nextToastID: number;
}

export const useToastStore = defineStore('toast', {
  state: (): ToastStoreState => ({ toasts: [] as Toast[], nextToastID: 0 }),
  actions: {
    showToast(message: string, variant: string) {
      this.toasts.push({ id: this.nextToastID++, message, variant });
    },
    removeToast(id: number) {
      this.toasts = this.toasts.filter((x) => x.id !== id);
    }
  }
});
