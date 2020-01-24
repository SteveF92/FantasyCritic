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
            <th class="bg-primary">"Free" Droppable Games</th>
            <td v-show="!leagueYearOptions.unlimitedFreeDroppableGames">{{leagueYearOptions.freeDroppableGames}}</td>
            <td v-show="leagueYearOptions.unlimitedFreeDroppableGames">Unlimited</td>
          </tr>
          <tr>
            <th class="bg-primary">"Will not Release" Droppable Games</th>
            <td v-show="!leagueYearOptions.unlimitedWillNotReleaseDroppableGames">{{leagueYearOptions.willNotReleaseDroppableGames}}</td>
            <td v-show="leagueYearOptions.unlimitedWillNotReleaseDroppableGames">Unlimited</td>
          </tr>
          <tr>
            <th class="bg-primary">"Will Release" Droppable Games</th>
            <td v-show="!leagueYearOptions.unlimitedWillReleaseDroppableGames">{{leagueYearOptions.willReleaseDroppableGames}}</td>
            <td v-show="leagueYearOptions.unlimitedWillReleaseDroppableGames">Unlimited</td>
          </tr>
          <tr>
            <th class="bg-primary">Drop Only Drafted Games</th>
            <td>{{leagueYearOptions.dropOnlyDraftGames | yesNo }}</td>
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
            <th class="bg-primary">Allow Free to Play</th>
            <td>{{leagueYearOptions.allowFreeToPlay | yesNo}}</td>
          </tr>
          <tr>
            <th class="bg-primary">Allow Released Internationally</th>
            <td>{{leagueYearOptions.allowReleasedInternationally | yesNo}}</td>
          </tr>
          <tr>
            <th class="bg-primary">Allow Expansion Packs</th>
            <td>{{leagueYearOptions.allowExpansions | yesNo}}</td>
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
