<template>
  <tr v-bind:class="{ 'table-danger': game.counterPick, 'table-warning': game.currentlyIneligible }">
    <td>
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
      <span v-if="game.manualWillNotRelease && game.linked" class="game-status">
        Will not Release (League Override)
      </span>
      <span v-if="game.manualCriticScore && game.linked" class="game-status">
        Manually Scored
      </span>
      <span v-if="game.currentlyIneligible" class="game-ineligible">
        Ineligible
        <font-awesome-icon color="white" size="md" icon="info-circle" v-b-popover.hover="inEligibleText" />
      </span>
    </td>
    <td v-if="game.releaseDate">{{releaseDate}}</td>
    <td v-else>{{game.estimatedReleaseDate}} (Estimated)</td>
    <td>{{game.criticScore | score(2)}}</td>
    <td>{{game.fantasyPoints | score(2)}}</td>
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
  props: ['game'],
  computed: {
    releaseDate() {
      return moment(this.game.releaseDate).format('MMMM Do, YYYY');
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
  tr {
    height: 40px;
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

  .game-ineligible {
    float: right;
    color: white;
    font-style: italic;
    padding-left: 5px;
  }

  .master-game-popover {
    float: left;
  }
</style>
