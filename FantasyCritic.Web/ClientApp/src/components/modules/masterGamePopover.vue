<template>
  <div>
    <div v-if="masterGame">
      <popper ref="gamePopoverRef" trigger="click" :options="{ placement: 'top', modifiers: { offset: { offset: '0,0px' } }}">
        <div class="popper">
          <masterGameSummary :masterGame="masterGame"></masterGameSummary>

          <div class="close-button fake-link" v-on:click="closePopover">
            <font-awesome-icon icon="times" size="lg" :style="{ color: 'd6993a' }" />
          </div>
        </div>

        <span slot="reference" class="text-primary fake-link">
          {{masterGame.gameName}}
        </span>
      </popper>
    </div>
  </div>
</template>

<script>
  import Vue from "vue";
  import axios from "axios";
  import moment from "moment";
  import Popper from 'vue-popperjs';
  import 'vue-popperjs/dist/vue-popper.css';
  import MasterGameSummary from "@/components/modules/masterGameSummary";

  export default {
    data() {
      return {
        error: ""
      }
    },
    components: {
      'popper': Popper,
      MasterGameSummary
    },
    props: ['masterGame'],
    methods: {
      closePopover() {
        this.$refs.gamePopoverRef.doClose();
      }
    }
  }
</script>
<style scoped>
  .close-button {
    position: absolute;
    right: 5px;
    top: 0px;
  }
</style>
