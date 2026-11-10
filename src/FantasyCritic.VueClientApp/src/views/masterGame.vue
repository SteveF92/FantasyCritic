<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <b-alert v-if="error" variant="danger" :model-value="true">{{ error }}</b-alert>
    <template v-if="masterGame">
      <div class="row master-game-section">
        <div class="col-xl-6 col-lg-12">
          <div class="game-image-area">
            <div v-if="ggLink && ggCoverArtLink" class="gg-image-area">
              <img v-if="ggCoverArtLink" :src="ggCoverArtLink" alt="Cover Image" class="game-image" />
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

          <MasterGameYearStats v-if="masterGameYears.length > 0" :master-game-id="mastergameid" :master-game-years="masterGameYears"></MasterGameYearStats>

          <div v-if="sortedYears.length > 0" class="text-well league-game-section">
            <h2>Your Leagues with this Game</h2>

            <div v-for="year in sortedYears" :key="year">
              <h3>{{ year }}</h3>
              <ul>
                <li v-for="league in leaguesWithGameByYear[year]" :key="league.leagueID">
                  <a :href="leagueLink(league.leagueID, league.year)">{{ league.leagueName }}</a>
                  <span v-if="league.isCounterPick" class="counter-pick-label">(Counter Picked)</span>
                </li>
              </ul>
            </div>
          </div>

          <div v-if="changeLog && changeLog.length > 0" class="text-well master-game-section">
            <h2>Change Log</h2>
            <b-table :items="changeLog" :fields="changeLogFields" bordered striped responsive small>
              <template #cell(timestamp)="data">
                {{ formatDateLong(data.item.timestamp) }}
              </template>
              <template #cell(changedByUser)="data">
                {{ data.item.changedByUser.displayName }}
              </template>
            </b-table>
          </div>
        </div>
      </div>

      <div v-if="masterGame.subGames && masterGame.subGames.length > 0" class="row">
        <h2>Sub Games (Episodes)</h2>
        <div v-for="subGame in masterGame.subGames" :key="subGame.masterGameID">
          <h3>{{ subGame.gameName }}</h3>
          <p>
            <span class="detail-label">Release Date:</span>
            <span v-if="subGame.releaseDate">{{ formatDateLong(subGame.releaseDate) }}</span>
            <span v-else>{{ subGame.estimatedReleaseDate }} (Estimated)</span>
          </p>
          <p v-if="subGame.openCriticID">
            <a :href="openCriticLink(subGame)" target="_blank">
              OpenCritic Link
              <font-awesome-icon icon="external-link-alt" size="xs" />
            </a>
          </p>
        </div>
      </div>
    </template>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import { type TableField } from 'bootstrap-vue-next';

import { gameClient, LeagueYearWithMasterGameViewModel, MasterGameChangeLogEntryViewModel, MasterGameViewModel, MasterGameYearViewModel, MasterSubGameViewModel } from '@/api/clients';
import { useAuthStore } from '@/store/authStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatDateLong } from '@/utilities/formatters';
import { getGGCoverArtLinkForGame, getGGLinkForGame } from '@/utilities/ggLinks';
import MasterGameDetails from '@/components/masterGameDetails.vue';
import MasterGameYearStats from '@/components/masterGameYearStats.vue';

interface MasterGameData {
  masterGame: MasterGameViewModel | null;
  masterGameYears: MasterGameYearViewModel[];
  leaguesWithGameByYear: Record<number, LeagueYearWithMasterGameViewModel[]>;
  changeLog: MasterGameChangeLogEntryViewModel[];
  error: string;
}

export default defineComponent({
  setup() {
    const baseChangeLogFields: TableField<MasterGameChangeLogEntryViewModel>[] = [
      { key: 'timestamp', label: 'Date of Change', thClass: 'bg-primary' },
      { key: 'description', label: 'Description', thClass: 'bg-primary' }
    ];
    const factCheckerChangeLogFields: TableField<MasterGameChangeLogEntryViewModel>[] = [{ key: 'changedByUser', label: 'Changed by', thClass: 'bg-primary' }];

    return { baseChangeLogFields, factCheckerChangeLogFields };
  },
  components: {
    MasterGameDetails,
    MasterGameYearStats
  },
  props: {
    mastergameid: { type: String, required: true }
  },
  data(): MasterGameData {
    return {
      masterGame: null,
      masterGameYears: [],
      leaguesWithGameByYear: {},
      changeLog: [],
      error: ''
    };
  },
  computed: {
    ...mapState(useAuthStore, ['isFactChecker']),
    ggCoverArtLink(): string | null {
      if (!this.masterGame) {
        return null;
      }
      return getGGCoverArtLinkForGame(this.masterGame, 307);
    },
    ggLink(): string | null {
      if (!this.masterGame) {
        return null;
      }
      return getGGLinkForGame(this.masterGame);
    },
    changeLogFields(): TableField<MasterGameChangeLogEntryViewModel>[] {
      if (this.isFactChecker) {
        return this.baseChangeLogFields.concat(this.factCheckerChangeLogFields);
      }

      return this.baseChangeLogFields;
    },
    sortedYears(): number[] {
      return Object.keys(this.leaguesWithGameByYear)
        .map((x) => Number(x))
        .sort((a, b) => b - a);
    }
  },
  watch: {
    async $route() {
      await this.loadAll();
    }
  },
  async created() {
    await this.loadAll();
  },
  methods: {
    formatDateLong,
    async loadAll() {
      await this.fetchMasterGame();
      const tasks = [this.fetchMasterGameChangeLog(), this.fetchMasterGameYears(), this.fetchLeaguesWithMasterGame()];
      await Promise.all(tasks);
    },
    async fetchMasterGame() {
      try {
        this.masterGame = await gameClient.masterGame(this.mastergameid);
      } catch (error) {
        this.error = getApiErrorMessage(error);
      }
    },
    async fetchMasterGameChangeLog() {
      try {
        this.changeLog = await gameClient.masterGameChangeLog(this.mastergameid);
      } catch (error) {
        this.error = getApiErrorMessage(error);
      }
    },
    async fetchMasterGameYears() {
      try {
        this.masterGameYears = await gameClient.masterGameYears(this.mastergameid);
      } catch (error) {
        this.error = getApiErrorMessage(error);
      }
    },
    async fetchLeaguesWithMasterGame() {
      try {
        const leagues = await gameClient.leagueYearsWithMasterGame(this.mastergameid);
        const sortedLeagues = [...leagues].sort((a, b) => a.leagueName.toLowerCase().localeCompare(b.leagueName.toLowerCase()));

        const leaguesByYear: Record<number, LeagueYearWithMasterGameViewModel[]> = {};
        for (const league of sortedLeagues) {
          (leaguesByYear[league.year] ??= []).push(league);
        }
        this.leaguesWithGameByYear = leaguesByYear;
      } catch (error) {
        this.error = getApiErrorMessage(error);
      }
    },
    leagueLink(leagueId: string, year: number): string {
      return `/league/${leagueId}/${year}`;
    },
    openCriticLink(subGame: MasterSubGameViewModel): string {
      return `https://opencritic.com/game/${subGame.openCriticID}/b`;
    }
  }
});
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
