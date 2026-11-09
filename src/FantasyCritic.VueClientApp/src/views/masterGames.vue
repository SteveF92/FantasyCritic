<template>
  <div v-if="interLeagueDataLoaded">
    <div class="col-lg-10 offset-lg-1 col-md-12">
      <div class="row">
        <div class="header-row">
          <div class="header-flex">
            <div class="page-name">
              <h1 class="header">Master Games List</h1>
            </div>

            <div class="selector-area">
              <b-form-select v-model="selectedYear" :options="selectableYears" class="year-selector" @update:model-value="changeYear" />
            </div>
          </div>
        </div>
      </div>
      <hr />
      <div class="master-games-nav">
        <template v-for="(item, index) in navItems" :key="item.name">
          <router-link :to="{ name: item.name }" class="master-games-nav-link text-primary">{{ item.label }}</router-link>
          <span v-if="index < navItems.length - 1" :key="item.name + '-sep'" class="master-games-nav-sep">|</span>
        </template>
      </div>

      <div class="special-selectors text-well">
        <div v-if="isPlusUser">
          <label class="league-label">View Games for League</label>
          <b-form-select v-model="selectedLeague" class="league-selector" @update:model-value="fetchGamesForYear">
            <template #first>
              <option value="">-- none --</option>
            </template>
            <option v-for="league in myLeaguesForYear" :key="league.leagueID" :value="league">
              {{ league.leagueName }}
            </option>
          </b-form-select>
          <br />
          <b-button v-show="selectedLeague && (eligibilityFilter || takenStatusFilter)" variant="info" class="clear-league-filter-button" @click="clearLeagueFilter">Clear League Filter</b-button>
          <b-form-group v-show="selectedLeague">
            <b-form-radio v-model="eligibilityFilter" value="eligibleOnly">Show only eligible games</b-form-radio>
            <b-form-radio v-model="eligibilityFilter" value="eligibleInOpenSlotOnly">Show only games eligible in open slot</b-form-radio>
            <b-form-radio v-model="eligibilityFilter" value="ineligibleOnly">Show only ineligible games</b-form-radio>
          </b-form-group>
          <b-form-group v-show="selectedLeague">
            <b-form-radio v-model="takenStatusFilter" value="taken">Show only games currently taken</b-form-radio>
            <b-form-radio v-model="takenStatusFilter" value="notTaken">Show only games not currently taken</b-form-radio>
          </b-form-group>
        </div>
        <b-form-checkbox v-model="unreleasedOnlyFilter">
          <span class="checkbox-label">Show only unreleased games</span>
        </b-form-checkbox>
      </div>

      <div v-if="showGames">
        <masterGamesTable :master-games="gamesToShow" :show-year-stats="selectedYearIsOpenForPlayOrFinished"></masterGamesTable>
      </div>

      <div v-else class="spinner">
        <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';

import { gameClient, leagueClient, LeagueWithStatusViewModel, LeagueYearViewModel, MasterGameYearViewModel, PossibleMasterGameYearViewModel } from '@/api/clients';
import { useAuthStore } from '@/store/authStore';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import MasterGamesTable from '@/components/gameTables/masterGamesTable.vue';

interface NavButton {
  name: string;
  label: string;
}

interface MasterGamesData {
  isBusy: boolean;
  selectedYear: number;
  eligibilityFilter: string | null;
  takenStatusFilter: string | null;
  unreleasedOnlyFilter: boolean;
  flatMasterGameYears: MasterGameYearViewModel[] | null;
  possibleMasterGameYears: PossibleMasterGameYearViewModel[] | null;
  myLeaguesForYear: LeagueWithStatusViewModel[];
  selectedLeague: LeagueYearViewModel | null;
}

const loggedInNavItems: NavButton[] = [
  { name: 'masterGameRequest', label: 'Request Game' },
  { name: 'masterGameChangeRequest', label: 'Suggest Correction' }
];

const unconditionalNavItems: NavButton[] = [
  { name: 'gameChanges', label: 'Recent Changes' },
  { name: 'mostDesiredReviews', label: 'Most Desired Reviews' },
  { name: 'advancedGameStats', label: 'Advanced Stats' },
  { name: 'topBidsAndDrops', label: 'Top Bids/Drops' }
];

export default defineComponent({
  setup() {
    const interLeagueStore = useInterLeagueStore();
    const mostRecentYear = interLeagueStore.mostRecentYear.year;
    return { mostRecentYear };
  },
  components: {
    MasterGamesTable
  },
  data(): MasterGamesData {
    return {
      isBusy: true,
      selectedYear: this.mostRecentYear,
      eligibilityFilter: null,
      takenStatusFilter: null,
      unreleasedOnlyFilter: false,
      flatMasterGameYears: null,
      possibleMasterGameYears: null,
      myLeaguesForYear: [],
      selectedLeague: null
    };
  },
  computed: {
    ...mapState(useAuthStore, ['isPlusUser', 'isAuth']),
    ...mapState(useInterLeagueStore, ['interLeagueDataLoaded', 'supportedYears']),
    selectableYears(): number[] {
      return this.supportedYears.map((x) => x.year);
    },
    navItems(): NavButton[] {
      const items: NavButton[] = [];
      if (this.isAuth) {
        items.push(...loggedInNavItems);
      }
      items.push(...unconditionalNavItems);
      return items;
    },
    gamesToShow(): MasterGameYearViewModel[] {
      if (this.flatMasterGameYears) {
        if (!this.unreleasedOnlyFilter) {
          return this.flatMasterGameYears;
        }

        return this.flatMasterGameYears.filter((x) => !x.isReleased);
      }

      if (this.possibleMasterGameYears) {
        let filteredGames = this.possibleMasterGameYears;
        if (this.eligibilityFilter === 'eligibleOnly') {
          filteredGames = filteredGames.filter((x) => x.isEligible);
        } else if (this.eligibilityFilter === 'eligibleInOpenSlotOnly') {
          filteredGames = filteredGames.filter((x) => x.isEligibleInOpenSlot);
        } else if (this.eligibilityFilter === 'ineligibleOnly') {
          filteredGames = filteredGames.filter((x) => !x.isEligible);
        }

        if (this.takenStatusFilter === 'taken') {
          filteredGames = filteredGames.filter((x) => x.taken);
        } else if (this.takenStatusFilter === 'notTaken') {
          filteredGames = filteredGames.filter((x) => !x.taken);
        }

        if (this.unreleasedOnlyFilter) {
          filteredGames = filteredGames.filter((x) => !x.isReleased);
        }

        const flattenedGames = filteredGames.map((v) => v.masterGame);
        return flattenedGames;
      }

      return [];
    },
    showGames(): boolean {
      return this.gamesToShow && !this.isBusy;
    },
    selectedYearIsOpenForPlayOrFinished(): boolean {
      const selectedYear = this.supportedYears.filter((x) => x.year === this.selectedYear)[0];
      return selectedYear.openForPlay || selectedYear.finished;
    }
  },
  async created() {
    this.selectedYear = this.supportedYears.filter((x) => x.openForPlay)[0].year;
    await Promise.all([this.fetchGamesForYear(), this.fetchMyLeaguesForYear()]);
  },
  methods: {
    async fetchGamesForYear() {
      this.isBusy = true;
      this.flatMasterGameYears = null;
      this.possibleMasterGameYears = null;

      if (!this.selectedLeague) {
        this.flatMasterGameYears = await gameClient.masterGameYearAll(this.selectedYear);
      } else {
        this.possibleMasterGameYears = await gameClient.masterGameYearInLeagueContext(this.selectedYear, this.selectedLeague.leagueID);
      }

      this.isBusy = false;
    },
    async fetchMyLeaguesForYear() {
      if (!this.isAuth) {
        return;
      }
      const response = await leagueClient.myLeagues(this.selectedYear);
      this.myLeaguesForYear = response.filter((x) => !x.testLeague && !x.userIsFollowingLeague && x.userIsActiveInMostRecentYearForLeague && x.leagueIsActiveInActiveYear);
    },
    async changeYear() {
      await Promise.all([this.fetchGamesForYear(), this.fetchMyLeaguesForYear()]);
    },
    clearLeagueFilter() {
      this.eligibilityFilter = null;
      this.takenStatusFilter = null;
      this.unreleasedOnlyFilter = false;
    }
  }
});
</script>

<style scoped>
.header-row {
  width: 100%;
}

.header-flex {
  display: flex;
  justify-content: space-between;
  flex-wrap: wrap;
}

.selector-area {
  display: flex;
  align-items: flex-start;
}

.year-selector {
  width: 100px;
}

.master-games-nav {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: center;
  gap: 0.35rem;
  margin-bottom: 1rem;
  font-size: 1.05rem;
}

.master-games-nav-sep {
  color: #6c757d;
  user-select: none;
}

.league-label {
  margin-right: 10px;
}

.league-selector {
  width: 250px;
}

.special-selectors {
  margin: 10px;
  flex-wrap: wrap;
}

.special-selectors div {
  margin-right: 10px;
}

.spinner {
  display: flex;
  justify-content: space-around;
}

.clear-league-filter-button {
  margin-top: 10px;
  margin-bottom: 10px;
}
</style>
