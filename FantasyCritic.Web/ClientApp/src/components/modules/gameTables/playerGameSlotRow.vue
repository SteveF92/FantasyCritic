<template>
  <tr v-bind:class="{ 'table-warning': gameSlot && !gameSlot.gameMeetsSlotCriteria, 'minimal-game-row': minimal }">
    <template v-if="game">
      <td>
        <span class="game-name-column">
          <b-button variant="danger" class="move-button" v-show="moveMode && !holdingGame && !gameSlot.counterPick" v-on:click="holdGame">Move</b-button>
          <b-button variant="success" class="move-button" v-show="holdingGame && !gameSlot.counterPick" v-on:click="placeGame">Here</b-button>
          <slotTypeBadge v-if="hasSpecialSlots || gameSlot.counterPick" :gameSlot="gameSlot"></slotTypeBadge>
          <span class="master-game-popover">
            <masterGamePopover v-if="game.linked" :masterGame="game.masterGame" :currentlyIneligible="!gameSlot.gameMeetsSlotCriteria"></masterGamePopover>
            <span v-if="!game.linked">{{game.gameName}}</span>
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
          <span v-if="!gameSlot.gameMeetsSlotCriteria" class="game-status">
            Ineligible
            <font-awesome-icon color="white" size="lg" icon="info-circle" v-b-popover.hover="inEligibleText" />
          </span>
        </span>
      </td>
      <template v-if="!minimal">
        <td v-if="game.releaseDate">{{releaseDate}}</td>
        <td v-else>{{game.estimatedReleaseDate}} (Estimated)</td>
        <td>{{acquireDate}}</td>
        <td class="score-column">{{game.criticScore | score(2)}}</td>
        <td class="score-column">{{game.fantasyPoints | score(2)}}</td>
      </template>
      <template v-else>
        <td class="score-column">{{game.criticScore | score }}</td>
        <td class="score-column">{{game.fantasyPoints | score }}</td>
      </template>
    </template>
    <template v-else>
      <td>
        <span class="game-name-column">
          <b-button variant="success" class="move-button" v-show="holdingGame && !gameSlot.counterPick" v-on:click="placeGame">Here</b-button>
          <slotTypeBadge v-if="hasSpecialSlots" :gameSlot="gameSlot"></slotTypeBadge>
          <span v-if="gameSlot.counterPick" class="game-status">
            Warning!
            <font-awesome-icon color="white" size="lg" icon="info-circle" v-b-popover.hover="emptyCounterpickText" />
          </span>
        </span>
      </td>
      <template v-if="!minimal">
        <td></td>
        <td></td>
      </template>
      <td class="score-column"></td>
      <td class="score-column">{{emptySlotScore}}</td>
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
  props: ['minimal','gameSlot', 'yearFinished', 'hasSpecialSlots'],
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
    moveMode() {
      return this.$store.getters.moveMode;
    },
    holdingGame() {
      return this.$store.getters.holdingGame;
    },
    emptySlotScore() {
      if (this.gameSlot.counterPick && this.yearFinished) {
        return '-15';
      }

      return '';
    },
    inEligibleText() {
      return {
        html: true,
        title: () => {
          return "What does this mean?";
        },
        content: () => {
          if (this.hasSpecialSlots) {
            return 'This game is not eligible for this slot. Until you take action, the points the game recieved will not count. <br/> <br/>' +
              'You can either move this game for a different slot, or, if your league disagrees with this the tags this game has, you can override the tags for this game.';
          } else {
            return 'This game is currently ineligible based on your league rules. Until you take action, the points the game recieved will not count. <br/> <br/>' +
              'The intention is for the league to discuss what should happen. If you manually mark the game as eligible or change your ' +
              'league rules, this will disappear. <br/> <br/>' +
              'You could also choose to remove the game. The manager can use "Remove Publisher Game" to do that.';
          }
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
          return 'If you do not fill this slot by the end of the year, it will count as -15 points. <br/> <br/>' +
            'See the FAQ for a full explanation.';
        }
      }
    }
  },
  methods:{
    holdGame() {
      this.$store.commit('holdGame', this.gameSlot);
    },
    placeGame() {
      this.$store.dispatch('moveGame', this.gameSlot);
    }
  }
};
</script>
<style scoped>
  tr {
    height: 40px;
  }

  .minimal-game-row {
    height: 35px;
  }

  .minimal-game-row td {
    font-size: 10pt;
  }

  .game-name-column {
      display: inline-flex;
      width: 100%;
  }

  .game-status {
    color: #B1B1B1;
    font-style: italic;
    margin-left: auto;
  }

  .move-button {
    font-size: 12px;
    padding: 3px;
    height: 25px;
    border-radius: 4px;
    color: #ffffff;
    text-shadow: 1px 0 0 #000, 0 -1px 0 #000, 0 1px 0 #000, -1px 0 0 #000;
  }
</style>
