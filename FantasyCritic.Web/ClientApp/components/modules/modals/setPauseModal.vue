<template>
  <b-modal id="setPauseModal" ref="setPauseModalRef" title="Warning!" @ok="setPause">
    <p v-if="!paused">
      Do you wish to pause the draft? No one will be able to draft any games while the draft is paused.
    </p>
    <p v-if="paused">
      Do you wish to unpause the draft? Things will resume as normal.
    </p>
  </b-modal>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    props: ['paused'],
    methods: {
      setPause() {
        this.$refs.setPauseModalRef.hide();
        let pauseInfo = {
          pause: !this.paused
        }
        this.$emit('setPause', pauseInfo);
      }
    }
  }
</script>
