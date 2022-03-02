<template>
  <div>
    <form class="form-horizontal" hide-footer>
      <b-modal id="proposeTradeForm" size="lg" ref="proposeTradeFormRef" title="Propose Trade" @hidden="clearData">
        <div class="form-group">
          <label for="counterParty" class="control-label">Publisher to trade with</label>
          <b-form-select v-model="counterParty">
            <option v-for="publisher in otherPublishers" v-bind:value="publisher">
              {{ publisher.publisherName }}
            </option>
          </b-form-select>
        </div>

        <div v-if="counterParty">
          <div class="row">
            <div class="col-6">
              <h4 class="text-black">Offer</h4>

              <div>
                <div v-for="(item, index) in proposerPublisherGames" :key="item.publisherGameID">
                  <div class="trade-game-row">
                    <label>{{index + 1}}</label>
                    <b-form-select v-model="item.game">
                      <option v-for="publisherGame in publisher.games" v-bind:value="publisherGame">
                        {{ getGameOptionName(publisherGame) }}
                      </option>
                    </b-form-select>

                    <div class="close-button fake-link" v-on:click="removeProposerGame(item.id)">
                      <font-awesome-icon icon="times" size="lg" :style="{ color: '#414141' }" />
                    </div>
                  </div>

                </div>
                <b-button variant="secondary" class="add-game-button" v-on:click="addGame(proposerPublisherGames)">Add Game</b-button>
              </div>
            </div>
            <div class="col-6">
              <h4 class="text-black">Receive</h4>

              <div>
                <div v-for="(item, index) in counterPartyPublisherGames" :key="item.publisherGameID">
                  <div class="trade-game-row">
                    <label>{{index + 1}}</label>
                    <b-form-select v-model="item.game">
                      <option v-for="publisherGame in counterParty.games" v-bind:value="publisherGame">
                        {{ getGameOptionName(publisherGame) }}
                      </option>
                    </b-form-select>

                    <div class="close-button fake-link" v-on:click="removeCounterPartyGame(item.id)">
                      <font-awesome-icon icon="times" size="lg" :style="{ color: '#414141' }" />
                    </div>
                  </div>

                </div>
                <b-button variant="secondary" class="add-game-button" v-on:click="addGame(counterPartyPublisherGames)">Add Game</b-button>
              </div>
            </div>
          </div>
          <div class="row">
            <div class="col-6">
              <label>Budget (Current Budget: ${{publisher.budget}})</label>
              <input v-model="proposerBudgetSendAmount" id="proposerBudgetSendAmount" name="proposerBudgetSendAmount" type="number" class="form-control input" />
            </div>
            <div class="col-6">
              <label>Budget (Current Budget: ${{counterParty.budget}})</label>
              <input v-model="counterPartyBudgetSendAmount" id="counterPartyBudgetSendAmount" name="counterPartyBudgetSendAmount" type="number" class="form-control input" />
            </div>
          </div>

          <div class="form-group">
            <label for="messageText" class="control-label">Message (All players will see this message.)</label>
            <textarea class="form-control" v-model="message" rows="3"></textarea>
          </div>
        </div>

        <div class="alert alert-warning" v-show="clientError">{{clientError}}</div>
        <div class="alert alert-danger" v-show="serverError">{{serverError}}</div>

        <div slot="modal-footer">
          <input type="submit" v-show="counterParty" class="btn btn-primary" value="Propose Trade" v-on:click="proposeTrade" />
        </div>
      </b-modal>
    </form>
  </div>
</template>
<script>
import Vue from 'vue';
import axios from 'axios';

export default {
  data() {
    return {
      counterParty: null,
      proposerPublisherGames: [],
      counterPartyPublisherGames: [],
      proposerBudgetSendAmount: 0,
      counterPartyBudgetSendAmount: 0,
      message: "",
      indexer: 0,
      clientError: '',
      serverError: '',
    };
  },
  props: ['leagueYear', 'publisher'],
  computed: {
    getTradeError() {
      if (this.proposerPublisherGames.length === 0 && this.counterPartyPublisherGames.length === 0) {
        return "A trade must involve at least one game.";
      }

      let allGames = this.proposerPublisherGames.concat(this.counterPartyPublisherGames);
      let nullGames = _.some(allGames, x => !x.game);
      if (nullGames) {
        return "All games must be defined.";
      }

      if (this.proposerPublisherGames.length === 0 && !this.proposerBudgetSendAmount) {
        return "You must offer something.";
      }

      if (this.counterPartyPublisherGames.length === 0 && !this.counterPartyBudgetSendAmount) {
        return "You must receive something.";
      }

      if (this.proposerBudgetSendAmount && this.counterPartyBudgetSendAmount) {
        return "You cannot have budget on both sides of the trade.";
      }

      if (!this.message) {
        return "You must include a message.";
      }

      return "";
    },
    otherPublishers() {
      return _.filter(this.leagueYear.publishers, x => x.publisherID !== this.publisher.publisherID);
    }
  },
  methods: {
    addGame(list) {
      const element = {
        id: this.indexer++,
        game: null
      }
      list.push(element);
    },
    removeProposerGame(id) {
      this.proposerPublisherGames = _.filter(this.proposerPublisherGames, x => x.id !== id);
    },
    removeCounterPartyGame(id) {
      this.counterPartyPublisherGames = _.filter(this.counterPartyPublisherGames, x => x.id !== id);
    },
    getGameOptionName(game) {
      if (game.counterPick) {
        return `${game.gameName} (Counter Pick)`;
      }

      return game.gameName;
    },
    proposeTrade() {
      this.clientError = this.getTradeError;
      if (this.clientError) {
        return;
      }

      var request = {
        proposerPublisherID: this.publisher.publisherID,
        counterPartyPublisherID: this.counterParty.publisherID,
        proposerPublisherGameIDs: this.proposerPublisherGames.map(x => x.game.publisherGameID),
        counterPartyPublisherGameIDs: this.counterPartyPublisherGames.map(x => x.game.publisherGameID),
        proposerBudgetSendAmount: this.proposerBudgetSendAmount,
        counterPartyBudgetSendAmount: this.counterPartyBudgetSendAmount,
        message: this.message
      };
      axios
        .post('/api/league/ProposeTrade', request)
        .then(response => {
          this.$refs.proposeTradeFormRef.hide();
          this.$emit('tradeProposed');
          this.clearData();
        })
        .catch(response => {
          this.serverError = response.response.data;
        });
    },
    clearData() {
      this.counterParty = null;
      this.proposerPublisherGames = [];
      this.counterPartyPublisherGames = [];
      this.proposerBudgetSendAmount = 0;
      this.counterPartyBudgetSendAmount = 0;
      this.message = "";
    }
  }
};
</script>
<style scoped>
  .trade-game-row {
    width: 100%;
    display: inline-flex;
    justify-content: space-between;
    align-items: center;
    gap: 10px;
    margin-bottom: 5px;
  }

  .add-game-button {
    width: 100%;
  }
</style>
