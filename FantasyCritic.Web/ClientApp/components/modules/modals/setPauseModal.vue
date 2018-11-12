<template>
  <b-modal id="setPause" ref="setPauseRef" title="Warning!" @ok="setPause">
    <div v-if="!paused">
      Do you wish to pause the draft? No one will be able to draft any games while the draft is paused.
    </div>
    <div v-if="paused">
      Do you wish to unpause the draft? Things will resume as normal.
    </div>
  </b-modal>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    props: ['paused'],
    methods: {
      setPause() {
        this.$refs.setPauseRef.hide();
        let pauseInfo = new {
          pause: !paused
        }
        this.$emit('setPause', pauseInfo);
      }
    }
  }
</script>
