<template>
  <b-table :sort-by.sync="sortBy"
           :sort-desc.sync="sortDesc"
           :items="gameRows"
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
    <template slot="projectedOrRealFantasyPoints" slot-scope="data">
      <template v-if="data.item.isReleased || !data.item.willRelease">
        {{data.item.projectedOrRealFantasyPoints | score(1)}}
      </template>
      <template v-else>
        <em>~{{data.item.projectedOrRealFantasyPoints | score(1)}}</em>
      </template>
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
</template>
<script>
  import Vue from 'vue';
  import axios from "axios";
  import moment from "moment";
  import MasterGamePopover from "components/modules/masterGamePopover";
  import EligibilityBadge from "components/modules/eligibilityBadge";
  import MasterGamesTable from "components/modules/gameTables/masterGamesTable";

  export default {
    props: ['masterGames'],
    data() {
      return {
        gameFields: [
          { key: 'gameName', label: 'Name', sortable: true, thClass: 'bg-primary', stickyColumn: true },
          { key: 'sortableEstimatedReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
          { key: 'criticScore', label: 'Critic Score Link', thClass: ['bg-primary', 'md-screen-minimum'], tdClass: 'md-screen-minimum' },
          { key: 'dateAdjustedHypeFactor', label: 'Hype Factor', sortable: true, thClass: 'bg-primary' },
          { key: 'projectedOrRealFantasyPoints', label: 'Points', sortable: true, thClass: 'bg-primary' },
          { key: 'eligiblePercentStandardGame', label: '% Picked', sortable: true, thClass: ['bg-primary', 'md-screen-minimum'], tdClass: 'md-screen-minimum' },
          { key: 'eligiblePercentCounterPick', label: '% Counter Picked', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
          { key: 'eligibilityLevel', label: 'Eligibility Level', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
          { key: 'addedTimestamp', label: 'Date Added', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' }
        ],
        sortBy: 'dateAdjustedHypeFactor',
        sortDesc: true
      }
    },
    components: {
      MasterGamePopover,
      EligibilityBadge,
      MasterGamesTable
    },
    computed: {
      maximumEligibilityLevel() {
        let level = {
          level: 5
        };
        return level;
      },
      tableIsSmall() {
        if (window.innerWidth < 500) {
          return true;
        }

        return false;
      },
      gameRows() {
        let gameRows = this.masterGames;
        if (!gameRows) {
            return [];
        }
        for (var i = 0; i < gameRows.length; ++i) {
          if (gameRows[i].error) {
            gameRows[i]._rowVariant = 'danger';
          }
        }
        return gameRows;
      }
    },
    methods: {
      getReleaseDate(game) {
        if (game.releaseDate) {
          return moment(game.releaseDate).format('YYYY-MM-DD');
        }
        return game.estimatedReleaseDate + ' (Estimated)'
      },
      openCriticLink(game) {
        return "https://opencritic.com/game/" + game.openCriticID + "/a";
      }
    }
  }
</script>
