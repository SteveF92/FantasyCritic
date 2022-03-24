<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Action Processing Dry Run Results</h1>

      <div v-if="dryRunResults && dryRunResults.length === 0" class="alert alert-info">No actioned games.</div>
      <div v-if="dryRunResults && dryRunResults.length !== 0">
        <h2>Drops</h2>
        <b-button v-b-toggle.drop-collapse variant="primary">Toggle Collapse</b-button>
        <b-collapse id="drop-collapse" class="mt-2">
          <div class="row">
            <masterGamesTable :masterGames="dryRunResults.dropActions"></masterGamesTable>
          </div>
          <hr />
        </b-collapse>

        <h2>Bids</h2>
        <b-button v-b-toggle.bid-collapse variant="primary">Toggle Collapse</b-button>
        <b-collapse id="bid-collapse" class="mt-2">
          <div class="row">
            <masterGamesTable :masterGames="dryRunResults.pickupActions"></masterGamesTable>
          </div>
          <hr />
        </b-collapse>

        <h2>Actions</h2>
        <b-button v-b-toggle.action-collapse variant="primary">Toggle Collapse</b-button>
        <b-collapse id="action-collapse" class="mt-2">
          <div class="row">
            <div class="history-table">
              <b-table :sort-by.sync="sortBy" :sort-desc.sync="sortDesc" :items="dryRunResults.leagueActions" :fields="actionFields" bordered striped responsive>
                <template #cell(timestamp)="data">
                  {{ data.item.timestamp | dateTime }}
                </template>
                <template #cell(managerAction)="data">
                  {{ data.item.managerAction | yesNo }}
                </template>
              </b-table>
            </div>
          </div>
        </b-collapse>

        <h2>League Action Sets</h2>
        <b-button v-b-toggle.league-action-sets-collapse variant="primary">Toggle Collapse</b-button>
        <b-collapse id="league-action-sets-collapse" class="mt-2">
          <div v-for="leagueActionSet in dryRunResults.leagueActionSets" :key="`${leagueActionSet.leagueID}-${leagueActionSet.processSetID}`" class="row">
            <leagueActionSet :leagueActionSet="leagueActionSet" :mode="'dryRunPage'"></leagueActionSet>
          </div>
        </b-collapse>
      </div>

      <div v-else class="spinner">
        <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
      </div>
    </div>
  </div>
</template>
<script>
import axios from 'axios';
import MasterGamesTable from '@/components/gameTables/masterGamesTable';
import LeagueActionSet from '@/components/leagueActionSet';

export default {
  components: {
    MasterGamesTable,
    LeagueActionSet
  },
  data() {
    return {
      dryRunResults: null,
      actionFields: [
        { key: 'leagueName', label: 'League Name', sortable: true, thClass: 'bg-primary' },
        { key: 'publisherName', label: 'Publisher Name', sortable: true, thClass: 'bg-primary' },
        { key: 'timestamp', label: 'Timestamp', sortable: true, thClass: 'bg-primary' },
        { key: 'actionType', label: 'Action Type', sortable: true, thClass: 'bg-primary' },
        { key: 'description', label: 'Description', thClass: 'bg-primary' },
        { key: 'managerAction', label: 'Manager Action?', thClass: 'bg-primary' }
      ],
      sortBy: 'timestamp',
      sortDesc: true
    };
  },
  mounted() {
    this.fetchActionedGames();
  },
  methods: {
    fetchActionedGames() {
      axios
        .get('/api/admin/ActionProcessingDryRun')
        .then((response) => {
          this.dryRunResults = response.data;
        })
        .catch(() => {});
    }
  }
};
</script>
<style scoped>
.spinner {
  display: flex;
  justify-content: space-around;
}
</style>
