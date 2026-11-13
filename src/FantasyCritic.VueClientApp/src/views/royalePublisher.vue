<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="publisher">
      <div class="publisher-header bg-secondary">
        <div v-if="publisher.publisherIcon && iconIsValid" class="publisher-icon">
          {{ publisher.publisherIcon }}
        </div>
        <h1 class="publisher-name">{{ publisher.publisherName }}</h1>
        <h2 v-if="publisher.publisherSlogan" class="publisher-slogan">~"{{ publisher.publisherSlogan }}"</h2>
        <h4>
          Player Name:
          <router-link :to="{ name: 'royaleHistory', params: { userid: publisher.userID } }" class="history-link">{{ publisher.playerName }}</router-link>
        </h4>
        <h4>
          Year/Quarter:
          <router-link :to="{ name: 'criticsRoyale', params: { year: publisher.yearQuarter.year, quarter: publisher.yearQuarter.quarter } }">
            {{ publisher.yearQuarter.year }}-Q{{ publisher.yearQuarter.quarter }}
          </router-link>
        </h4>
        <h4>Rank: {{ publisher.ranking ?? '--' }}</h4>
      </div>

      <div v-if="publisher.quartersWon.length > 0" class="won-quarters">
        <span v-for="quarter in publisher.quartersWon" :key="`${quarter.year}-${quarter.quarter}`" class="badge badge-success">
          <font-awesome-icon icon="crown" class="quarter-winner-crown" />
          {{ quarter.year }}-Q{{ quarter.quarter }} Winner
        </span>
      </div>

      <div class="row">
        <div class="col-md-12 col-lg-8">
          <h2>Total Points: {{ publisher.totalFantasyPoints }}</h2>
          <h4>Remaining Budget: {{ formatCurrency(publisher.budget) }}</h4>
        </div>

        <div class="col-md-12 col-lg-4">
          <div v-if="userIsPublisher && !quarterIsFinished" class="user-actions">
            <b-button v-b-modal="'royalePurchaseGameForm'" variant="primary" class="action-button">Purchase a Game</b-button>
            <b-button v-b-modal="'royaleChangePublisherNameForm'" variant="secondary" class="action-button">Change Publisher Name</b-button>
            <b-button v-if="isPlusUser" v-b-modal="'royaleChangePublisherIconForm'" variant="secondary" class="action-button">Change Publisher Icon</b-button>
            <b-button v-if="isPlusUser" v-b-modal="'royaleChangePublisherSloganForm'" variant="secondary" class="action-button">Change Publisher Slogan</b-button>

            <royalePurchaseGameForm :user-royale-publisher="publisher" @gamePurchased="gamePurchased"></royalePurchaseGameForm>
            <royaleChangePublisherNameForm :user-royale-publisher="publisher" @publisherNameChanged="publisherNameChanged"></royaleChangePublisherNameForm>
            <royaleChangePublisherIconForm :user-royale-publisher="publisher" @publisherIconChanged="publisherIconChanged"></royaleChangePublisherIconForm>
            <royaleChangePublisherSloganForm :user-royale-publisher="publisher" @publisherSloganChanged="publisherSloganChanged"></royaleChangePublisherSloganForm>
          </div>
        </div>
      </div>

      <hr />
      <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>

      <h2>Games</h2>
      <b-table v-if="publisher.publisherGames.length !== 0" striped bordered small responsive :items="publisher.publisherGames" :fields="allGameFields" :tbody-tr-class="publisherGameRowClass">
        <template #cell(masterGame.gameName)="data">
          <template v-if="data.item.masterGame">
            <span class="master-game-popover">
              <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
            </span>

            <span v-if="data.item.currentlyIneligible" class="game-ineligible">
              Ineligible
              <b-popover title="What does this mean?">
                <template #target>
                  <font-awesome-icon color="white" size="lg" icon="info-circle" />
                </template>
                This game's status has changed since it was purchased, and it is currently ineligible based on the royale rules. Any points the game receives will NOT count.
                <br />
                <br />
                Alternatively, this can happen if the game was purchased the same day as it released, but before the site was updated with the correct release date.
                <br />
                <br />
                The game can be dropped for a full refund.
              </b-popover>
            </span>

            <span v-if="data.item.gameHidden" class="game-ineligible">
              Hidden
              <font-awesome-icon v-b-popover.hover.focus="getHiddenText('game')" color="white" size="lg" icon="eye-slash" />
            </span>
          </template>
          <span v-else class="hidden-text">Hidden Until Release</span>
        </template>
        <template #cell(masterGame.maximumReleaseDate)="data">
          <template v-if="data.item.masterGame">{{ getReleaseDate(data.item.masterGame) }}</template>
          <template v-else>--</template>
        </template>
        <template #cell(amountSpent)="data">
          <template v-if="data.item.masterGame">{{ formatCurrency(data.item.amountSpent) }}</template>
          <template v-else>--</template>
        </template>
        <template #cell(advertisingMoney)="data">
          {{ formatCurrency(data.item.advertisingMoney) }}
          <b-button v-if="userIsPublisher && !data.item.locked && !quarterIsFinished" variant="info" size="sm" @click="setGameToSetBudget(data.item)">Set Budget</b-button>
        </template>
        <template #cell(masterGame.criticScore)="data">
          <template v-if="data.item.masterGame">
            {{ formatScore(data.item.masterGame.criticScore, 2) }}
          </template>
        </template>
        <template #cell(fantasyPoints)="data">
          {{ formatScore(data.item.fantasyPoints, 2) }}
        </template>
        <template #cell(timestamp)="data">
          {{ formatDateIso(data.item.timestamp) }}
        </template>
        <template #cell(lockDateTime)="data">
          {{ getLockCountdown(data.item.lockDateTime) }}
        </template>
        <template #cell(sellGame)="data">
          <b-button v-if="!data.item.locked && !quarterIsFinished" v-b-modal="'sellRoyaleGameModal'" variant="danger" class="w-100" @click="setGameToSell(data.item)">Sell</b-button>
        </template>
      </b-table>
      <div v-else class="alert alert-info">
        <template v-if="userIsPublisher">You have not bought any games yet!</template>
        <template v-else>This publisher has not bought any games yet.</template>
      </div>

      <sellRoyaleGameModal :publisher-game="gameToModify" @sellGame="sellGame"></sellRoyaleGameModal>
    </div>
    <b-alert v-else-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>

    <b-modal id="setAdvertisingMoneyModal" ref="setAdvertisingMoneyModalRef" title="Set Advertising Budget" @ok="setBudget">
      <div v-if="gameToModify">
        <p>
          How much money do you want to allocate to
          <strong>{{ gameToModify.masterGame.gameName }}</strong>
          ?
        </p>
        <p>Each dollar allocated will increase your fantasy points received by 5%</p>
        <p>You can spend up to $10 for a bonus of 50%.</p>
        <p>You can adjust this up until the game is locked, which happens when the game is 7 days away from release, or when it gets its first review on OpenCritic.</p>
        <div class="form-group row">
          <label for="advertisingBudgetToSet" class="col-sm-2 col-form-label">Budget</label>
          <div class="col-sm-10">
            <input id="advertisingBudgetToSet" v-model.number="advertisingBudgetToSet" type="number" min="0" max="10" class="form-control" />
          </div>
        </div>
      </div>
    </b-modal>

    <div v-if="publisher && publisher.statistics.length > 0" class="royale-chart-container">
      <RoyalePublisherGraph :royale-publishers="[publisher]"></RoyalePublisherGraph>
    </div>

    <div v-if="publisher && publisher.publisherActions.length > 0">
      <h2>Action History</h2>
      <b-table striped bordered small responsive :items="publisher.publisherActions" :fields="actionFields">
        <template #cell(timestamp)="data">
          {{ formatDateTime(data.item.timestamp) }}
        </template>
        <template #cell(masterGame.gameName)="data">
          <template v-if="data.item.masterGame">
            <span class="master-game-popover">
              <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
            </span>
            <span v-if="data.item.gameHidden" class="game-ineligible">
              Hidden
              <font-awesome-icon v-b-popover.hover.focus="getHiddenText('action')" color="white" size="lg" icon="eye-slash" />
            </span>
          </template>
          <span v-else class="hidden-text">Hidden Until Release</span>
        </template>
        <template #cell(description)="data">
          <span v-if="data.item.description" class="preserve-whitespace">
            {{ data.item.description }}
          </span>
          <span v-else class="hidden-text">Hidden Until Release</span>
        </template>
      </b-table>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import type { BModal, TableField, TableRowType, TableStrictClassValue } from 'bootstrap-vue-next';
import { DateTime } from 'luxon';

import {
  royaleClient,
  type MasterGameYearViewModel,
  type RoyaleActionViewModel,
  type RoyalePublisherGameViewModel,
  type RoyalePublisherViewModel,
  type SellRoyaleGameRequest,
  type SetAdvertisingMoneyRequest
} from '@/api/clients';
import { formatMoney, numericComparer, publisherIconIsValid } from '@/globalFunctions';
import { useAuthStore } from '@/store/authStore';
import type { PopoverContent, RoyaleGamePurchase } from '@/types/vueTypes';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatCurrency, formatDateIso, formatDateTime, formatScore } from '@/utilities/formatters';
import { asOwnRoyalePublisherGame, type OwnRoyalePublisherGame } from '@/utilities/royaleGames';
import { makeToast } from '@/utilities/toast';
import MasterGamePopover from '@/components/masterGamePopover.vue';
import RoyaleChangePublisherIconForm from '@/components/modals/royaleChangePublisherIconForm.vue';
import RoyaleChangePublisherNameForm from '@/components/modals/royaleChangePublisherNameForm.vue';
import RoyaleChangePublisherSloganForm from '@/components/modals/royaleChangePublisherSloganForm.vue';
import RoyalePurchaseGameForm from '@/components/modals/royalePurchaseGameForm.vue';
import SellRoyaleGameModal from '@/components/modals/sellRoyaleGameModal.vue';
import RoyalePublisherGraph from '@/components/royalePublisherGraph.vue';

interface RoyalePublisherData {
  errorInfo: string;
  publisher: RoyalePublisherViewModel | null;
  gameToModify: OwnRoyalePublisherGame | null;
  advertisingBudgetToSet: number;
}

export default defineComponent({
  setup() {
    const gameFields: TableField<RoyalePublisherGameViewModel>[] = [
      { key: 'masterGame.gameName', label: 'Game', thClass: 'bg-primary', sortable: true },
      { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
      { key: 'amountSpent', label: 'Amount Spent', thClass: 'bg-primary', sortable: true, sortCompare: numericComparer },
      { key: 'advertisingMoney', label: 'Advertising Budget', thClass: 'bg-primary', sortable: true, sortCompare: numericComparer },
      { key: 'masterGame.criticScore', label: 'Critic Score', thClass: 'bg-primary', sortable: true, sortCompare: numericComparer },
      { key: 'fantasyPoints', label: 'Fantasy Points', thClass: 'bg-primary', sortable: true, sortCompare: numericComparer },
      { key: 'timestamp', label: 'Purchase Date', thClass: 'bg-primary', sortable: true }
    ];
    const actionFields: TableField<RoyaleActionViewModel>[] = [
      { key: 'timestamp', label: 'Timestamp', sortable: true, thClass: 'bg-primary' },
      { key: 'actionType', label: 'Action Type', sortable: true, thClass: 'bg-primary' },
      { key: 'masterGame.gameName', label: 'Game', thClass: 'bg-primary', sortable: true },
      { key: 'description', label: 'Description', thClass: 'bg-primary' }
    ];
    const lockDateTimeField: TableField<RoyalePublisherGameViewModel> = { key: 'lockDateTime', label: 'Locks In', thClass: 'bg-primary', sortable: true };
    const sellGameField: TableField<RoyalePublisherGameViewModel> = { key: 'sellGame', thClass: 'bg-primary', label: 'Sell' };

    return { gameFields, actionFields, lockDateTimeField, sellGameField };
  },
  components: {
    RoyaleChangePublisherNameForm,
    RoyaleChangePublisherIconForm,
    RoyalePurchaseGameForm,
    MasterGamePopover,
    SellRoyaleGameModal,
    RoyaleChangePublisherSloganForm,
    RoyalePublisherGraph
  },
  props: {
    publisherid: { type: String, required: true }
  },
  data(): RoyalePublisherData {
    return {
      errorInfo: '',
      publisher: null,
      gameToModify: null,
      advertisingBudgetToSet: 0
    };
  },
  computed: {
    ...mapState(useAuthStore, ['isPlusUser', 'isAuth']),
    userIsPublisher(): boolean {
      const authStore = useAuthStore();
      return this.isAuth && this.publisher !== null && this.publisher.userID === authStore.userInfo?.userID;
    },
    allGameFields(): TableField<RoyalePublisherGameViewModel>[] {
      let allFields = this.gameFields.slice();
      if (this.userIsPublisher && this.publisher !== null && this.publisher.publisherGames.some((x) => !x.locked) && !this.quarterIsFinished) {
        allFields.push(this.lockDateTimeField);
        allFields.push(this.sellGameField);
      }
      return allFields;
    },
    iconIsValid(): boolean {
      if (!this.publisher) {
        return false;
      }
      return publisherIconIsValid(this.publisher.publisherIcon ?? '');
    },
    quarterIsFinished(): boolean {
      if (!this.publisher) {
        return false;
      }
      return this.publisher.yearQuarter.finished;
    }
  },
  watch: {
    async $route() {
      await this.fetchPublisher();
    }
  },
  async created() {
    await this.fetchPublisher();
  },
  methods: {
    formatCurrency,
    formatDateIso,
    formatDateTime,
    formatScore,
    async fetchPublisher(): Promise<void> {
      try {
        this.publisher = await royaleClient.getRoyalePublisher(this.publisherid);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error);
      }
    },
    async gamePurchased(purchaseInfo: RoyaleGamePurchase): Promise<void> {
      await this.fetchPublisher();
      let message = purchaseInfo.gameName + ' was purchased for ' + formatMoney(purchaseInfo.purchaseCost, 2);
      makeToast(message);
    },
    async publisherNameChanged(): Promise<void> {
      await this.fetchPublisher();
      let message = 'Publisher name changed.';
      makeToast(message);
    },
    async publisherIconChanged(): Promise<void> {
      await this.fetchPublisher();
      let message = 'Publisher icon changed.';
      makeToast(message);
    },
    async publisherSloganChanged(): Promise<void> {
      await this.fetchPublisher();
      let message = 'Publisher slogan changed.';
      makeToast(message);
    },
    setGameToSell(publisherGame: RoyalePublisherGameViewModel): void {
      this.gameToModify = asOwnRoyalePublisherGame(publisherGame);
    },
    async sellGame(): Promise<void> {
      if (!this.publisher || !this.gameToModify) {
        return;
      }

      this.errorInfo = '';
      const request: SellRoyaleGameRequest = {
        publisherID: this.publisher.publisherID,
        masterGameID: this.gameToModify.masterGame.masterGameID,
        expectedRefundAmount: this.gameToModify.refundAmount,
        expectedAdvertisingMoney: this.gameToModify.advertisingMoney
      };

      try {
        await royaleClient.sellGame(request);
        await this.fetchPublisher();
        let message = this.gameToModify.masterGame.gameName + ' was sold for ' + formatMoney(this.gameToModify.refundAmount, 2);
        makeToast(message);
      } catch (error) {
        this.errorInfo = "You can't sell that game. " + getApiErrorMessage(error);
        await this.fetchPublisher();
      }
    },
    setGameToSetBudget(publisherGame: RoyalePublisherGameViewModel): void {
      this.gameToModify = asOwnRoyalePublisherGame(publisherGame);
      this.advertisingBudgetToSet = publisherGame.advertisingMoney;
      (this.$refs.setAdvertisingMoneyModalRef as InstanceType<typeof BModal> | undefined)?.show();
    },
    async setBudget(): Promise<void> {
      if (!this.publisher || !this.gameToModify) {
        return;
      }

      this.errorInfo = '';
      const request: SetAdvertisingMoneyRequest = {
        publisherID: this.publisher.publisherID,
        masterGameID: this.gameToModify.masterGame.masterGameID,
        advertisingMoney: this.advertisingBudgetToSet
      };

      try {
        await royaleClient.setAdvertisingMoney(request);
        await this.fetchPublisher();
        this.advertisingBudgetToSet = 0;
      } catch (error) {
        this.advertisingBudgetToSet = 0;
        this.errorInfo = "You can't set that budget. " + getApiErrorMessage(error);
      }
    },
    getReleaseDate(game: MasterGameYearViewModel): string {
      if (game.releaseDate) {
        return DateTime.fromISO(game.releaseDate).toFormat('yyyy-MM-dd');
      }
      return game.estimatedReleaseDate + ' (Estimated)';
    },
    publisherGameRowClass(item: RoyalePublisherGameViewModel | null, type: TableRowType): TableStrictClassValue {
      if (!item || type !== 'row') {
        return '';
      }
      if (item.currentlyIneligible) {
        return 'table-warning';
      }

      return '';
    },
    getLockCountdown(lockDateTime: string | undefined): string {
      if (!lockDateTime) {
        return '--';
      }
      const now = DateTime.now();
      const lock = DateTime.fromISO(lockDateTime);
      const totalHours = lock.diff(now, 'hours').hours;
      if (totalHours <= 0) {
        return 'Locked';
      }
      const diff = lock.diff(now, ['days', 'hours']);
      const days = Math.floor(diff.days);
      const hours = Math.floor(diff.hours);
      if (days < 5) {
        if (days === 0) {
          return `${hours}h`;
        }
        return `${days}d ${hours}h`;
      }
      return `${days} days`;
    },
    getHiddenText(gameOrAction: string): PopoverContent {
      return {
        title: 'What does this mean?',
        body: `This ${gameOrAction} is hidden from other players. See the Royale home page for more details.`
      };
    }
  }
});
</script>

<style scoped>
.publisher-header {
  margin-top: 10px;
  border: 2px;
  border-color: #d6993a;
  border-style: solid;
  padding-left: 5px;
}

.publisher-name {
  display: block;
  max-width: 100%;
  word-wrap: break-word;
}

.publisher-icon {
  font-size: 75px;
  padding: 5px;
  float: left;
}

.top-area {
  margin-top: 10px;
  margin-bottom: 20px;
}

.main-buttons {
  display: flex;
  flex-direction: row;
  justify-content: space-around;
}

.main-button {
  margin-top: 5px;
  min-width: 200px;
}

.action-button {
  width: 100%;
}

.action-button + .action-button {
  margin-top: 8px;
}

.won-quarters {
  margin-top: 10px;
  margin-bottom: 10px;
}

.won-quarters span {
  margin: 5px;
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

.quarter-winner-crown {
  color: #d6993a;
}

.hidden-text {
  font-style: italic;
}

.history-link {
  display: inline-block;
  margin-bottom: 8px;
  color: #d6993a;
}

.royale-chart-container {
  height: 380px;
  background: #252525;
  border: 1px solid rgba(214, 153, 58, 0.35);
  border-radius: 8px;
}
</style>
