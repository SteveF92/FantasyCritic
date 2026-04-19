<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="royaleYearQuarterOptions && selectedYear" class="quarter-selection">
      <b-dropdown id="dropdown-1" :text="selectedYear.toString()">
        <b-dropdown-item v-for="year in years" :key="year" :active="selectedYear === year" @click="selectYear(year)">
          {{ year }}
        </b-dropdown-item>
      </b-dropdown>
      <b-dropdown :text="`Q${quarter}`">
        <b-dropdown-item
          v-for="royaleYearQuarterOption in quartersInSelectedYear"
          :key="royaleYearQuarterOption.year + '-' + royaleYearQuarterOption.quarter"
          :active="royaleYearQuarterOption.year === year && royaleYearQuarterOption.quarter === quarter"
          :to="{ name: 'criticsRoyale', params: { year: royaleYearQuarterOption.year, quarter: royaleYearQuarterOption.quarter } }">
          {{ royaleYearQuarterOption.year }}-Q{{ royaleYearQuarterOption.quarter }}
        </b-dropdown-item>
      </b-dropdown>
    </div>
    <div class="critic-royale-header-area bg-secondary">
      <img class="critic-royale-header" src="@/assets/critic-royale-logo.svg" />
    </div>

    <div class="critic-royale-header-area-simple">
      <h1>Critics Royale</h1>
    </div>

    <div v-if="!userPublisherBusy && !userRoyalePublisherID && royaleYearQuarter.openForPlay && !royaleYearQuarter.finished">
      <div v-if="isAuth" class="alert alert-info">
        Create your publisher to start playing!
        <b-button v-b-modal="'createRoyalePublisher'" class="login-button" variant="primary">Create Publisher</b-button>
        <createRoyalePublisherForm :royale-year-quarter="royaleYearQuarter"></createRoyalePublisherForm>
      </div>
      <div v-if="!isAuth" class="alert alert-success">
        Sign up or log in to start playing now!
        <b-button variant="info" href="/Account/Login">
          <span>Log In</span>
          <font-awesome-icon class="topnav-button-icon" icon="sign-in-alt" />
        </b-button>
        <b-button variant="primary" href="/Account/Register">
          <span>Sign Up</span>
          <font-awesome-icon class="topnav-button-icon" icon="user-plus" />
        </b-button>
      </div>
    </div>

    <div class="leaderboard-header">
      <h2>Leaderboards {{ year }}-Q{{ quarter }}</h2>
      <b-button v-if="royaleStandings && userRoyalePublisherID" variant="info" :to="{ name: 'royalePublisher', params: { publisherid: userRoyalePublisherID } }">View My Publisher</b-button>
    </div>

    <div class="row royale-leaderboard-row">
      <div class="col-xl-8 col-lg-12">
        <div v-if="royaleStandings">
          <b-table striped bordered responsive small :items="royaleStandings" :fields="standingsFields" :per-page="perPage" :current-page="currentPage">
            <template #cell(ranking)="data">
              <template v-if="data.item.ranking">
                {{ data.item.ranking }}
              </template>
              <template v-else>--</template>
            </template>
            <template #cell(publisherName)="data">
              <router-link :to="{ name: 'royalePublisher', params: { publisherid: data.item.publisherID } }">
                {{ data.item.publisherName }}
              </router-link>
            </template>
            <template #cell(playerName)="data">
              {{ data.item.playerName }}
              <font-awesome-icon v-if="data.item.previousQuarterWinner" v-b-popover.hover.focus="'Reigning Champion'" icon="crown" class="previous-quarter-winner" />
              <font-awesome-icon v-if="data.item.oneTimeWinner && !data.item.previousQuarterWinner" v-b-popover.hover.focus="'Previous Champion'" icon="crown" class="onetime-winner" />
            </template>
          </b-table>
          <b-pagination v-model="currentPage" class="pagination-dark" :total-rows="rows" :per-page="perPage" aria-controls="my-table"></b-pagination>
        </div>
        <div v-else class="spinner">
          <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
        </div>
      </div>

      <div class="col-xl-4 col-lg-12">
        <RoyaleGroupsWidget
          :my-groups="myGroups"
          :rules-based-groups="rulesBasedGroups"
          :year="year"
          :quarter="quarter"
          :group-search-query="groupSearchQuery"
          :group-search-results="groupSearchResults"
          @search-query-change="onGroupSearchQueryChange" />
      </div>

      <div v-if="showTopRankedChart" class="col-lg-12 top-publishers-section">
        <h3>Top 10 Publisher Statistics</h3>
        <div class="royale-chart-container">
          <RoyalePublisherGraph chart-id="quarter-points-chart" :chart-height="650" :royale-publishers="topPublishers" />
        </div>
      </div>
    </div>

    <b-modal id="createRoyaleGroupModal" ref="createRoyaleGroupModalRef" title="Create Royale Group" @ok="createGroup">
      <div class="form-group">
        <label for="newGroupName">Group Name</label>
        <b-form-input id="newGroupName" v-model="newGroupName" placeholder="Enter a group name"></b-form-input>
      </div>
    </b-modal>

    <CriticsRoyaleInfo class="critics-royale-info" />
  </div>
</template>

<script>
import axios from 'axios';
import CreateRoyalePublisherForm from '@/components/modals/createRoyalePublisherForm.vue';
import RoyaleGroupsWidget from '@/components/royaleGroupsWidget.vue';
import RoyalePublisherGraph from '@/components/royalePublisherGraph.vue';
import CriticsRoyaleInfo from '@/components/criticsRoyaleInfo.vue';
import { orderBy } from '@/globalFunctions';

export default {
  components: {
    CreateRoyalePublisherForm,
    RoyaleGroupsWidget,
    RoyalePublisherGraph,
    CriticsRoyaleInfo
  },
  props: {
    year: { type: Number, default: null },
    quarter: { type: Number, default: null }
  },
  data() {
    return {
      perPage: 10,
      currentPage: 1,
      selectedYear: null,
      userRoyalePublisherID: null,
      royaleYearQuarter: null,
      royaleYearQuarterOptions: null,
      royaleStandings: null,
      topPublishers: [],
      userPublisherBusy: true,
      groupSearchQuery: '',
      groupSearchResults: null,
      myGroups: null,
      rulesBasedGroups: null,
      newGroupName: '',
      groupSearchTimeout: null,
      standingsFields: [
        { key: 'ranking', label: 'Rank', thClass: ['bg-primary', 'ranking-column'], tdClass: 'ranking-column' },
        { key: 'publisherName', label: 'Publisher', thClass: 'bg-primary' },
        { key: 'playerName', label: 'Player Name', thClass: 'bg-primary' },
        { key: 'totalFantasyPoints', label: 'Total Points', thClass: 'bg-primary' }
      ],
      topRankedChartCanvasWidth: 1200,
      topRankedChartCanvasHeight: 600,
      topRankedChartCanvasStyles: {
        width: '100%',
        height: '100%',
        position: 'relative'
      }
    };
  },
  computed: {
    rows() {
      return this.royaleStandings.length;
    },
    years() {
      const yearsList = this.royaleYearQuarterOptions.map((x) => x.year);

      const uniqueYearsList = [...new Set(yearsList)];
      const sortedYears = orderBy(uniqueYearsList, (x) => x);
      return sortedYears;
    },
    quartersInSelectedYear() {
      const matchingQuarters = this.royaleYearQuarterOptions
        .filter((entry) => entry.year === this.selectedYear) // Filter objects with the specified 'year'
        .sort((a, b) => a.quarter - b.quarter); // Sort the quarters numerically

      return matchingQuarters;
    },
    showTopRankedChart() {
      return true;
    }
  },
  watch: {
    async $route() {
      await this.initializePage();
    }
  },
  async created() {
    await this.initializePage();
  },
  methods: {
    async initializePage() {
      if (!this.year || !this.quarter) {
        await this.fetchRoyaleQuarters();
        const mostRecentQuarter = this.royaleYearQuarterOptions[this.royaleYearQuarterOptions.length - 1];
        const parameters = {
          year: mostRecentQuarter.year.toString(),
          quarter: mostRecentQuarter.quarter.toString()
        };
        this.$router.replace({ params: parameters });
        return;
      }

      this.selectedYear = this.year;
      await this.fetchRoyaleData();
    },
    selectYear(year) {
      this.selectedYear = year;
    },
    async fetchRoyaleQuarters() {
      const response = await axios.get('/api/Royale/RoyaleQuarters');
      this.royaleYearQuarterOptions = response.data;
    },
    async fetchRoyaleData() {
      const response = await axios.get(`/api/Royale/RoyaleData/${this.year}/${this.quarter}`);
      this.royaleYearQuarterOptions = response.data.royaleYearQuarters;
      this.royaleYearQuarter = response.data.royaleYearQuarter;
      this.royaleStandings = response.data.royaleStandings;
      this.userRoyalePublisherID = response.data.userRoyalePublisherID;
      this.topPublishers = response.data.topPublishers;
      this.userPublisherBusy = false;
      await Promise.all([this.fetchMyGroups(), this.fetchRulesBasedGroups()]);
    },
    async fetchMyGroups() {
      if (!this.isAuth) return;
      try {
        const response = await axios.get('/api/RoyaleGroup/GetGroupsForUser');
        this.myGroups = response.data;
      } catch {
        this.myGroups = [];
      }
    },
    async fetchRulesBasedGroups() {
      try {
        const response = await axios.get('/api/RoyaleGroup/GetRulesBasedGroups');
        this.rulesBasedGroups = response.data;
      } catch {
        this.rulesBasedGroups = [];
      }
    },
    searchGroups() {
      if (this.groupSearchTimeout) {
        clearTimeout(this.groupSearchTimeout);
      }
      if (!this.groupSearchQuery || this.groupSearchQuery.length < 2) {
        this.groupSearchResults = null;
        return;
      }
      this.groupSearchTimeout = setTimeout(async () => {
        try {
          const response = await axios.get('/api/RoyaleGroup/SearchRoyaleGroups', { params: { query: this.groupSearchQuery } });
          this.groupSearchResults = response.data;
        } catch {
          this.groupSearchResults = [];
        }
      }, 300);
    },
    async createGroup(bvModalEvent) {
      bvModalEvent.preventDefault();
      if (!this.newGroupName || !this.newGroupName.trim()) return;
      try {
        const response = await axios.post('/api/RoyaleGroup/CreateManualRoyaleGroup', { groupName: this.newGroupName.trim() });
        this.$refs.createRoyaleGroupModalRef.hide();
        this.newGroupName = '';
        this.$router.push({ name: 'royaleGroup', params: { groupid: response.data.groupID } });
      } catch (error) {
        const message = error.response?.data || 'Failed to create group.';
        this.$bvToast.toast(message, { variant: 'danger', solid: true });
      }
    },
    onGroupSearchQueryChange(value) {
      this.groupSearchQuery = value;
      this.searchGroups();
    }
  }
};
</script>
<style scoped>
.leaderboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  margin-bottom: 5px;
}

.spinner {
  display: flex;
  justify-content: space-around;
}

.critic-royale-header-area {
  margin-top: 10px;
  margin-bottom: 10px;
  display: flex;
  justify-content: center;
  margin-right: 25%;
  margin-left: 25%;
  border-radius: 5px;
}

.critic-royale-header {
  height: 220px;
}

@media only screen and (max-width: 1000px) {
  .critic-royale-header-area {
    display: none;
  }
}

@media only screen and (min-width: 1001px) {
  .critic-royale-header-area-simple {
    display: none;
  }
}

.royale-leaderboard-row {
  margin-top: 5px;
}

div >>> div.card {
  background: rgba(50, 50, 50, 0.8);
}

div >>> .tab-header {
  margin-bottom: 5px;
}

div >>> .tab-header a {
  border-radius: 0px;
  font-weight: bolder;
  color: white;
}

.login-button {
  margin-left: 15px;
}

.quarter-selection {
  float: right;
}

.previous-quarter-winner {
  margin-left: 4px;
  color: #d6993a;
}

.onetime-winner {
  margin-left: 4px;
  color: white;
}

.royale-chart-container {
  height: 625px;
  background: #252525;
  border: 1px solid rgba(214, 153, 58, 0.35);
  border-radius: 8px;
  margin-top: 10px;
}

.top-publishers-section {
  margin-top: 20px;
}

.critics-royale-info {
  margin-top: 20px;
}
</style>
<style>
.ranking-column {
  width: 50px;
  text-align: right;
}
</style>
