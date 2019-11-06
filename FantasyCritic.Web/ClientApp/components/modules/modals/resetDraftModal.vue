<template>
  <b-modal id="resetDraftModal" ref="resetDraftModalRef" title="Warning!" @ok="resetDraft">
    <p>
      Are you sure you want to reset the draft? Any games that have been drafted will be reset, and you will be able to change players/draft order/game settings again.
    </p>
  </b-modal>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    methods: {
      resetDraft() {
        this.$refs.resetDraftModalRef.hide();
        this.$emit('resetDraft');
      }
    }
  }
</script>
