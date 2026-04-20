<template>
  <span>
    <span v-if="isAvailable" class="badge badge-primary badge-success">Available</span>
    <span v-if="alreadyOwned" class="badge badge-primary badge-info">Already Owned</span>
    <span v-if="locked" class="badge badge-primary badge-warning text-wrap text-left royale-status-badge">Locked</span>
    <span v-if="!isAvailable && !alreadyOwned && !locked" class="badge badge-primary badge-warning text-wrap text-left royale-status-badge">{{ status }}</span>
  </span>
</template>
<script>
export default {
  props: {
    possibleMasterGame: { type: Object, required: true }
  },
  computed: {
    status() {
      return this.possibleMasterGame.status;
    },
    isAvailable() {
      return this.possibleMasterGame.isAvailable;
    },
    alreadyOwned() {
      return this.possibleMasterGame.alreadyOwned;
    },
    locked() {
      return this.possibleMasterGame.status === 'Game will release within 5 days.';
    }
  }
};
</script>
