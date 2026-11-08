<template>
  <div class="toast-container position-fixed top-0 end-0 p-3">
    <b-toast v-for="toast in toasts" :key="toast.id" :model-value="5000" no-progress solid no-close-button :variant="toast.variant" @hidden="removeToast(toast.id)">
      <template #default="{ hide }">
        <div class="toast-flex">
          <span>{{ toast.message }}</span>
          <button type="button" class="font-awesome-button" aria-label="Close" @click="hide()">
            <font-awesome-icon class="toast-close" icon="xmark" size="xl" />
          </button>
        </div>
      </template>
    </b-toast>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';

import { useToastStore } from '@/store/toastStore';

export default defineComponent({
  computed: {
    ...mapState(useToastStore, ['toasts'])
  },
  methods: {
    ...mapActions(useToastStore, ['removeToast'])
  }
});
</script>
