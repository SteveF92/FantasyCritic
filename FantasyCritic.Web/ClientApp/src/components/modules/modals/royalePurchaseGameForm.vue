<template>
  <b-modal id="royalePurchaseGameForm" ref="royalePurchaseGameFormRef" size="lg" title="Purchase Game" hide-footer @hidden="clearData">
    <p class="text-black">
      You can purchase up to 25 games, provided you have the money.
      <br />
      You currently have <strong>{{ownedGamesCount}}</strong> game(s).
    </p>

    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="searchGame">
      <div class="form-group">
        <label for="PurchaseGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input v-model="searchGameName" id="searchGameName" name="searchGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" v-on:click="searchGame">Search Game</b-button>
          </span>
        </div>

        <h3 class="text-black" v-if="searchedTop">Top games</h3>
        <possibleRoyaleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="purchaseRoyaleGame" :possibleGames="possibleMasterGames" v-on:input="newGameSelected"></possibleRoyaleMasterGamesTable>

        <div v-show="searched && !purchaseRoyaleGame && possibleMasterGames.length === 0" class="alert" v-bind:class="{ 'alert-info': possibleMasterGames.length > 0, 'alert-warning': possibleMasterGames.length === 0 }">
          <div class="row">
            <span class="col-12 col-md-7" >No games were found.</span>
          </div>
        </div>

        <label v-if="purchaseRoyaleGame" for="purchaseRoyaleGame" class="control-label">Selected Game: {{purchaseRoyaleGame.masterGame.gameName}}</label>
      </div>
    </form>

    <div class="form-horizontal">
      <div>
        <b-button variant="primary" class="add-game-button" v-on:click="addGame" v-if="formIsValid" :disabled="isBusy">
          Purchase Game for {{purchaseRoyaleGame.cost | money}}
        </b-button>
      </div>
      <div v-if="purchaseResult && !purchaseResult.success" class="alert purchase-error" v-bind:class="{ 'alert-danger': !purchaseResult.overridable, 'alert-warning': purchaseResult.overridable }">
        <h3 class="alert-heading" v-if="purchaseResult.overridable">Warning!</h3>
        <h3 class="alert-heading" v-if="!purchaseResult.overridable">Error!</h3>
        <ul>
          <li v-for="error in purchaseResult.errors">{{error}}</li>
        </ul>
      </div>
    </div>
  </b-modal>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    import PossibleRoyaleMasterGamesTable from "@/components/modules/possibleRoyaleMasterGamesTable";

    export default {
      data() {
          return {
            searchGameName: null,
            purchaseRoyaleGame: null,
            purchaseResult: null,
            possibleMasterGames: [],
            searched: false,
            searchedTop: false,
            isBusy: false
          }
      },
      components: {
          PossibleRoyaleMasterGamesTable
      },
      computed: {
        formIsValid() {
          return this.purchaseRoyaleGame;
        },
        ownedGamesCount() {
          return this.userRoyalePublisher.publisherGames.length;
        }
      },
      props: ['yearQuarter', 'userRoyalePublisher'],
      methods: {
        searchGame() {
          this.searched = false;
          this.searchedTop = false;
          this.purchaseResult = null;
          this.possibleMasterGames = [];

          let apiString = '';
          if (this.searchGameName) {
            apiString = '/api/royale/PossibleMasterGames?gameName=' + this.searchGameName + '&publisherID=' + this.userRoyalePublisher.publisherID;
          }
          else {
            apiString = '/api/royale/PossibleMasterGames?publisherID=' + this.userRoyalePublisher.publisherID;
          }
          axios
              .get(apiString)
              .then(response => {
                this.possibleMasterGames = response.data;
                this.searched = true;
                this.showingUnlistedField = false;
                this.purchaseRoyaleGame = null;
                if (!this.searchGameName) {
                  this.searchedTop = true;
                }
              })
              .catch(response => {

              });
        },
        addGame() {
          this.isBusy = true;

          var masterGameID = null;
          if (this.purchaseRoyaleGame !== null) {
              masterGameID = this.purchaseRoyaleGame.masterGame.masterGameID;
          }

          var request = {
              publisherID: this.userRoyalePublisher.publisherID,
              masterGameID: masterGameID,
          };

          axios
            .post('/api/royale/PurchaseGame', request)
            .then(response => {
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
              .catch(response => {

              });
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
    },
    mounted() {
      this.searchGame();
    }
  }
</script>
<style scoped>
.add-game-button{
  width: 100%;
}
.purchase-error{
  margin-top: 10px;
}
.game-search-input{
  margin-bottom: 15px;
}
.text-black {
  color: black;
}
</style>
