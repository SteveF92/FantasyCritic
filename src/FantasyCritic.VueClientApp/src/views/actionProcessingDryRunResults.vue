<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Action Processing Dry Run Results</h1>

    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div v-else-if="dryRunResults">
      <h2>Drops</h2>
      <b-button v-b-toggle.drop-collapse variant="primary">Toggle Collapse</b-button>
      <b-collapse id="drop-collapse" class="mt-2">
        <b-alert v-if="dryRunResults.dropActions.length === 0" variant="info" :model-value="true">No games dropped.</b-alert>
        <b-table v-else :items="dryRunResults.dropActions" :fields="gameFields" primary-key="masterGameID" :tbody-tr-class="gameRowClass" bordered striped small responsive>
          <template #cell(gameName)="data">
            <masterGamePopover :master-game="data.item"></masterGamePopover>
          </template>
          <template #cell(maximumReleaseDate)="data">
            {{ formatMasterGameReleaseDate(data.item, false) }}
          </template>
          <template #cell(tags)="data">
            <span v-for="tag in data.item.tags" :key="tag">
              <masterGameTagBadge :tag-name="tag" short></masterGameTagBadge>
            </span>
          </template>
        </b-table>
        <hr />
      </b-collapse>

      <h2>Bids</h2>
      <b-button v-b-toggle.bid-collapse variant="primary">Toggle Collapse</b-button>
      <b-collapse id="bid-collapse" class="mt-2">
        <b-alert v-if="dryRunResults.pickupActions.length === 0" variant="info" :model-value="true">No games picked up.</b-alert>
        <b-table v-else :items="dryRunResults.pickupActions" :fields="gameFields" primary-key="masterGameID" :tbody-tr-class="gameRowClass" bordered striped small responsive>
          <template #cell(gameName)="data">
            <masterGamePopover :master-game="data.item"></masterGamePopover>
          </template>
          <template #cell(maximumReleaseDate)="data">
            {{ formatMasterGameReleaseDate(data.item, false) }}
          </template>
          <template #cell(tags)="data">
            <span v-for="tag in data.item.tags" :key="tag">
              <masterGameTagBadge :tag-name="tag" short></masterGameTagBadge>
            </span>
          </template>
        </b-table>
        <hr />
      </b-collapse>

      <h2>Actions</h2>
      <b-button v-b-toggle.action-collapse variant="primary">Toggle Collapse</b-button>
      <b-collapse id="action-collapse" class="mt-2">
        <div class="row">
          <div class="history-table">
            <b-table v-model:sort-by="sortBy" :items="dryRunResults.leagueActions" :fields="actionFields" bordered striped responsive>
              <template #cell(timestamp)="data">
                {{ formatDateTime(data.item.timestamp) }}
              </template>
              <template #cell(managerAction)="data">
                {{ formatYesNo(data.item.managerAction) }}
              </template>
            </b-table>
          </div>
        </div>
      </b-collapse>

      <h2>League Action Sets</h2>
      <b-button v-b-toggle.league-action-sets-collapse variant="primary">Toggle Collapse</b-button>
      <b-collapse id="league-action-sets-collapse" class="mt-2">
        <div v-for="leagueActionSet in dryRunResults.leagueActionSets" :key="`${leagueActionSet.leagueID}-${leagueActionSet.processSetID}`" class="row">
          <leagueActionSet :league-action-set="leagueActionSet" mode="dryRunPage"></leagueActionSet>
        </div>
      </b-collapse>
    </div>

    <div v-else class="spinner">
      <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
    </div>
  </div>
</template>
<script lang="ts">
import { defineComponent } from 'vue';
import type { BTableSortBy, TableField, TableRowType, TableStrictClassValue } from 'bootstrap-vue-next';

import { actionRunnerClient, type ActionedGameSetViewModel, type LeagueActionViewModel, type MasterGameViewModel } from '@/api/clients';
import { formatMasterGameReleaseDate } from '@/globalFunctions';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatDateTime, formatYesNo } from '@/utilities/formatters';
import LeagueActionSet from '@/components/leagueActionSet.vue';
import MasterGamePopover from '@/components/masterGamePopover.vue';
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

interface ActionProcessingDryRunResultsData {
  dryRunResults: ActionedGameSetViewModel | null;
  errorInfo: string;
  sortBy: BTableSortBy[];
}

export default defineComponent({
  setup() {
    const gameFields: TableField<MasterGameViewModel>[] = [
      { key: 'gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
      { key: 'maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
      { key: 'tags', label: 'Tags', thClass: 'bg-primary' }
    ];
    const actionFields: TableField<LeagueActionViewModel>[] = [
      { key: 'leagueName', label: 'League Name', sortable: true, thClass: 'bg-primary' },
      { key: 'publisherName', label: 'Publisher Name', sortable: true, thClass: 'bg-primary' },
      { key: 'timestamp', label: 'Timestamp', sortable: true, thClass: 'bg-primary' },
      { key: 'actionType', label: 'Action Type', sortable: true, thClass: 'bg-primary' },
      { key: 'description', label: 'Description', thClass: 'bg-primary' },
      { key: 'managerAction', label: 'Manager Action?', thClass: 'bg-primary' }
    ];

    return { gameFields, actionFields };
  },
  components: {
    LeagueActionSet,
    MasterGamePopover,
    MasterGameTagBadge
  },
  data(): ActionProcessingDryRunResultsData {
    return {
      dryRunResults: null,
      errorInfo: '',
      sortBy: [{ key: 'timestamp', order: 'desc' }]
    };
  },
  async created() {
    try {
      this.dryRunResults = await actionRunnerClient.actionProcessingDryRun();
    } catch (error) {
      this.errorInfo = getApiErrorMessage(error, 'Something went wrong with the dry run.');
    }
  },
  methods: {
    formatDateTime,
    formatYesNo,
    formatMasterGameReleaseDate,
    //A game the dry run couldn't process is red.
    gameRowClass(item: MasterGameViewModel | null, type: TableRowType): TableStrictClassValue {
      if (!item || type !== 'row' || !item.error) {
        return '';
      }

      return 'table-danger';
    }
  }
});
</script>
<style scoped>
.spinner {
  display: flex;
  justify-content: space-around;
}
</style>
