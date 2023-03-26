<template>
  <b-modal id="leagueOptionsModal" ref="leagueOptionsModalRef" title="League Options" hide-footer>
    <div v-if="league && leagueYear && leagueYearOptions && possibleLeagueOptions">
      <table class="table table-responsive-sm table-bordered table-striped">
        <tbody>
          <tr>
            <th class="bg-primary">Total Standard Games</th>
            <td>{{ leagueYearOptions.standardGames }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Games to Draft</th>
            <td>{{ leagueYearOptions.gamesToDraft }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Pickup Games</th>
            <td>{{ leagueYearOptions.standardGames - leagueYearOptions.gamesToDraft }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Total Counter Picks</th>
            <td>{{ leagueYearOptions.counterPicks }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Counter Picks to Draft</th>
            <td>{{ leagueYearOptions.counterPicksToDraft }}</td>
          </tr>
          <tr v-if="leagueYearOptions.counterPickDeadline !== `${supportedYear.year}-12-31`">
            <th class="bg-primary">Counter Pick Deadline</th>
            <td>{{ formatLongDate(leagueYearOptions.counterPickDeadline) }}</td>
          </tr>
          <tr v-if="leagueYearOptions.mightReleaseDroppableDate">
            <th class="bg-primary">Might Release Droppable Date</th>
            <td>{{ formatLongDate(leagueYearOptions.mightReleaseDroppableDate) }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Minimum Bid Amount</th>
            <td>{{ leagueYearOptions.minimumBidAmount }}</td>
          </tr>
          <tr>
            <th class="bg-primary">"Any Unreleased" Droppable Games</th>
            <td v-if="!leagueYearOptions.unlimitedFreeDroppableGames">{{ leagueYearOptions.freeDroppableGames }}</td>
            <td v-else>Unlimited</td>
          </tr>
          <tr>
            <th class="bg-primary">"Will not Release" Droppable Games</th>
            <td v-if="!leagueYearOptions.unlimitedWillNotReleaseDroppableGames">{{ leagueYearOptions.willNotReleaseDroppableGames }}</td>
            <td v-else>Unlimited</td>
          </tr>
          <tr>
            <th class="bg-primary">"Will Release" Droppable Games</th>
            <td v-if="!leagueYearOptions.unlimitedWillReleaseDroppableGames">{{ leagueYearOptions.willReleaseDroppableGames }}</td>
            <td v-else>Unlimited</td>
          </tr>
          <tr>
            <th class="bg-primary">Drop Only Drafted Games</th>
            <td>{{ leagueYearOptions.dropOnlyDraftGames | yesNo }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Automatic Super Drops</th>
            <td>{{ leagueYearOptions.grantSuperDrops | yesNo }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Counter Picks Block Drops</th>
            <td>{{ leagueYearOptions.counterPicksBlockDrops | yesNo }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Bidding System</th>
            <td>{{ leagueYearOptions.pickupSystem | selectTextFromPossibleOptions(possibleLeagueOptions.pickupSystems) }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Tiebreak System</th>
            <td>{{ leagueYearOptions.tiebreakSystem | selectTextFromPossibleOptions(possibleLeagueOptions.tiebreakSystems) }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Trading System</th>
            <td>{{ leagueYearOptions.tradingSystem | selectTextFromPossibleOptions(possibleLeagueOptions.tradingSystems) }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Game Release Rule</th>
            <td>{{ leagueYearOptions.releaseSystem | selectTextFromPossibleOptions(possibleLeagueOptions.releaseSystems) }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Scoring Rule</th>
            <td>{{ leagueYearOptions.scoringSystem | selectTextFromPossibleOptions(possibleLeagueOptions.scoringSystems) }}</td>
          </tr>

          <tr>
            <th class="bg-primary">Banned Tags</th>
            <td>
              <span v-for="tag in leagueYearOptions.tags.banned" :key="tag">
                <masterGameTagBadge :tag-name="tag" short></masterGameTagBadge>
              </span>
            </td>
          </tr>
          <tr>
            <th class="bg-primary">Special Game Slots</th>
            <td v-if="leagueYearOptions.specialGameSlots && leagueYearOptions.specialGameSlots.length > 0">{{ leagueYearOptions.specialGameSlots.length }}</td>
            <td v-else>None</td>
          </tr>

          <tr>
            <th class="bg-primary">Public League</th>
            <td>{{ league.publicLeague | yesNo }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Custom Rules League</th>
            <td>{{ league.customRulesLeague | yesNo }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Test League</th>
            <td>{{ league.testLeague | yesNo }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </b-modal>
</template>

<script>
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  components: {
    MasterGameTagBadge
  },
  mixins: [LeagueMixin],
  computed: {
    leagueYearOptions() {
      return this.leagueYear.settings;
    }
  }
};
</script>
