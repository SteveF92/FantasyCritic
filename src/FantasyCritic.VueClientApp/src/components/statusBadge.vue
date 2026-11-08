<template>
  <span>
    <span v-if="isAvailable" class="badge bg-success">Available</span>
    <span v-if="alreadyOwned" class="badge bg-info">Already Owned</span>
    <span v-if="locked" class="badge bg-warning text-wrap text-start royale-status-badge">Locked</span>
    <span v-if="!isAvailable && !alreadyOwned && !locked" class="badge bg-warning text-wrap text-start royale-status-badge">{{ status }}</span>
  </span>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';

import { PossibleMasterGameYearViewModel } from '@/api/clients';

export default defineComponent({
  props: {
    possibleMasterGame: { type: Object as PropType<PossibleMasterGameYearViewModel>, required: true },
    stripThat: { type: Boolean, default: false }
  },
  computed: {
    status(): string {
      const status = this.possibleMasterGame.status;

      if (this.stripThat) {
        // If string starts with "That game", change to just "Game"
        return status.replace(/^That game/, 'Game');
      }

      return status;
    },
    isAvailable(): boolean {
      return this.possibleMasterGame.isAvailable;
    },
    alreadyOwned(): boolean {
      return this.possibleMasterGame.alreadyOwned;
    },
    locked(): boolean {
      return /^Game will release within \d+ days\.$/.test(this.possibleMasterGame.status);
    }
  }
});
</script>
