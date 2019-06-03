<template>
  <b-modal id="manageEligibilityOverridesModal" ref="eligibilityOverridesModalRef" title="Manage Eligibility Overrides" hide-footer>
    <table class="table table-bordered table-striped">
      <thead>
        <tr class="bg-primary">
          <th scope="col" class="game-column">Game</th>
          <th scope="col">Eligible?</th>
          <th scope="col">Reset?</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="eligibilityOverride in leagueYear.eligibilityOverrides">
          <td>{{eligibilityOverride.masterGame.gameName}}</td>
          <td>{{eligibilityOverride.eligible | yesNo}}</td>
          <td class="select-cell">
            <b-button variant="danger" v-on:click="resetEligibility(eligibilityOverride)">Reset</b-button>
          </td>
        </tr>
      </tbody>
    </table>
  </b-modal>
</template>

<script>
  import axios from "axios";

  export default {
    props: ['leagueYear'],
    methods: {
      resetEligibility(eligibilityOverride) {
        var model = {
          leagueID: this.leagueYear.leagueID,
          year: this.leagueYear.year,
          masterGameID: eligibilityOverride.masterGame.masterGameID,
          eligible: null
        };
        axios
          .post('/api/leagueManager/SetGameEligibilityOverride', model)
          .then(response => {
            var gameInfo = {
              gameName: eligibilityOverride.masterGame.gameName
            };
            this.$emit('gameEligiblityReset', gameInfo);
          })
          .catch(response => {

          });
      }
    }
  }
</script>
<style scoped>
  .select-cell {
    text-align: center;
  }
</style>
