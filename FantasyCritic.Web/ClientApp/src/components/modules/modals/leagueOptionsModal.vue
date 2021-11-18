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
            <th class="bg-primary">Total Counter Picks</th>
            <td>{{leagueYearOptions.counterPicks}}</td>
          </tr>
          <tr>
            <th class="bg-primary">Counter Picks to Draft</th>
            <td>{{leagueYearOptions.counterPicksToDraft}}</td>
          </tr>
          <tr>
            <th class="bg-primary">Minimum Bid Amount</th>
            <td>{{leagueYearOptions.minimumBidAmount}}</td>
          </tr>
          <tr>
            <th class="bg-primary">"Any Unreleased" Droppable Games</th>
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
            <th class="bg-primary">Counter Picks Block Drops</th>
            <td>{{leagueYearOptions.counterPicksBlockDrops | yesNo }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Banned Tags</th>
            <td>
              <span v-for="tag in leagueYearOptions.tags.banned">
                <masterGameTagBadge :tagName="tag" short="true"></masterGameTagBadge>
              </span>
            </td>
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
import axios from 'axios';
import MasterGameTagBadge from '@/components/modules/masterGameTagBadge';

export default {
  props: ['league', 'leagueYear'],
  components: {
    MasterGameTagBadge
  },
  data() {
    return {
      leagueYearOptions: null
    };
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
};
</script>
<style scoped>

</style>
