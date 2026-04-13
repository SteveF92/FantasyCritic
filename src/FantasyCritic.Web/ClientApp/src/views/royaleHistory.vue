<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="historyData">
      <div class="publisher-header bg-secondary">
        <h1>Royale History</h1>
        <h4>Player: {{ historyData.playerName }}</h4>
      </div>

      <div v-if="historyData.quartersWon && historyData.quartersWon.length > 0" class="won-quarters">
        <span v-for="quarter in historyData.quartersWon" :key="`${quarter.year}-${quarter.quarter}`" class="badge badge-success">
          <font-awesome-icon icon="crown" class="quarter-winner-crown" />
          {{ quarter.year }}-Q{{ quarter.quarter }} Winner
        </span>
      </div>

      <div v-if="historyData.publishers.length > 0" class="history-table-area">
        <b-table striped bordered responsive small :items="historyData.publishers" :fields="historyFields">
          <template #cell(yearQuarter)="data">
            <router-link :to="{ name: 'criticsRoyale', params: { year: data.item.year, quarter: data.item.quarter } }">
              {{ data.item.year }}-Q{{ data.item.quarter }}
            </router-link>
          </template>
          <template #cell(publisherName)="data">
            <span v-if="data.item.publisherIcon" class="publisher-icon-small">{{ data.item.publisherIcon }}</span>
            <router-link :to="{ name: 'royalePublisher', params: { publisherid: data.item.publisherID } }">
              {{ data.item.publisherName }}
            </router-link>
          </template>
          <template #cell(ranking)="data">
            <template v-if="data.item.ranking">{{ data.item.ranking }}</template>
            <template v-else>--</template>
          </template>
          <template #cell(totalFantasyPoints)="data">
            {{ data.item.totalFantasyPoints | score(2) }}
          </template>
        </b-table>
      </div>
      <div v-else class="alert alert-info">This player has no Royale history yet.</div>
    </div>
    <div v-else class="spinner">
      <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
    </div>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  props: {
    userid: { type: String, required: true }
  },
  data() {
    return {
      historyData: null,
      historyFields: [
        { key: 'yearQuarter', label: 'Quarter', thClass: ['bg-primary', 'quarter-column'], tdClass: 'quarter-column' },
        { key: 'publisherName', label: 'Publisher', thClass: 'bg-primary' },
        { key: 'totalFantasyPoints', label: 'Total Points', thClass: 'bg-primary', sortable: true },
        { key: 'ranking', label: 'Ranking', thClass: ['bg-primary', 'ranking-column'], tdClass: 'ranking-column' }
      ]
    };
  },
  watch: {
    async $route() {
      await this.fetchHistory();
    }
  },
  async created() {
    await this.fetchHistory();
  },
  methods: {
    async fetchHistory() {
      try {
        const response = await axios.get('/api/Royale/UserRoyaleHistory/' + this.userid);
        this.historyData = response.data;
      } catch {
        this.historyData = null;
      }
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

.won-quarters {
  margin-top: 10px;
  margin-bottom: 10px;
}

.won-quarters span {
  margin: 5px;
}

.quarter-winner-crown {
  color: #d6993a;
}

.history-table-area {
  margin-top: 15px;
}

.publisher-icon-small {
  margin-right: 5px;
}

.spinner {
  display: flex;
  justify-content: space-around;
  margin-top: 50px;
}
</style>

<style>
.ranking-column {
  width: 80px;
  text-align: right;
}

.quarter-column {
  width: 120px;
}
</style>
