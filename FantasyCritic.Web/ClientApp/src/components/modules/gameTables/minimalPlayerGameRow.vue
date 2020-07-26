<template>
  <tr class="minimal-game-row table-default" v-bind:class="{ 'counter-pick-row': game.counterPick }">
    <td class="game-column">
      <span class="master-game-popover">
        <masterGamePopover v-if="game.linked" :masterGame="game.masterGame"></masterGamePopover>
        <span v-if="!game.linked">{{game.gameName}}</span>
      </span>


      <span v-if="game.counterPick" class="counter-pick-text">
        (Counter-Pick)
      </span>

      <span v-if="!game.linked" class="game-status">
        Not linked to Master Game
      </span>

      <span v-if="!game.willRelease && game.linked" class="game-status">
        <span v-show="!yearFinished">Will not Release</span>
        <span v-show="yearFinished">Did not Release</span>
      </span>
      <span v-if="game.manualCriticScore && game.linked" class="game-status">
        Manually Scored
      </span>
    </td>
    <td class="score-column">{{game.criticScore | score}}</td>
    <template v-if="advancedProjections">
      <td class="score-column" v-if="game.fantasyPoints || !game.willRelease">{{game.fantasyPoints | score}}</td>
      <td class="score-column" v-else><em>~{{game.advancedProjectedFantasyPoints | score}}</em></td>
    </template>
    <template v-else>
      <td class="score-column">{{game.fantasyPoints | score}}</td>
    </template>
  </tr>
</template>
<script>
  import Vue from "vue";
  import moment from "moment";
  import MasterGamePopover from "@/components/modules/masterGamePopover";

  export default {
    components: {
      MasterGamePopover
    },
    props: ['game', 'yearFinished'],
    computed: {
      releaseDate() {
            return moment(this.game.releaseDate).format('MMMM Do, YYYY');
      },
      advancedProjections: {
        get() {
          return this.$store.getters.advancedProjections;
        }
      }
    }
  }
</script>
<style scoped>
  .minimal-game-row td {
    font-size: 10pt;
  }
  .counter-pick-text {
    color: #B1B1B1;
    font-style: italic;
    margin-left: 3px;
  }
  .game-status {
    float: right;
    color: #B1B1B1;
    font-style: italic;
  }

  .popper {
    background: #415262;
  }
  .master-game-popover {
    float:left;
  }

  .counter-pick-row td {
    background-color: #AA1E1E !important;
  }
</style>
