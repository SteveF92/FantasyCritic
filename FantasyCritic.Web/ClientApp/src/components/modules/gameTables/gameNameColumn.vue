<template>
  <span class="game-name-column">
    <span class="game-name-side">
      <b-button variant="danger" class="move-button" v-show="moveMode && game && !holdingGame && !gameSlot.counterPick" v-on:click="holdGame">Move</b-button>
      <b-button variant="success" class="move-button" v-show="holdingGame && !gameSlot.counterPick" v-on:click="placeGame">Here</b-button>
      <slotTypeBadge v-if="hasSpecialSlots || gameSlot.counterPick" :gameSlot="gameSlot"></slotTypeBadge>
      <span class="master-game-popover" v-if="game">
        <masterGamePopover v-if="game.linked" :masterGame="game.masterGame" :currentlyIneligible="!gameSlot.gameMeetsSlotCriteria"></masterGamePopover>
        <span v-if="!game.linked">{{game.gameName}}</span>
      </span>
    </span>

    <span class="game-info-side" v-if="game">
      <font-awesome-icon v-if="!game.linked" color="white" size="lg" icon="question-circle" v-b-popover.hover="unlinkedText" />
      <font-awesome-icon v-if="game.linked && !game.willRelease" color="white" size="lg" icon="calendar-times" v-b-popover.hover="willNotReleaseText" />
      <font-awesome-icon v-if="game.linked && game.masterGame.delayContention" color="white" size="lg" icon="balance-scale" v-b-popover.hover="delayContentionText" />
      <font-awesome-icon v-if="game.counterPicked && !game.dropBlocked" color="white" size="lg" icon="crosshairs" v-b-popover.hover="counterPickedText" />
      <font-awesome-icon v-if="game.dropBlocked" color="white" size="lg" icon="lock" v-b-popover.hover="gameDropBlockedText" />
      <font-awesome-icon v-if="game.released && game.linked && !game.criticScore && !supportedYear.finished" color="white" size="lg" icon="hourglass-half" v-b-popover.hover="needsMoreReviewsText" />
      <font-awesome-icon v-if="game.manualCriticScore" color="white" size="lg" icon="pen" v-b-popover.hover="manuallyScoredText" />
      <font-awesome-icon v-if="!gameSlot.gameMeetsSlotCriteria" color="white" size="lg" icon="exclamation-triangle" v-b-popover.hover="inEligibleText" />
    </span>

    <span v-if="gameSlot.counterPick && !game" class="game-status">
      Warning!
      <font-awesome-icon color="white" size="lg" icon="exclamation-triangle" v-b-popover.hover="emptyCounterpickText" />
    </span>
  </span>
</template>
<script>
import MasterGamePopover from '@/components/modules/masterGamePopover';
import SlotTypeBadge from '@/components/modules/gameTables/slotTypeBadge';

export default {
  components: {
    MasterGamePopover,
    SlotTypeBadge
  },
  props: ['gameSlot', 'hasSpecialSlots', 'supportedYear'],
  computed: {
    game() {
      return this.gameSlot.publisherGame;
    },
    moveMode() {
      return this.$store.getters.moveMode;
    },
    holdingGame() {
      return this.$store.getters.holdingGame;
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
    delayContentionText() {
      return {
        html: true,
        title: () => {
          return "Delay in Contention";
        },
        content: () => {
          return 'There are very credible reports that this game has been delayed and therefore will not release this year. The game is still counted as a "will release" ' +
            'game for drop purposes, but it cannot be counter picked, just like a "will not release" game cannot be counter picked.';
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
          if (this.supportedYear.finished) {
            return "Did Not Release";
          }
          if (this.game.manualWillNotRelease) {
            return "Will Not Release (League Override)";
          }
          return "Will Not Release";
        },
        content: () => {
          if (this.supportedYear.finished) {
            return "This game did not release in the league year.";
          }
          if (this.game.manualWillNotRelease) {
            return 'This game has been marked as "Will Not Release" manually by the league manager.';
          }
          return "This game will not release this year.";
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

  .lock-icon {
    margin-left: 5px;
  }
</style>
