<template>
  <div v-if="interLeagueDataLoaded">
    <div class="col-lg-10 offset-lg-1 col-md-12">
      <div class="row">
        <div class="header-row">
          <div class="header-flex">
            <div class="page-name">
              <h1 class="header">Master Games List</h1>
            </div>

            <div class="selector-area">
              <b-form-select v-model="selectedYear" :options="selectableYears" class="year-selector" @change="changeYear" />
            </div>
          </div>
        </div>
      </div>
      <hr />
      <div class="row">
        <div class="col-12 col-lg-3">
          <b-button v-if="isAuth" variant="info" :to="{ name: 'masterGameRequest' }" class="nav-link request-button">Request new Master Game</b-button>
        </div>
        <div class="col-12 col-lg-3">
          <b-button v-if="isAuth" variant="info" :to="{ name: 'masterGameChangeRequest' }" class="nav-link request-button">Suggest a Correction</b-button>
        </div>
        <div class="col-12 col-lg-3">
          <b-button variant="info" :to="{ name: 'gameChanges' }" class="nav-link request-button">See Recent Game Changes</b-button>
        </div>
        <div class="col-12 col-lg-3">
          <b-button variant="info" :to="{ name: 'topBidsAndDrops' }" class="nav-link request-button">See Top Bids/Drops</b-button>
        </div>
      </div>

      <div class="special-selectors text-well">
        <div v-if="isPlusUser">
          <label class="league-label">View Games for League</label>
          <b-form-select v-model="selectedLeague" class="league-selector" @change="fetchGamesForYear">
            <template #first>
              <option value="">-- none --</option>
            </template>
            <option v-for="league in myLeaguesForYear" :key="league.leagueID" :value="league">
              {{ league.leagueName }}
            </option>
          </b-form-select>
          <br />
          <b-button v-show="selectedLeague && (eligibilityFilter || takenStatusFilter)" variant="info" class="clear-league-filter-button" @click="clearLeagueFilter">Clear League Filter</b-button>
          <b-form-group v-show="selectedLeague">
            <b-form-radio v-model="eligibilityFilter" value="eligibleOnly">Show only eligible games</b-form-radio>
            <b-form-radio v-model="eligibilityFilter" value="eligibleInOpenSlotOnly">Show only games eligible in open slot</b-form-radio>
            <b-form-radio v-model="eligibilityFilter" value="ineligibleOnly">Show only ineligible games</b-form-radio>
          </b-form-group>
          <b-form-group v-show="selectedLeague">
            <b-form-radio v-model="takenStatusFilter" value="taken">Show only games currently taken</b-form-radio>
            <b-form-radio v-model="takenStatusFilter" value="notTaken">Show only games not currently taken</b-form-radio>
          </b-form-group>
        </div>
        <b-form-checkbox v-model="unreleasedOnlyFilter">
          <span class="checkbox-label">Show only unreleased games</span>
        </b-form-checkbox>
      </div>

      <div v-if="showGames">
        <masterGamesTable :master-games="gamesToShow" :show-year-stats="selectedYearIsOpenForPlayOrFinished"></masterGamesTable>
      </div>

      <div v-else class="spinner">
        <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import _ from 'lodash';

import MasterGamesTable from '@/components/gameTables/masterGamesTable.vue';

export default {
  components: {
    MasterGamesTable
  },
  data() {
    return {
      isBusy: true,
      selectedYear: null,
      eligibilityFilter: null,
      takenStatusFilter: null,
      unreleasedOnlyFilter: false,
      flatMasterGameYears: null,
      possibleMasterGameYears: null,
      myLeaguesForYear: [],
      selectedLeague: ''
    };
  },
  computed: {
    selectableYears() {
      return this.supportedYears.map((x) => x.year);
    },
    gamesToShow() {
      if (this.flatMasterGameYears) {
        if (!this.unreleasedOnlyFilter) {
          return this.flatMasterGameYears;
        }

        return _.filter(this.flatMasterGameYears, { isReleased: false });
      }

      if (this.possibleMasterGameYears) {
        let filteredGames = this.possibleMasterGameYears;
        if (this.eligibilityFilter === 'eligibleOnly') {
          filteredGames = _.filter(filteredGames, { isEligible: true });
        } else if (this.eligibilityFilter === 'eligibleInOpenSlotOnly') {
          filteredGames = _.filter(filteredGames, { isEligibleInOpenSlot: true });
        } else if (this.eligibilityFilter === 'ineligibleOnly') {
          filteredGames = _.filter(filteredGames, { isEligible: false });
        }

        if (this.takenStatusFilter === 'taken') {
          filteredGames = _.filter(filteredGames, { taken: true });
        } else if (this.takenStatusFilter === 'notTaken') {
          filteredGames = _.filter(filteredGames, { taken: false });
        }

        if (this.unreleasedOnlyFilter) {
          filteredGames = _.filter(filteredGames, { isReleased: false });
        }

        let flattenedGames = filteredGames.map((v) => v.masterGame);
        return flattenedGames;
      }

      return [];
    },
    showGames() {
      return this.gamesToShow && !this.isBusy;
    },
    selectedYearIsOpenForPlayOrFinished() {
      const selectedYear = this.supportedYears.filter((x) => x.year === this.selectedYear)[0];
      return selectedYear.openForPlay || selectedYear.finished;
    }
  },
  async created() {
    this.selectedYear = this.supportedYears.filter((x) => x.openForPlay)[0].year;
    await Promise.all([this.fetchGamesForYear(), this.fetchMyLeaguesForYear()]);
  },
  methods: {
    async fetchGamesForYear() {
      this.isBusy = true;
      this.flatMasterGameYears = null;
      this.possibleMasterGameYears = null;

      if (!this.selectedLeague) {
        const response = await axios.get('/api/game/MasterGameYear/' + this.selectedYear);
        this.flatMasterGameYears = response.data;
      } else {
        const response = await axios.get(`/api/game/MasterGameYearInLeagueContext/${this.selectedYear}?leagueID=${this.selectedLeague.leagueID}`);
        this.possibleMasterGameYears = response.data;
      }

      this.isBusy = false;
    },
    async fetchMyLeaguesForYear() {
      const response = await axios.get('/api/League/MyLeagues?year=' + this.selectedYear);
      let allLeaguesForYear = response.data;
      this.myLeaguesForYear = _.filter(allLeaguesForYear, { testLeague: false, userIsFollowingLeague: false });
    },
    async changeYear() {
      await Promise.all([this.fetchGamesForYear(), this.fetchMyLeaguesForYear()]);
    },
    clearLeagueFilter() {
      this.eligibilityFilter = null;
      this.takenStatusFilter = null;
      this.unreleasedOnlyFilter = false;
    }
  }
};
</script>
<style scoped>
.header-row {
  width: 100%;
}

.header-flex {
  display: flex;
  justify-content: space-between;
  flex-wrap: wrap;
}

.selector-area {
  display: flex;
  align-items: flex-start;
}

.year-selector {
  width: 100px;
}

.request-button {
  width: 100%;
  margin-bottom: 15px;
}

.league-label {
  margin-right: 10px;
}

.league-selector {
  width: 250px;
}

.special-selectors {
  margin: 10px;
  flex-wrap: wrap;
}

.special-selectors div {
  margin-right: 10px;
}

.spinner {
  display: flex;
  justify-content: space-around;
}

.clear-league-filter-button {
  margin-top: 10px;
  margin-bottom: 10px;
}
</style>
