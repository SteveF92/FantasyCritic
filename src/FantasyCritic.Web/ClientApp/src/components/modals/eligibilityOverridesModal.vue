<template>
  <b-modal id="eligibilityOverridesModal" ref="eligibilityOverridesModalRef" title="Eligibility Overrides" hide-footer>
    <table class="table table-sm table-responsive-sm table-bordered table-striped">
      <thead>
        <tr class="bg-primary">
          <th scope="col" class="game-column">Game</th>
          <th scope="col">Eligible?</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="eligibilityOverride in leagueYear.eligibilityOverrides" :key="eligibilityOverride.masterGame.masterGameID">
          <td>{{ eligibilityOverride.masterGame.gameName }}</td>
          <td>{{ yesNo(eligibilityOverride.eligible) }}</td>
        </tr>
      </tbody>
    </table>
  </b-modal>
</template>
<script>
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin]
};
</script>
