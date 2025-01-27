<template>
  <div class="hall-of-fame-game">
    <div class="game-image-area" :class="{ 'small-game-image-area': useSmallImages }">
      <a :id="popoverID" href="javascript:;" class="no-link-style">
        <img v-if="masterGame.ggToken && masterGame.ggCoverArtFileName" :src="ggCoverArtLink" alt="Cover Image" class="game-image" />
        <div v-else class="game-text game-name">
          {{ masterGame.gameName }}
        </div>
      </a>
    </div>

    <div>
      <div v-for="(stat, statName, index) in hallOfFameGame.stats" :key="index" class="game-text">{{ statName }}: {{ formatStat(stat) }}</div>
      <hr class="divider" />
      <div class="game-text">
        Picked by
        <router-link :to="{ name: 'publisher', params: { publisherid: hallOfFameGame.pickedBy.publisherID } }">
          {{ hallOfFameGame.pickedBy.playerName }}
        </router-link>
        in {{ hallOfFameGame.pickedBy.year }}
      </div>
    </div>

    <b-popover :target="popoverID" triggers="click blur" custom-class="master-game-popover">
      <div class="mg-popover">
        <masterGameSummary :master-game="masterGame" hide-image></masterGameSummary>
      </div>
    </b-popover>
  </div>
</template>

<script>
import MasterGameSummary from '@/components/masterGameSummary.vue';
import GGMixin from '@/mixins/ggMixin.js';
import { roundNumber, formatMoney } from '@/globalFunctions';
export default {
  components: {
    MasterGameSummary
  },
  mixins: [GGMixin],
  props: {
    hallOfFameGame: { type: Object, required: true }
  },
  computed: {
    masterGame() {
      return this.hallOfFameGame.masterGame;
    },
    useSmallImages() {
      if (window.innerWidth < 500) {
        return true;
      }

      return false;
    },
    popoverID() {
      return `mg-popover-${this._uid}`;
    },
    ggCoverArtLink() {
      let width = 165;
      if (this.useSmallImages) {
        width = 125;
      }

      return this.getGGCoverArtLinkForGame(this.masterGame, width);
    }
  },
  methods: {
    formatStat(stat) {
      const statValue = stat.stat;
      const statType = stat.statType;
      if (statValue === 'DNR') {
        return 'DNR';
      }
      if (statType === 'Score' || statType === 'Factor') {
        return roundNumber(statValue, 2);
      }
      if (statType === 'Budget') {
        return formatMoney(statValue, 0);
      }

      return statValue;
    }
  }
};
</script>
<style scoped>
.hall-of-fame-game {
  border-radius: 5%;
  display: flex;
  flex-direction: column;
  align-items: center;
  background-color: #333333;
  padding: 12px;

  text-align: center;
  width: 200px;
}

.game-image-area {
  width: 165px;
  height: 248px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
}

.small-game-image-area {
  width: 125px !important;
  height: 200px !important;
}

.game-image {
  display: block;
  margin: auto;
  border-radius: 5%;
}

.game-text {
  text-shadow:
    0 0 2px black,
    0 0 2px black,
    0 0 2px black,
    0 0 2px black;
  font-weight: bold;
  text-align: center;
  font-size: 1rem;
}

.no-link-style {
  color: white;
  text-decoration: none;
}

.divider {
  margin-top: 5px;
  margin-bottom: 5px;
}
</style>
