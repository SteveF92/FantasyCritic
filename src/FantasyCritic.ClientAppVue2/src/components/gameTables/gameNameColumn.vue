<template>
  <span class="game-name-column">
    <span class="game-name-side">
      <b-button v-show="moveMode && game && !heldSlot && !gameSlot.counterPick" variant="danger" class="move-button" @click="holdGame">Move</b-button>
      <b-button v-show="heldSlot && !gameSlot.counterPick" variant="success" class="move-button" @click="placeGame">Here</b-button>
      <slotTypeBadge v-if="hasSpecialSlots || gameSlot.counterPick || gameSlot.dropped" :game-slot="gameSlot"></slotTypeBadge>
      <span v-if="game" class="master-game-popover">
        <masterGamePopover v-if="game.linked" :master-game="game.masterGame" :show-as-warning="gameSlot.showWarningRow"></masterGamePopover>
        <span v-if="!game.linked">{{ game.gameName }}</span>
      </span>
    </span>

    <span v-if="game" class="game-info-side">
      <font-awesome-icon v-if="!game.linked" v-b-popover.hover.focus="unlinkedText" color="white" size="lg" icon="question-circle" />
      <font-awesome-icon v-if="game.linked && !game.willRelease" v-b-popover.hover.focus="willNotReleaseText" color="white" size="lg" icon="calendar-times" />
      <font-awesome-icon v-if="game.linked && game.willReleaseEarlyAccessOnly" v-b-popover.hover.focus="willReleaseEarlyAccessOnlyText" color="white" size="lg" icon="calendar-days" />
      <font-awesome-icon v-if="game.linked && game.masterGame.delayContention" v-b-popover.hover.focus="delayContentionText" color="white" size="lg" icon="balance-scale" />
      <font-awesome-icon v-if="game.counterPicked && !game.dropBlocked" v-b-popover.hover.focus="counterPickedText" color="white" size="lg" icon="crosshairs" />
      <font-awesome-icon v-if="gameSlot.counterPick && counterPickSourcePublisher" v-b-popover.hover.focus="counterPickSourceText" color="white" size="lg" icon="user" />
      <font-awesome-icon v-if="game.dropBlocked" v-b-popover.hover.focus="gameDropBlockedText" color="white" size="lg" icon="lock" />
      <font-awesome-icon v-if="game.linked && game.masterGame.showNote && game.masterGame.notes" v-b-popover.hover.focus="game.masterGame.notes" color="white" size="lg" icon="flag" />
      <template v-if="game.released && game.linked && !supportedYear.finished && !game.criticScore">
        <font-awesome-icon v-if="!game.masterGame.openCriticID" v-b-popover.hover.focus="needsOpenCriticPage" color="white" size="lg" icon="link-slash" />
        <font-awesome-icon v-else v-b-popover.hover.focus="needsMoreReviewsText" color="white" size="lg" icon="hourglass-half" />
      </template>

      <font-awesome-icon v-if="game.manualCriticScore" v-b-popover.hover.focus="manuallyScoredText" color="white" size="lg" icon="pen" />
      <b-popover v-if="!gameSlot.gameMeetsSlotCriteria && !gameSlot.gameIsCancelled" title="Ineligible Game">
        <template #target>
          <font-awesome-icon color="white" size="lg" icon="exclamation-triangle" />
        </template>
        <template v-if="gameSlot.gameEligibleInAnyLeagueSlot">
          This game is not eligible for this slot. {{ ineligiblePointsText }}
          <br />
          <br />
          You can either move this game to a different slot, or, if your league disagrees with the tags this game has, you can override the tags for this game.
        </template>
        <template v-else-if="gameSlot.ineligibleGameSystemValue === 'DroppableAsWillNotRelease'">
          This game is ineligible in this league. {{ ineligiblePointsText }}
          <br />
          <br />
          <!-- prettier-ignore -->
          <span>Per your league settings, this game can be dropped using a <strong>Will Not Release</strong> drop.</span>
        </template>
        <template v-else-if="gameSlot.ineligibleGameSystemValue === 'FreelyDroppable'">
          This game is ineligible in this league. {{ ineligiblePointsText }}
          <br />
          <br />
          <!-- prettier-ignore -->
          <span>Per your league settings, this game can be dropped <strong>for free</strong>, without using any of your drops.</span>
        </template>
        <template v-else-if="gameSlot.ineligibleGameSystemValue === 'NotDroppable'">
          This game is ineligible in this league. {{ ineligiblePointsText }}
          <br />
          <br />
          <!-- prettier-ignore -->
          <span>Per your league settings, this game <strong>cannot be dropped</strong>.</span>
        </template>
        <template v-else>
          This game is currently ineligible based on your league rules. {{ ineligiblePointsText }}
          <br />
          <br />
          The intention is for the league to discuss what should happen. If you manually mark the game as eligible or change your league rules, this will disappear.
          <br />
          <br />
          You could also choose to remove the game. The manager can use "Remove Publisher Game" to do that.
        </template>
        <br />
        <br />
        <h5>Errors</h5>
        <ul>
          <li v-for="error in gameSlot.eligibilityErrors" :key="error">{{ error }}</li>
        </ul>
      </b-popover>
      <font-awesome-icon v-if="gameSlot.counterPickedGameIsInvalid" v-b-popover.hover.focus="counterPickedGameIsInvalidText" color="white" size="lg" icon="exclamation-triangle" />
    </span>

    <span v-if="gameSlot.counterPick && !game" class="game-status">
      <template v-if="!supportedYear.finished">
        Warning!
        <b-popover title="Warning!">
          <template #target>
            <font-awesome-icon color="white" size="lg" icon="exclamation-triangle" />
          </template>
          If you do not fill this slot by the end of the year, it will count as -15 points.
          <br />
          <br />
          <template v-if="hasEarlyCounterPickDeadline">
            Additionally, after {{ formatLongDate(counterPickDeadline) }} you will only be able to counter pick a game with a confirmed release date.
            <br />
            <br />
          </template>
          See the FAQ for a full explanation.
        </b-popover>
      </template>
      <template v-else>
        <font-awesome-icon v-b-popover.hover.focus="emptyCounterpickFinalText" color="white" size="lg" icon="exclamation-triangle" />
      </template>
    </span>
  </span>
</template>
<script>
import { usePublisherStore } from '@/store/publisherStore';

import MasterGamePopover from '@/components/masterGamePopover.vue';
import SlotTypeBadge from '@/components/gameTables/slotTypeBadge.vue';
import { useLeagueStore } from '@/store/leagueStore';
import { formatLongDate } from '@/globalFunctions';

export default {
  components: {
    MasterGamePopover,
    SlotTypeBadge
  },
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
    leagueYear() {
      const leagueStore = useLeagueStore();
      return leagueStore.leagueYear;
    },
    moveMode() {
      const publisherStore = usePublisherStore();
      return publisherStore.moveMode;
    },
    heldSlot() {
      const publisherStore = usePublisherStore();
      return publisherStore.heldSlot;
    },
    counterPickSourcePublisher() {
      if (!this.gameSlot.counterPick || !this.game?.masterGame || !this.leagueYear?.publishers) {
        return null;
      }
      const masterGameID = this.game.masterGame.masterGameID;
      for (const publisher of this.leagueYear.publishers) {
        for (const slot of publisher.gameSlots) {
          if (!slot.counterPick && slot.publisherGame?.masterGame?.masterGameID === masterGameID) {
            return publisher;
          }
        }
      }
      return null;
    },
    ineligiblePointsText() {
      if (this.supportedYear.year < 2022) {
        return 'Until you take action, the points the game received will still count.';
      }
      return 'Until you take action, the points the game received will not count.';
    },
    hasEarlyCounterPickDeadline() {
      return this.counterPickDeadline !== `${this.supportedYear.year}-12-31`;
    },
    counterPickedGameIsInvalidText() {
      return {
        title: 'Counter Picked Game is Ineligible',
        body: 'This counter pick counts for 0 points because the game in question is not eligible in this league, or in the slot the other player currently has it in.'
      };
    },
    gameDropBlockedText() {
      return {
        title: 'Locked!',
        body: `This game was counter picked by ${this.game.counterPickedBy}, so it cannot be dropped.`
      };
    },
    counterPickedText() {
      return {
        title: 'Counter Picked!',
        body: `This game was counter picked by ${this.game.counterPickedBy}!`
      };
    },
    unlinkedText() {
      let body = 'This is a "custom game" that has not been linked to a master game. The league manager can link it using "associate game" in the sidebar.';
      if (this.hasSpecialSlots) {
        body += ' Since there is no way for the site to know what kind of game this is, it can be moved into any slot.';
      }
      return {
        title: 'Not Linked to Master Game',
        body
      };
    },
    delayContentionText() {
      return {
        title: 'Delay in Contention',
        body:
          'There are very credible reports that this game has been delayed and therefore will not release this year. The game is still counted as a "will release" ' +
          'game for drop purposes, but it cannot be counter picked, just like a "will not release" game cannot be counter picked.'
      };
    },
    needsOpenCriticPage() {
      return {
        title: 'Needs OpenCritic Link',
        body: "This game has released, but is not linked to an OpenCritic page. If one exists, you can 'suggest a correction' and we'll get it fixed."
      };
    },
    needsMoreReviewsText() {
      return {
        title: 'Needs more reviews',
        body: 'This game has released, and has an OpenCritic page, but there are not enough reviews yet.'
      };
    },
    manuallyScoredText() {
      return {
        title: 'Manually Scored',
        body: 'This game was manually scored by the league manager.'
      };
    },
    willNotReleaseText() {
      if (this.supportedYear.finished) {
        return {
          title: 'Did Not Release',
          body: 'This game did not release in the league year.'
        };
      }
      if (this.game.manualWillNotRelease) {
        return {
          title: 'Will Not Release (League Override)',
          body: 'This game has been marked as "Will Not Release" manually by the league manager.'
        };
      }
      return {
        title: 'Will Not Release',
        body: 'This game will not release this year.'
      };
    },
    willReleaseEarlyAccessOnlyText() {
      return {
        title: 'Planned for Early Access',
        body:
          'This game is planned for early access, and will not see a full release this year. ' +
          'However, as your league uses "Only Needs Score" mode, you will still receive points if the game receives a score on OpenCritic this year.'
      };
    },
    counterPickSourceText() {
      return {
        title: 'Publisher With Game',
        body: `This game is in ${this.counterPickSourcePublisher.publisherName}'s roster.`
      };
    },
    emptyCounterpickFinalText() {
      return {
        title: 'Woops!',
        body: 'Since this slot was not filled, it counts as -15 points.'
      };
    }
  },
  methods: {
    formatLongDate,
    holdGame() {
      const publisherStore = usePublisherStore();
      publisherStore.holdGame(this.gameSlot);
    },
    placeGame() {
      const publisherStore = usePublisherStore();
      publisherStore.moveGame(this.gameSlot);
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
  text-shadow:
    0 0 2px black,
    0 0 2px black,
    0 0 2px black,
    0 0 2px black;
}

.lock-icon {
  margin-left: 5px;
}
</style>
