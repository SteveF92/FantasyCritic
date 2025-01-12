<template>
  <div v-if="interLeagueDataLoaded">
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
          <b-button v-if="isPlusUser" variant="primary" :to="{ name: 'createConference' }" class="main-button">Create a Conference</b-button>
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
                <template slot="title">
                  League Invites
                  <font-awesome-icon icon="exclamation-circle" size="lg" />
                </template>
                <leagueTable :leagues="invitedLeagues" :league-icon="'envelope'"></leagueTable>
              </b-tab>
              <b-tab v-if="myConferences && myConferences.length > 0" title="My Conferences" title-item-class="tab-header">
                <conferenceTable :conferences="myConferences"></conferenceTable>
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
          <TopBidsAndDrops :top-bids-and-drops="topBidsAndDrops" :process-date="topBidsAndDropsProcessDate"></TopBidsAndDrops>
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
              <b-table :sort-by.sync="sortBy" :sort-desc.sync="sortDesc" :items="publicLeagues" :fields="publicLeagueFields" bordered striped responsive small>
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

import TopBidsAndDrops from '@/components/topBidsAndDropsWidget.vue';
import LeagueTable from '@/components/leagueTable.vue';
import ConferenceTable from '@/components/conferenceTable.vue';
import GameNews from '@/components/gameNews.vue';

export default {
  components: {
    LeagueTable,
    ConferenceTable,
    GameNews,
    TopBidsAndDrops
  },
  data() {
    return {
      errorInfo: '',
      myLeagues: [],
      invitedLeagues: [],
      myFollowedLeagues: [],
      myConferences: [],
      selectedYear: null,
      activeRoyaleYearQuarter: null,
      publicLeagues: [],
      userRoyalePublisher: null,
      publicLeagueFields: [
        { key: 'leagueName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'numberOfFollowers', label: 'Followers', sortable: true, thClass: 'bg-primary' }
      ],
      sortBy: 'numberOfFollowers',
      sortDesc: true,
      gameNews: null,
      topBidsAndDropsProcessDate: null,
      topBidsAndDrops: null
    };
  },
  computed: {
    myStandardLeagues() {
      return this.myLeagues.filter((x) => !x.testLeague && !x.archived);
    },
    myArchivedLeagues() {
      return this.myLeagues.filter((x) => !x.testLeague && x.archived);
    },
    myTestLeagues() {
      return this.myLeagues.filter((x) => x.testLeague);
    }
  },
  async created() {
    this.selectedYear = this.supportedYears.filter((x) => x.openForPlay)[0].year;
    await this.fetchHomePageData();
  },
  methods: {
    async fetchHomePageData() {
      try {
        const response = await axios.get('/api/CombinedData/HomePageData');

        //My Leagues
        this.myLeagues = response.data.myLeagues.filter((x) => x.userIsInLeague);
        this.myFollowedLeagues = response.data.myLeagues.filter((x) => x.userIsFollowingLeague);
        this.fetchingLeagues = false;

        //Invited Leagues
        this.invitedLeagues = response.data.myInvites;

        //My Conferences
        this.myConferences = response.data.myConferences;

        //Game News
        this.gameNews = response.data.myGameNews;

        //Top Bids and Drops
        if (response.data.topBidsAndDrops) {
          this.topBidsAndDropsProcessDate = response.data.topBidsAndDrops.processDate;
          const allData = response.data.topBidsAndDrops.data;
          const yearWithMostData = response.data.topBidsAndDrops.yearWithMostData;
          this.topBidsAndDrops = allData[yearWithMostData];
        } else {
          this.topBidsAndDrops = [];
        }

        //Public Leagues
        this.publicLeagues = response.data.publicLeagues;

        //Active Royale Quarter
        this.activeRoyaleYearQuarter = response.data.activeRoyaleQuarter;

        //User Royale Publisher
        this.userRoyalePublisher = response.data.userRoyalePublisher;
      } catch (error) {
        this.errorInfo = error.response.data;
      }
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

div >>> div.card {
  background: rgba(50, 50, 50, 0.8);
}

div >>> .tab-header {
  margin-bottom: 5px;
}

div >>> .tab-header a {
  border-radius: 0px;
  font-weight: bolder;
  color: white;
}
</style>
