<template>
  <b-modal id="sellRoyaleGameModal" ref="sellRoyaleGameModalRef" title="Sell Game" @ok="sellGame">
    <template v-if="publisherGame">
      <div v-if="!publisherGame.currentlyIneligible">
        <p>
          Are you sure you want to sell
          <strong>{{ publisherGame.masterGame.gameName }}</strong>
          ?
        </p>
        <template v-if="isYearQuarter2026Q3FeatureSupported">
          <template v-if="publisherGame.inRegretWindow">
            <p>Since you are still within the 10 minute "regret window", you can sell this game back for a full refund.</p>
          </template>
          <template v-else>
            <p>You will get back half of what you spent on the game, plus any advertising money currently assigned to it.</p>
            <p>If the games is confirmed to not release in the quarter, you get back 75% of what you spent on it instead.</p>
          </template>
        </template>
        <template v-else>
          <p>You will get back half of the current market value of the game, plus any advertising money currently assigned to it.</p>
          <p>If the games is confirmed to not release in the quarter, you get back 75% of market value instead.</p>
        </template>
        <p>
          Money to receive:
          <strong>{{ (publisherGame.refundAmount + publisherGame.advertisingMoney) | money }}</strong>
        </p>
      </div>
      <div v-else>
        <p>
          Because
          <strong>{{ publisherGame.masterGame.gameName }}</strong>
          is not eligible for points, you will get a full refund on the money spent.
        </p>
        <p>You'll also get back any advertising money currently assigned to it.</p>
        <p>
          Money to recieve:
          <strong>{{ (publisherGame.refundAmount + publisherGame.advertisingMoney) | money }}</strong>
        </p>
      </div>
    </template>
  </b-modal>
</template>
<script>
import { yearQuarter2026Q3FeatureSupported } from '@/models/royale/RoyaleYearQuarter';

export default {
  props: {
    publisherGame: { type: Object, default: null },
    yearQuarter: { type: Object, required: true }
  },
  computed: {
    isYearQuarter2026Q3FeatureSupported() {
      return yearQuarter2026Q3FeatureSupported(this.yearQuarter);
    }
  },
  methods: {
    sellGame() {
      if (!this.publisherGame) {
        return;
      }

      this.$refs.sellRoyaleGameModalRef.hide();
      this.$emit('sellGame', this.publisherGame);
    }
  }
};
</script>
