<template>
  <div class="container-fluid">
    <span class="full-toggle-section">
      <span v-if="leagueYear.settings.hasSpecialSlots" class="single-toggle-section">
        <label>View Mode</label>
        <toggleSwitch v-model="editableDraftOrderView" class="toggle" :labels="{ checked: 'Draft Order', unchecked: 'Slot Order' }" :width="107" />
      </span>

      <span class="single-toggle-section">
        <label>Show Projections</label>
        <toggleSwitch v-model="editableShowProjections" class="toggle" :labels="{ checked: 'On', unchecked: 'Off' }" />
      </span>
    </span>
    <div class="row league-summary">
      <div v-for="publisher in publishers" :key="publisher.publisherID" class="col-xl-6 col-lg-12">
        <a :id="publisher.publisherID" />
        <minimalPlayerGameTable :publisher="publisher" :league-year="leagueYear"></minimalPlayerGameTable>
      </div>
    </div>
  </div>
</template>
<script>
import { mapState } from 'pinia';

import { useLeagueStore } from '@/store/leagueStore';
import MinimalPlayerGameTable from '@/components/gameTables/minimalPlayerGameTable.vue';
import ToggleSwitch from '@/components/toggleSwitch.vue';

export default {
  components: {
    MinimalPlayerGameTable,
    ToggleSwitch
  },
  computed: {
    ...mapState(useLeagueStore, ['leagueYear', 'showProjections', 'draftOrderView', 'publishers']),
    editableShowProjections: {
      get() {
        return this.showProjections;
      },
      set(value) {
        const leagueStore = useLeagueStore();
        leagueStore.setShowProjections(value);
      }
    },
    editableDraftOrderView: {
      get() {
        return this.draftOrderView;
      },
      set(value) {
        const leagueStore = useLeagueStore();
        leagueStore.setDraftOrderView(value);
      }
    }
  }
};
</script>
<style scoped>
.full-toggle-section {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.single-toggle-section {
  display: flex;
  gap: 3px;
}
</style>
