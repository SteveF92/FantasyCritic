<template>
  <b-modal id="royalePurchaseGameForm" ref="royalePurchaseGameFormRef" size="lg" title="Purchase Game" no-footer @hidden="clearData">
    <p class="text-black">
      You can purchase up to 15 games, provided you have the money.
      <br />
      You currently have
      <strong>{{ ownedGamesCount }}</strong>
      game(s).
    </p>

    <form method="post" class="form-horizontal" role="form" @submit.prevent="searchGame">
      <div class="form-group">
        <label for="royale-release-filter" class="control-label">Games shown</label>
        <div class="d-flex mb-3 royale-games-shown-row">
          <b-form-select id="royale-release-filter" v-model="releaseFilter" :options="releaseFilterOptions" class="flex-fill mr-2"></b-form-select>
          <b-button type="button" variant="secondary" @click="listGames">List Games</b-button>
        </div>

        <label for="PurchaseGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input id="searchGameName" v-model="searchGameName" name="searchGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" :disabled="!searchGameName" @click="searchGame">Search Game</b-button>
          </span>
        </div>

        <h3 v-if="searchedTop" class="text-black">Top games</h3>
        <possibleRoyaleMasterGamesTable
          v-if="possibleMasterGames.length > 0"
          v-model="purchaseRoyaleGame"
          :possible-games="possibleMasterGames"
          @update:model-value="newGameSelected"></possibleRoyaleMasterGamesTable>

        <div
          v-show="searched && !purchaseRoyaleGame && possibleMasterGames.length === 0"
          class="alert"
          :class="{ 'alert-info': possibleMasterGames.length > 0, 'alert-warning': possibleMasterGames.length === 0 }">
          <div class="row">
            <span class="col-12 col-md-7">No games were found.</span>
          </div>
        </div>

        <label v-if="purchaseRoyaleGame" for="purchaseRoyaleGame" class="control-label">Selected Game: {{ purchaseRoyaleGame.masterGame.gameName }}</label>
      </div>
    </form>

    <div class="form-horizontal">
      <div>
        <b-button v-if="purchaseRoyaleGame && formIsValid" variant="primary" class="full-width-button" :disabled="isBusy" @click="addGame">
          Purchase Game for {{ formatCurrency(purchaseRoyaleGame.cost) }}
        </b-button>
      </div>
      <div v-if="purchaseResult && !purchaseResult.success" class="alert purchase-error" :class="{ 'alert-danger': !purchaseResult.overridable, 'alert-warning': purchaseResult.overridable }">
        <h3 v-if="purchaseResult.overridable" class="alert-heading">Warning!</h3>
        <h3 v-if="!purchaseResult.overridable" class="alert-heading">Error!</h3>
        <ul>
          <li v-for="error in purchaseResult.errors" :key="error">{{ error }}</li>
        </ul>
      </div>
    </div>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import type { BModal } from 'bootstrap-vue-next';

import {
  royaleClient,
  RoyalePossibleMasterGamesReleaseFilter,
  type PlayerClaimResultViewModel,
  type PossibleRoyaleMasterGameViewModel,
  type PurchaseRoyaleGameRequest,
  type RoyalePublisherViewModel,
  type RoyaleYearQuarterViewModel
} from '@/api/clients';
import type { RoyaleGamePurchase } from '@/types/vueTypes';
import { formatCurrency } from '@/utilities/formatters';
import PossibleRoyaleMasterGamesTable from '@/components/possibleRoyaleMasterGamesTable.vue';

interface RoyalePurchaseGameFormData {
  searchGameName: string | null;
  purchaseRoyaleGame: PossibleRoyaleMasterGameViewModel | null;
  purchaseResult: PlayerClaimResultViewModel | null;
  possibleMasterGames: PossibleRoyaleMasterGameViewModel[];
  searched: boolean;
  searchedTop: boolean;
  isBusy: boolean;
  releaseFilter: RoyalePossibleMasterGamesReleaseFilter;
}

export default defineComponent({
  setup() {
    const releaseFilterOptions: { value: RoyalePossibleMasterGamesReleaseFilter; text: string }[] = [
      { value: RoyalePossibleMasterGamesReleaseFilter.All, text: 'All Possible Releases' },
      { value: RoyalePossibleMasterGamesReleaseFilter.ExpectedToReleaseInQuarter, text: 'Games Expected to Release in Quarter' },
      { value: RoyalePossibleMasterGamesReleaseFilter.ConfirmedReleaseInQuarter, text: 'Games with Confirmed Release in Quarter' }
    ];

    return { releaseFilterOptions };
  },
  components: { PossibleRoyaleMasterGamesTable },
  props: {
    yearQuarter: { type: Object as PropType<RoyaleYearQuarterViewModel>, required: true },
    userRoyalePublisher: { type: Object as PropType<RoyalePublisherViewModel>, required: true }
  },
  emits: {
    gamePurchased: (_purchaseInfo: RoyaleGamePurchase) => true
  },
  data(): RoyalePurchaseGameFormData {
    return {
      searchGameName: null,
      purchaseRoyaleGame: null,
      purchaseResult: null,
      possibleMasterGames: [],
      searched: false,
      searchedTop: false,
      isBusy: false,
      releaseFilter: RoyalePossibleMasterGamesReleaseFilter.All
    };
  },
  computed: {
    formIsValid(): boolean {
      return this.purchaseRoyaleGame !== null && this.purchaseRoyaleGame.isAvailable;
    },
    ownedGamesCount(): number {
      return this.userRoyalePublisher.publisherGames.length;
    }
  },
  async created() {
    await this.searchGame();
  },
  methods: {
    formatCurrency,
    async listGames(): Promise<void> {
      this.searchGameName = null;
      await this.searchGame();
    },
    async searchGame(): Promise<void> {
      this.searched = false;
      this.searchedTop = false;
      this.purchaseResult = null;
      this.possibleMasterGames = [];

      this.possibleMasterGames = await royaleClient.possibleMasterGames(this.searchGameName || null, this.userRoyalePublisher.publisherID, this.releaseFilter);
      this.searched = true;
      this.purchaseRoyaleGame = null;
      if (!this.searchGameName) {
        this.searchedTop = true;
      }
    },
    async addGame(): Promise<void> {
      const purchaseRoyaleGame = this.purchaseRoyaleGame;
      if (!purchaseRoyaleGame) {
        return;
      }

      this.isBusy = true;

      const request: PurchaseRoyaleGameRequest = {
        publisherID: this.userRoyalePublisher.publisherID,
        masterGameID: purchaseRoyaleGame.masterGame.masterGameID,
        expectedCost: purchaseRoyaleGame.cost
      };

      this.purchaseResult = await royaleClient.purchaseGame(request);
      if (!this.purchaseResult.success) {
        this.isBusy = false;
        return;
      }

      const purchaseInfo: RoyaleGamePurchase = { gameName: purchaseRoyaleGame.masterGame.gameName, purchaseCost: purchaseRoyaleGame.cost };
      this.$emit('gamePurchased', purchaseInfo);
      await this.clearData();
      (this.$refs.royalePurchaseGameFormRef as InstanceType<typeof BModal> | undefined)?.hide();
    },
    async clearData(): Promise<void> {
      this.isBusy = false;
      this.searchGameName = null;
      this.purchaseRoyaleGame = null;
      this.purchaseResult = null;
      this.possibleMasterGames = [];
      this.searched = false;
      await this.searchGame();
    },
    newGameSelected(): void {
      this.purchaseResult = null;
    }
  }
});
</script>
<style scoped>
.purchase-error {
  margin-top: 10px;
}
.royale-games-shown-row .btn {
  flex-shrink: 0;
  white-space: nowrap;
}
.game-search-input {
  margin-bottom: 15px;
}
.text-black {
  color: black;
}
</style>
