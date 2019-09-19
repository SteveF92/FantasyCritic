<template>
  <b-modal id="royalePurchaseGameForm" ref="royalePurchaseGameFormRef" size="lg" title="Purchase Game" hide-footer @hidden="clearData">
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="searchGame">
      <div class="form-group">
        <label for="PurchaseGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input v-model="searchGameName" id="searchGameName" name="searchGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" v-on:click="searchGame">Search Game</b-button>
          </span>
        </div>

        <possibleRoyaleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="purchaseMasterGame" :possibleGames="possibleMasterGames" v-on:input="newGameSelected"></possibleRoyaleMasterGamesTable>

        <!--<div v-show="searched && !purchaseMasterGame" class="alert" v-bind:class="{ 'alert-info': possibleMasterGames.length > 0, 'alert-warning': possibleMasterGames.length === 0 }">
          <div class="row">
            <span class="col-12 col-md-7" v-show="possibleMasterGames.length > 0">Don't see the game you are looking for?</span>
            <span class="col-12 col-md-7" v-show="possibleMasterGames.length === 0">No games were found.</span>
            <b-button variant="primary" v-on:click="showUnlistedField" size="sm" class="col-12 col-md-5">Select unlisted game</b-button>
          </div>

          <div v-if="showingUnlistedField">
            <label for="purchaseUnlistedGame" class="control-label">Custom Game Name</label>
            <div class="input-group game-search-input">
              <input v-model="purchaseUnlistedGame" id="purchaseUnlistedGame" name="purchaseUnlistedGame" type="text" class="form-control input" />
            </div>
            <div>Enter the full name of the game you want.</div>
            <div v-show="!isManager">Your league manager can link this custom game with a "master game" later.</div>
            <div v-show="isManager">You as league manager can link this custom game with a "master game" later.</div>
          </div>
        </div>

        <label v-if="purchaseMasterGame" for="purchaseMasterGame" class="control-label">Selected Game: {{purchaseMasterGame.gameName}}</label>-->
      </div>
    </form>
    <!--<form method="post" class="form-horizontal" role="form" v-on:submit.prevent="addGame">
      <div>
        <input type="submit" class="btn btn-primary add-game-button" value="Draft Game" v-if="formIsValid" :disabled="isBusy" />
      </div>
      <div v-if="purchaseResult && !purchaseResult.success" class="alert purchase-error" v-bind:class="{ 'alert-danger': !purchaseResult.overridable, 'alert-warning': purchaseResult.overridable }">
        <h3 class="alert-heading" v-if="purchaseResult.overridable">Warning!</h3>
        <h3 class="alert-heading" v-if="!purchaseResult.overridable">Error!</h3>
        <ul>
          <li v-for="error in purchaseResult.errors">{{error}}</li>
        </ul>

        <div class="form-check" v-if="purchaseResult.overridable">
          <span>
            <label v-show="!isManager" class="form-check-label">Your league manager can override these warnings.</label>
            <label v-show="isManager" class="form-check-label">
              <span>You as league manager can override these warnings.</span>
              <br />
              <span>Use the "Select Next Game" button under "Manager Actions".</span>
            </label>
          </span>
        </div>
      </div>
    </form>-->
  </b-modal>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    import PossibleRoyaleMasterGamesTable from "components/modules/possibleRoyaleMasterGamesTable";
    export default {
        data() {
            return {
              searchGameName: null,
              purchaseMasterGame: null,
              purchaseResult: null,
              possibleMasterGames: [],
              searched: false,
              isBusy: false
            }
        },
        components: {
            PossibleRoyaleMasterGamesTable
        },
        computed: {
          formIsValid() {
            return this.purchaseMasterGame;
          }
        },
        props: ['yearQuarter'],
        methods: {
          searchGame() {
            this.purchaseResult = null;
            this.possibleMasterGames = [];

            let apiString = '';
            if (this.searchGameName) {
              apiString = '/api/royale/PossibleMasterGames?gameName=' + this.searchGameName + '&year=' + this.yearQuarter.year + '&quarter=' + this.yearQuarter.quarter;
            }
            else {
              apiString = '/api/royale/PossibleMasterGames?year=' + this.yearQuarter.year + '&quarter=' + this.yearQuarter.quarter;
            }
            axios
                .get(apiString)
                .then(response => {
                  this.possibleMasterGames = response.data;
                  this.searched = true;
                  this.showingUnlistedField = false;
                  this.purchaseMasterGame = null;
                })
                .catch(response => {

                });
          },
          addGame() {
            this.isBusy = true;
            var gameName = "";
            if (this.purchaseMasterGame !== null) {
              gameName = this.purchaseMasterGame.gameName;
            } else if (this.purchaseUnlistedGame !== null) {
              gameName = this.purchaseUnlistedGame;
            }

            var masterGameID = null;
            if (this.purchaseMasterGame !== null) {
                masterGameID = this.purchaseMasterGame.masterGameID;
            }

            var request = {
                publisherID: this.userRoyalePublisher.publisherID,
                gameName: gameName,
                counterPick: this.purchaseCounterPick,
                masterGameID: masterGameID,
                managerOverride: this.purchaseOverride
            };

            axios
              .post('/api/league/PurchaseGame', request)
              .then(response => {
                  this.purchaseResult = response.data;
                  if (!this.purchaseResult.success) {
                    this.isBusy = false;
                    return;
                  }
                  this.$refs.royalePurchaseGameFormRef.hide();
                  var purchaseInfo = {
                    gameName
                  };
                  this.$emit('gameDrafted', purchaseInfo);
                  this.clearData();
                })
                .catch(response => {
                      
                });
          },
          clearData() {
            this.isBusy = false;
            this.searchGameName = null;
            this.purchaseMasterGame = null;
            this.purchaseResult = null;
            this.possibleMasterGames = [];
            this.searched = false;
          },
          newGameSelected() {
            this.purchaseResult = null;
          }
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
.override-checkbox {
  margin-left: 10px;
  margin-top: 8px;
}
</style>
