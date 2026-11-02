<template>
  <div>
    <b-toast v-for="toast in toasts" :key="toast.id" visible solid no-close-button :variant="toast.variant" @hidden="removeToast(toast.id)">
      <template #default="{ hide }">
        <div class="toast-flex">
          <span>{{ toast.message }}</span>
          <font-awesome-icon class="toast-close" icon="xmark" size="xl" @click="hide" />
        </div>
      </template>
    </b-toast>
  </div>
</template>

<script>
import { mapState, mapActions } from 'pinia';
import { useToastStore } from '@/store/toastStore';

export default {
  computed: {
    ...mapState(useToastStore, ['toasts'])
  },
  methods: {
    ...mapActions(useToastStore, ['removeToast'])
  }
};
</script>
