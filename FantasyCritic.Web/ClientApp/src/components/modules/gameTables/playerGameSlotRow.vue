<template>
  <tr v-bind:class="{ 'table-warning': gameSlot && !gameSlot.gameMeetsSlotCriteria, 'minimal-game-row': minimal }">
    <template v-if="game">
      <td>
        <span class="game-name-column">
          <span class="game-name-side">
            <b-button variant="danger" class="move-button" v-show="moveMode && !holdingGame && !gameSlot.counterPick" v-on:click="holdGame">Move</b-button>
            <b-button variant="success" class="move-button" v-show="holdingGame && !gameSlot.counterPick" v-on:click="placeGame">Here</b-button>
            <slotTypeBadge v-if="hasSpecialSlots || gameSlot.counterPick" :gameSlot="gameSlot"></slotTypeBadge>
            <span class="master-game-popover">
              <masterGamePopover v-if="game.linked" :masterGame="game.masterGame" :currentlyIneligible="!gameSlot.gameMeetsSlotCriteria"></masterGamePopover>
              <span v-if="!game.linked">{{game.gameName}}</span>
            </span>
          </span>

          <span class="game-info-side">
            <font-awesome-icon v-if="!game.linked" color="white" size="lg" icon="question-circle" v-b-popover.hover.top="unlinkedText" />
            <font-awesome-icon v-if="game.linked && !game.willRelease" color="white" size="lg" icon="calendar-times" v-b-popover.hover.top="willNotReleaseText" />
            <font-awesome-icon v-if="game.counterPicked && !game.dropBlocked" color="white" size="lg" icon="crosshairs" v-b-popover.hover.top="counterPickedText" />
            <font-awesome-icon v-if="game.dropBlocked" color="white" size="lg" icon="lock" v-b-popover.hover.top="gameDropBlockedText" />
            <font-awesome-icon v-if="game.released && game.linked && !game.criticScore && !yearFinished" color="white" size="lg" icon="hourglass-half" v-b-popover.hover.top="needsMoreReviewsText" />
            <font-awesome-icon v-if="game.manualCriticScore" color="white" size="lg" icon="pen" v-b-popover.hover.top="manuallyScoredText" />
            <font-awesome-icon v-if="!gameSlot.gameMeetsSlotCriteria" color="white" size="lg" icon="exclamation-triangle" v-b-popover.hover.top="inEligibleText" />
          </span>
        </span>
      </td>
      <template v-if="!minimal">
        <td v-if="game.releaseDate">{{releaseDate}}</td>
        <td v-else>{{game.estimatedReleaseDate}} (Estimated)</td>
        <td>{{acquireDate}}</td>
        <td class="score-column">{{game.criticScore | score(2)}}</td>
        <td class="score-column"><em>~{{game.masterGame.projectedFantasyPoints | score(2)}}</em></td>
        <td class="score-column">{{game.fantasyPoints | score(2)}}</td>
      </template>
      <template v-else>
        <td class="score-column">{{game.criticScore | score}}</td>
        <template v-if="advancedProjections">
          <td class="score-column" v-if="game.fantasyPoints || !game.willRelease">{{game.fantasyPoints | score}}</td>
          <td class="score-column" v-else><em>~{{gameSlot.advancedProjectedFantasyPoints | score}}</em></td>
        </template>
        <template v-else>
          <td class="score-column">{{game.fantasyPoints | score}}</td>
        </template>
      </template>
    </template>
    <template v-else>
      <td>
        <span class="game-name-column">
          <span class="game-name-side">
            <b-button variant="success" class="move-button" v-show="holdingGame && !gameSlot.counterPick" v-on:click="placeGame">Here</b-button>
            <slotTypeBadge v-if="hasSpecialSlots || gameSlot.counterPick" :gameSlot="gameSlot"></slotTypeBadge>
          </span>
          <span v-if="gameSlot.counterPick" class="game-status">
            Warning!
            <font-awesome-icon color="white" size="lg" icon="exclamation-triangle" v-b-popover.hover.top="emptyCounterpickText" />
          </span>
        </span>
      </td>
      <template v-if="!minimal">
        <td></td>
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
  props: ['minimal', 'gameSlot', 'supportedYear', 'hasSpecialSlots'],
  computed: {
    game(){
      return this.gameSlot.publisherGame;
    },
    yearFinished() {
      return this.supportedYear.finished;
    },
    advancedProjections() {
      return this.$store.getters.advancedProjections;
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
          return "Ineligible Game";
        },
        content: () => {
          let eligibilityErrorsList = '';
          this.gameSlot.eligibilityErrors.forEach(error => {
            eligibilityErrorsList += `<li>${error}</li>`
          });

          let eligibilityErrorsListElement = `<h5>Errors</h5><ul>${eligibilityErrorsList}</ul>`;

          let pointsText = 'Until you take action, the points the game received will not count.';
          if (this.supportedYear.year < 2022) {
            pointsText = 'Until you take action, the points the game received will still count.';
          }

          let mainText = `This game is currently ineligible based on your league rules. ${pointsText} <br/> <br/>` +
            'The intention is for the league to discuss what should happen. If you manually mark the game as eligible or change your ' +
            'league rules, this will disappear. <br/> <br/>' +
            'You could also choose to remove the game. The manager can use "Remove Publisher Game" to do that.';
          if (this.hasSpecialSlots) {
            mainText = `This game is not eligible for this slot. ${pointsText} <br/> <br/>` +
              'You can either move this game for a different slot, or, if your league disagrees with this the tags this game has, you can override the tags for this game.';
          }

          let fullText = `${mainText}<br/><br/>${eligibilityErrorsListElement}`;
          return fullText;
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
    },
    gameDropBlockedText() {
      return {
        html: true,
        title: () => {
          return "Locked!";
        },
        content: () => {
          return 'This game was counter picked, so it cannot be dropped.';
        }
      }
    },
    counterPickedText() {
      return {
        html: true,
        title: () => {
          return "Counter Picked!";
        },
        content: () => {
          return 'This game was counter picked!';
        }
      }
    },
    unlinkedText() {
      return {
        html: true,
        title: () => {
          return "Not Linked to Master Game";
        },
        content: () => {
          return 'This is a "custom game" that has not be linked to a master game. The league manager can link it using "associate game" in the sidebar.';
        }
      }
    },
    needsMoreReviewsText() {
      return {
        html: true,
        title: () => {
          return "Needs more reviews";
        },
        content: () => {
          return 'This game has released, and has an Open Critic page, but there are not enough reviews yet.';
        }
      }
    },
    manuallyScoredText() {
      return {
        html: true,
        title: () => {
          return "Manually Scored";
        },
        content: () => {
          return 'This game was manually scored by the league manager.';
        }
      }
    },
    willNotReleaseText() {
      return {
        html: true,
        title: () => {
          if (this.yearFinished) {
            return "Did Not Release";
          }
          if (this.game.manualWillNotRelease) {
            return "Will Not Release (League Override)";
          }
          return "Will Not Release";
        },
        content: () => {
          if (this.yearFinished) {
            return "This game did not release in the league year.";
          }
          if (this.game.manualWillNotRelease) {
            return 'This game has been marked as "Will Not Release" manually by the league manager.';
          }
          return "This game will not release this year.";
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
      justify-content: space-between;
      width: 100%;
  }

  .game-name-side {
    display: inline-flex;
    justify-content: flex-start;
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

  .lock-icon{
      margin-left: 5px;
  }
</style>
