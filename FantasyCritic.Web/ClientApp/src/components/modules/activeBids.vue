<template>
  <div>
    <div v-if="games && games.length > 0">
      <b-table :sort-by.sync="sortBy"
               :sort-desc.sync="sortDesc"
               :items="games"
               :fields="gamesFields"
               bordered
               striped
               responsive
               small>
        <template v-slot:cell(gameName)="data">
          <masterGamePopover :masterGame="data.item"></masterGamePopover>
        </template>
        <template v-slot:cell(maximumReleaseDate)="data">
          {{getReleaseDate(data.item)}}
        </template>
      </b-table>
    </div>
    <div v-else>
      <div class="alert alert-info" role="alert">
        There are no games being bid upon this week.
      </div>
    </div>
  </div>
</template>
<script>
  import moment from 'moment';
  import MasterGamePopover from '@/components/modules/masterGamePopover';

  export default {
    props: ['games'],
    data() {
      return {
        sortBy: 'maximumReleaseDate',
        sortDesc: false,
        gamesFields: [
          { key: 'gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
          { key: 'maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        ]
      };
    },
    components: {
      MasterGamePopover,
    },
    methods: {
      getReleaseDate(game) {
        if (game.releaseDate) {
          return moment(game.releaseDate).format('YYYY-MM-DD');
        }
        return game.estimatedReleaseDate + ' (Estimated)';
      }
    }
  };
</script>
