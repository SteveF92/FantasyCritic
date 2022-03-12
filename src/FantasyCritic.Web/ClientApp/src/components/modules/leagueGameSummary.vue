<template>
  <div class="container-fluid">
    <span class="view-mode-toggle-section" v-show="leagueYear.hasSpecialSlots">
      <label class="view-mode-label">View Mode</label>
      <toggle-button class="toggle" v-model="draftOrderView" :sync="true" :labels="{ checked: 'Draft Order', unchecked: 'Slot Order' }" :css-colors="true" :font-size="13" :width="107" :height="28" />
    </span>
    <div class="row league-summary">
      <div class="col-xl-6 col-lg-12" v-for="publisher in publishers">
        <a :name="publisher.publisherID" />
        <minimalPlayerGameTable :publisher="publisher" :leagueYear="leagueYear"></minimalPlayerGameTable>
      </div>
    </div>
  </div>
</template>
<script>
import Vue from 'vue';
import MinimalPlayerGameTable from '@/components/modules/gameTables/minimalPlayerGameTable';
import { ToggleButton } from 'vue-js-toggle-button';

export default {
  components: {
    MinimalPlayerGameTable,
    ToggleButton
  },
  props: ['leagueYear'],
  computed: {
    publishers() {
      return this.leagueYear.publishers;
    },
    draftOrderView: {
      get() {
        return this.$store.getters.draftOrderView;
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
