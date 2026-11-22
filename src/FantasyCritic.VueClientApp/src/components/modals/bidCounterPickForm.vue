<template>
  <b-modal id="bid-counter-pick-form" ref="bidCounterPickFormRef" size="lg" title="Make a Counter Pick Bid" no-footer @hidden="clearData" @show="getPossibleCounterPicks">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <p>
      You can use this form to place a bid on a game.
      <br />
      Bids are processed on Saturday Nights. See the FAQ for more info.
      <br />
      Games that have already released, or are confirmed not to release this year are not available to be counter picked.
    </p>

    <div v-if="!counterPickBiddingAllowed" class="alert alert-warning">No counter pick bids were placed before bids were revealed, so counter pick bidding is closed for this week.</div>

    <div v-if="publisherSlotsAreFilled" class="alert alert-danger">You have already filled all of your counter pick slots!</div>

    <form method="post" class="form-horizontal" role="form" @submit.prevent>
      <div class="search-results">
        <h3 class="text-black">Available Counter Picks</h3>
        <b-form-select v-model="bidCounterPick">
          <option v-for="publisherGame in availableCounterPicks" :key="publisherGame.publisherGameID" :value="publisherGame">
            {{ publisherGame.gameName }}
          </option>
        </b-form-select>

        <label for="bidAmount" class="control-label">Bid Amount (Remaining: {{ formatCurrency(requiredUserPublisher.budget, 0) }})</label>

        <ValidationField v-slot="{ errors }" :model-value="bidAmount" name="bidAmount" rules="required|integer">
          <input id="bidAmount" v-model="bidAmount" name="bidAmount" type="number" class="form-control input" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationField>

        <div v-show="counterPickInvalid" class="alert alert-warning" role="alert">Unfortunately, you cannot make a counter pick bid for a game that is not linked to a master game.</div>

        <b-button variant="primary" class="full-width-button" :disabled="!canPlaceCounterPickBid" @click="bidGame">Place Counter Pick Bid</b-button>
        <div v-if="bidResult && !bidResult.success" class="alert alert-danger bid-error">
          <h3 class="alert-heading">Error!</h3>
          <ul>
            <li v-for="error in bidResult.errors" :key="error">{{ error }}</li>
          </ul>
        </div>
      </div>
    </form>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueClient, type PickupBidRequest, type PickupBidResultViewModel, type PublisherGameViewModel } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatCurrency } from '@/utilities/formatters';

interface BidCounterPickFormData {
  bidCounterPick: PublisherGameViewModel | null;
  bidAmount: number;
  bidResult: PickupBidResultViewModel | null;
  possibleCounterPicks: PublisherGameViewModel[];
  errorInfo: string;
  isBusy: boolean;
}

export default defineComponent({
  data(): BidCounterPickFormData {
    return {
      bidCounterPick: null,
      bidAmount: 0,
      bidResult: null,
      possibleCounterPicks: [],
      errorInfo: '',
      isBusy: false
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear', 'requiredUserPublisher', 'counterPickBiddingAllowed']),
    canPlaceCounterPickBid(): boolean {
      return !!this.bidCounterPick && !this.publisherSlotsAreFilled && !this.isBusy && !this.counterPickInvalid && this.counterPickBiddingAllowed;
    },
    publisherSlotsAreFilled(): boolean {
      const userGames = this.requiredUserPublisher.games;
      const counterPickSlots = this.requiredLeagueYear.settings.counterPicks;
      const counterPicks = userGames.filter((x) => x.counterPick);
      return counterPicks.length >= counterPickSlots;
    },
    counterPickInvalid(): boolean {
      return !!this.bidCounterPick && !this.bidCounterPick.masterGame;
    },
    availableCounterPicks(): PublisherGameViewModel[] {
      return this.possibleCounterPicks.filter((x) => x.willRelease && !x.released);
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    formatCurrency,
    async getPossibleCounterPicks(): Promise<void> {
      this.clearData();
      this.isBusy = true;

      try {
        this.possibleCounterPicks = await leagueClient.possibleCounterPicks(this.requiredUserPublisher.publisherID);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to load counter picks.');
      } finally {
        this.isBusy = false;
      }
    },
    async bidGame(): Promise<void> {
      if (!this.bidCounterPick) {
        throw new Error('No game is selected to bid on.');
      }

      if (!this.bidCounterPick.masterGame) {
        throw new Error('Games not linked to a master game cannot be counter picked via bids, due to technical restrictions.');
      }

      const request: PickupBidRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        masterGameID: this.bidCounterPick.masterGame.masterGameID,
        bidAmount: this.bidAmount,
        counterPick: true,
        allowIneligibleSlot: false //This has no bearing on a counter pick request, but we must include it regardless. This could change if we add special slots for counter picks.
      };

      this.isBusy = true;
      try {
        this.bidResult = await leagueClient.makePickupBid(request);
        if (!this.bidResult.success) {
          return;
        }

        (this.$refs.bidCounterPickFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('Bid for ' + this.bidCounterPick.masterGame.gameName + ' for $' + this.bidAmount + ' was made.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to place bid.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.errorInfo = '';
      this.bidCounterPick = null;
      this.bidResult = null;
      this.bidAmount = 0;
      this.isBusy = false;
    }
  }
});
</script>

<style scoped>
.search-results {
  margin-top: 20px;
}
</style>
