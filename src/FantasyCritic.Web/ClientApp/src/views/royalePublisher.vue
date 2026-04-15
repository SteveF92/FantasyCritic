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
      </div>

      <div v-if="publisher.quartersWon" class="won-quarters">
        <span v-for="quarter in publisher.quartersWon" :key="`${quarter.year}-${quarter.quarter}`" class="badge badge-success">
          <font-awesome-icon icon="crown" class="quarter-winner-crown" />
          {{ quarter.year }}-Q{{ quarter.quarter }} Winner
        </span>
      </div>

      <div class="row">
        <div class="col-md-12 col-lg-8">
          <h2>Total Points: {{ publisher.totalFantasyPoints }}</h2>
          <h4>Remaining Budget: {{ publisher.budget | money }}</h4>
        </div>

        <div class="col-md-12 col-lg-4">
          <div v-if="userIsPublisher && !quarterIsFinished" class="user-actions">
            <b-button v-b-modal="'royalePurchaseGameForm'" block variant="primary" class="action-button">Purchase a Game</b-button>
            <b-button v-b-modal="'royaleChangePublisherNameForm'" block variant="secondary" class="action-button">Change Publisher Name</b-button>
            <b-button v-if="isPlusUser" v-b-modal="'royaleChangePublisherIconForm'" block variant="secondary" class="action-button">Change Publisher Icon</b-button>
            <b-button v-if="isPlusUser" v-b-modal="'royaleChangePublisherSloganForm'" block variant="secondary" class="action-button">Change Publisher Slogan</b-button>

            <royalePurchaseGameForm :year-quarter="publisher.yearQuarter" :user-royale-publisher="publisher" @gamePurchased="gamePurchased"></royalePurchaseGameForm>
            <royaleChangePublisherNameForm :user-royale-publisher="publisher" @publisherNameChanged="publisherNameChanged"></royaleChangePublisherNameForm>
            <royaleChangePublisherIconForm :user-royale-publisher="publisher" @publisherIconChanged="publisherIconChanged"></royaleChangePublisherIconForm>
            <royaleChangePublisherSloganForm :user-royale-publisher="publisher" @publisherSloganChanged="publisherSloganChanged"></royaleChangePublisherSloganForm>
          </div>
        </div>
      </div>

      <hr />
      <div v-if="errorInfo" class="alert alert-danger">{{ errorInfo }}</div>

      <h2>Games</h2>
      <b-table v-if="publisher.publisherGames.length !== 0" striped bordered small responsive :items="publisher.publisherGames" :fields="allGameFields" :tbody-tr-class="publisherGameRowClass">
        <template #cell(masterGame.gameName)="data">
          <template v-if="data.item.masterGame">
            <span class="master-game-popover">
              <masterGamePopover :master-game="data.item.masterGame" :currently-ineligible="data.item.currentlyIneligible"></masterGamePopover>
            </span>

            <span v-if="data.item.currentlyIneligible" class="game-ineligible">
              Ineligible
              <font-awesome-icon v-b-popover.hover.focus="inEligibleText" color="white" size="lg" icon="info-circle" />
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
          <template v-if="data.item.masterGame">{{ data.item.amountSpent | money }}</template>
          <template v-else>--</template>
        </template>
        <template #cell(advertisingMoney)="data">
          {{ data.item.advertisingMoney | money }}
          <b-button v-if="userIsPublisher && !data.item.locked && !quarterIsFinished" variant="info" size="sm" @click="setGameToSetBudget(data.item)">Set Budget</b-button>
        </template>
        <template #cell(masterGame.criticScore)="data">
          <template v-if="data.item.masterGame">
            {{ data.item.masterGame.criticScore | score(2) }}
          </template>
        </template>
        <template #cell(fantasyPoints)="data">
          {{ data.item.fantasyPoints | score(2) }}
        </template>
        <template #cell(timestamp)="data">
          {{ data.item.timestamp | date }}
        </template>
        <template #cell(lockDateTime)="data">
          {{ getLockCountdown(data.item.lockDateTime) }}
        </template>
        <template #cell(sellGame)="data">
          <b-button v-if="!data.item.locked && !quarterIsFinished" v-b-modal="'sellRoyaleGameModal'" block variant="danger" @click="setGameToSell(data.item)">Sell</b-button>
        </template>
      </b-table>
      <div v-else class="alert alert-info">
        <template v-if="userIsPublisher">You have not bought any games yet!</template>
        <template v-else>This publisher has not bought any games yet.</template>
      </div>
    </div>

    <sellRoyaleGameModal v-if="gameToModify" :publisher-game="gameToModify" @sellGame="sellGame"></sellRoyaleGameModal>

    <b-modal id="setAdvertisingMoneyModal" ref="setAdvertisingMoneyModalRef" title="Set Advertising Budget" @ok="setBudget">
      <div v-if="gameToModify">
        <p>
          How much money do you want to allocate to
          <strong>{{ gameToModify.masterGame.gameName }}</strong>
          ?
        </p>
        <p>Each dollar allocated will increase your fantasy points received by 10%</p>
        <p>You can spend up to $10 for a bonus of 100% (thereby doubling the points you get from the game).</p>
        <p>You can adjust this up until the game is locked, which happens when the game is 5 days away from release, or when it gets its first review on Open Critic.</p>
        <div class="form-group row">
          <label for="advertisingBudgetToSet" class="col-sm-2 col-form-label">Budget</label>
          <div class="col-sm-10">
            <input v-model="advertisingBudgetToSet" class="form-control" />
          </div>
        </div>
      </div>
    </b-modal>

    <div v-if="publisher?.statistics.length > 0" class="royale-chart-container">
      <LineChartGenerator :chart-options="chartOptions" :chart-data="chartData" chart-id="line-chart" dataset-id-key="label" />
    </div>

    <div v-if="publisher?.publisherActions.length > 0">
      <h2>Action History</h2>
      <b-table striped bordered small responsive :items="publisher.publisherActions" :fields="actionFields" :tbody-tr-class="publisherGameRowClass">
        <template #cell(timestamp)="data">
          {{ data.item.timestamp | dateTime }}
        </template>
        <template #cell(masterGame.gameName)="data">
          <template v-if="data.item.masterGame">
            <span class="master-game-popover">
              <masterGamePopover :master-game="data.item.masterGame" :currently-ineligible="data.item.currentlyIneligible"></masterGamePopover>
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

<script>
import axios from 'axios';
import { DateTime } from 'luxon';
import { Line as LineChartGenerator } from 'vue-chartjs/legacy';

import MasterGamePopover from '@/components/masterGamePopover.vue';
import RoyalePurchaseGameForm from '@/components/modals/royalePurchaseGameForm.vue';
import RoyaleChangePublisherNameForm from '@/components/modals/royaleChangePublisherNameForm.vue';
import RoyaleChangePublisherIconForm from '@/components/modals/royaleChangePublisherIconForm.vue';
import RoyaleChangePublisherSloganForm from '@/components/modals/royaleChangePublisherSloganForm.vue';
import SellRoyaleGameModal from '@/components/modals/sellRoyaleGameModal.vue';

import { publisherIconIsValid } from '@/globalFunctions';

export default {
  components: {
    RoyaleChangePublisherNameForm,
    RoyaleChangePublisherIconForm,
    RoyalePurchaseGameForm,
    MasterGamePopover,
    SellRoyaleGameModal,
    RoyaleChangePublisherSloganForm,
    LineChartGenerator
  },
  props: {
    publisherid: { type: String, required: true }
  },
  data() {
    return {
      errorInfo: '',
      publisher: null,
      gameToModify: null,
      advertisingBudgetToSet: 0,
      gameFields: [
        { key: 'masterGame.gameName', label: 'Game', thClass: 'bg-primary', sortable: true },
        { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        { key: 'amountSpent', label: 'Amount Spent', thClass: 'bg-primary', sortable: true },
        { key: 'advertisingMoney', label: 'Advertising Budget', thClass: 'bg-primary', sortable: true },
        { key: 'masterGame.criticScore', label: 'Critic Score', thClass: 'bg-primary', sortable: true },
        { key: 'fantasyPoints', label: 'Fantasy Points', thClass: 'bg-primary', sortable: true },
        { key: 'timestamp', label: 'Purchase Date', thClass: 'bg-primary', sortable: true }
      ],
      actionFields: [
        { key: 'timestamp', label: 'Timestamp', sortable: true, thClass: 'bg-primary' },
        { key: 'actionType', label: 'Action Type', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame.gameName', label: 'Game', thClass: 'bg-primary', sortable: true },
        { key: 'description', label: 'Description', thClass: 'bg-primary' }
      ],
      lockDateTimeField: { key: 'lockDateTime', label: 'Locks In', thClass: 'bg-primary', sortable: true },
      sellGameField: { key: 'sellGame', thClass: 'bg-primary', label: 'Sell' }
    };
  },
  computed: {
    userIsPublisher() {
      return this.isAuth && this.publisher.userID === this.$store.getters.userInfo.userID;
    },
    allGameFields() {
      let allFields = this.gameFields.slice();
      if (this.userIsPublisher) {
        allFields.push(this.lockDateTimeField);
        if (this.publisher.publisherGames.some((x) => !x.locked)) {
          allFields.push(this.sellGameField);
        }
      }
      return allFields;
    },
    inEligibleText() {
      return {
        html: true,
        title: () => {
          return 'What does this mean?';
        },
        content: () => {
          return (
            "This game's status has changed since it was purchased, and it is currently ineligible based on the royale rules. Any points the game receives will NOT count. <br/> <br/>" +
            'Alternatively, this can happen if the game was purchased the same day as it released, but before the site was updated with the correct release date. <br/> <br/>' +
            'The game can be dropped for a full refund.'
          );
        }
      };
    },
    iconIsValid() {
      return publisherIconIsValid(this.publisher.publisherIcon);
    },
    quarterIsFinished() {
      return this.publisher.yearQuarter.finished;
    },
    statisticsDateLabels() {
      if (!this.publisher?.statistics?.length) {
        return [];
      }
      return this.publisher.statistics.map((x) => x.date);
    },
    releaseDateAnnotations() {
      if (!this.publisher?.publisherGames?.length || !this.statisticsDateLabels.length) {
        return {};
      }
      const labelSet = new Set(this.statisticsDateLabels);
      const annotations = {};
      let index = 0;
      for (const pg of this.publisher.publisherGames) {
        if (!pg.masterGame) {
          continue;
        }
        const label = this.masterGameReleaseChartLabel(pg.masterGame);
        if (!label || !labelSet.has(label)) {
          continue;
        }
        const gameName = pg.masterGame.gameName;
        annotations[`release-${pg.masterGame.masterGameID}-${index}`] = {
          type: 'line',
          scaleID: 'x',
          value: label,
          borderColor: 'rgba(255, 255, 255, 0.4)',
          borderWidth: 2,
          borderDash: [4, 4],
          drawTime: 'beforeDatasetsDraw',
          z: -1,
          label: {
            display: false,
            content: gameName,
            drawTime: 'afterDatasetsDraw',
            backgroundColor: 'rgba(20, 20, 20, 0.95)',
            borderColor: 'rgba(214, 153, 58, 0.5)',
            borderWidth: 1,
            color: '#ffffff',
            font: { size: 12 },
            padding: 8,
            borderRadius: 4,
            position: 'start',
            yAdjust: -6
          },
          enter({ element }) {
            if (element.label) {
              element.label.options.display = true;
            }
            return true;
          },
          leave({ element }) {
            if (element.label) {
              element.label.options.display = false;
            }
            return true;
          }
        };
        index += 1;
      }
      return annotations;
    },
    chartOptions() {
      return {
        responsive: true,
        maintainAspectRatio: false,
        layout: {
          padding: {
            top: 4,
            right: 8,
            bottom: 52,
            left: 4
          }
        },
        interaction: { mode: 'index', intersect: false },
        plugins: {
          legend: {
            labels: {
              color: 'rgba(255, 255, 255, 0.9)',
              font: { size: 13 },
              usePointStyle: true,
              padding: 16
            }
          },
          tooltip: {
            backgroundColor: 'rgba(20, 20, 20, 0.95)',
            titleColor: '#fff',
            bodyColor: '#fff',
            borderColor: 'rgba(214, 153, 58, 0.5)',
            borderWidth: 1
          },
          annotation: {
            interaction: {
              mode: 'nearest',
              axis: 'x',
              intersect: false
            },
            annotations: this.releaseDateAnnotations
          }
        },
        scales: {
          x: {
            ticks: {
              color: 'rgba(255, 255, 255, 0.75)',
              maxRotation: 45,
              minRotation: 0,
              padding: 6
            },
            grid: { color: 'rgba(255, 255, 255, 0.08)' }
          },
          y: {
            min: 0,
            ticks: { color: 'rgba(255, 255, 255, 0.75)' },
            grid: { color: 'rgba(255, 255, 255, 0.08)' }
          }
        }
      };
    },
    chartData() {
      if (!this.publisher?.statistics?.length) {
        return { labels: [], datasets: [] };
      }
      const accent = '#d6993a';
      return {
        labels: this.statisticsDateLabels,
        datasets: [
          {
            label: 'Fantasy Points',
            borderColor: accent,
            backgroundColor: 'rgba(214, 153, 58, 0.18)',
            borderWidth: 2,
            stepped: true,
            pointRadius: 4,
            pointHoverRadius: 6,
            pointBackgroundColor: '#ffffff',
            pointBorderColor: accent,
            pointBorderWidth: 2,
            data: this.publisher.statistics.map((x) => x.fantasyPoints)
          }
        ]
      };
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
    async fetchPublisher() {
      try {
        const response = await axios.get('/api/Royale/GetRoyalePublisher/' + this.publisherid);
        this.publisher = response.data;
      } catch (error) {
        this.error = error.response.data;
      }
    },
    async gamePurchased(purchaseInfo) {
      await this.fetchPublisher();
      let message = purchaseInfo.gameName + ' was purchased for ' + this.$options.filters.money(purchaseInfo.purchaseCost);
      this.makeToast(message);
    },
    async publisherNameChanged() {
      await this.fetchPublisher();
      let message = 'Publisher name changed.';
      this.makeToast(message);
    },
    async publisherIconChanged() {
      await this.fetchPublisher();
      let message = 'Publisher icon changed.';
      this.makeToast(message);
    },
    async publisherSloganChanged() {
      await this.fetchPublisher();
      let message = 'Publisher slogan changed.';
      this.makeToast(message);
    },
    setGameToSell(publisherGame) {
      this.gameToModify = publisherGame;
      this.$refs.sellRoyaleGameModalRef.show();
    },
    async sellGame() {
      const request = {
        publisherID: this.publisher.publisherID,
        masterGameID: this.gameToModify.masterGame.masterGameID
      };

      try {
        await axios.post('/api/royale/SellGame', request);
        await this.fetchPublisher();
        let message = this.gameToModify.masterGame.gameName + ' was sold for ' + this.$options.filters.money(this.gameToModify.refundAmount);
        this.makeToast(message);
      } catch (error) {
        this.errorInfo = "You can't sell that game. " + error.response.data;
      }
    },
    setGameToSetBudget(publisherGame) {
      this.gameToModify = publisherGame;
      this.advertisingBudgetToSet = publisherGame.advertisingMoney;
      this.$refs.setAdvertisingMoneyModalRef.show();
    },
    async setBudget() {
      const request = {
        publisherID: this.publisher.publisherID,
        masterGameID: this.gameToModify.masterGame.masterGameID,
        advertisingMoney: this.advertisingBudgetToSet
      };

      try {
        await axios.post('/api/royale/SetAdvertisingMoney', request);
        await this.fetchPublisher();
        this.advertisingBudgetToSet = 0;
      } catch (error) {
        this.advertisingBudgetToSet = 0;
        this.errorInfo = "You can't set that budget. " + error.response.data;
      }
    },
    getReleaseDate(game) {
      if (game.releaseDate) {
        return DateTime.fromISO(game.releaseDate).toFormat('yyyy-MM-dd');
      }
      return game.estimatedReleaseDate + ' (Estimated)';
    },
    openCriticLink(game) {
      return `https://opencritic.com/game/${game.openCriticID}/${game.openCriticSlug ?? 'b'}`;
    },
    publisherGameRowClass(item, type) {
      if (!item || type !== 'row') {
        return;
      }
      if (item.currentlyIneligible) {
        return 'table-warning';
      }
    },
    getLockCountdown(lockDateTime) {
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
    getHiddenText(gameOrAction) {
      return {
        html: true,
        title: () => {
          return 'What does this mean?';
        },
        content: () => {
          return `This ${gameOrAction} is hidden from other players. See the Royale home page for more details.`;
        }
      };
    },
    masterGameReleaseChartLabel(game) {
      return game?.releaseDate ?? null;
    }
  }
};
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
  max-width: 100%;
  padding: 16px 12px 20px;
  margin-top: 8px;
  margin-bottom: 12px;
  background: #252525;
  border: 1px solid rgba(214, 153, 58, 0.35);
  border-radius: 8px;
  box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.04);
  box-sizing: border-box;
}
</style>
