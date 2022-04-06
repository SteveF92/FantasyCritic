<template>
  <div class="container-fluid">
    <span v-show="leagueYear.hasSpecialSlots" class="full-toggle-section">
      <span class="single-toggle-section">
        <label c>View Mode</label>
        <toggle-button
          v-model="editableDraftOrderView"
          class="toggle"
          :sync="true"
          :labels="{ checked: 'Draft Order', unchecked: 'Slot Order' }"
          :css-colors="true"
          :font-size="13"
          :width="107"
          :height="28" />
      </span>

      <span class="single-toggle-section">
        <label>Show Projections</label>
        <toggle-button v-model="editableShowProjections" class="toggle" :sync="true" :labels="{ checked: 'On', unchecked: 'Off' }" :css-colors="true" :font-size="13" :width="60" :height="28" />
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
import MinimalPlayerGameTable from '@/components/gameTables/minimalPlayerGameTable';
import { ToggleButton } from 'vue-js-toggle-button';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  components: {
    MinimalPlayerGameTable,
    ToggleButton
  },
  mixins: [LeagueMixin],
  computed: {
    editableShowProjections: {
      get() {
        return this.showProjections;
      },
      set(value) {
        this.$store.commit('setShowProjections', value);
      }
    },
    editableDraftOrderView: {
      get() {
        return this.draftOrderView;
      },
      set(value) {
        this.$store.commit('setDraftOrderView', value);
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
