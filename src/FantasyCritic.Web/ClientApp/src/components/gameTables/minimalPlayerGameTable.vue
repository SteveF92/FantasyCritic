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

      return orderBy(this.publisher.gameSlots, (x) => `${x.counterPick ? '1' : '0'}-${x.publisherGame.timestamp}`);
    },
    iconIsValid() {
      return publisherIconIsValid(this.publisher.publisherIcon);
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
