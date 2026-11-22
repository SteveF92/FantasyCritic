<template>
  <b-modal id="eligibility-overrides-modal" title="Eligibility Overrides" no-footer>
    <table class="table table-sm table-responsive-sm table-bordered table-striped">
      <thead>
        <tr class="bg-primary">
          <th scope="col" class="game-column">Game</th>
          <th scope="col">Eligible?</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="eligibilityOverride in requiredLeagueYear.eligibilityOverrides" :key="eligibilityOverride.masterGame.masterGameID">
          <td>{{ eligibilityOverride.masterGame.gameName }}</td>
          <td>{{ formatYesNo(eligibilityOverride.eligible) }}</td>
        </tr>
      </tbody>
    </table>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';

import { useLeagueStore } from '@/store/leagueStore';
import { formatYesNo } from '@/utilities/formatters';

export default defineComponent({
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear'])
  },
  methods: {
    formatYesNo
  }
});
</script>
