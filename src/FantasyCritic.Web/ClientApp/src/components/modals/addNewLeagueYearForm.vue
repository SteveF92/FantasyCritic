<template>
  <b-modal id="addNewLeagueYear" ref="addNewLeagueYearRef" title="Start New Year" @show="fetchAvailableYears">
    <div class="alert alert-info">
      Starting a new year will not impact your current year. You will always be able to access all years of your league, and starting a new year does not end your current year.
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
      <input type="submit" class="btn btn-primary" value="Start New Year" :disabled="!selectedYear" @click="addNewLeagueYear" />
    </template>
  </b-modal>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      availableYears: [],
      selectedYear: '',
      error: ''
    };
  },
  methods: {
    async fetchAvailableYears() {
      if (!this.isManager) {
        return;
      }

      try {
        const response = await axios.get('/api/LeagueManager/AvailableYears/' + this.league.leagueID);
        this.availableYears = response.data;
      } catch (error) {
        this.error = error.response.data;
      }
    },
    async addNewLeagueYear() {
      const model = {
        leagueID: this.league.leagueID,
        year: this.selectedYear
      };

      try {
        await axios.post('/api/leagueManager/AddNewLeagueYear', model);
        this.$refs.addNewLeagueYearRef.hide();
        this.$router.push({ name: 'editLeague', params: { leagueid: this.league.leagueID, year: this.selectedYear }, query: { freshSettings: true } });
      } catch (error) {
        this.error = error.response.data;
      }
    }
  }
};
</script>
