<template>
  <b-modal id="sellRoyaleGameModal" ref="sellRoyaleGameModalRef" title="Sell Game" @ok="sellGame">
    <p>Are you sure you want to sell <strong>{{publisherGame.masterGame.gameName}}</strong>?</p>
    <p>You will get back half the money you bought it for, and any advertising money currently assigned to it.</p>
    <p>Money to recieve: <strong>{{publisherGame.amountSpent / 2 + publisherGame.advertisingMoney | money}}</strong></p>
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
