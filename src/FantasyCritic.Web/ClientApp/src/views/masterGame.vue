<template>
  <div>
    <div v-if="masterGame" class="col-md-10 offset-md-1 col-sm-12">
      <div class="row master-game-section">
        <div class="col-xl-6 col-lg-12">
          <div class="game-image-area">
            <div v-if="masterGame.ggToken && masterGame.ggCoverArtFileName" class="gg-image-area">
              <img v-show="masterGame.ggCoverArtFileName" :src="ggCoverArtLink" alt="Cover Image" class="game-image" />
              <a :href="ggLink" target="_blank">
                <strong>
                  Image Provided by GG|
                  <font-awesome-icon icon="external-link-alt" />
                </strong>
              </a>
            </div>
            <font-awesome-layers v-show="!masterGame.ggCoverArtFileName" class="fa-8x no-game-image">
              <font-awesome-icon :icon="['far', 'square']" />
              <font-awesome-layers-text transform="shrink-14" value="No image found" />
            </font-awesome-layers>
          </div>
        </div>

        <div class="col-xl-6 col-lg-12">
          <h1>{{ masterGame.gameName }}</h1>
          <div>
            <router-link class="text-primary" :to="{ name: 'masterGameChangeRequest', query: { mastergameid: masterGame.masterGameID } }"><strong>Suggest a correction</strong></router-link>
          </div>
          <div v-if="isAdmin">
            <router-link class="text-primary" :to="{ name: 'masterGameEditor', params: { mastergameid: masterGame.masterGameID } }"><strong>Edit Master Game</strong></router-link>
          </div>
          <hr />
          <div class="text-well">
            <h2>Details</h2>
            <masterGameDetails :masterGame="masterGame"></masterGameDetails>
          </div>

          <div v-for="masterGameYear in reversedMasterGameYears" :key="masterGameYear.year" class="text-well master-game-year-section">
            <h2>Stats for {{ masterGameYear.year }}</h2>
            <ul>
              <li>Drafted or picked up in {{ masterGameYear.eligiblePercentStandardGame | percent(1) }} of leagues where it is eligible.</li>

              <li v-show="masterGameYear.averageDraftPosition">Average Draft Position: {{ masterGameYear.averageDraftPosition | score(1) }}</li>
              <li v-show="!masterGameYear.averageDraftPosition">Average Draft Position: Undrafted</li>

              <li v-show="masterGameYear.dateAdjustedHypeFactor">Hype Factor: {{ masterGameYear.dateAdjustedHypeFactor | score(1) }}</li>
              <li v-show="!masterGameYear.dateAdjustedHypeFactor">Hype Factor: Unhyped...</li>

              <template v-if="masterGameYear.year >= 2022 && masterGameYear.peakHypeFactor > masterGameYear.dateAdjustedHypeFactor">
                <li v-show="masterGameYear.peakHypeFactor">
                  Peak Hype Factor: {{ masterGameYear.peakHypeFactor | score(1) }}
                  <font-awesome-icon color="white" size="lg" icon="info-circle" v-b-popover.hover.top="peakHypeFactorText" />
                </li>
                <li v-show="!masterGameYear.peakHypeFactor">
                  Peak Hype Factor: Unhyped...
                  <font-awesome-icon color="white" icon="info-circle" v-b-popover.hover.top="peakHypeFactorText" />
                </li>
              </template>

              <li v-show="masterGameYear.projectedFantasyPoints">Projected Points: ~{{ masterGameYear.projectedFantasyPoints | score(1) }}</li>

              <li>Counter Picked in {{ masterGameYear.adjustedPercentCounterPick | percent(1) }} of leagues where it is published.</li>
            </ul>
          </div>
        </div>
      </div>

      <div class="row" v-if="masterGame.subGames && masterGame.subGames.length > 0">
        <h2>Sub Games (Episodes)</h2>
        <div v-for="subGame in masterGame.subGames" :key="subGame.masterGameID">
          <h3>{{ subGame.gameName }}</h3>
          <p>
            <span class="detail-label">Release Date:</span>
            <span v-if="subGame.releaseDate">{{ releaseDate(subGame) }}</span>
            <span v-else>{{ subGame.estimatedReleaseDate }} (Estimated)</span>
          </p>
          <p v-if="subGame.openCriticID">
            <a :href="openCriticLink(subGame)" target="_blank">
              Open Critic Link
              <font-awesome-icon icon="external-link-alt" size="xs" />
            </a>
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import MasterGameDetails from '@/components/masterGameDetails';

export default {
  components: {
    MasterGameDetails
  },
  data() {
    return {
      masterGame: null,
      masterGameYears: [],
      error: ''
    };
  },
  props: {
    mastergameid: String
  },
  computed: {
    ggCoverArtLink() {
      if (this.masterGame.ggCoverArtFileName) {
        return `https://ggapp.imgix.net/media/games/${this.masterGame.ggToken}/${this.masterGame.ggCoverArtFileName}?w=307&dpr=1&fit=crop&auto=compress&q=95`;
      }
      return null;
    },
    ggLink() {
      return `https://ggapp.io/games/${this.masterGame.ggToken}`;
    },
    isAdmin() {
      return this.$store.getters.isAdmin;
    },
    reversedMasterGameYears() {
      let tempMasterGameYears = _.cloneDeep(this.masterGameYears);
      tempMasterGameYears.reverse();
      return tempMasterGameYears;
    },
    peakHypeFactorText() {
      return {
        html: true,
        title: () => {
          return 'Peak Hype Factor';
        },
        content: () => {
          return (
            "Sometimes a game's hype factor will go down over the course of the year, particularly if it gets delayed and many players drop it. " +
            "This number is the highest this game's hype factor ever was in the year."
          );
        }
      };
    }
  },
  mounted() {
    this.fetchMasterGame();
    this.fetchMasterGameYears();
  },
  watch: {
    $route() {
      this.fetchMasterGame();
    }
  },
  methods: {
    fetchMasterGame() {
      axios
        .get('/api/game/MasterGame/' + this.mastergameid)
        .then((response) => {
          this.masterGame = response.data;
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    fetchMasterGameYears() {
      axios
        .get('/api/game/MasterGameYears/' + this.mastergameid)
        .then((response) => {
          this.masterGameYears = response.data;
        })
        .catch((returnedError) => (this.error = returnedError));
    }
  }
};
</script>
<style scoped>
.detail-label {
  font-weight: bold;
  margin-right: 4px;
}

.master-game-section {
  margin-top: 20px;
  margin-bottom: 20px;
}

.game-image {
  border-radius: 5%;
}

.game-image-area {
  margin: auto;
}

.gg-image-area {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.master-game-year-section {
  margin-top: 10px;
}
</style>
