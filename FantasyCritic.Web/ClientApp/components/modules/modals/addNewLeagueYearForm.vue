<template>
  <div>
    <b-modal id="addNewLeagueYear" ref="addNewLeagueYearRef" title="Start new Year">
      <div class="form-horizontal">
        <div class="form-group">
          <label for="selectedYear" class="control-label">New Year to Play</label>
          <select class="form-control" v-model="selectedYear" id="selectedYear">
            <option v-for="possibleYear in availableYears" v-bind:value="possibleYear">{{ possibleYear }}</option>
          </select>
        </div>
      </div>
      <div slot="modal-footer">
        <input type="submit" class="btn btn-primary" value="Start New Year" v-on:click="addNewLeagueYear" :disabled="!selectedYear" />
      </div>
    </b-modal>
  </div>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    data() {
      return {
        availableYears: [],
        selectedYear: "",
        error: ""
      }
    },
    props: ['league', 'isManager'],
    methods: {
      addNewLeagueYear() {
        var model = {
          leagueID: this.league.leagueID,
          year: this.selectedYear
        };
        axios
          .post('/api/leagueManager/AddNewLeagueYear', model)
          .then(response => {
            this.$refs.addNewLeagueYearRef.hide();
            this.$emit('newYearAdded');
          })
          .catch(response => {
            this.error = response;
          });
      }
    },
    mounted() {
      if (!this.isManager) {
        return;
      }
      axios
        .get('/api/LeagueManager/AvailableYears/' + this.league.leagueID)
        .then(response => {
          this.availableYears = response.data;
        })
        .catch(returnedError => (this.error = returnedError));
    }
  }
</script>
<style scoped>

</style>
