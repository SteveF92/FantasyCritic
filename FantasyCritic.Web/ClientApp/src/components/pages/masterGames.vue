<template>
  <div>
    <div class="col-lg-10 offset-lg-1 col-md-12">
      <div class="row">
        <div class="header-row">
          <div class="header-flex">
            <div class="page-name">
              <h1 class="header">Master Games List</h1>
            </div>

            <div class="selector-area">
              <b-form-select v-model="selectedYear" :options="supportedYears" v-on:change="changeYear" class="year-selector" />
            </div>
          </div>
        </div>
      </div>
      <hr />
      <div class="row">
        <div class="col-12 col-lg-6">
          <b-button variant="info" :to="{ name: 'masterGameRequest' }" v-show="isAuth" class="nav-link request-button">Request new Master Game</b-button>
        </div>
        <div class="col-12 col-lg-6">
          <b-button variant="info" :to="{ name: 'masterGameChangeRequest' }" v-show="isAuth" class="nav-link request-button">Suggest a Correction</b-button>
        </div>
      </div>

      <div class="special-selectors text-well">
        <div v-if="isPlusUser">
          <label class="league-label">View Games for League</label>
          <b-form-select v-model="selectedLeague" v-on:change="fetchGamesForYear" class="league-selector">
            <template #first>
              <option value="">-- none --</option>
            </template>
            <option v-for="league in myLeaguesForYear" v-bind:value="league">
              {{ league.leagueName }}
            </option>
          </b-form-select>
          <br />
          <b-button variant="info" v-show="selectedLeague && leagueFilterMode" v-on:click="clearLeagueFilter" class="clear-league-filter-button">Clear League Filter</b-button>
          <b-form-group v-show="selectedLeague">
            <b-form-radio v-model="leagueFilterMode" value="availableOnly">Show only available games</b-form-radio>
            <b-form-radio v-model="leagueFilterMode" value="eligibleOnly">Show only eligible games</b-form-radio>
            <b-form-radio v-model="leagueFilterMode" value="eligibleInOpenSlotOnly">Show only games eligible in open slot</b-form-radio>
            <b-form-radio v-model="leagueFilterMode" value="takenOnly">Show only taken games</b-form-radio>
          </b-form-group>
        </div>
        <b-form-checkbox v-model="unreleasedOnly">
          <span class="checkbox-label">Show only unreleased games</span>
        </b-form-checkbox>
      </div>
      
      <div v-if="showGames">
        <masterGamesTable :masterGames="gamesToShow"></masterGamesTable>
      </div>

      <div v-else class="spinner">
        <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
      </div>
    </div>
  </div>
</template>

<script>
import Vue from 'vue';
import axios from 'axios';
import moment from 'moment';
import MasterGamePopover from '@/components/modules/masterGamePopover';
import MasterGamesTable from '@/components/modules/gameTables/masterGamesTable';

export default {
  data() {
    return {
      isBusy: true,
      selectedYear: null,
      leagueFilterMode: null,
      availableOnly: false,
      eligibleOnly: false,
      eligibleInOpenSlotOnly: false,
      takenOnly: false,
      unreleasedOnly: false,
      supportedYears: [],
      flatMasterGameYears: null,
      possibleMasterGameYears: null,
      myLeaguesForYear: [],
      selectedLeague: ""
    };
  },
  components: {
    MasterGamePopover,
    MasterGamesTable
  },
  computed: {
    isAuth() {
      return this.$store.getters.isAuthenticated;
    },
    gamesToShow() {
      if (this.flatMasterGameYears) {
        if (!this.unreleasedOnly) {
          return this.flatMasterGameYears;
        }

        return _.filter(this.flatMasterGameYears, { 'isReleased': false });
      }

      if (this.possibleMasterGameYears) {
        let filteredGames = this.possibleMasterGameYears;
        if (this.leagueFilterMode === "availableOnly") {
          filteredGames = _.filter(filteredGames, { 'isAvailable': true })
        } else if (this.leagueFilterMode === "eligibleOnly") {
          filteredGames = _.filter(filteredGames, { 'isEligible': true })
        } else if (this.leagueFilterMode === "eligibleInOpenSlotOnly") {
          filteredGames = _.filter(filteredGames, { 'isEligibleInOpenSlot': true })
        } else if (this.leagueFilterMode === "takenOnly") {
          filteredGames = _.filter(filteredGames, { 'taken': true })
        }

        if (this.unreleasedOnly) {
          filteredGames = _.filter(filteredGames, { 'isReleased': false })
        }
        
        let flattenedGames = filteredGames.map(v => v.masterGame);
        return flattenedGames;
      }

      return [];
    },
    showGames() {
      return this.gamesToShow && !this.isBusy;
    },
    isPlusUser() {
      return this.$store.getters.isPlusUser;
    }
  },
  methods: {
    fetchSupportedYears() {
      axios
        .get('/api/game/SupportedYears')
        .then(response => {
          let supportedYears = response.data;
          let openYears = _.filter(supportedYears, { 'openForPlay': true });
          let finishedYears = _.filter(supportedYears, { 'finished': true });
          this.supportedYears = openYears.concat(finishedYears).map(function (v) {
            return v.year;
          });
          this.selectedYear = this.supportedYears[0];
          this.fetchGamesForYear();
          this.fetchMyLeaguesForYear();
        })
        .catch(response => {

        });
    },
    fetchGamesForYear() {
      this.isBusy = true;
      this.flatMasterGameYears = null;
      this.possibleMasterGameYears = null;

      if (!this.selectedLeague) {
        axios
          .get('/api/game/MasterGameYear/' + this.selectedYear)
          .then(response => {
            this.flatMasterGameYears = response.data;
            this.isBusy = false;
          })
          .catch(response => {

          });
      } else {
        axios
          .get(`/api/game/MasterGameYearInLeagueContext/${this.selectedYear}?leagueID=${this.selectedLeague.leagueID}`)
          .then(response => {
            this.possibleMasterGameYears = response.data;
            this.isBusy = false;
          })
          .catch(response => {

          });
      }
    },
    fetchMyLeaguesForYear() {
      axios
        .get('/api/League/MyLeagues?year=' + this.selectedYear)
        .then(response => {
          let allLeaguesForYear = response.data;
          this.myLeaguesForYear = _.filter(allLeaguesForYear, { 'testLeague': false })
        })
        .catch(response => {

        });
    },
    changeYear() {
      this.fetchGamesForYear();
      this.fetchMyLeaguesForYear();
    },
    clearLeagueFilter() {
      this.leagueFilterMode = null;
    }
  },
  mounted() {
    this.fetchSupportedYears();
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

  .selector-area{
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
