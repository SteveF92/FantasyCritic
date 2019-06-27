<template>
    <table class="table table-sm table-bordered table-striped">
        <thead>
          <tr class="bg-primary">
            <th>Game Name</th>
            <th>Estimated Release Date</th>
            <th class="no-mobile">Status</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="game in possibleGames">
            <td>
              <masterGamePopover ref="gamePopoverWrapperRef" :masterGame="game.masterGame" v-on:newPopoverShown="newPopoverShown"></masterGamePopover>
            </td>
            <td v-bind:class="{ 'text-danger': game.masterGame.isReleased }" class="release-date">
              <span>{{game.masterGame.estimatedReleaseDate}}</span>
              <span v-show="game.masterGame.isReleased">(Released)</span>
            </td>
            <td class="no-mobile">
              <statusBadge :taken="game.taken" :isEligible="game.isEligible"></statusBadge>
            </td>
            <td class="select-cell">
              <b-button size="sm" variant="info" v-on:click="selectGame(game.masterGame)">Select</b-button>
            </td>
          </tr>
        </tbody>
    </table>
</template>
<script>
  import StatusBadge from "components/modules/statusBadge";
  import MasterGamePopover from "components/modules/masterGamePopover";

  export default {
    data() {
      return {
        selectedMasterGame: null,
        lastPopoverShown: null
      }
    },
    components: {
      StatusBadge,
      MasterGamePopover
    },
    props: ['possibleGames', 'value', 'maximumEligibilityLevel'],
    methods: {
      selectGame(masterGame) {
        this.selectedMasterGame = masterGame;
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

  @media only screen and (max-width: 450px) {
    .no-mobile {
      display: none;
    }
  }
</style>
