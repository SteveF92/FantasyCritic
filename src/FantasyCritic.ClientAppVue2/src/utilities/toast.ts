import { pinia } from '@/store/pinia';
import { useToastStore } from '@/store/toastStore';

export function makeToast(message: string, variant = 'info'): void {
  useToastStore(pinia).showToast(message, variant);
}
