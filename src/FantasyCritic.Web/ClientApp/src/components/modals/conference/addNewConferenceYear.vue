<template>
  <b-modal id="addNewConferenceYear" ref="addNewConferenceYearRef" title="Add New Conference Year" size="lg" @hidden="clearData">
    <div class="alert alert-info">Add New Conference Year</div>

    <b-alert variant="danger" :show="!!errorInfo">{{ errorInfo }}</b-alert>

    <template #modal-footer>
      <input type="submit" class="btn btn-primary" value="Add New Conference Year" @click="addNewYear" />
    </template>
  </b-modal>
</template>
<script>
import axios from 'axios';
import ConferenceMixin from '@/mixins/conferenceMixin.js';

export default {
  mixins: [ConferenceMixin],
  data() {
    return {
      newYear: null,
      errorInfo: null
    };
  },
  mounted() {
    this.clearData();
  },
  methods: {
    async addNewYear() {
      const model = {
        conferenceID: this.conference.conferenceID,
        year: this.newYear
      };

      try {
        await axios.post('/api/conference/AddNewConferenceYear', model);
        this.$refs.addNewConferenceYearRef.hide();
        await this.notifyAction('A new year has been added.');
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    clearData() {
      this.newYear = null;
      this.errorInfo = null;
    }
  }
};
</script>
