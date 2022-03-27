<template>
  <span>
    <span v-show="isAvailable" class="badge badge-primary badge-success">Available</span>
    <span v-show="alreadyOwned" class="badge badge-primary badge-info">Already Owned</span>
    <span v-show="!isAvailable && !alreadyOwned" class="badge badge-primary badge-warning">{{ status }}</span>
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
    }
  }
};
</script>
