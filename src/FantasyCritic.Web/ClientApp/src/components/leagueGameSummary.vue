<template>
  <div class="container-fluid">
    <span v-show="leagueYear.hasSpecialSlots" class="view-mode-toggle-section">
      <label class="view-mode-label">View Mode</label>
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
    editableDraftOrderView: {
      get() {
        return this.draftOrderView;
      },
      set(value) {
        this.$store.commit('setDraftOrderView', value);
      }
    },
    options() {
      var options = {
        standardGameSlots: this.leagueYear.standardGames,
        counterPickSlots: this.leagueYear.counterPicks
      };

      return options;
    }
  }
};
</script>
<style scoped>
.view-mode-toggle-section {
  display: flex;
  justify-content: flex-end;
}
.view-mode-label {
  margin-right: 3px;
}
.view-mode-toggle {
  width: 100px;
}
</style>
