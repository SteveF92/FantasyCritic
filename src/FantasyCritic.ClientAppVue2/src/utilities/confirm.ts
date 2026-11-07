import { useModal, type ColorVariant } from 'bootstrap-vue-next';

export interface ConfirmOptions {
  title?: string;
  okTitle?: string;
  okVariant?: ColorVariant;
  cancelTitle?: string;
}

export type Confirm = (message: string, options?: ConfirmOptions) => Promise<boolean>;

// Call from setup(): useModal reads the modal orchestrator that BApp provides.
export function useConfirm(): Confirm {
  const { create } = useModal();
  return async (message, options = {}) => {
    const modal = create({ body: message, ...options });
    try {
      return (await modal.show()).ok === true;
    } finally {
      await modal.destroy();
    }
  };
}
