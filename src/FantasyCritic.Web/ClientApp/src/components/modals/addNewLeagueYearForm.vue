<template>
  <div>
    <b-modal id="addNewLeagueYear" ref="addNewLeagueYearRef" title="Start new Year">
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
  </div>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      availableYears: [],
      selectedYear: '',
      error: ''
    };
  },
  mounted() {
    if (!this.isManager) {
      return;
    }
    axios
      .get('/api/LeagueManager/AvailableYears/' + this.league.leagueID)
      .then((response) => {
        this.availableYears = response.data;
      })
      .catch((returnedError) => (this.error = returnedError));
  },
  methods: {
    addNewLeagueYear() {
      var model = {
        leagueID: this.league.leagueID,
        year: this.selectedYear
      };
      axios
        .post('/api/leagueManager/AddNewLeagueYear', model)
        .then(() => {
          this.$refs.addNewLeagueYearRef.hide();
          this.$router.push({ name: 'editLeague', params: { leagueid: this.league.leagueID, year: this.selectedYear }, query: { freshSettings: true } });
        })
        .catch((response) => {
          this.error = response;
        });
    }
  }
};
</script>
