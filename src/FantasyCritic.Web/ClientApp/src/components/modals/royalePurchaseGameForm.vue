<template>
  <b-modal id="royalePurchaseGameForm" ref="royalePurchaseGameFormRef" size="lg" title="Purchase Game" hide-footer @hidden="clearData">
    <p class="text-black">
      You can purchase up to 15 games, provided you have the money.
      <br />
      You currently have
      <strong>{{ ownedGamesCount }}</strong>
      game(s).
    </p>

    <form method="post" class="form-horizontal" role="form" @submit.prevent="searchGame">
      <div class="form-group">
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
          @input="newGameSelected"></possibleRoyaleMasterGamesTable>

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
        <b-button v-if="formIsValid" variant="primary" class="full-width-button" :disabled="isBusy" @click="addGame">Purchase Game for {{ purchaseRoyaleGame.cost | money }}</b-button>
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

<script>
import axios from 'axios';
import PossibleRoyaleMasterGamesTable from '@/components/possibleRoyaleMasterGamesTable.vue';

export default {
  components: { PossibleRoyaleMasterGamesTable },
  props: { yearQuarter: { type: Object, required: true }, userRoyalePublisher: { type: Object, required: true } },
  data() {
    return { searchGameName: null, purchaseRoyaleGame: null, purchaseResult: null, possibleMasterGames: [], searched: false, searchedTop: false, isBusy: false };
  },
  computed: {
    formIsValid() {
      return this.purchaseRoyaleGame;
    },
    ownedGamesCount() {
      return this.userRoyalePublisher.publisherGames.length;
    }
  },
  async created() {
    await this.searchGame();
  },
  methods: {
    async searchGame() {
      this.searched = false;
      this.searchedTop = false;
      this.purchaseResult = null;
      this.possibleMasterGames = [];

      let apiString = '';
      if (this.searchGameName) {
        apiString = '/api/royale/PossibleMasterGames?gameName=' + this.searchGameName + '&publisherID=' + this.userRoyalePublisher.publisherID;
      } else {
        apiString = '/api/royale/PossibleMasterGames?publisherID=' + this.userRoyalePublisher.publisherID;
      }

      const response = await axios.get(apiString);
      this.possibleMasterGames = response.data;
      this.searched = true;
      this.showingUnlistedField = false;
      this.purchaseRoyaleGame = null;
      if (!this.searchGameName) {
        this.searchedTop = true;
      }
    },
    async addGame() {
      this.isBusy = true;

      let masterGameID = null;
      if (this.purchaseRoyaleGame !== null) {
        masterGameID = this.purchaseRoyaleGame.masterGame.masterGameID;
      }

      let request = { publisherID: this.userRoyalePublisher.publisherID, masterGameID: masterGameID };

      const response = await axios.post('/api/royale/PurchaseGame', request);
      this.purchaseResult = response.data;
      if (!this.purchaseResult.success) {
        this.isBusy = false;
        return;
      }

      let gameName = this.purchaseRoyaleGame.masterGame.gameName;
      let purchaseCost = this.purchaseRoyaleGame.cost;
      const purchaseInfo = { gameName, purchaseCost };
      this.$emit('gamePurchased', purchaseInfo);
      await this.clearData();
      this.$refs.royalePurchaseGameFormRef.hide();
    },
    async clearData() {
      this.isBusy = false;
      this.searchGameName = null;
      this.purchaseRoyaleGame = null;
      this.purchaseResult = null;
      this.possibleMasterGames = [];
      this.searched = false;
      await this.searchGame();
    },
    newGameSelected() {
      this.purchaseResult = null;
    }
  }
};
</script>
<style scoped>
.purchase-error {
  margin-top: 10px;
}
.game-search-input {
  margin-bottom: 15px;
}
.text-black {
  color: black;
}
</style>
