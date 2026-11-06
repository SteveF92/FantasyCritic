<template>
  <span>
    <span v-if="isAvailable" class="badge bg-success">Available</span>
    <span v-if="alreadyOwned" class="badge bg-info">Already Owned</span>
    <span v-if="locked" class="badge bg-warning text-wrap text-start royale-status-badge">Locked</span>
    <span v-if="!isAvailable && !alreadyOwned && !locked" class="badge bg-warning text-wrap text-start royale-status-badge">{{ status }}</span>
  </span>
</template>
<script>
export default {
  props: {
    possibleMasterGame: { type: Object, required: true },
    stripThat: { type: Boolean, required: false }
  },
  computed: {
    status() {
      const status = this.possibleMasterGame.status;

      if (this.stripThat) {
        // If string starts with "That game", change to just "Game"
        return status.replace(/^That game/, 'Game');
      }

      return status;
    },
    isAvailable() {
      return this.possibleMasterGame.isAvailable;
    },
    alreadyOwned() {
      return this.possibleMasterGame.alreadyOwned;
    },
    locked() {
      return /^Game will release within \d+ days\.$/.test(this.possibleMasterGame.status);
    }
  }
};
</script>
