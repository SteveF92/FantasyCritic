<template>
  <b-card :title="heading" class="homepage-section">
    <div class="row" v-if="upcomingGames && upcomingGames.length > 0">
      <b-table :sort-by.sync="sortBy"
               :sort-desc.sync="sortDesc"
               :items="upcomingGames"
               :fields="upcomingGamesFields"
               bordered
               striped
               small>
        <template slot="gameName" slot-scope="data">
          <masterGamePopover :masterGame="data.item"></masterGamePopover>
        </template>
        <template slot="sortableEstimatedReleaseDate" slot-scope="data">
          {{getReleaseDate(data.item)}}
        </template>
      </b-table>
    </div>
  </b-card>
</template>
<script>
  import moment from "moment";
  import MasterGamePopover from "components/modules/masterGamePopover";

  export default {
    props: ['upcomingGames', 'heading'],
    data() {
      return {
        sortBy: 'sortableEstimatedReleaseDate',
        sortDesc: false,
        upcomingGamesFields: [
          { key: 'gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
          { key: 'sortableEstimatedReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        ],
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
