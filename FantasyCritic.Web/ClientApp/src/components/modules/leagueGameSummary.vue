<template>
  <div class="container-fluid">
    <div class="row league-summary">
      <div class="col-xl-6 col-lg-12" v-for="publisher in publishers">
        <a :name="publisher.publisherID"/>
        <minimalPlayerGameTable :publisher="publisher" :options="options" :yearFinished="leagueYear.supportedYear.finished"></minimalPlayerGameTable>
      </div>
    </div>
  </div>
</template>
<script>
import Vue from 'vue';
import MinimalPlayerGameTable from '@/components/modules/gameTables/minimalPlayerGameTable';

export default {
  components: {
    MinimalPlayerGameTable
  },
  props: ['leagueYear'],
  computed: {
    publishers() {
      return this.leagueYear.publishers;
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
