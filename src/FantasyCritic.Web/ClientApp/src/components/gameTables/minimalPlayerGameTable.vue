<template>
  <div :id="'publisher-' + publisher.publisherID" class="player-summary bg-secondary" :class="{ 'publisher-is-next': publisher.nextToDraft }">
    <div class="publisher-player-names">
      <div class="publisher-name-and-icon">
        <span v-if="publisher.publisherIcon && iconIsValid" class="publisher-icon">
          {{ publisher.publisherIcon }}
        </span>
        <div>
          <div class="publisher-name">
            {{ publisher.publisherName }}
          </div>
          <div v-if="publisher.publisherSlogan" class="publisher-slogan">"{{ publisher.publisherSlogan }}"</div>
          <router-link v-show="!renderingSnapshot" :to="{ name: 'publisher', params: { publisherid: publisher.publisherID } }">
            <font-awesome-icon icon="info-circle" />
            Details
          </router-link>
        </div>
      </div>
      <div class="player-name">
        Player: {{ publisher.playerName }}
        <div v-show="!renderingSnapshot">
          <b-button v-if="userIsPublisher && isPlusUser" variant="secondary" size="sm" @click="prepareSnapshot">
            <font-awesome-icon icon="share-alt" size="lg" class="share-button" />
            <span>Share</span>
          </b-button>
        </div>
      </div>
    </div>
    <div class="publisher-stats-container">
      <div class="publisher-stats">
        <span class="publisher-stat">
          <span class="stat-value">{{ publisher.budget | money(0) }}</span>
          <span class="stat-label">Budget</span>
        </span>
        <span class="publisher-stat">
          <span class="stat-value">{{ publisher.gamesReleased }}</span>
          <span class="stat-label">Released</span>
        </span>
        <span class="publisher-stat">
          <span class="stat-value">{{ publisher.gamesWillRelease }}</span>
          <span class="stat-label">Expecting</span>
        </span>
      </div>
      <div v-if="hasAnyDrops" class="publisher-stats publisher-stats-drops">
        <span class="drops-remaining-label">Available Drops</span>
        <span v-if="publisher.willReleaseDroppableGames !== 0" class="publisher-stat">
          <span class="stat-value">{{ dropStatus(publisher.willReleaseGamesDropped, publisher.willReleaseDroppableGames) }}</span>
          <span class="stat-label">Will Release</span>
        </span>
        <span v-if="publisher.willNotReleaseDroppableGames !== 0" class="publisher-stat">
          <span class="stat-value">{{ dropStatus(publisher.willNotReleaseGamesDropped, publisher.willNotReleaseDroppableGames) }}</span>
          <span class="stat-label">Won't Release</span>
        </span>
        <span v-if="publisher.unrestrictedReleaseStatusDroppableGames !== 0" class="publisher-stat">
          <span class="stat-value">{{ dropStatus(publisher.unrestrictedReleaseStatusGamesDropped, publisher.unrestrictedReleaseStatusDroppableGames) }}</span>
          <span class="stat-label">Any Unreleased</span>
        </span>
        <span v-if="publisher.superDropsAvailable !== 0" class="publisher-stat">
          <span class="stat-value">{{ publisher.superDropsAvailable }}</span>
          <span class="stat-label">Super Drops</span>
        </span>
      </div>
    </div>
    <table class="table table-striped">
      <thead>
        <tr class="bg-secondary">
          <th scope="col" class="game-column">Game</th>
          <th scope="col" class="score-column">Critic</th>
          <th scope="col" class="score-column">Points</th>
        </tr>
      </thead>
      <tbody>
        <minimalPlayerGameSlotRow v-for="gameSlot in gameSlots" :key="gameSlot.overallSlotNumber" :minimal="true" :game-slot="gameSlot"></minimalPlayerGameSlotRow>
        <tr class="minimal-game-row">
          <td id="total-description">
            <span v-if="!showProjections" id="total-description-text">
              Total Fantasy Points
              <font-awesome-icon v-if="showRoundingWarning" v-b-popover.hover.focus="roundingWarning" color="white" size="lg" icon="exclamation-triangle" />
            </span>
            <span v-else id="total-description-text">Projected Fantasy Points</span>
          </td>
          <template v-if="!showProjections">
            <td id="total-column" class="bg-success" colspan="2">{{ publisher.totalFantasyPoints | score(decimalsToShow) }}</td>
          </template>
          <template v-else>
            <td id="total-column" class="bg-info" colspan="2">
              <em>~{{ publisher.totalProjectedPoints | score(decimalsToShow) }}</em>
            </td>
          </template>
        </tr>
      </tbody>
    </table>
  </div>
</template>
<script>
import html2canvas from 'html2canvas';

import LeagueMixin from '@/mixins/leagueMixin.js';
import MinimalPlayerGameSlotRow from '@/components/gameTables/minimalPlayerGameSlotRow.vue';
import { orderBy, publisherIconIsValid } from '@/globalFunctions';

export default {
  components: {
    MinimalPlayerGameSlotRow
  },
  mixins: [LeagueMixin],
  props: {
    publisher: { type: Object, required: true }
  },
  data() {
    return {
      renderingSnapshot: false
    };
  },
  computed: {
    userIsPublisher() {
      return this.userInfo && this.publisher.userID === this.userInfo.userID;
    },
    gameSlots() {
      if (!this.draftOrderView) {
        return this.publisher.gameSlots;
      }

      return orderBy(this.publisher.gameSlots, (x) => `${x.counterPick ? '1' : '0'}-${x.publisherGame?.timestamp ?? 'Z' + String(x.slotNumber).padStart(4, '0')}`);
    },
    iconIsValid() {
      return publisherIconIsValid(this.publisher.publisherIcon);
    },
    hasAnyDrops() {
      if (this.publisher.superDropsAvailable > 0) {
        return true;
      }
      if ((this.publisher.willReleaseDroppableGames === -1 && this.publisher.willNotReleaseDroppableGames === -1) || this.publisher.unrestrictedReleaseStatusDroppableGames === -1) {
        return false;
      }
      return this.publisher.willReleaseDroppableGames > 0 || this.publisher.willNotReleaseDroppableGames > 0 || this.publisher.unrestrictedReleaseStatusDroppableGames > 0;
    },
    showRoundingWarning() {
      if (this.userInfo?.showDecimalPlaces) {
        return false;
      }

      const roundedTotal = Math.round(this.publisher.totalFantasyPoints);
      const fantasyPoints = this.publisher.games.map((x) => x.fantasyPoints);
      const roundedFantasyPoints = fantasyPoints.map((x) => Math.round(x));
      let roundedSum = roundedFantasyPoints.reduce((partialSum, a) => partialSum + a, 0);
      roundedSum += this.getEmptyCounterPickSlotPoints;
      return roundedTotal !== roundedSum;
    },
    getEmptyCounterPickSlotPoints() {
      if (!this.leagueYear.supportedYear.finished) {
        return 0;
      }

      const expectedNumberOfCounterPicks = this.leagueYear.settings.counterPicks;
      const numberCounterPicks = this.publisher.games.filter((x) => x.counterPick).length;
      const emptySlots = expectedNumberOfCounterPicks - numberCounterPicks;
      const points = emptySlots * -15;
      return points;
    },
    roundingWarning() {
      return {
        html: true,
        title: () => {
          return "Why aren't these points adding up correctly?";
        },
        content: () => {
          return (
            'Points displayed in this table are rounded, and may not add up the way they appear. You can choose to turn on decimal points in the "Manage Account" page. ' +
            'Note: points are calculated to 4 decimal points no matter your display option.'
          );
        }
      };
    }
  },
  methods: {
    dropStatus(dropped, droppable) {
      if (droppable === -1) {
        return '\u221E';
      }
      return droppable - dropped + '/' + droppable;
    },
    prepareSnapshot() {
      this.renderingSnapshot = true;
      setTimeout(this.sharePublisher, 1);
    },
    sharePublisher() {
      let elementID = '#publisher-' + this.publisher.publisherID;
      html2canvas(document.querySelector(elementID)).then(async (canvas) => {
        const dataUrl = canvas.toDataURL('png');
        const blob = await (await fetch(dataUrl)).blob();
        const filesArray = [
          new File([blob], 'myPublisher.png', {
            type: blob.type,
            lastModified: new Date().getTime()
          })
        ];
        const shareData = {
          files: filesArray,
          title: 'My Fantasy Critic Publisher'
        };
        navigator.share(shareData);
      });
      this.renderingSnapshot = false;
    }
  }
};
</script>
<style scoped>
.player-summary {
  margin-bottom: 10px;
  padding: 5px;
  border: solid;
  border-width: 1px;
  padding: 0;
}

.publisher-is-next {
  border-color: #45621d;
  border-width: 5px;
  border-style: solid;
  padding: 0;
}

.publisher-is-next table thead tr.table-primary th {
  background-color: #ed9d2b;
}

.player-summary table {
  margin-bottom: 0;
}

.publisher-player-names {
  margin: 15px;
  margin-bottom: 0;
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
}

.publisher-name-and-icon {
  display: flex;
  align-items: center;
}

.publisher-name {
  font-weight: bold;
  font-size: 1.3em;
  color: white;
  word-break: break-word;
}

.publisher-icon {
  font-size: 50px;
  line-height: 50px;
  padding-bottom: 5px;
}

.player-name {
  color: white;
  font-weight: bold;
}

.publisher-stats-container {
  margin-top: 6px;
  border-top: 1px solid rgba(255, 255, 255, 0.15);
  border-bottom: 1px solid rgba(255, 255, 255, 0.15);
  background-color: rgba(0, 0, 0, 0.25);
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.publisher-stats {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-around;
  padding: 5px 10px;
}

.publisher-stats-drops {
  margin-top: 0;
  padding: 4px 10px 8px;
  flex-wrap: wrap;
  background-color: rgba(0, 0, 0, 0.2);
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.drops-remaining-label {
  width: 100%;
  font-size: 10px;
  color: rgba(255, 255, 255, 0.75);
  text-transform: uppercase;
  letter-spacing: 1px;
  margin-bottom: 0;
  text-align: center;
  font-weight: bold;
}

.publisher-stats-drops .publisher-stat {
  padding: 0px 4px;
  min-width: 80px;
}

.publisher-stats-drops .stat-label {
  max-width: 100px;
}

.publisher-stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  flex: 1;
  min-width: 60px;
  padding: 2px 4px;
  position: relative;
}

.publisher-stat + .publisher-stat::before {
  content: '';
  position: absolute;
  left: 0;
  top: 10%;
  height: 80%;
  width: 1px;
  background-color: rgba(255, 255, 255, 0.15);
}

.stat-label {
  font-size: 9px;
  color: rgba(255, 255, 255, 0.6);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 1px;
  text-align: center;
  line-height: 1.2;
  max-width: 80px;
}

.stat-value {
  font-size: 12px;
  font-weight: bold;
  color: white;
}

.publisher-slogan {
  font-size: 15px;
}
</style>
<style>
.player-summary table th {
  border-top: none;
}

.player-summary table td {
  border: 1px solid white;
}

.player-summary table thead tr th {
  height: 35px;
  padding: 5px;
}

.player-summary table tbody tr td {
  height: 35px;
  padding: 5px;
}

.type-column {
  width: 30px;
  text-align: center;
}

.score-column {
  width: 30px;
  text-align: center;
  font-weight: bold;
  height: 20px;
  line-height: 20px;
}

#total-description {
  vertical-align: middle;
}

#total-description-text {
  float: right;
  text-align: right;
  display: table-cell;
  vertical-align: middle;
  font-weight: bold;
  font-size: 20px;
}

#total-column {
  text-align: center;
  font-weight: bold;
  font-size: 25px;
}

.share-button {
  color: white;
}
</style>
