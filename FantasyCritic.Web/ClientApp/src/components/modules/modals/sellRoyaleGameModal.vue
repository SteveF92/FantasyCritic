<template>
  <b-modal id="sellRoyaleGameModal" ref="sellRoyaleGameModalRef" title="Sell Game" @ok="sellGame">
    <div v-if="!publisherGame.currentlyIneligible">
      <p>Are you sure you want to sell <strong>{{publisherGame.masterGame.gameName}}</strong>?</p>
      <p>You will get back half of the money you bought it for, and any advertising money currently assigned to it.</p>
      <p>Money to recieve: <strong>{{publisherGame.refundAmount + publisherGame.advertisingMoney | money}}</strong></p>
    </div>
    <div v-else>
      <p>Because <strong>{{publisherGame.masterGame.gameName}}</strong> is not eligible for points, you will get a full refund on the money spent.</p>
      <p>You'll also get back any advertising money currently assigned to it.</p>
      <p>Money to recieve: <strong>{{publisherGame.refundAmount + publisherGame.advertisingMoney | money}}</strong></p>
    </div>
  </b-modal>
</template>
<script>
import Vue from 'vue';
import axios from 'axios';

export default {
  props: ['publisherGame'],
  methods: {
    sellGame() {
      this.$refs.sellRoyaleGameModalRef.hide();
      this.$emit('sellGame', this.publisherGame);
    }
  }
};
</script>
