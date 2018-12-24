<template>
    <table class="table table-sm table-responsive-sm table-bordered table-striped">
        <thead>
          <tr class="bg-primary">
            <th>Game Name</th>
            <th>Estimated Release Date</th>
            <th>Eligibility Level</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
            <tr v-for="game in possibleGames">
              <td>
                <masterGamePopover ref="gamePopoverWrapperRef" :masterGame="game" v-on:newPopoverShown="newPopoverShown"></masterGamePopover>
              </td>
              <td v-bind:class="{ 'text-danger': game.isReleased }" class="release-date">
                <span>{{game.estimatedReleaseDate}}</span>
                <span v-show="game.isReleased">(Released)</span>
              </td>
              <td>
                <eligibilityBadge :eligibilityLevel="game.eligibilityLevel" :maximumEligibilityLevel="maximumEligibilityLevel"></eligibilityBadge>
              </td>
              <td class="select-cell">
                  <b-button variant="info" v-on:click="selectGame(game)">Select</b-button>
              </td>
            </tr>
        </tbody>
    </table>
</template>
<script>
  import EligibilityBadge from "components/modules/eligibilityBadge";
  import MasterGamePopover from "components/modules/masterGamePopover";

  export default {
    data() {
      return {
        selectedMasterGame: null,
        lastPopoverShown: null
      }
    },
    components: {
      EligibilityBadge,
      MasterGamePopover
    },
    props: ['possibleGames', 'value', 'maximumEligibilityLevel'],
    methods: {
      selectGame(game) {
          this.selectedMasterGame = game;
          this.$emit('input', this.selectedMasterGame);
      },
      newPopoverShown(masterGame) {
        this.$refs.gamePopoverWrapperRef.forEach(function (popover) {
          if (popover.masterGame.masterGameID !== masterGame.masterGameID) {
            popover.closePopover();
          }
        });
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

  .release-date{
    font-weight: bold;
  }

  .select-cell {
    text-align: center;
  }
</style>
