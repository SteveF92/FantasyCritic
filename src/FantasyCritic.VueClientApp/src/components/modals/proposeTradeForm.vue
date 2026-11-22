<template>
  <form class="form-horizontal" no-footer>
    <b-modal id="propose-trade-form" ref="proposeTradeFormRef" size="lg" title="Propose Trade" @hidden="clearData">
      <div class="form-group">
        <label for="counterParty" class="control-label">Publisher to trade with</label>
        <b-form-select v-model="counterParty">
          <option v-for="publisher in otherPublishers" :key="publisher.publisherID" :value="publisher">
            {{ publisher.publisherName }}
          </option>
        </b-form-select>
      </div>

      <div v-if="counterParty">
        <div class="row">
          <div class="col-6">
            <h4 class="text-black">Offer</h4>

            <div>
              <div v-for="(item, index) in proposerPublisherGames" :key="item.id">
                <div class="trade-game-row">
                  <label>{{ index + 1 }}</label>
                  <b-form-select v-model="item.game">
                    <option v-for="publisherGame in requiredUserPublisher.games" :key="publisherGame.publisherGameID" :value="publisherGame">
                      {{ getGameOptionName(publisherGame) }}
                    </option>
                  </b-form-select>

                  <div class="close-button fake-link" @click="removeProposerGame(item.id)">
                    <font-awesome-icon icon="times" size="lg" :style="{ color: '#414141' }" />
                  </div>
                </div>
              </div>
              <b-button variant="secondary" class="full-width-button" @click="addGame(proposerPublisherGames)">Add Game</b-button>
            </div>
          </div>
          <div class="col-6">
            <h4 class="text-black">Receive</h4>

            <div>
              <div v-for="(item, index) in counterPartyPublisherGames" :key="item.id">
                <div class="trade-game-row">
                  <label>{{ index + 1 }}</label>
                  <b-form-select v-model="item.game">
                    <option v-for="publisherGame in counterParty.games" :key="publisherGame.publisherGameID" :value="publisherGame">
                      {{ getGameOptionName(publisherGame) }}
                    </option>
                  </b-form-select>

                  <div class="close-button fake-link" @click="removeCounterPartyGame(item.id)">
                    <font-awesome-icon icon="times" size="lg" :style="{ color: '#414141' }" />
                  </div>
                </div>
              </div>
              <b-button variant="secondary" class="full-width-button" @click="addGame(counterPartyPublisherGames)">Add Game</b-button>
            </div>
          </div>
        </div>
        <div class="row">
          <div class="col-6">
            <label>Budget (Current Budget: ${{ requiredUserPublisher.budget }})</label>
            <input id="proposerBudgetSendAmount" v-model="proposerBudgetSendAmount" name="proposerBudgetSendAmount" type="number" class="form-control input" />
          </div>
          <div class="col-6">
            <label>Budget (Current Budget: ${{ counterParty.budget }})</label>
            <input id="counterPartyBudgetSendAmount" v-model="counterPartyBudgetSendAmount" name="counterPartyBudgetSendAmount" type="number" class="form-control input" />
          </div>
        </div>

        <div class="form-group">
          <label for="messageText" class="control-label">Message (All players will see this message.)</label>
          <textarea v-model="message" class="form-control" rows="3"></textarea>
        </div>
      </div>

      <div v-show="clientError" class="alert alert-warning">{{ clientError }}</div>
      <div v-show="serverError" class="alert alert-danger">{{ serverError }}</div>

      <template #footer>
        <input v-show="counterParty" type="submit" class="btn btn-primary" value="Propose Trade" :disabled="isBusy" @click="proposeTrade" />
      </template>
    </b-modal>
  </form>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import { BModal } from 'bootstrap-vue-next';

import { leagueClient, type ProposeTradeRequest, type PublisherGameViewModel, type PublisherViewModel } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface TradableGame {
  id: number;
  game: PublisherGameViewModel | null;
}

// getTradeError has already rejected rows with no game selected.
function getSelectedGameIDs(items: TradableGame[]): string[] {
  return items.flatMap((x) => (x.game ? [x.game.publisherGameID] : []));
}

interface ProposeTradeFormData {
  counterParty: PublisherViewModel | null;
  proposerPublisherGames: TradableGame[];
  counterPartyPublisherGames: TradableGame[];
  proposerBudgetSendAmount: number;
  counterPartyBudgetSendAmount: number;
  message: string;
  indexer: number;
  clientError: string;
  serverError: string;
  isBusy: boolean;
}

export default defineComponent({
  data(): ProposeTradeFormData {
    return {
      counterParty: null,
      proposerPublisherGames: [],
      counterPartyPublisherGames: [],
      proposerBudgetSendAmount: 0,
      counterPartyBudgetSendAmount: 0,
      message: '',
      indexer: 0,
      clientError: '',
      serverError: '',
      isBusy: false
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear', 'requiredUserPublisher']),
    getTradeError(): string {
      if (this.proposerPublisherGames.length === 0 && this.counterPartyPublisherGames.length === 0) {
        return 'A trade must involve at least one game.';
      }

      const allGames = this.proposerPublisherGames.concat(this.counterPartyPublisherGames);
      const nullGames = allGames.some((x) => !x.game);
      if (nullGames) {
        return 'All games must be defined.';
      }

      if (this.proposerPublisherGames.length === 0 && !this.proposerBudgetSendAmount) {
        return 'You must offer something.';
      }

      if (this.counterPartyPublisherGames.length === 0 && !this.counterPartyBudgetSendAmount) {
        return 'You must receive something.';
      }

      if (this.proposerBudgetSendAmount > 0 && this.counterPartyBudgetSendAmount > 0) {
        return 'You cannot have budget on both sides of the trade.';
      }

      if (!this.message) {
        return 'You must include a message.';
      }

      return '';
    },
    otherPublishers(): PublisherViewModel[] {
      return this.requiredLeagueYear.publishers.filter((x) => x.publisherID !== this.requiredUserPublisher.publisherID && !!x.userID);
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    addGame(list: TradableGame[]): void {
      const element: TradableGame = {
        id: this.indexer++,
        game: null
      };
      list.push(element);
    },
    removeProposerGame(id: number): void {
      this.proposerPublisherGames = this.proposerPublisherGames.filter((x) => x.id !== id);
    },
    removeCounterPartyGame(id: number): void {
      this.counterPartyPublisherGames = this.counterPartyPublisherGames.filter((x) => x.id !== id);
    },
    getGameOptionName(game: PublisherGameViewModel): string {
      if (game.counterPick) {
        return `${game.gameName} (Counter Pick)`;
      }

      return game.gameName;
    },
    async proposeTrade(): Promise<void> {
      this.clientError = this.getTradeError;
      if (this.clientError) {
        return;
      }

      if (!this.counterParty) {
        throw new Error('No counter party selected.');
      }

      const request: ProposeTradeRequest = {
        proposerPublisherID: this.requiredUserPublisher.publisherID,
        counterPartyPublisherID: this.counterParty.publisherID,
        proposerPublisherGameIDs: getSelectedGameIDs(this.proposerPublisherGames),
        counterPartyPublisherGameIDs: getSelectedGameIDs(this.counterPartyPublisherGames),
        proposerBudgetSendAmount: this.proposerBudgetSendAmount || 0,
        counterPartyBudgetSendAmount: this.counterPartyBudgetSendAmount || 0,
        message: this.message
      };

      this.isBusy = true;
      this.serverError = '';
      try {
        await leagueClient.proposeTrade(request);

        (this.$refs.proposeTradeFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('You proposed a trade.');
        this.clearData();
      } catch (error) {
        this.serverError = getApiErrorMessage(error, 'Something went wrong with your trade proposal.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.counterParty = null;
      this.proposerPublisherGames = [];
      this.counterPartyPublisherGames = [];
      this.proposerBudgetSendAmount = 0;
      this.counterPartyBudgetSendAmount = 0;
      this.message = '';
      this.clientError = '';
      this.serverError = '';
    }
  }
});
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
</style>
