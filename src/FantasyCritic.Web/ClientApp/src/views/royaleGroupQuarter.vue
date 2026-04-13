<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="groupQuarter">
      <div class="group-header bg-secondary">
        <h1>{{ groupQuarter.groupName }}</h1>
        <h4>
          <router-link :to="{ name: 'criticsRoyale', params: { year: groupQuarter.year, quarter: groupQuarter.quarter } }">{{ groupQuarter.year }}-Q{{ groupQuarter.quarter }}</router-link>
        </h4>
      </div>

      <div class="nav-area">
        <router-link :to="{ name: 'royaleGroup', params: { groupid: groupid } }">Back to Group</router-link>
      </div>

      <div v-if="allQuarters && selectedYear" class="quarter-selection">
        <b-dropdown :text="selectedYear.toString()">
          <b-dropdown-item v-for="y in years" :key="y" :active="selectedYear === y" @click="selectYear(y)">
            {{ y }}
          </b-dropdown-item>
        </b-dropdown>
        <b-dropdown :text="`Q${quarter}`">
          <b-dropdown-item
            v-for="q in quartersInSelectedYear"
            :key="q.year + '-' + q.quarter"
            :active="q.year === year && q.quarter === quarter"
            :to="{ name: 'royaleGroupQuarter', params: { groupid, year: q.year, quarter: q.quarter } }">
            {{ q.year }}-Q{{ q.quarter }}
          </b-dropdown-item>
        </b-dropdown>
      </div>

      <h2>Standings</h2>
      <b-table v-if="groupQuarter.members.length > 0" striped bordered small responsive :items="groupQuarter.members" :fields="standingsFields">
        <template #cell(ranking)="data">
          <template v-if="data.item.ranking">{{ data.item.ranking }}</template>
          <template v-else>--</template>
        </template>
        <template #cell(displayName)="data">
          <router-link :to="{ name: 'royaleHistory', params: { userid: data.item.userID } }">{{ data.item.displayName }}</router-link>
        </template>
        <template #cell(publisherName)="data">
          <template v-if="data.item.hasPublisher">
            <router-link :to="{ name: 'royalePublisher', params: { publisherid: data.item.publisherID } }">
              {{ data.item.publisherName }}
            </router-link>
          </template>
          <template v-else>
            <span class="text-muted">No publisher yet</span>
          </template>
        </template>
        <template #cell(totalFantasyPoints)="data">
          <template v-if="data.item.hasPublisher">{{ data.item.totalFantasyPoints }}</template>
          <template v-else>--</template>
        </template>
      </b-table>
      <div v-else>
        <p>No members in this group.</p>
      </div>
    </div>
    <div v-else-if="notFound">
      <div class="alert alert-warning">Group or quarter not found.</div>
    </div>
    <div v-else class="spinner">
      <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import { orderBy } from '@/globalFunctions';

export default {
  props: {
    groupid: { type: String, required: true },
    year: { type: Number, required: true },
    quarter: { type: Number, required: true }
  },
  data() {
    return {
      groupQuarter: null,
      allQuarters: null,
      selectedYear: null,
      notFound: false,
      standingsFields: [
        { key: 'ranking', label: 'Rank', thClass: ['bg-primary', 'ranking-column'], tdClass: 'ranking-column' },
        { key: 'displayName', label: 'Player', thClass: 'bg-primary' },
        { key: 'publisherName', label: 'Publisher', thClass: 'bg-primary' },
        { key: 'totalFantasyPoints', label: 'Total Points', thClass: 'bg-primary' }
      ]
    };
  },
  computed: {
    years() {
      if (!this.allQuarters) return [];
      const yearsList = this.allQuarters.map((x) => x.year);
      return orderBy([...new Set(yearsList)], (x) => x);
    },
    quartersInSelectedYear() {
      if (!this.allQuarters) return [];
      return this.allQuarters.filter((q) => q.year === this.selectedYear).sort((a, b) => a.quarter - b.quarter);
    }
  },
  watch: {
    async $route() {
      await this.fetchData();
    }
  },
  async created() {
    await this.fetchData();
  },
  methods: {
    async fetchData() {
      try {
        const [quarterResponse, allQuartersResponse] = await Promise.all([
          axios.get(`/api/RoyaleGroup/GetRoyaleGroupQuarter/${this.groupid}/${this.year}/${this.quarter}`),
          axios.get('/api/Royale/RoyaleQuarters')
        ]);
        this.groupQuarter = quarterResponse.data;
        this.allQuarters = allQuartersResponse.data;
        this.selectedYear = this.year;
      } catch {
        this.notFound = true;
      }
    },
    selectYear(year) {
      this.selectedYear = year;
    }
  }
};
</script>

<style scoped>
.group-header {
  padding: 15px;
  border-radius: 5px;
  margin-bottom: 15px;
}

.nav-area {
  margin-bottom: 10px;
}

.quarter-selection {
  float: right;
  margin-bottom: 10px;
}

.spinner {
  display: flex;
  justify-content: space-around;
}
</style>

<style>
.ranking-column {
  width: 50px;
  text-align: right;
}
</style>
