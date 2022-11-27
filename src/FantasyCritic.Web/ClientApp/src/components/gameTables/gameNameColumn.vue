<template>
  <span class="game-name-column">
    <span class="game-name-side">
      <b-button v-show="moveMode && game && !holdingGame && !gameSlot.counterPick" variant="danger" class="move-button" @click="holdGame">Move</b-button>
      <b-button v-show="holdingGame && !gameSlot.counterPick" variant="success" class="move-button" @click="placeGame">Here</b-button>
      <slotTypeBadge v-if="hasSpecialSlots || gameSlot.counterPick || gameSlot.dropped" :game-slot="gameSlot"></slotTypeBadge>
      <span v-if="game" class="master-game-popover">
        <masterGamePopover v-if="game.linked" :master-game="game.masterGame" :currently-ineligible="!gameSlot.gameMeetsSlotCriteria"></masterGamePopover>
        <span v-if="!game.linked">{{ game.gameName }}</span>
      </span>
    </span>

    <span v-if="game" class="game-info-side">
      <font-awesome-icon v-if="!game.linked" v-b-popover.hover.focus="unlinkedText" color="white" size="lg" icon="question-circle" />
      <font-awesome-icon v-if="game.linked && !game.willRelease" v-b-popover.hover.focus="willNotReleaseText" color="white" size="lg" icon="calendar-times" />
      <font-awesome-icon v-if="game.linked && game.masterGame.delayContention" v-b-popover.hover.focus="delayContentionText" color="white" size="lg" icon="balance-scale" />
      <font-awesome-icon v-if="game.counterPicked && !game.dropBlocked" v-b-popover.hover.focus="counterPickedText" color="white" size="lg" icon="crosshairs" />
      <font-awesome-icon v-if="game.dropBlocked" v-b-popover.hover.focus="gameDropBlockedText" color="white" size="lg" icon="lock" />
      <font-awesome-icon v-if="game.linked && game.masterGame.showNote && game.masterGame.notes" v-b-popover.hover.focus="game.masterGame.notes" color="white" size="lg" icon="flag" />
      <template v-if="game.released && game.linked && !supportedYear.finished && !game.criticScore">
        <font-awesome-icon v-if="!game.masterGame.openCriticID" v-b-popover.hover.focus="needsOpenCriticPage" color="white" size="lg" icon="link-slash" />
        <font-awesome-icon v-else v-b-popover.hover.focus="needsMoreReviewsText" color="white" size="lg" icon="hourglass-half" />
      </template>

      <font-awesome-icon v-if="game.manualCriticScore" v-b-popover.hover.focus="manuallyScoredText" color="white" size="lg" icon="pen" />
      <font-awesome-icon v-if="!gameSlot.gameMeetsSlotCriteria" v-b-popover.hover.focus="inEligibleText" color="white" size="lg" icon="exclamation-triangle" />
    </span>

    <span v-if="gameSlot.counterPick && !game" class="game-status">
      Warning!
      <font-awesome-icon v-b-popover.hover.focus="emptyCounterpickText" color="white" size="lg" icon="exclamation-triangle" />
    </span>
  </span>
</template>
<script>
import MasterGamePopover from '@/components/masterGamePopover';
import SlotTypeBadge from '@/components/gameTables/slotTypeBadge';
import BasicMixin from '@/mixins/basicMixin';

export default {
  components: {
    MasterGamePopover,
    SlotTypeBadge
  },
  mixins: [BasicMixin],
  props: {
    gameSlot: { type: Object, required: true },
    supportedYear: { type: Object, required: true },
    hasSpecialSlots: { type: Boolean, required: true },
    counterPickDeadline: { type: String, required: true }
  },
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
          return 'Ineligible Game';
        },
        content: () => {
          let eligibilityErrorsList = '';
          this.gameSlot.eligibilityErrors.forEach((error) => {
            eligibilityErrorsList += `<li>${error}</li>`;
          });

          let eligibilityErrorsListElement = `<h5>Errors</h5><ul>${eligibilityErrorsList}</ul>`;

          let pointsText = 'Until you take action, the points the game received will not count.';
          if (this.supportedYear.year < 2022) {
            pointsText = 'Until you take action, the points the game received will still count.';
          }

          let mainText =
            `This game is currently ineligible based on your league rules. ${pointsText} <br/> <br/>` +
            'The intention is for the league to discuss what should happen. If you manually mark the game as eligible or change your ' +
            'league rules, this will disappear. <br/> <br/>' +
            'You could also choose to remove the game. The manager can use "Remove Publisher Game" to do that.';
          if (this.hasSpecialSlots) {
            mainText =
              `This game is not eligible for this slot. ${pointsText} <br/> <br/>` +
              'You can either move this game for a different slot, or, if your league disagrees with this the tags this game has, you can override the tags for this game.';
          }

          let fullText = `${mainText}<br/><br/>${eligibilityErrorsListElement}`;
          return fullText;
        }
      };
    },
    gameDropBlockedText() {
      return {
        html: true,
        title: () => {
          return 'Locked!';
        },
        content: () => {
          return 'This game was counter picked, so it cannot be dropped.';
        }
      };
    },
    counterPickedText() {
      return {
        html: true,
        title: () => {
          return 'Counter Picked!';
        },
        content: () => {
          return 'This game was counter picked!';
        }
      };
    },
    unlinkedText() {
      return {
        html: true,
        title: () => {
          return 'Not Linked to Master Game';
        },
        content: () => {
          return 'This is a "custom game" that has not been linked to a master game. The league manager can link it using "associate game" in the sidebar.';
        }
      };
    },
    delayContentionText() {
      return {
        html: true,
        title: () => {
          return 'Delay in Contention';
        },
        content: () => {
          return (
            'There are very credible reports that this game has been delayed and therefore will not release this year. The game is still counted as a "will release" ' +
            'game for drop purposes, but it cannot be counter picked, just like a "will not release" game cannot be counter picked.'
          );
        }
      };
    },
    needsOpenCriticPage() {
      return {
        html: true,
        title: () => {
          return 'Needs Open Critic Link';
        },
        content: () => {
          return "This game has released, but is not linked to an Open Critic page. If one exists, you can 'suggest a correction' and we'll get it fixed.";
        }
      };
    },
    needsMoreReviewsText() {
      return {
        html: true,
        title: () => {
          return 'Needs more reviews';
        },
        content: () => {
          return 'This game has released, and has an Open Critic page, but there are not enough reviews yet.';
        }
      };
    },
    manuallyScoredText() {
      return {
        html: true,
        title: () => {
          return 'Manually Scored';
        },
        content: () => {
          return 'This game was manually scored by the league manager.';
        }
      };
    },
    willNotReleaseText() {
      return {
        html: true,
        title: () => {
          if (this.supportedYear.finished) {
            return 'Did Not Release';
          }
          if (this.game.manualWillNotRelease) {
            return 'Will Not Release (League Override)';
          }
          return 'Will Not Release';
        },
        content: () => {
          if (this.supportedYear.finished) {
            return 'This game did not release in the league year.';
          }
          if (this.game.manualWillNotRelease) {
            return 'This game has been marked as "Will Not Release" manually by the league manager.';
          }
          return 'This game will not release this year.';
        }
      };
    },
    emptyCounterpickText() {
      return {
        html: true,
        title: () => {
          return 'Warning!';
        },
        content: () => {
          const partOne = 'If you do not fill this slot by the end of the year, it will count as -15 points.';
          const lineBreak = '<br/> <br/>';
          const optionalPart = `Additionally, after ${this.formatLongDate(this.counterPickDeadline)} you will only be able to counter pick a game with a confirmed release date.`;
          const finalPart = 'See the FAQ for a full explanation.';
          let finalString = partOne + lineBreak;
          if (this.counterPickDeadline !== `${this.supportedYear.year}-12-31`) {
            finalString += optionalPart + lineBreak;
          }
          finalString += finalPart;
          return finalString;
        }
      };
    }
  },
  methods: {
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
  color: #b1b1b1;
  font-style: italic;
  margin-left: auto;
}

.move-button {
  font-size: 12px;
  padding: 3px;
  height: 25px;
  border-radius: 4px;
  color: #ffffff;
  text-shadow: 0 0 2px black, 0 0 2px black, 0 0 2px black, 0 0 2px black;
}

.lock-icon {
  margin-left: 5px;
}
</style>
