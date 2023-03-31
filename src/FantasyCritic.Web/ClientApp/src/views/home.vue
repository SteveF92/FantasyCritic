<template>
  <div>
    <div v-if="userInfo && !userInfo.emailConfirmed" class="alert alert-warning">
      <div>Your email address has not been confirmed. You cannot accept league invites via email until you do so.</div>
      <div>Check your email account for an email from us.</div>
      <span>
        If you are having issues, check out our
        <a href="/faq#technical" class="text-secondary" target="_blank">FAQ</a>
        page.
      </span>
    </div>

    <div class="col-md-10 offset-md-1 col-sm-12">
      <div v-if="userInfo" class="text-well welcome-area">
        <div class="row welcome-header">
          <h1>Welcome {{ userInfo.displayName }}!</h1>
        </div>
        <div v-if="activeRoyaleYearQuarter" class="row main-buttons">
          <b-button variant="primary" :to="{ name: 'createLeague' }" class="main-button">Create a League</b-button>
          <b-button
            v-if="!userRoyalePublisher"
            variant="primary"
            :to="{ name: 'criticsRoyale', params: { year: activeRoyaleYearQuarter.year, quarter: activeRoyaleYearQuarter.quarter } }"
            class="main-button">
            Play Critics Royale
          </b-button>
          <b-button v-if="userRoyalePublisher" variant="primary" :to="{ name: 'royalePublisher', params: { publisherid: userRoyalePublisher.publisherID } }" class="main-button">
            Critics Royale
          </b-button>
          <b-button variant="info" :to="{ name: 'howtoplay' }" class="main-button">Learn to Play</b-button>
          <b-button v-show="isFactChecker || isAdmin" variant="warning" :to="{ name: 'adminConsole' }" class="main-button">Admin Console</b-button>
        </div>
      </div>

      <div class="row">
        <div class="col-lg-8 col-md-12">
          <b-card v-if="userInfo" title="Leagues">
            <b-tabs pills class="league-tabs">
              <b-tab title="My Leagues" title-item-class="tab-header">
                <div v-if="myStandardLeagues && myStandardLeagues.length > 0">
                  <leagueTable :leagues="myStandardLeagues" :league-icon="'user'" :show-archive="true"></leagueTable>
                </div>
                <div v-else>
                  <label>You are not in any leagues! Why not create one?</label>
                </div>
              </b-tab>
              <b-tab v-if="invitedLeagues && invitedLeagues.length > 0" title-item-class="tab-header">
                <template #title>
                  League Invites
                  <font-awesome-icon icon="exclamation-circle" size="lg" />
                </template>
                <leagueTable :leagues="invitedLeagues" :league-icon="'envelope'"></leagueTable>
              </b-tab>
              <b-tab title="Followed Leagues" title-item-class="tab-header">
                <div v-if="myFollowedLeagues && myFollowedLeagues.length > 0">
                  <leagueTable :leagues="myFollowedLeagues" :league-icon="'users'"></leagueTable>
                </div>
                <div v-else>
                  <label>You are not following any public leagues!</label>
                </div>
              </b-tab>
              <b-tab v-if="myArchivedLeagues && myArchivedLeagues.length > 0" title="Archived Leagues" title-item-class="tab-header">
                <leagueTable :leagues="myArchivedLeagues" :league-icon="'archive'" :show-un-archive="true"></leagueTable>
              </b-tab>
              <b-tab v-if="myTestLeagues && myTestLeagues.length > 0" title="Test Leagues" title-item-class="tab-header">
                <leagueTable :leagues="myTestLeagues" :league-icon="'atom'"></leagueTable>
              </b-tab>
            </b-tabs>
          </b-card>
        </div>

        <div class="col-lg-4 col-md-12">
          <hr class="d-md-block d-lg-none" />
          <tweets></tweets>
        </div>
      </div>

      <hr />

      <div class="row">
        <div class="col-lg-8 col-md-12">
          <b-card v-if="gameNews">
            <gameNews :game-news="gameNews" mode="user" />
          </b-card>
        </div>

        <div class="col-lg-4 col-md-12">
          <hr class="d-md-block d-lg-none" />
          <b-card title="Popular Public Leagues">
            <h5><router-link :to="{ name: 'publicLeagues' }">View All</router-link></h5>

            <div v-if="publicLeagues && publicLeagues.length > 0" class="row">
              <b-table v-model:sort-by="sortBy" v-model:sort-desc="sortDesc" :items="publicLeagues" :fields="publicLeagueFields" bordered striped responsive small>
                <template #cell(leagueName)="data">
                  <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: selectedYear } }">{{ data.item.leagueName }}</router-link>
                </template>
              </b-table>
            </div>
          </b-card>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import _ from 'lodash';

import Tweets from '@/components/tweets.vue';
import LeagueTable from '@/components/leagueTable.vue';
import GameNews from '@/components/gameNews.vue';

export default {
  components: {
    Tweets,
    LeagueTable,
    GameNews
  },
  data() {
    return {
      errorInfo: '',
      myLeagues: [],
      invitedLeagues: [],
      myFollowedLeagues: [],
      selectedYear: null,
      supportedYears: [],
      activeRoyaleYearQuarter: null,
      publicLeagues: [],
      userRoyalePublisher: null,
      publicLeagueFields: [
        { key: 'leagueName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'numberOfFollowers', label: 'Followers', sortable: true, thClass: 'bg-primary' }
      ],
      sortBy: 'numberOfFollowers',
      sortDesc: true,
      gameNews: null
    };
  },
  computed: {
    myStandardLeagues() {
      let nonTest = _.filter(this.myLeagues, ['testLeague', false]);
      return _.filter(nonTest, ['archived', false]);
    },
    myArchivedLeagues() {
      let nonTest = _.filter(this.myLeagues, ['testLeague', false]);
      return _.filter(nonTest, ['archived', true]);
    },
    myTestLeagues() {
      return _.filter(this.myLeagues, ['testLeague', true]);
    }
  },
  async mounted() {
    const tasks = [this.fetchMyLeagues(), this.fetchFollowedLeagues(), this.fetchInvitedLeagues(), this.fetchSupportedYears(), this.fetchGameNews(), this.fetchActiveRoyaleYearQuarter()];
    await Promise.all(tasks);
  },
  methods: {
    fetchMyLeagues() {
      return axios
        .get('/api/League/MyLeagues')
        .then((response) => {
          this.myLeagues = response.data;
          this.fetchingLeagues = false;
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    fetchInvitedLeagues() {
      return axios
        .get('/api/League/MyInvites')
        .then((response) => {
          this.invitedLeagues = response.data;
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    fetchFollowedLeagues() {
      return axios
        .get('/api/League/FollowedLeagues')
        .then((response) => {
          this.myFollowedLeagues = response.data;
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    fetchSupportedYears() {
      return axios
        .get('/api/game/SupportedYears')
        .then((response) => {
          let supportedYears = response.data;
          let openYears = _.filter(supportedYears, { openForPlay: true });
          let finishedYears = _.filter(supportedYears, { finished: true });
          this.supportedYears = openYears.concat(finishedYears).map(function (v) {
            return v.year;
          });
          this.selectedYear = this.supportedYears[0];
          this.fetchPublicLeaguesForYear(this.selectedYear);
        })
        .catch(() => {});
    },
    fetchActiveRoyaleYearQuarter() {
      return axios
        .get('/api/royale/ActiveRoyaleQuarter')
        .then((response) => {
          this.activeRoyaleYearQuarter = response.data;
          this.fetchUserRoyalePublisher();
        })
        .catch(() => {});
    },
    fetchPublicLeaguesForYear(year) {
      return axios
        .get('/api/league/PublicLeagues/' + year + '?count=10')
        .then((response) => {
          this.publicLeagues = response.data;
        })
        .catch(() => {});
    },
    fetchGameNews() {
      return axios
        .get('/api/league/MyGameNews/')
        .then((response) => {
          this.gameNews = response.data;
        })
        .catch(() => {});
    },
    fetchUserRoyalePublisher() {
      this.userRoyalePublisher = null;
      return axios
        .get('/api/royale/GetUserRoyalePublisher/' + this.activeRoyaleYearQuarter.year + '/' + this.activeRoyaleYearQuarter.quarter)
        .then((response) => {
          this.userRoyalePublisher = response.data;
        })
        .catch(() => {});
    }
  }
};
</script>
<style scoped>
.welcome-area {
  margin-top: 10px;
  margin-bottom: 20px;
}
.welcome-header {
  justify-content: center;
  text-align: center;
}

.main-buttons {
  display: flex;
  flex-direction: row;
  justify-content: space-around;
}

.main-button {
  margin-top: 5px;
  min-width: 200px;
}

div :deep(div.card) {
  background: rgba(50, 50, 50, 0.8);
}

div :deep(.tab-header) {
  margin-bottom: 5px;
}

div :deep(.tab-header a) {
  border-radius: 0px;
  font-weight: bolder;
  color: white;
}
</style>
