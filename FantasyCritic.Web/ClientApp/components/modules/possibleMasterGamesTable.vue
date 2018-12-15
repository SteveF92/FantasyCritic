<template>
    <table class="table table-sm table-responsive-sm">
        <thead>
          <tr>
            <th>Game Name</th>
            <th>Estimated Release Date</th>
            <th>Eligibility Level</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
            <tr v-for="game in possibleGames">
              <td>
                <popper :ref="'gamePopoverRef' + game.masterGameID" trigger="click" :options="{ placement: 'top', modifiers: { offset: { offset: '0,10px' } }}">
                  <div class="popper">
                    <masterGamePopover :mastergameid="game.masterGameID" v-on:closePopover="closePopover"></masterGamePopover>
                  </div>

                  <span slot="reference" class="text-primary fake-link">
                    {{game.gameName}}
                  </span>
                </popper>
              </td>
                <td>{{game.estimatedReleaseDate}}</td>
                <td>
                  <eligibilityBadge :eligibilityLevel="game.eligibilityLevel" :maximumEligibilityLevel="maximumEligibilityLevel"></eligibilityBadge>
                </td>
                <td>
                    <b-button variant="info" v-on:click="selectGame(game)">Select</b-button>
                </td>
            </tr>
        </tbody>
    </table>
</template>
<script>
  import EligibilityBadge from "components/modules/eligibilityBadge";
  import Popper from 'vue-popperjs';
  import 'vue-popperjs/dist/css/vue-popper.css';
  import MasterGamePopover from "components/modules/masterGamePopover";


  export default {
    data() {
      return {
          selectedMasterGame: null
      }
    },
    components: {
      EligibilityBadge,
      'popper': Popper,
      MasterGamePopover
    },
    props: ['possibleGames', 'value', 'maximumEligibilityLevel'],
    methods: {
      selectGame(game) {
          this.selectedMasterGame = game;
          this.$emit('input', this.selectedMasterGame);
      },
      closePopover(mastergameid) {
        let refName = 'gamePopoverRef' + mastergameid;
        this.$refs[refName][0].doClose();
      }
    }
  }
</script>
<style scoped>
  .fake-link {
    text-decoration: underline;
    cursor: pointer;
  }

  .popper {
    background: #415262;
  }
</style>
