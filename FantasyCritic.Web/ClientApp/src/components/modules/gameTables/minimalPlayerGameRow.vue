<template>
  <tr class="minimal-game-row table-default" v-bind:class="{ 'counter-pick-row': game.counterPick, 'table-warning': game.currentlyIneligible }">
    <td class="game-column">
      <span class="master-game-popover">
        <masterGamePopover v-if="game.linked" :masterGame="game.masterGame" :currentlyIneligible="game.currentlyIneligible"></masterGamePopover>
        <span v-if="!game.linked">{{game.gameName}}</span>
      </span>

      <span v-if="game.counterPick" class="counter-pick-text">
        (Counter-Pick)
      </span>

      <span v-if="!game.linked" class="game-status">
        Not linked to Master Game
      </span>

      <span v-if="!game.willRelease && game.linked && !game.manualWillNotRelease" class="game-status">
        <span v-show="!yearFinished">Will not Release</span>
        <span v-show="yearFinished">Did not Release</span>
      </span>
      <span v-if="game.released && game.linked && !game.criticScore && !yearFinished" class="game-status">
        <span>Needs more reviews</span>
      </span>
      <span v-if="game.manualWillNotRelease && game.linked && !yearFinished" class="game-status">
        Will not Release (League Override)
      </span>
      <span v-if="game.manualCriticScore && game.linked" class="game-status">
        Manually Scored
      </span>
      <span v-if="game.currentlyIneligible" class="game-ineligible">
        Ineligible
        <font-awesome-icon color="white" size="lg" icon="info-circle" v-b-popover.hover="inEligibleText" />
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
import Vue from 'vue';
import moment from 'moment';
import MasterGamePopover from '@/components/modules/masterGamePopover';

export default {
  components: {
    MasterGamePopover
  },
  props: ['game', 'yearFinished'],
  computed: {
    releaseDate() {
      return moment(this.game.releaseDate).format('MMMM Do, YYYY');
    },
    advancedProjections() {
      return this.$store.getters.advancedProjections;
    },
    inEligibleText() {
      return {
        html: true,
        title: () => {
          return "What does this mean?";
        },
        content: () => {
          return 'This game is currently ineligible based on your league rules. Until you take action, any points the game recieved still count. <br/> <br/>' +
            'The intention is for the league to discuss what should happen. If you manually mark the game as eligible or change your ' + 
            'league rules, this will disappear. <br/> <br/>' +
            'You could also choose to remove the game. The manager can use "Remove Publisher Game" to do that.';
        }
      }
    }
  }
};
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
    padding-left:5px;
  }

  .game-ineligible {
    float: right;
    color: white;
    font-style: italic;
    padding-left: 5px;
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
