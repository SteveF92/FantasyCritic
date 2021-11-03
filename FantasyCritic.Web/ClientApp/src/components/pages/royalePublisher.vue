<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="publisher">
      <div class="publisher-name">
        <h1>{{publisher.publisherName}}</h1>
      </div>
      <div class="row">
        <div class="col-md-12 col-lg-6">
          <h4>Player Name: {{publisher.playerName}}</h4>
          <h4>
            Year/Quarter:
            <router-link :to="{ name: 'criticsRoyale', params: {year: publisher.yearQuarter.year, quarter: publisher.yearQuarter.quarter }}">
              {{publisher.yearQuarter.year}}-Q{{publisher.yearQuarter.quarter}}
            </router-link>
          </h4>
          <h2>
            Total Points: {{publisher.totalFantasyPoints}}
          </h2>
        </div>

        <div class="col-md-12 col-lg-6">
          <h4>Remaining Budget: {{publisher.budget | money}}</h4>
          <div v-if="userIsPublisher" class="user-actions">
            <b-button variant="primary" v-b-modal="'royalePurchaseGameForm'" class="action-button">Purchase a Game</b-button>
            <b-button variant="secondary" v-b-modal="'royaleChangePublisherNameForm'" class="action-button">Change Publisher Name</b-button>
            <royalePurchaseGameForm :yearQuarter="publisher.yearQuarter" :userRoyalePublisher="publisher" v-on:gamePurchased="gamePurchased"></royalePurchaseGameForm>
            <royaleChangePublisherNameForm :userRoyalePublisher="publisher" v-on:publisherNameChanged="publisherNameChanged"></royaleChangePublisherNameForm>
          </div>
          <ul class="won-quarters-list">
            <li v-for="quarter in publisher.quartersWon">
              <span class="badge badge-pill badge-primary">{{quarter.year}}-Q{{quarter.quarter}} Winner</span>
            </li>
          </ul>
        </div>
      </div>

      <hr />
      <div class="alert alert-danger" v-if="errorInfo">{{errorInfo}}</div>

      <h1>Games</h1>
      <b-table striped bordered small responsive :items="publisher.publisherGames" :fields="allFields" v-if="publisher.publisherGames.length !== 0" :tbody-tr-class="publisherGameRowClass">
        <template v-slot:cell(masterGame)="data">
          <span class="master-game-popover">
            <masterGamePopover :masterGame="data.item.masterGame" :currentlyIneligible="data.item.currentlyIneligible"> </masterGamePopover>
          </span>
          
          <span v-if="data.item.currentlyIneligible" class="game-ineligible">
            Ineligible
            <font-awesome-icon color="white" size="lg" icon="info-circle" v-b-popover.hover="inEligibleText" />
          </span>
        </template>
        <template v-slot:cell(masterGame.maximumReleaseDate)="data">
          {{getReleaseDate(data.item.masterGame)}}
        </template>
        <template v-slot:cell(amountSpent)="data">
          {{ data.item.amountSpent | money }}
        </template>
        <template v-slot:cell(advertisingMoney)="data">
          {{ data.item.advertisingMoney | money }}
          <b-button variant="info" size="sm" v-if="userIsPublisher && !data.item.locked" v-on:click="setGameToSetBudget(data.item)">Set Budget</b-button>
        </template>
        <template v-slot:cell(criticScore)="data">
          {{ data.item.masterGame.criticScore | score(2) }}
        </template>
        <template v-slot:cell(fantasyPoints)="data">
          {{ data.item.fantasyPoints | score(2) }}
        </template>
        <template v-slot:cell(timestamp)="data">
          {{ data.item.timestamp | date }}
        </template>
        <template v-slot:cell(sellGame)="data">
          <b-button block variant="danger" v-if="!data.item.locked" v-on:click="setGameToSell(data.item)">Sell</b-button>
        </template>
      </b-table>
      <div v-else class="alert alert-info">
        <template v-if="userIsPublisher">
          You have not bought any games yet!
        </template>
        <template v-else>
          This publisher has not bought any games yet.
        </template>
      </div>
    </div>

    <b-modal id="sellRoyaleGameModal" ref="sellRoyaleGameModalRef" title="Sell Game" @ok="sellGame">
      <div v-if="gameToModify">
        <p>Are you sure you want to sell <strong>{{gameToModify.masterGame.gameName}}</strong>?</p>
        <p>You will get back half the money you bought it for, and any advertising money currently assigned to it.</p>
        <p>Money to recieve: <strong>{{gameToModify.amountSpent / 2 + gameToModify.advertisingMoney | money}}</strong></p>
      </div>
    </b-modal>

    <b-modal id="setAdvertisingMoneyModal" ref="setAdvertisingMoneyModalRef" title="Set Advertising Budget" @ok="setBudget">
      <div v-if="gameToModify">
        <p>How much money do you want to allocate to <strong>{{gameToModify.masterGame.gameName}}</strong>?</p>
        <p>Each dollar allocated will increase your fantasy points received by 5%</p>
        <p>You can spend up to $10 for a bonus of 50%.</p>
        <p>You can adjust this up until the game (or reviews) come out.</p>
        <div class="form-group row">
          <label for="advertisingBudgetToSet" class="col-sm-2 col-form-label">Budget</label>
          <div class="col-sm-10">
            <input class="form-control" v-model="advertisingBudgetToSet">
          </div>
        </div>
      </div>
    </b-modal>
  </div>
</template>

<script>
import Vue from 'vue';
import axios from 'axios';
import moment from 'moment';

import MasterGamePopover from '@/components/modules/masterGamePopover';
import RoyalePurchaseGameForm from '@/components/modules/modals/royalePurchaseGameForm';
import RoyaleChangePublisherNameForm from '@/components/modules/modals/royaleChangePublisherNameForm';

export default {
  props: ['publisherid'],
  data() {
    return {
      errorInfo: '',
      publisher: null,
      gameToModify: null,
      advertisingBudgetToSet: 0,
      gameFields: [
        { key: 'masterGame', label: 'Game', thClass: 'bg-primary', sortable: true },
        { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        { key: 'amountSpent', label: 'Amount Spent', thClass: 'bg-primary', sortable: true },
        { key: 'advertisingMoney', label: 'Advertising Budget', thClass: 'bg-primary', sortable: true },
        { key: 'criticScore', label: 'Critic Score', thClass: 'bg-primary', sortable: true },
        { key: 'fantasyPoints', label: 'Fantasy Points', thClass: 'bg-primary', sortable: true },
        { key: 'timestamp', label: 'Purchase Date', thClass: 'bg-primary', sortable: true }
      ],
      userPublisherFields: [
        { key: 'sellGame', label: '', thClass: 'bg-primary', label: 'Sell' }
      ]
    };
  },
  components: {
    RoyaleChangePublisherNameForm,
    RoyalePurchaseGameForm,
    MasterGamePopover
  },
  computed: {
    isAuth() {
      return this.$store.getters.tokenIsCurrent();
    },
    userIsPublisher() {
      return this.isAuth && (this.publisher.userID === this.$store.getters.userInfo.userID);
    },
    allFields() {
      let conditionalFields = [];
      if (this.userIsPublisher) {
        conditionalFields = conditionalFields.concat(this.userPublisherFields);
      }
      return this.gameFields.concat(conditionalFields);
    },
    inEligibleText() {
      return {
        html: true,
        title: () => {
          return "What does this mean?";
        },
        content: () => {
          return 'This game\'s status has changed since you purchased it, and it is currently ineligible based on the royale rules. Any points the game receives will NOT count. <br/> <br/>' +
            'You can drop the game for a full refund.';
        }
      }
    }
  },
  methods: {
    fetchPublisher() {
      axios
        .get('/api/Royale/GetRoyalePublisher/' + this.publisherid)
        .then(response => {
          this.publisher = response.data;
        })
        .catch(returnedError => (this.error = returnedError));
    },
    gamePurchased(purchaseInfo) {
      this.fetchPublisher();
      let message = purchaseInfo.gameName + ' was purchased for ' + this.$options.filters.money(purchaseInfo.purchaseCost);
      let toast = this.$toasted.show(message, {
        theme: 'primary',
        position: 'top-right',
        duration: 5000
      });
    },
    publisherNameChanged() {
      this.fetchPublisher();
      let message = 'Publisher name changed.';
      let toast = this.$toasted.show(message, {
        theme: 'primary',
        position: 'top-right',
        duration: 5000
      });
    },
    setGameToSell(publisherGame) {
      this.gameToModify = publisherGame;
      this.$refs.sellRoyaleGameModalRef.show();
    },
    sellGame() {
      var request = {
        publisherID: this.publisher.publisherID,
        masterGameID: this.gameToModify.masterGame.masterGameID
      };

      axios
        .post('/api/royale/SellGame', request)
        .then(response => {
          this.fetchPublisher();
          let message = this.gameToModify.masterGame.gameName + ' was sold for ' + this.$options.filters.money(this.gameToModify.amountSpent / 2);
          let toast = this.$toasted.show(message, {
            theme: 'primary',
            position: 'top-right',
            duration: 5000
          });
        })
        .catch(response => {
          this.errorInfo = 'You can\'t sell that game. ' + response.response.data;
        });
    },
    setGameToSetBudget(publisherGame) {
      this.gameToModify = publisherGame;
      this.advertisingBudgetToSet = publisherGame.advertisingMoney;
      this.$refs.setAdvertisingMoneyModalRef.show();
    },
    setBudget() {
      var request = {
        publisherID: this.publisher.publisherID,
        masterGameID: this.gameToModify.masterGame.masterGameID,
        advertisingMoney: this.advertisingBudgetToSet
      };

      axios
        .post('/api/royale/SetAdvertisingMoney', request)
        .then(response => {
          this.fetchPublisher();
          this.advertisingBudgetToSet = 0;
        })
        .catch(response => {
          this.advertisingBudgetToSet = 0;
          this.errorInfo = 'You can\'t set that budget. ' + response.response.data;
        });
    },
    getReleaseDate(game) {
      if (game.releaseDate) {
        return moment(game.releaseDate).format('YYYY-MM-DD');
      }
      return game.estimatedReleaseDate + ' (Estimated)';
    },
    openCriticLink(game) {
      return 'https://opencritic.com/game/' + game.openCriticID + '/a';
    },
    publisherGameRowClass(item, type) {
      if (!item || type !== 'row') {
        return;
      }
      if (item.currentlyIneligible) {
        return 'table-warning';
      }
    }
  },
  mounted() {
    this.fetchPublisher();
  },
  watch: {
    '$route'(to, from) {
      this.fetchPublisher();
    }
  }
};
</script>
<style scoped>
  .publisher-name {
    display: block;
    max-width: 100%;
    word-wrap: break-word;
  }
  .top-area{
    margin-top: 10px;
    margin-bottom: 20px;
  }

  .main-buttons {
    display: flex;
    flex-direction: row;
    justify-content: space-around;
  }

  .main-button{
    margin-top: 5px;
    min-width: 200px;
  }

  .user-actions {
    display: flex;
  }

  .user-actions .action-button {
    margin: 5px;
  }

  .won-quarters-list{
      list-style: none;
  }

  .game-ineligible {
    float: right;
    color: white;
    font-style: italic;
    padding-left: 5px;
  }

  .master-game-popover {
    float: left;
  }
</style>
