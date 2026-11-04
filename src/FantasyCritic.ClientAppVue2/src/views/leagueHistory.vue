<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="forbidden">
      <div class="alert alert-danger" role="alert">You do not have permission to view this league.</div>
    </div>

    <div v-if="league && !league.publicLeague && !(league.userIsInLeague || league.outstandingInvite)" class="alert alert-warning" role="info">You are viewing a private league.</div>

    <div v-if="league && leagueYear">
      <h1>{{ league.leagueName }} History ({{ year }})</h1>
      <router-link :to="{ name: 'league', params: { leagueid: league.leagueID, year: leagueYear.year } }">Back to League</router-link>
      <div v-if="canDownloadYearExports" class="history-download-actions">
        <b-button variant="info" @click="exportHistoryToCsv">
          <font-awesome-icon icon="download" />
          Download History (CSV)
        </b-button>
        <b-button variant="info" @click="downloadYearAllDataJson">
          <font-awesome-icon icon="download" />
          Download All Data (JSON)
        </b-button>
      </div>
      <hr />
      <div v-if="leagueYear && leagueYear.managerMessages && leagueYear.managerMessages.length > 0">
        <h2>Manager's Messages</h2>
        <div v-for="message in leagueYear.managerMessages" :key="message.messageID" class="alert alert-info">
          <b-button v-if="league.isManager" class="delete-button" variant="warning" @click="deleteMessage(message)">Delete</b-button>
          <h5>{{ formatDateTime(message.timestamp) }}</h5>
          <div class="preserve-whitespace">{{ message.messageText }}</div>
        </div>
        <hr />
      </div>

      <div v-if="leagueActionSets && leagueActionSets.length > 0">
        <h2 class="mb-3">Detailed Bid/Drop Results</h2>
        <div v-for="leagueActionSet in leagueActionSets" :key="leagueActionSet.processSetID" class="history-table">
          <collapseCard>
            <template #header>
              {{ formatDateLong(leagueActionSet.processTime) }}
              <template v-if="leagueActionSet.isSpecialAuction">(Special Auction)</template>
            </template>
            <template #body>
              <leagueActionSet :league-action-set="leagueActionSet" :mode="'leagueHistory'"></leagueActionSet>
            </template>
          </collapseCard>
        </div>
      </div>

      <div v-if="historicalTrades && historicalTrades.length > 0">
        <h2>Trades</h2>
        <tradeSummary v-for="trade in historicalTrades" :key="trade.tradeID" :trade="trade"></tradeSummary>
      </div>

      <h2>Full Actions History</h2>
      <div class="history-table">
        <b-table v-model:sort-by="sortBy" :items="leagueActions" :fields="actionFields" bordered striped responsive>
          <template #cell(timestamp)="data">
            {{ formatDateTime(data.item.timestamp) }}
          </template>
          <template #cell(description)="data">
            <span class="preserve-whitespace">
              {{ data.item.description }}
            </span>
          </template>
          <template #cell(managerAction)="data">
            {{ formatYesNo(data.item.managerAction) }}
          </template>
        </b-table>
      </div>
    </div>
  </div>
</template>
<script>
import axios from 'axios';
import LeagueActionSet from '@/components/leagueActionSet.vue';
import CollapseCard from '@/components/collapseCard.vue';
import TradeSummary from '@/components/tradeSummary.vue';
import { mapState } from 'pinia';
import { useLeagueStore } from '@/store/leagueStore';
import { formatDateLong, formatDateTime, formatYesNo } from '@/utilities/formatters';

export default {
  components: {
    LeagueActionSet,
    CollapseCard,
    TradeSummary
  },
  props: {
    leagueid: { type: String, required: true },
    year: { type: Number, required: true }
  },
  data() {
    return {
      errorInfo: '',
      actionFields: [
        { key: 'publisherName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'timestamp', label: 'Timestamp', sortable: true, thClass: 'bg-primary' },
        { key: 'actionType', label: 'Action Type', sortable: true, thClass: 'bg-primary' },
        { key: 'description', label: 'Description', thClass: 'bg-primary' },
        { key: 'managerAction', label: 'Manager Action?', thClass: 'bg-primary' }
      ],
      sortBy: [{ key: 'timestamp', order: 'desc' }],
      lastID: 1
    };
  },
  watch: {
    $route(to, from) {
      if (to.path !== from.path) {
        this.initializePage();
      }
    }
  },
  created() {
    this.initializePage();
  },
  computed: {
    ...mapState(useLeagueStore, ['forbidden', 'leagueYear', 'leagueActions', 'leagueActionSets', 'historicalTrades', 'league']),
    canDownloadYearExports() {
      return this.league && this.leagueYear && ((this.league.userIsInLeague && this.leagueYear.userIsActive) || this.league.isManager);
    }
  },
  methods: {
    formatDateLong,
    formatDateTime,
    formatYesNo,
    initializePage() {
      const leaguePageParams = { leagueID: this.leagueid, year: this.year };
      const leagueStore = useLeagueStore();
      leagueStore.initializeHistoryPage(leaguePageParams);
    },
    getCollapseID() {
      let thisID = this.lastID;
      this.lastID = this.lastID + 1;
      return thisID;
    },
    async deleteMessage(message) {
      const model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year,
        messageID: message.messageID
      };
      await axios.post('/api/leagueManager/DeleteManagerMessage', model);
      await useLeagueStore().refreshLeagueYear();
    },
    exportHistoryToCsv() {
      window.location.href = `/api/league/ExportLeagueActionSetsToCSV?leagueID=${this.leagueid}&year=${this.year}`;
    },
    downloadYearAllDataJson() {
      window.location.href = `/api/league/DownloadConsolidatedLeagueYearData?leagueID=${this.leagueid}&year=${this.year}`;
    }
  }
};
</script>
<style scoped>
.history-download-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  align-items: center;
  margin-top: 0.75rem;
  margin-bottom: 0.75rem;
}

.history-table {
  margin-left: 15px;
  margin-right: 15px;
}
.delete-button {
  float: right;
}
</style>
