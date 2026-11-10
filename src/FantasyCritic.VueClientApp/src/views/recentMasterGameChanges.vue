<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Recent Master Game Changes</h1>

    <div class="mb-3">
      <b-button class="me-2" :variant="mode === RecentMasterGameChangeMode.All ? 'primary' : 'secondary'" :disabled="isBusy" @click="mode = RecentMasterGameChangeMode.All">All</b-button>
      <b-button class="me-2" :variant="mode === RecentMasterGameChangeMode.NewGames ? 'primary' : 'secondary'" :disabled="isBusy" @click="mode = RecentMasterGameChangeMode.NewGames">
        New games
      </b-button>
      <b-button :variant="mode === RecentMasterGameChangeMode.Changes ? 'primary' : 'secondary'" :disabled="isBusy" @click="mode = RecentMasterGameChangeMode.Changes">Changes</b-button>
    </div>

    <div v-if="recentChanges && !isBusy">
      <b-table small bordered striped responsive :items="recentChanges" :fields="gameFields">
        <template #cell(masterGame.gameName)="data">
          <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
        </template>
        <template #cell(timestamp)="data">
          {{ formatDateLong(data.item.change.timestamp) }}
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

<script lang="ts">
import { defineComponent } from 'vue';
import { TableField } from 'bootstrap-vue-next';

import { CompleteMasterGameChangeViewModel, gameClient, RecentMasterGameChangeMode } from '@/api/clients';
import { formatDateLong } from '@/utilities/formatters';
import MasterGamePopover from '@/components/masterGamePopover.vue';

interface RecentMasterGameChangesData {
  mode: RecentMasterGameChangeMode;
  recentChanges: CompleteMasterGameChangeViewModel[] | null;
  isBusy: boolean;
}

export default defineComponent({
  setup() {
    return { RecentMasterGameChangeMode };
  },
  components: {
    MasterGamePopover
  },
  data(): RecentMasterGameChangesData {
    return {
      mode: RecentMasterGameChangeMode.All,
      recentChanges: null,
      isBusy: true
    };
  },
  computed: {
    gameFields(): TableField<CompleteMasterGameChangeViewModel>[] {
      const fields: TableField<CompleteMasterGameChangeViewModel>[] = [
        { key: 'masterGame.gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        {
          key: 'timestamp',
          label: this.mode === RecentMasterGameChangeMode.NewGames ? 'Date Added' : 'Date of Change',
          sortable: true,
          thClass: 'bg-primary'
        }
      ];
      if (this.mode !== RecentMasterGameChangeMode.NewGames) {
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
    formatDateLong,
    async fetchRecentChanges() {
      this.isBusy = true;
      this.recentChanges = await gameClient.getRecentMasterGameChanges(this.mode);
      this.isBusy = false;
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
