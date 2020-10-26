<template>
  <span>
    <span class="badge badge-pill badge-primary badge-success" v-show="isAvailable">Available</span>
    <span class="badge badge-pill badge-primary badge-info" v-show="alreadyOwned">Already Owned</span>
    <span class="badge badge-pill badge-primary badge-warning" v-show="!isAvailable && !alreadyOwned">{{status}}</span>
  </span>
</template>
<script>
import Vue from 'vue';

export default {
    props: ['possibleMasterGame'],
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
