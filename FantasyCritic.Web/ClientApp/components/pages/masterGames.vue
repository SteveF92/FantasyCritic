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

      <div class="row" v-if="gamesToShow && gamesToShow.length > 0">
        <b-table :sort-by.sync="sortBy"
                 :sort-desc.sync="sortDesc"
                 :items="gamesToShow"
                 :fields="gameFields"
                 bordered
                 :small="tableIsSmall"
                 responsive
                 striped>
          <template slot="gameName" slot-scope="data">
            <masterGamePopover :masterGame="data.item"></masterGamePopover>
          </template>
          <template slot="sortableEstimatedReleaseDate" slot-scope="data">
            {{getReleaseDate(data.item)}}
          </template>
          <template slot="criticScore" slot-scope="data">
            <a v-if="data.item.openCriticID && data.item.criticScore" :href="openCriticLink(data.item)" target="_blank"><strong>OpenCritic <font-awesome-icon icon="external-link-alt" /></strong></a>
            <span v-else>--</span>
          </template>
          <template slot="dateAdjustedHypeFactor" slot-scope="data">
            {{data.item.dateAdjustedHypeFactor | score(1)}}
          </template>
          <template slot="eligiblePercentStandardGame" slot-scope="data">
            {{data.item.eligiblePercentStandardGame | percent(1)}}
          </template>
          <template slot="eligiblePercentCounterPick" slot-scope="data">
            {{data.item.eligiblePercentCounterPick | percent(1)}}
          </template>
          <template slot="addedTimestamp" slot-scope="data">
            {{data.item.addedTimestamp | date}}
          </template>
          <template slot="eligibilityLevel" slot-scope="data">
            <eligibilityBadge :eligibilityLevel="data.item.eligibilitySettings.eligibilityLevel" :maximumEligibilityLevel="maximumEligibilityLevel"></eligibilityBadge>
          </template>
        </b-table>
      </div>
    </div>
  </div>
</template>

<script>
  import Vue from 'vue';
  import axios from "axios";
  import moment from "moment";
  import MasterGamePopover from "components/modules/masterGamePopover";
  import EligibilityBadge from "components/modules/eligibilityBadge";

  export default {
    data() {
      return {
        selectedYear: null,
        unreleasedOnly: false,
        supportedYears: [],
        gamesForYear: [],
        gameFields: [
          { key: 'gameName', label: 'Name', sortable: true, thClass:'bg-primary' },
          { key: 'sortableEstimatedReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
          { key: 'criticScore', label: 'Critic Score Link', thClass: ['bg-primary','md-screen-minimum'], tdClass: 'md-screen-minimum' },
          { key: 'dateAdjustedHypeFactor', label: 'Hype Factor', sortable: true, thClass: 'bg-primary' },
          { key: 'eligiblePercentStandardGame', label: '% Picked', sortable: true, thClass: ['bg-primary','md-screen-minimum'], tdClass: 'md-screen-minimum' },
          { key: 'eligiblePercentCounterPick', label: '% Counter Picked', sortable: true, thClass: ['bg-primary','lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
          { key: 'eligibilityLevel', label: 'Eligibility Level', sortable: true, thClass: ['bg-primary','lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
          { key: 'addedTimestamp', label: 'Date Added', sortable: true, thClass: ['bg-primary','lg-screen-minimum'], tdClass: 'lg-screen-minimum' }
        ],
        sortBy: 'dateAdjustedHypeFactor',
        sortDesc: true
      }
    },
    components: {
      MasterGamePopover,
      EligibilityBadge
    },
    computed: {
      maximumEligibilityLevel() {
        let level = {
          level: 5
        };
        return level;
      },
      isAuth() {
        return this.$store.getters.tokenIsCurrent();
      },
      tableIsSmall() {
        if (window.innerWidth < 500) {
          return true;
        }

        return false;
      },
      gamesToShow() {
        if (!this.unreleasedOnly) {
          return this.gamesForYear;
        }

        return _.filter(this.gamesForYear, { 'isReleased': false });
      }
    },
    methods: {
      fetchSupportedYears() {
        axios
          .get('/api/game/SupportedYears')
          .then(response => {
            this.supportedYears = response.data;
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
      },
      getReleaseDate(game) {
        if (game.releaseDate) {
          return moment(game.releaseDate).format('YYYY-MM-DD');
        }
        return game.estimatedReleaseDate + ' (Estimated)'
      },
      openCriticLink(game) {
        return "https://opencritic.com/game/" + game.openCriticID + "/a";
      }
    },
    mounted() {
      this.fetchSupportedYears();
    }
  }
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
</style>
