<template>
  <b-modal id="startDraft" ref="startDraftRef" title="Warning!" @ok="startDraft">
    <p>Warning! Once you start drafting, you can no longer add or remove players, and the draft order is locked in.</p>
  </b-modal>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    methods: {
      startDraft() {
        this.$refs.startDraftRef.hide();
        this.$emit('draftStarted');
      }
    }
  }
</script>
