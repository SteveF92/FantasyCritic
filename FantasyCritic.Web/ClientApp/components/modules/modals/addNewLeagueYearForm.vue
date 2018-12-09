<template>
  <div>
    <b-modal id="addNewLeagueYear" ref="addNewLeagueYearRef" title="Start new Year">
      <div class="form-horizontal">
        <div class="form-group">
          <label for="selectedYear" class="control-label">Year to Play</label>
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
    props: ['league'],
    methods: {
      addNewLeagueYear() {

      }
    },
    mounted() {
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
