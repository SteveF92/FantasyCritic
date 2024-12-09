<template>
  <b-modal id="renewLeagueForNewYear" ref="renewLeagueForNewYearRef" title="Renew League for New Year" size="lg" @hidden="clearData">
    <div class="alert alert-info">Renew League</div>

    <b-alert variant="danger" :show="!!errorInfo">{{ errorInfo }}</b-alert>

    <template #modal-footer>
      <input type="submit" class="btn btn-primary" value="Renew League" @click="renewLeague" />
    </template>
  </b-modal>
</template>
<script>
//import axios from 'axios';
import ConferenceMixin from '@/mixins/conferenceMixin.js';

export default {
  mixins: [ConferenceMixin],
  data() {
    return {
      errorInfo: null
    };
  },
  mounted() {
    this.clearData();
  },
  methods: {
    async addNewLeague() {},
    clearData() {
      this.errorInfo = null;
    }
  }
};
</script>
