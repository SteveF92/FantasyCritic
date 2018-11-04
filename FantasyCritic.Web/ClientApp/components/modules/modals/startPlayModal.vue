<template>
  <b-modal id="startPlay" ref="startPlayRef" title="Warning!" @ok="startPlay">
    Warning! Once you start drafting, you can no longer add or remove players, and the draft order is locked in.
  </b-modal>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    methods: {
      startPlay() {
        this.$refs.startPlayRef.hide();
        this.$emit('playStarted');
      }
    }
  }
</script>
