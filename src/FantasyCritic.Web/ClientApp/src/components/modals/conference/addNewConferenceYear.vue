<template>
  <b-modal id="addNewConferenceYear" ref="addNewConferenceYearRef" title="Add New Conference Year" size="lg" @show="fetchAvailableYears" @hidden="clearData">
    <div class="alert alert-info">
      Starting a new year will not impact your current year. You will always be able to access all years of your conference, and starting a new year does not end your current year.
      <br />
      <br />
      Starting the new year will renew your Primary League, and you will be able to choose your settings which will then be applied to any other leagues you renew.
    </div>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="selectedYear" class="control-label">New Year to Play</label>
        <b-form-select id="selectedYear" v-model="selectedYear" class="form-control">
          <template #first>
            <option value="">Select a Year</option>
          </template>
          <option v-for="possibleYear in availableYears" :key="possibleYear" :value="possibleYear">{{ possibleYear }}</option>
        </b-form-select>
      </div>
    </div>
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
      availableYears: [],
      selectedYear: '',
      errorInfo: null
    };
  },
  mounted() {
    this.clearData();
  },
  methods: {
    async fetchAvailableYears() {
      try {
        const response = await axios.get('/api/Conference/AvailableYears/' + this.conference.conferenceID);
        this.availableYears = response.data;
      } catch (error) {
        this.error = error.response.data;
      }
    },
    async addNewYear() {
      const model = {
        conferenceID: this.conference.conferenceID,
        year: this.selectedYear
      };

      try {
        await axios.post('/api/Conference/AddNewConferenceYear', model);
        this.$refs.addNewConferenceYearRef.hide();
        this.$router.push({ name: 'editLeague', params: { leagueid: this.conference.primaryLeague.leagueID, year: this.selectedYear } });
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
