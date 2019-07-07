<template>
  <div>
    <div class="alert alert-warning" v-show="userInfo && !userInfo.emailConfirmed">
      <div>Your email address has not been confirmed. You cannot accept league invites until you do so.</div>
      <div>Check your email account for an email from us.</div>
      <span>If you are having issues, check out our <a href="/faq#technical" class="text-secondary" target="_blank">FAQ</a> page.</span>
    </div>

    <div class="col-md-10 offset-md-1 col-sm-12">
      <div class="text-well welcome-area" v-if="userInfo">
        <div class="row welcome-header">
          <h1>Welcome {{userInfo.displayName}}!</h1>
        </div>
        <div class="row main-buttons">
          <b-button variant="primary" :to="{ name: 'createLeague' }" class="main-button">Create a League</b-button>
          <b-button variant="info" :to="{ name: 'howtoplay' }" class="main-button">Learn to Play</b-button>
        </div>
      </div>

      <div class="row">
        <div class="col-lg-8 col-md-12">
          <b-card title="Leagues" class="homepage-section" v-if="userInfo">
            <b-tabs pills>
              <b-tab title="My Leagues" title-item-class="tab-header">
                <leagueTable :leagues="nonTestLeagues" :leagueIcon="'user'" :userID="userInfo.userID"></leagueTable>
              </b-tab>
              <b-tab v-if="anyInvitedLeagues" title-item-class="tab-header">
                <template slot="title">
                  League Invites
                  <font-awesome-icon icon="exclamation-circle" size="lg" />
                </template>
                <leagueTable :leagues="invitedLeagues" :leagueIcon="'envelope'" :userID="userInfo.userID"></leagueTable>
              </b-tab>
              <b-tab title="Followed Leagues" title-item-class="tab-header">
                <div v-if="anyFollowedLeagues">
                  <leagueTable :leagues="myFollowedLeagues" :leagueIcon="'users'" :userID="userInfo.userID"></leagueTable>
                </div>
                <div v-else>
                  <label>You are not following any public leagues!</label>
                </div>
              </b-tab>
              <b-tab title="Test Leagues" v-if="anyTestLeagues" title-item-class="tab-header">
                <leagueTable :leagues="testLeagues" :leagueIcon="'atom'" :userID="userInfo.userID"></leagueTable>
              </b-tab>
            </b-tabs>
          </b-card>
        </div>

        <div class="col-lg-4 col-md-12">
          <hr class="d-md-block d-lg-none" />
          <tweets class="homepage-section"></tweets>
        </div>
      </div>

      <hr />

      <div class="row">
        <div class="col-lg-8 col-md-12">
          <b-card title="My Upcoming Games" class="homepage-section">
            <div class="row" v-if="upcomingGames && upcomingGames.length > 0">
              <b-table :sort-by.sync="sortBy"
                       :sort-desc.sync="sortDesc"
                       :items="upcomingGames"
                       :fields="upcomingGamesFields"
                       bordered
                       striped
                       small>
                <template slot="gameName" slot-scope="data">
                  <masterGamePopover :masterGame="data.item"></masterGamePopover>
                </template>
                <template slot="sortableEstimatedReleaseDate" slot-scope="data">
                  {{getReleaseDate(data.item)}}
                </template>
              </b-table>
            </div>
          </b-card>
        </div>

        <div class="col-lg-4 col-md-12">
          <hr class="d-md-block d-lg-none" />
          <b-card title="Popular Public Leagues" class="homepage-section">
            <h5><router-link :to="{ name: 'publicLeagues' }">View All</router-link></h5>

            <div class="row" v-if="publicLeagues && publicLeagues.length > 0">
              <b-table :sort-by.sync="sortBy"
                       :sort-desc.sync="sortDesc"
                       :items="publicLeagues"
                       :fields="publicLeagueFields"
                       bordered
                       striped
                       small>
                <template slot="leagueName" slot-scope="data">
                  <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: selectedYear }}">{{data.item.leagueName}}</router-link>
                </template>
              </b-table>
            </div>
          </b-card>
        </div>
      </div>

      <h5 v-if="isAdmin"><router-link :to="{ name: 'adminConsole' }">Admin Console</router-link></h5>

    </div>
  </div>
</template>

<script>
  import Vue from "vue";
  import axios from "axios";
  import _ from "lodash";
  import Tweets from "components/modules/tweets";
  import MasterGamePopover from "components/modules/masterGamePopover";
  import LeagueTable from "components/modules/leagueTable";
  import moment from "moment";

  export default {
    data() {
      return {
        errorInfo: "",
        myLeagues: [],
        invitedLeagues: [],
        myFollowedLeagues: [],
        selectedYear: null,
        supportedYears: [],
        publicLeagues: [],
        publicLeagueFields: [
          { key: 'leagueName', label: 'Name', sortable: true, thClass: 'bg-primary' },
          { key: 'numberOfFollowers', label: 'Followers', sortable: true, thClass: 'bg-primary' },
        ],
        sortBy: 'numberOfFollowers',
        sortDesc: true,
        upcomingGames: [],
        upcomingGamesFields: [
          { key: 'gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
          { key: 'sortableEstimatedReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        ],
      }
    },
    components: {
      Tweets,
      MasterGamePopover,
      LeagueTable
    },
    computed: {
      anyManagedLeagues() {
        return _(this.myLeagues).some('isManager');
      },
      anyTestLeagues() {
        return _(this.myLeagues).some('testLeague');
      },
      anyPlayerLeagues() {
        return _.some(this.myLeagues, ['isManager', false]);
      },
      anyInvitedLeagues() {
        return this.invitedLeagues.length > 0;
      },
      anyFollowedLeagues() {
        return this.myFollowedLeagues.length > 0;
      },
      nonTestLeagues() {
        return _.filter(this.myLeagues, ['testLeague', false]);
      },
      testLeagues() {
        return _.filter(this.myLeagues, ['testLeague', true]);
      },
      noLeagues() {
        return (!(this.anyPlayerLeagues || this.anyManagedLeagues));
      },
      userInfo() {
        return this.$store.getters.userInfo;
      },
      isAdmin() {
        return this.$store.getters.isAdmin;
      }
    },
    methods: {
      async fetchMyLeagues() {
        axios
          .get('/api/League/MyLeagues')
          .then(response => {
            this.myLeagues = response.data;
            this.fetchingLeagues = false;
          })
          .catch(returnedError => (this.error = returnedError));
      },
      async fetchInvitedLeagues() {
        axios
          .get('/api/League/MyInvites')
          .then(response => {
            this.invitedLeagues = response.data;
          })
          .catch(returnedError => (this.error = returnedError));
      },
      async fetchFollowedLeagues() {
        axios
          .get('/api/League/FollowedLeagues')
          .then(response => {
            this.myFollowedLeagues = response.data;
          })
          .catch(returnedError => (this.error = returnedError));
      },
      async fetchSupportedYears() {
        axios
          .get('/api/game/SupportedYears')
          .then(response => {
            this.supportedYears = response.data;
            this.selectedYear = this.supportedYears[0];
            this.fetchPublicLeaguesForYear(this.selectedYear);
          })
          .catch(response => {

          });
      },
      async fetchPublicLeaguesForYear(year) {
        axios
          .get('/api/league/PublicLeagues/' + year + "?count=10")
          .then(response => {
            this.publicLeagues = response.data;
          })
          .catch(response => {

          });
      },
      async fetchUpcomingGames() {
        axios
          .get('/api/league/MyUpcomingGames/')
          .then(response => {
            this.upcomingGames = response.data;
          })
          .catch(response => {

          });
      },
      getReleaseDate(game) {
        if (game.releaseDate) {
          return moment(game.releaseDate).format('YYYY-MM-DD');
        }
        return game.estimatedReleaseDate + ' (Estimated)'
      }
    },
    async mounted() {
      await Promise.all([this.fetchMyLeagues(), this.fetchFollowedLeagues(), this.fetchInvitedLeagues(), this.fetchSupportedYears(), this.fetchUpcomingGames()]);
    }
  }
</script>
<style scoped>
  .welcome-area{
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

  .main-button{
    margin-top: 5px;
    min-width: 200px;
  }

  div >>> div.card {
    background: rgba(50, 50, 50, 0.8);
  }

  div >>> .tab-header a{
    border-radius: 0px;
    font-weight: bolder;
    color: white;   
  }

  div >>> .tab-header .active{
    background-color: #414141;
  }
</style>
