<template>
  <div v-if="upcomingGames && upcomingGames.length > 0">
    <b-table :sort-by.sync="sortBy"
              :sort-desc.sync="sortDesc"
              :items="upcomingGames"
              :fields="upcomingGamesFields"
              bordered
              striped
              small>
      <template slot="gameName" slot-scope="data">
        <masterGamePopover :masterGame="data.item.masterGame"></masterGamePopover>
      </template>
      <template slot="sortableEstimatedReleaseDate" slot-scope="data">
        {{getReleaseDate(data.item)}}
      </template>
      <template slot="league" slot-scope="data">
        <router-link v-if="data.item.leagueID" :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: data.item.year }}">{{data.item.leagueName}}</router-link>
        <span v-else>{{data.item.leagueName}}</span>
      </template>
      <template slot="publisher" slot-scope="data">
        <span v-if="!data.item.counterPickPublisherID">
          <router-link :to="{ name: 'publisher', params: { publisherid: data.item.publisherID }}">{{ data.item.publisherName }}</router-link>
        </span>
        <span v-else>
          <router-link :to="{ name: 'publisher', params: { publisherid: data.item.publisherID }}">{{ data.item.publisherName }}</router-link>
          - Counter Picked by:
          <router-link :to="{ name: 'publisher', params: { publisherid: data.item.counterPickPublisherID }}">{{ data.item.counterPickPublisherName }}</router-link>
        </span>
      </template>
    </b-table>
  </div>
</template>
<script>
  import moment from "moment";
  import MasterGamePopover from "components/modules/masterGamePopover";

  export default {
    props: ['upcomingGames', 'mode'],
    data() {
      return {
        sortBy: 'sortableEstimatedReleaseDate',
        sortDesc: false,
        baseUpcomingGamesFields: [
          { key: 'gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
          { key: 'sortableEstimatedReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        ],
        userUpcomingGamesFields: [
          { key: 'league', label: 'League', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
        ],
        leagueUpcomingGamesFields: [
          { key: 'publisher', label: 'Publisher', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
        ],
      }
    },
    computed: {
      upcomingGamesFields() {
        if (this.mode === "user") {
          return this.baseUpcomingGamesFields.concat(this.userUpcomingGamesFields);
        } else if (this.mode === "league") {
          return this.baseUpcomingGamesFields.concat(this.leagueUpcomingGamesFields);
        }

        return this.baseUpcomingGamesFields;
      }
    },
    components: {
      MasterGamePopover,
    },
    methods: {
      getReleaseDate(game) {
        if (game.releaseDate) {
          return moment(game.releaseDate).format('YYYY-MM-DD');
        }
        return game.estimatedReleaseDate + ' (Estimated)'
      }
    }
  }
</script>
