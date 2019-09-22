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
            Year/Quarter: {{publisher.yearQuarter.year}}-Q{{publisher.yearQuarter.quarter}}
          </h4>
        </div>

        <div class="col-md-12 col-lg-6">
          <h4>Remaining Budget: {{publisher.budget | money}}</h4>
          <div v-if="userIsPublisher">
            <b-button variant="primary" v-b-modal="'royalePurchaseGameForm'">Purchase a Game</b-button>
            <royalePurchaseGameForm :yearQuarter="publisher.yearQuarter" :userRoyalePublisher="publisher" v-on:gamePurchased="gamePurchased"></royalePurchaseGameForm>
          </div>
        </div>
      </div>

      <h1>Games</h1>
      <b-table striped bordered small responsive :items="publisher.publisherGames" :fields="allFields" v-if="publisher.publisherGames.length !== 0">
        <template slot="masterGame" slot-scope="data">
          <masterGamePopover :masterGame="data.item.masterGame"> </masterGamePopover>
        </template>
        <template slot="releaseDate" slot-scope="data">
          {{getReleaseDate(data.item.masterGame)}}
        </template>
        <template slot="amountSpent" slot-scope="data">
          {{ data.item.amountSpent | money }}
        </template>
        <template slot="advertisingMoney" slot-scope="data">
          {{ data.item.advertisingMoney | money }}
          <b-button variant="info" class="set-advertising-button" size="sm" v-if="userIsPublisher">Set Budget</b-button>
        </template>
        <template slot="criticScore" slot-scope="data">
          <a v-if="data.item.openCriticID && data.item.criticScore" :href="openCriticLink(data.item)" target="_blank"><strong>OpenCritic <font-awesome-icon icon="external-link-alt" /></strong></a>
          <span v-else>--</span>
        </template>
        <template slot="fantasyPoints" slot-scope="data">
          {{ data.item.fantasyPoints | score }}
        </template>
        <template slot="timestamp" slot-scope="data">
          {{ data.item.timestamp | date }}
        </template>
        <template slot="sellGame" slot-scope="data">
          <b-button variant="danger" v-on:click="setGameToSell(data.item)">Sell Game</b-button>
        </template>
      </b-table>
      <div v-else class="alert alert-info">
        You have not bought any games yet!
      </div>
    </div>

    <b-modal id="sellRoyaleGameModal" ref="sellRoyaleGameModalRef" title="Sell Game" @ok="sellGame">
      <div v-if="gameToSell">
        <p>Are you sure you want to sell <strong>{{gameToSell.masterGame.gameName}}</strong>?</p>
        <p>You will get back half the money you bought it for, and any advertising money currently assigned to it.</p>
        <p>Money to recieve: <strong>{{gameToSell.amountSpent / 2 + gameToSell.advertisingMoney | money}}</strong></p>
      </div>
    </b-modal>
  </div>
</template>

<script>
  import Vue from "vue";
  import axios from "axios";
  import moment from "moment";

  import MasterGamePopover from "components/modules/masterGamePopover";
  import RoyalePurchaseGameForm from "components/modules/modals/royalePurchaseGameForm";

  export default {
    props: ['publisherid'],
    data() {
      return {
        errorInfo: "",
        publisher: null,
        gameToSell: null,
        gameFields: [
          { key: 'masterGame', label: 'Game', thClass:'bg-primary', stickyColumn: true },
          { key: 'releaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
          { key: 'amountSpent', label: 'Amount Spent', thClass: 'bg-primary' },
          { key: 'advertisingMoney', label: 'Advertising Money', thClass: 'bg-primary' },
          { key: 'criticScore', label: 'Critic Score', thClass: 'bg-primary' },
          { key: 'fantasyPoints', label: 'Fantasy Points', thClass: 'bg-primary' },
          { key: 'timestamp', label: 'Purchase Date', thClass: 'bg-primary' }
        ],
        userPublisherFields: [
          { key: 'sellGame', label: '', thClass: 'bg-primary' }
        ]
      }
    },
    components: {
      RoyalePurchaseGameForm,
      MasterGamePopover
    },
    computed: {
      userIsPublisher() {
        return (this.publisher.userID === this.$store.getters.userInfo.userID);
      },
      allFields() {
          let conditionalFields = [];
          if (this.userIsPublisher) {
              conditionalFields = conditionalFields.concat(this.userPublisherFields);
          }
          return this.gameFields.concat(conditionalFields);
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
        let message = purchaseInfo.gameName + " was purchased for " + this.$options.filters.money(purchaseInfo.purchaseCost);
        let toast = this.$toasted.show(message, {
          theme: "primary",
          position: "top-right",
          duration: 5000
        });
      },
      setGameToSell(publisherGame) {
        this.gameToSell = publisherGame;
        this.$refs.sellRoyaleGameModalRef.show();
      },
      sellGame(publisherGame) {
          var request = {
            publisherID: this.publisher.publisherID,
            masterGameID: this.gameToSell.masterGame.masterGameID
          };

          axios
            .post('/api/royale/SellGame', request)
            .then(response => {
                this.fetchPublisher();
                let message = this.gameToSell.masterGame.gameName + " was sold for " + this.$options.filters.money(this.gameToSell.amountSpent / 2);
                let toast = this.$toasted.show(message, {
                  theme: "primary",
                  position: "top-right",
                  duration: 5000
                });
              })
            .catch(response => {

            });
        },
      getReleaseDate(game) {
        if (game.releaseDate) {
          return moment(game.releaseDate).format('YYYY-MM-DD');
        }
        return game.estimatedReleaseDate + ' (Estimated)'
      },
      openCriticLink(game) {
        return "https://opencritic.com/game/" + game.openCriticID + "/a";
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
  }
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
</style>
