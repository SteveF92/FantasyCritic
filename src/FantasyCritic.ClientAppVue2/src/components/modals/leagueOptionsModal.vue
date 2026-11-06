<template>
  <b-modal :id="`leagueOptionsModal_${leagueYearOptions.leagueID}-${leagueYearOptions.year}`" title="League Options" no-footer>
    <table class="table table-responsive-sm table-bordered table-striped">
      <tbody>
        <tr>
          <th class="bg-primary">Total Standard Games</th>
          <td>{{ leagueYearOptions.standardGames }}</td>
        </tr>
        <tr v-if="isSingleDraft">
          <th class="bg-primary">Games to Draft</th>
          <td>{{ firstDraft.gamesToDraft }}</td>
        </tr>
        <tr v-if="isSingleDraft">
          <th class="bg-primary">Pickup Games</th>
          <td>{{ leagueYearOptions.standardGames - firstDraft.gamesToDraft }}</td>
        </tr>
        <tr>
          <th class="bg-primary">Total Counter Picks</th>
          <td>{{ leagueYearOptions.counterPicks }}</td>
        </tr>
        <tr v-if="isSingleDraft">
          <th class="bg-primary">Counter Picks to Draft</th>
          <td>{{ firstDraft.counterPicksToDraft }}</td>
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
          <td v-if="!leagueYearOptions.unlimitedUnrestrictedReleaseStatusDroppableGames">{{ leagueYearOptions.unrestrictedReleaseStatusDroppableGames }}</td>
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
          <td>{{ formatYesNo(leagueYearOptions.dropOnlyDraftGames) }}</td>
        </tr>
        <tr>
          <th class="bg-primary">Automatic Super Drops</th>
          <td>
            <span>{{ formatYesNo(leagueYearOptions.grantSuperDrops) }}</span>
            <span v-if="leagueYearOptions.grantSuperDrops && superDropPointCutoff" class="point-cutoff">(Point Cutoff: {{ formatScore(superDropPointCutoff, 2) }})</span>
          </td>
        </tr>
        <tr>
          <th class="bg-primary">Counter Picks Block Drops</th>
          <td>{{ formatYesNo(leagueYearOptions.counterPicksBlockDrops) }}</td>
        </tr>
        <tr>
          <th class="bg-primary">Allow Games to be moved into ineligible slots</th>
          <td>{{ formatYesNo(leagueYearOptions.allowMoveIntoIneligible) }}</td>
        </tr>
        <tr>
          <th class="bg-primary">Bidding System</th>
          <td>{{ selectTextFromPossibleOptions(leagueYearOptions.pickupSystem, possibleLeagueOptions.pickupSystems) }}</td>
        </tr>
        <tr>
          <th class="bg-primary">Tiebreak System</th>
          <td>{{ selectTextFromPossibleOptions(leagueYearOptions.tiebreakSystem, possibleLeagueOptions.tiebreakSystems) }}</td>
        </tr>
        <tr>
          <th class="bg-primary">Trading System</th>
          <td>{{ selectTextFromPossibleOptions(leagueYearOptions.tradingSystem, possibleLeagueOptions.tradingSystems) }}</td>
        </tr>
        <tr>
          <th class="bg-primary">Game Release Rule</th>
          <td>{{ selectTextFromPossibleOptions(leagueYearOptions.releaseSystem, possibleLeagueOptions.releaseSystems) }}</td>
        </tr>
        <tr>
          <th class="bg-primary">Ineligible games on roster</th>
          <td>{{ selectTextFromPossibleOptions(leagueYearOptions.ineligibleGameSystem, possibleLeagueOptions.ineligibleGameSystems) }}</td>
        </tr>
        <tr>
          <th class="bg-primary">Scoring Rule</th>
          <td>{{ selectTextFromPossibleOptions(leagueYearOptions.scoringSystem, possibleLeagueOptions.scoringSystems) }}</td>
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
        <template v-if="league">
          <tr>
            <th class="bg-primary">Public League</th>
            <td>{{ formatYesNo(league.publicLeague) }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Custom Rules League</th>
            <td>{{ formatYesNo(league.customRulesLeague) }}</td>
          </tr>
          <tr>
            <th class="bg-primary">Test League</th>
            <td>{{ formatYesNo(league.testLeague) }}</td>
          </tr>
        </template>
      </tbody>
    </table>
  </b-modal>
</template>

<script>
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';
import { formatScore, formatYesNo, selectTextFromPossibleOptions } from '@/utilities/formatters';
import { mapState } from 'pinia';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import { formatLongDate } from '@/globalFunctions';

export default {
  components: {
    MasterGameTagBadge
  },
  props: {
    leagueYearOptions: { type: Object, required: true },
    league: { type: Object, required: false },
    drafts: { type: Array, required: false, default: () => [] },
    supportedYear: { type: Object, required: true },
    superDropPointCutoff: { type: Number, required: false }
  },
  computed: {
    ...mapState(useInterLeagueStore, ['possibleLeagueOptions']),
    isSingleDraft() {
      return this.drafts.length > 0 && this.drafts.length < 2;
    },
    firstDraft() {
      return this.drafts[0];
    }
  },
  methods: {
    formatLongDate,
    formatScore,
    formatYesNo,
    selectTextFromPossibleOptions
  }
};
</script>
<style scoped>
.point-cutoff {
  margin-left: 5px;
}
</style>
