import type { ColorVariant } from 'bootstrap-vue-next';

import { pinia } from '@/store/pinia';
import { useToastStore } from '@/store/toastStore';

export function makeToast(message: string, variant: ColorVariant = 'info'): void {
  useToastStore(pinia).showToast(message, variant);
}
