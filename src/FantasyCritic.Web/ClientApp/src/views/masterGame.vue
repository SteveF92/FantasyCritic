<template>
  <div>
    <div v-if="masterGame" class="col-md-10 offset-md-1 col-sm-12">
      <div class="row master-game-section">
        <div class="col-xl-6 col-lg-12">
          <div class="game-image-area">
            <div v-if="masterGame.ggToken && masterGame.ggCoverArtFileName" class="gg-image-area">
              <img v-if="masterGame.ggCoverArtFileName" :src="ggCoverArtLink" alt="Cover Image" class="game-image" />
              <a :href="ggLink" target="_blank">
                <strong>
                  Image Provided by GG|
                  <font-awesome-icon icon="external-link-alt" />
                </strong>
              </a>
            </div>
            <div v-else class="no-game-image-area">
              <font-awesome-layers v-if="!masterGame.ggCoverArtFileName" class="fa-10x no-game-image">
                <font-awesome-icon :icon="['far', 'square']" />
                <font-awesome-layers-text transform="shrink-14" value="No image found" />
              </font-awesome-layers>
            </div>
          </div>
        </div>

        <div class="col-xl-6 col-lg-12">
          <h1>{{ masterGame.gameName }}</h1>
          <div>
            <router-link class="text-primary" :to="{ name: 'masterGameChangeRequest', query: { mastergameid: masterGame.masterGameID } }"><strong>Suggest a correction</strong></router-link>
          </div>
          <div v-if="isFactChecker">
            <router-link class="text-primary" :to="{ name: 'masterGameEditor', params: { mastergameid: masterGame.masterGameID } }"><strong>Edit Master Game</strong></router-link>
          </div>
          <hr />

          <div class="text-well">
            <h2>Details</h2>
            <masterGameDetails :master-game="masterGame"></masterGameDetails>
          </div>

          <div v-if="changeLog && changeLog.length > 0" class="text-well master-game-section">
            <h2>Change Log</h2>
            <b-table :items="changeLog" :fields="changeLogFields" bordered striped responsive small>
              <template #cell(timestamp)="data">
                {{ data.item.timestamp | longDate }}
              </template>
              <template #cell(changedByUser)="data">
                {{ data.item.changedByUser.displayName }}
              </template>
            </b-table>
          </div>

          <div v-if="leaguesWithGame.length > 0" class="text-well league-game-section">
            <h2>Your Leagues with this Game</h2>

            <ul>
              <li v-for="league in leaguesWithGame" :key="league.leagueID">
                <a :href="leagueLink(league.leagueID, league.year)">{{ league.leagueName }} ({{ league.year }})</a>
                <span v-if="league.isCounterPick" class="counter-pick-label">(Counter Picked)</span>
              </li>
            </ul>
          </div>

          <div v-for="masterGameYear in reversedMasterGameYears" :key="masterGameYear.year" class="text-well master-game-section">
            <h2>Stats for {{ masterGameYear.year }}</h2>
            <ul>
              <li>Drafted or picked up in {{ masterGameYear.eligiblePercentStandardGame | percent(1) }} of leagues where it is eligible.</li>

              <li v-if="masterGameYear.averageDraftPosition">Average Draft Position: {{ masterGameYear.averageDraftPosition | score(1) }}</li>
              <li v-else>Average Draft Position: Undrafted</li>

              <li v-if="masterGameYear.dateAdjustedHypeFactor">Hype Factor: {{ masterGameYear.dateAdjustedHypeFactor | score(1) }}</li>
              <li v-else>Hype Factor: Unhyped...</li>

              <template v-if="masterGameYear.year >= 2022 && masterGameYear.peakHypeFactor > masterGameYear.dateAdjustedHypeFactor">
                <li v-if="masterGameYear.peakHypeFactor">
                  Peak Hype Factor: {{ masterGameYear.peakHypeFactor | score(1) }}
                  <font-awesome-icon v-b-popover.hover.top="peakHypeFactorText" color="white" size="lg" icon="info-circle" />
                </li>
                <li v-else>
                  Peak Hype Factor: Unhyped...
                  <font-awesome-icon v-b-popover.hover.top="peakHypeFactorText" color="white" icon="info-circle" />
                </li>
              </template>

              <li v-if="masterGameYear.projectedFantasyPoints">Projected Points: ~{{ masterGameYear.projectedFantasyPoints | score(1) }}</li>

              <li>Counter Picked in {{ masterGameYear.adjustedPercentCounterPick | percent(1) }} of leagues where it is published.</li>
            </ul>
          </div>
        </div>
      </div>

      <div v-if="masterGame.subGames && masterGame.subGames.length > 0" class="row">
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
import MasterGameDetails from '@/components/masterGameDetails.vue';
import GGMixin from '@/mixins/ggMixin.js';

export default {
  components: {
    MasterGameDetails
  },
  mixins: [GGMixin],
  props: {
    mastergameid: { type: String, required: true }
  },
  data() {
    return {
      masterGame: null,
      masterGameYears: [],
      leaguesWithGame: [],
      changeLog: [],
      error: '',
      baseChangeLogFields: [
        { key: 'timestamp', label: 'Date of Change', thClass: 'bg-primary' },
        { key: 'description', label: 'Description', thClass: 'bg-primary' }
      ],
      factCheckerChangeLogFields: [{ key: 'changedByUser', label: 'Changed by', thClass: 'bg-primary' }]
    };
  },
  computed: {
    ggCoverArtLink() {
      return this.getGGCoverArtLinkForGame(this.masterGame, 307);
    },
    ggLink() {
      return this.getGGLinkForGame(this.masterGame);
    },
    changeLogFields() {
      if (this.isFactChecker) {
        return this.baseChangeLogFields.concat(this.factCheckerChangeLogFields);
      }

      return this.baseChangeLogFields;
    },
    reversedMasterGameYears() {
      let tempMasterGameYears = structuredClone(this.masterGameYears);
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
  watch: {
    async $route() {
      await this.loadAll();
    }
  },
  async mounted() {
    await this.loadAll();
  },
  methods: {
    async loadAll() {
      await this.fetchMasterGame();
      const tasks = [this.fetchMasterGameChangeLog(), this.fetchMasterGameYears(), this.fetchLeaguesWithMasterGame()];
      await Promise.all(tasks);
    },
    async fetchMasterGame() {
      try {
        const response = await axios.get('/api/game/MasterGame/' + this.mastergameid);
        this.masterGame = response.data;
      } catch (error) {
        this.error = error.data;
      }
    },
    async fetchMasterGameChangeLog() {
      try {
        const response = await axios.get('/api/game/MasterGameChangeLog/' + this.mastergameid);
        this.changeLog = response.data;
      } catch (error) {
        this.error = error.response.data;
      }
    },
    async fetchMasterGameYears() {
      try {
        const response = await axios.get('/api/game/MasterGameYears/' + this.mastergameid);
        this.masterGameYears = response.data;
      } catch (error) {
        this.error = error.response.data;
      }
    },
    async fetchLeaguesWithMasterGame() {
      try {
        const response = await axios.get('/api/game/LeagueYearsWithMasterGame/' + this.mastergameid);
        this.leaguesWithGame = response.data.sort((l1, l2) => {
          if (l1.year > l2.year) {
            return -1;
          }
          if (l1.year < l2.year) {
            return 1;
          }

          const leagueName1 = l1.leagueName.toLowerCase();
          const leagueName2 = l2.leagueName.toLowerCase();

          if (leagueName1 < leagueName2) {
            return -1;
          }
          if (leagueName1 > leagueName2) {
            return 1;
          }
          return 0;
        });
      } catch (error) {
        this.error = error.response.data;
      }
    },
    leagueLink(leagueId, year) {
      return `/league/${leagueId}/${year}`;
    }
  }
};
</script>
<style scoped>
.detail-label {
  font-weight: bold;
  margin-right: 4px;
}

.master-game-section,
.league-game-section {
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

.no-game-image-area {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.master-game-section {
  margin-top: 10px;
}

.counter-pick-label {
  margin-left: 3px;
}
</style>
