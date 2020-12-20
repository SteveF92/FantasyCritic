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
              <b-form-checkbox v-model="unreleasedOnly" class="unreleased-checkbox">
                <span class="checkbox-label">Show only unreleased games</span>
              </b-form-checkbox>
              <b-form-select v-model="selectedYear" :options="supportedYears" v-on:change="fetchGamesForYear" class="year-selector" />
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
      selectedYear: null,
      unreleasedOnly: false,
      supportedYears: [],
      gamesForYear: []
    };
  },
  components: {
    MasterGamePopover,
    MasterGamesTable
  },
  computed: {
    isAuth() {
      return this.$store.getters.tokenIsCurrent();
    },
    gamesToShow() {
      if (!this.unreleasedOnly) {
        return this.gamesForYear;
      }

      return _.filter(this.gamesForYear, { 'isReleased': false });
    },
    showGames() {
      return this.gamesToShow && this.gamesToShow.length > 0;
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
          this.fetchGamesForYear(this.selectedYear);
        })
        .catch(response => {

        });
    },
    fetchGamesForYear(year) {
      axios
        .get('/api/game/MasterGameYear/' + year)
        .then(response => {
          this.gamesForYear = response.data;
        })
        .catch(response => {

        });
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

  .unreleased-checkbox {
    margin-top: 8px;
    margin-right: 8px;
  }

  .spinner {
    display: flex;
    justify-content: space-around;
  }
</style>
