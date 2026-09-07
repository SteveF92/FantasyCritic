<template>
  <div>
    <h3 v-if="mode === 'dryRunPage'">
      <router-link :to="{ name: 'league', params: { leagueid: leagueActionSet.leagueID, year: leagueActionSet.year } }">{{ leagueActionSet.leagueName }} - {{ leagueActionSet.year }}</router-link>
    </h3>
    <div v-if="leagueActionSet.drops.length > 0">
      <h4>Drops</h4>
      <b-table :items="leagueActionSet.drops" :fields="dropFields" bordered striped responsive class="action-table">
        <template #cell(publisherName)="data">
          <router-link :to="{ name: 'publisher', params: { publisherid: data.item.publisherID } }">
            {{ data.item.publisherName }}
          </router-link>
        </template>
        <template #cell(timestamp)="data">
          {{ data.item.timestamp | dateTime }}
        </template>
        <template #cell(masterGame)="data">
          <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
        </template>
        <template #cell(successful)="data">
          {{ data.item.successful | yesNo }}
        </template>
      </b-table>
    </div>
    <div v-if="leagueActionSet.bids.length > 0">
      <h4>Bids</h4>
      <b-table :items="leagueActionSet.bids" :fields="bidFields" bordered striped responsive class="action-table">
        <template #cell(publisherName)="data">
          <router-link :to="{ name: 'publisher', params: { publisherid: data.item.publisherID } }">
            {{ data.item.publisherName }}
          </router-link>
        </template>
        <template #cell(timestamp)="data">
          {{ data.item.timestamp | dateTime }}
        </template>
        <template #cell(masterGame)="data">
          <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
          <span v-if="data.item.counterPick">(Counter Pick)</span>
        </template>
        <template #cell(counterPick)="data">
          {{ data.item.counterPick | yesNo }}
        </template>
        <template #cell(conditionalDropPublisherGame)="data">
          <masterGamePopover v-if="data.item.conditionalDropPublisherGame" :master-game="data.item.conditionalDropPublisherGame.masterGame"></masterGamePopover>
          <span v-else>None</span>
        </template>
        <template #cell(projectedPointsAtTimeOfBid)="data">
          {{ data.item.projectedPointsAtTimeOfBid | score(2) }}
        </template>
        <template #cell(successful)="data">
          {{ data.item.successful | yesNo }}
        </template>
      </b-table>
    </div>
  </div>
</template>
<script>
import MasterGamePopover from '@/components/masterGamePopover.vue';

export default {
  components: {
    MasterGamePopover
  },
  props: {
    leagueActionSet: { type: Object, required: true },
    mode: { type: String, required: true }
  },
  data() {
    return {
      dropFields: [
        { key: 'publisherName', label: 'Publisher', sortable: true, thClass: 'bg-primary' },
        { key: 'timestamp', label: 'Time Placed', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame', label: 'Game', sortable: true, thClass: 'bg-primary' },
        { key: 'successful', label: 'Successful', sortable: true, thClass: 'bg-primary' }
      ],
      bidFields: [
        { key: 'publisherName', label: 'Publisher', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame', label: 'Game', sortable: true, thClass: 'bg-primary' },
        { key: 'successful', label: 'Successful', sortable: true, thClass: 'bg-primary' },
        { key: 'bidAmount', label: 'Bid Amount', sortable: true, thClass: 'bg-primary' },
        { key: 'priority', label: 'Priority', sortable: true, thClass: 'bg-primary' },
        { key: 'timestamp', label: 'Time Placed', sortable: true, thClass: 'bg-primary' },
        { key: 'projectedPointsAtTimeOfBid', label: 'Projected Points (for tiebreaks)', sortable: true, thClass: 'bg-primary' },
        { key: 'conditionalDropPublisherGame', label: 'Conditional Drop', sortable: true, thClass: 'bg-primary' },
        { key: 'outcome', label: 'Outcome', sortable: true, thClass: 'bg-primary' }
      ],
      sortBy: 'timestamp',
      sortDesc: true
    };
  }
};
</script>
<style scoped>
.action-table {
  width: 100%;
}
</style>
