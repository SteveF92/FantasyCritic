<template>
  <tr v-bind:class="{ 'table-danger': gameSlot.counterPick, 'table-warning': game && game.currentlyIneligible }">
    <template v-if="game">
      <td>
        <span class="game-name-column">
          <span class="master-game-popover">
            <masterGamePopover v-if="game.linked" :masterGame="game.masterGame" :currentlyIneligible="game.currentlyIneligible"></masterGamePopover>
            <span v-if="!game.linked">{{game.gameName}}</span>
          </span>

          <span v-if="game.counterPick" class="game-status">
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
            <font-awesome-icon color="white" size="md" icon="info-circle" v-b-popover.hover="inEligibleText" />
          </span>
        </span>
      </td>
      <td v-if="game.releaseDate">{{releaseDate}}</td>
      <td v-else>{{game.estimatedReleaseDate}} (Estimated)</td>
      <td>{{acquireDate}}</td>
      <td>{{game.criticScore | score(2)}}</td>
      <td>{{game.fantasyPoints | score(2)}}</td>
    </template>
    <template v-else>
      <td>
        <span class="blank-game-name-column">
          <span v-if="gameSlot.counterPick" class="game-status">
            Warning!
            <font-awesome-icon color="white" size="lg" icon="info-circle" v-b-popover.hover="emptyCounterpickText" />
          </span>
        </span>
      </td>
      <td></td>
      <td></td>
      <td>
      <template v-if="gameSlot.counterPick && yearFinished">
        -15
      </template></td>
      <td>
      <template v-if="gameSlot.counterPick && yearFinished">
        -15
      </template></td>
    </template>
  </tr>
</template>
<script>
import moment from 'moment';
import MasterGamePopover from '@/components/modules/masterGamePopover';
import SlotTypeBadge from '@/components/modules/gameTables/slotTypeBadge';

export default {
  components: {
    MasterGamePopover,
    SlotTypeBadge
  },
  props: ['gameSlot', 'yearFinished'],
    computed: {
    game(){
      return this.gameSlot.publisherGame;
    },
    releaseDate() {
      return moment(this.game.releaseDate).format('MMMM Do, YYYY');
    },
    acquireDate() {
      let type = 'Drafted';
      if (!this.game.overallDraftPosition) {
        type = 'Picked up';
      }
      let date = moment(this.game.timestamp).format('MMMM Do, YYYY');
      return type + ' on ' + date;
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
    },
    emptyCounterpickText() {
      return {
        html: true,
        title: () => {
          return "Warning!";
        },
        content: () => {
          return 'If you do not fill this gameSlot by the end of the year, it will count as -15 points. <br/> <br/>' +
            'See the FAQ for a full explanation.';
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

  .game-name-column {
      display: flex;
      justify-content: space-between;
  }

  .blank-game-name-column {
    display: flex;
    flex-direction: row-reverse;
    justify-content: space-between;
  }

  .game-status {
    color: #B1B1B1;
    font-style: italic;
    margin-left: 3px;
  }

  .game-ineligible {
    color: white;
    font-style: italic;
    padding-left: 5px;
  }

  .master-game-popover {

  }
</style>
