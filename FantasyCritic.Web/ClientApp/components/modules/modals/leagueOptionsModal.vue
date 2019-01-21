<template>
  <b-modal id="leagueOptionsModal" ref="leagueOptionsModalRef" title="League Options" hide-footer>
    <div v-if="league && leagueYear && leagueYearOptions">
      <table class="table table-responsive-sm table-bordered table-striped">
        <tbody>
          <tr>
            <th class="bg-primary">Total Standard Games</th>
            <td>{{leagueYearOptions.standardGames}}</td>
          </tr>
          <tr>
            <th class="bg-primary">Games to Draft</th>
            <td>{{leagueYearOptions.gamesToDraft}}</td>
          </tr>
          <tr>
            <th class="bg-primary">Pickup Games</th>
            <td>{{leagueYearOptions.standardGames - leagueYearOptions.gamesToDraft}}</td>
          </tr>
          <tr>
            <th class="bg-primary">Counter Picks</th>
            <td>{{leagueYearOptions.counterPicks}}</td>
          </tr>
          <tr>
            <th class="bg-primary">Maximum Eligibility Level</th>
            <td>{{leagueYearOptions.maximumEligibilityLevel}}</td>
          </tr>
          <tr>
            <th class="bg-primary">Allow Yearly Installments</th>
            <td>{{leagueYearOptions.allowYearlyInstallments | yesNo}}</td>
          </tr>
          <tr>
            <th class="bg-primary">Allow Early Access</th>
            <td>{{leagueYearOptions.allowEarlyAccess | yesNo}}</td>
          </tr>

          <tr>
            <th class="bg-primary">Public League</th>
            <td>{{league.publicLeague | yesNo}}</td>
          </tr>
          <tr>
            <th class="bg-primary">Test League</th>
            <td>{{league.testLeague | yesNo}}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </b-modal>
</template>

<script>
  import axios from "axios";

  export default {
    props: ['league', 'leagueYear'],
    data() {
      return {
        leagueYearOptions: null
      }
    },
    methods: {
      fetchLeagueYearOptions() {
        axios
          .get('/api/League/GetLeagueYearOptions?leagueID=' + this.league.leagueID + '&year=' + this.leagueYear.year)
          .then(response => {
            this.leagueYearOptions = response.data;
          })
          .catch(returnedError => (this.error = returnedError));
      }
    },
    mounted() {
      this.fetchLeagueYearOptions();
    }
  }
</script>
<style scoped>

</style>
