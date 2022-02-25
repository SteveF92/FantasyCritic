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

        <div v-show="counterParty">
          <div class="row">
            <div class="col-6">
              <h4 class="text-black">Offer</h4>

              <div>
                <div v-for="(item, index) in proposerPublisherGames" :key="item.publisherGameID">
                  <div class="trade-game-row">
                    <label>{{index}}</label>
                    <b-form-select v-model="item.game">
                      <option v-for="publisherGame in publisher.games" v-bind:value="publisherGame">
                        {{ publisherGame.gameName }}
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
                    <label>{{index}}</label>
                    <b-form-select v-model="item.game">
                      <option v-for="publisherGame in publisher.games" v-bind:value="publisherGame">
                        {{ publisherGame.gameName }}
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
              <ValidationProvider rules="required|integer" v-slot="{ errors }">
                <label>Budget</label>
                <input v-model="proposerBudgetSendAmount" id="proposerBudgetSendAmount" name="proposerBudgetSendAmount" type="number" class="form-control input" />
                <span class="text-danger">{{ errors[0] }}</span>
              </ValidationProvider>
            </div>
            <div class="col-6">
              <ValidationProvider rules="required|integer" v-slot="{ errors }">
                <label>Budget</label>
                <input v-model="counterPartyBudgetSendAmount" id="counterPartyBudgetSendAmount" name="counterPartyBudgetSendAmount" type="number" class="form-control input" />
                <span class="text-danger">{{ errors[0] }}</span>
              </ValidationProvider>
            </div>
          </div>
        </div>

        <div slot="modal-footer">
          <input type="submit" class="btn btn-primary" value="Propose Trade" v-on:click="proposeTrade" :disabled="!tradeIsValid" />
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
      errorInfo: '',
      indexer: 0
    };
  },
  props: ['leagueYear', 'publisher'],
  computed: {
    tradeIsValid() {
      return true;
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
      this.counterPartyPublisherGames = _.filter(this.proposerPublisherGames, x => x.id !== id);
    },
    proposeTrade() {
      var request = {

      };
      axios
        .post('/api/league/ProposeTrade', request)
        .then(response => {
          this.$refs.proposeTradeFormRef.hide();
          this.$emit('tradeProposed');
          this.clearData();
        })
        .catch(response => {
          this.errorInfo = response.response.data;
        });
    },
    clearData() {
      this.counterParty = null;
      this.proposerPublisherGames = [];
      this.counterPartyPublisherGames = [];
      this.proposerBudgetSendAmount = 0;
      this.counterPartyBudgetSendAmount = 0;
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
