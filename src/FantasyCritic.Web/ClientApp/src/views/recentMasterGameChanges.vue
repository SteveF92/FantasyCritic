<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Recent Master Game Changes</h1>

    <b-table small bordered striped responsive :items="recentChanges" :fields="gameFields">
      <template #cell(masterGame.gameName)="data">
        <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
      </template>
      <template #cell(timestamp)="data">
        {{ data.item.change.timestamp | longDate }}
      </template>
      <template #cell(description)="data">
        {{ data.item.change.description }}
      </template>
      <template #cell(changedByUser)="data">
        {{ data.item.change.changedByUser.displayName }}
      </template>
    </b-table>
  </div>
</template>

<script>
import axios from 'axios';
import MasterGamePopover from '@/components/masterGamePopover.vue';

export default {
  components: {
    MasterGamePopover
  },
  data() {
    return {
      recentChanges: null,
      gameFields: [
        { key: 'masterGame.gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'timestamp', label: 'Date of Change', sortable: true, thClass: 'bg-primary' },
        { key: 'description', label: 'Description', thClass: 'bg-primary' }
        //{ key: 'changedByUser', label: 'Changed by', thClass: 'bg-primary' }
      ]
    };
  },
  async created() {
    const response = await axios.get('/api/game/GetRecentMasterGameChanges');
    this.recentChanges = response.data;
  }
};
</script>
