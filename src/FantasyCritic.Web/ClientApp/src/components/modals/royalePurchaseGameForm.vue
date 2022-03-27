<template>
  <b-modal id="royalePurchaseGameForm" ref="royalePurchaseGameFormRef" size="lg" title="Purchase Game" hide-footer @hidden="clearData">
    <p class="text-black">
      You can purchase up to 25 games, provided you have the money.
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
            <b-button variant="info" @click="searchGame">Search Game</b-button>
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
        <b-button v-if="formIsValid" variant="primary" class="add-game-button" :disabled="isBusy" @click="addGame">Purchase Game for {{ purchaseRoyaleGame.cost | money }}</b-button>
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
import PossibleRoyaleMasterGamesTable from '@/components/possibleRoyaleMasterGamesTable';

export default {
  components: {
    PossibleRoyaleMasterGamesTable
  },
  props: {
    yearQuarter: { type: Object, required: true },
    userRoyalePublisher: { type: Object, required: true }
  },
  data() {
    return {
      searchGameName: null,
      purchaseRoyaleGame: null,
      purchaseResult: null,
      possibleMasterGames: [],
      searched: false,
      searchedTop: false,
      isBusy: false
    };
  },
  computed: {
    formIsValid() {
      return this.purchaseRoyaleGame;
    },
    ownedGamesCount() {
      return this.userRoyalePublisher.publisherGames.length;
    }
  },
  mounted() {
    this.searchGame();
  },
  methods: {
    searchGame() {
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
      axios
        .get(apiString)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.searched = true;
          this.showingUnlistedField = false;
          this.purchaseRoyaleGame = null;
          if (!this.searchGameName) {
            this.searchedTop = true;
          }
        })
        .catch(() => {});
    },
    addGame() {
      this.isBusy = true;

      var masterGameID = null;
      if (this.purchaseRoyaleGame !== null) {
        masterGameID = this.purchaseRoyaleGame.masterGame.masterGameID;
      }

      var request = {
        publisherID: this.userRoyalePublisher.publisherID,
        masterGameID: masterGameID
      };

      axios
        .post('/api/royale/PurchaseGame', request)
        .then((response) => {
          this.purchaseResult = response.data;
          if (!this.purchaseResult.success) {
            this.isBusy = false;
            return;
          }

          let gameName = this.purchaseRoyaleGame.masterGame.gameName;
          let purchaseCost = this.purchaseRoyaleGame.cost;
          var purchaseInfo = {
            gameName,
            purchaseCost
          };
          this.$emit('gamePurchased', purchaseInfo);
          this.clearData();
          this.$refs.royalePurchaseGameFormRef.hide();
        })
        .catch(() => {});
    },
    clearData() {
      this.isBusy = false;
      this.searchGameName = null;
      this.purchaseRoyaleGame = null;
      this.purchaseResult = null;
      this.possibleMasterGames = [];
      this.searched = false;
      this.searchGame();
    },
    newGameSelected() {
      this.purchaseResult = null;
    }
  }
};
</script>
<style scoped>
.add-game-button {
  width: 100%;
}
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
