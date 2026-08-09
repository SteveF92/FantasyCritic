<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Recent Master Game Changes</h1>

    <div class="mb-3">
      <b-button class="mr-2" :variant="mode === 'All' ? 'primary' : 'secondary'" :disabled="isBusy" @click="mode = 'All'">All</b-button>
      <b-button class="mr-2" :variant="mode === 'NewGames' ? 'primary' : 'secondary'" :disabled="isBusy" @click="mode = 'NewGames'">New games</b-button>
      <b-button :variant="mode === 'Changes' ? 'primary' : 'secondary'" :disabled="isBusy" @click="mode = 'Changes'">Changes</b-button>
    </div>

    <div v-if="showTable">
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

    <div v-else class="spinner">
      <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
    </div>
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
      mode: 'All',
      recentChanges: null,
      isBusy: true
    };
  },
  computed: {
    showTable() {
      return this.recentChanges && !this.isBusy;
    },
    gameFields() {
      const fields = [
        { key: 'masterGame.gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        {
          key: 'timestamp',
          label: this.mode === 'NewGames' ? 'Date Added' : 'Date of Change',
          sortable: true,
          thClass: 'bg-primary'
        }
      ];
      if (this.mode !== 'NewGames') {
        fields.push({ key: 'description', label: 'Description', thClass: 'bg-primary' });
      }
      return fields;
    }
  },
  watch: {
    mode() {
      this.fetchRecentChanges();
    }
  },
  async created() {
    await this.fetchRecentChanges();
  },
  methods: {
    async fetchRecentChanges() {
      this.isBusy = true;
      const response = await axios.get('/api/game/GetRecentMasterGameChanges', {
        params: { mode: this.mode }
      });
      this.recentChanges = response.data;
      this.isBusy = false;
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
