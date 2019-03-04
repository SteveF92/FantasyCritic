<template>
  <div>
    <h1>Welcome to Fantasy Critic!</h1>
    <div class="alert alert-warning" v-show="userInfo && !userInfo.emailConfirmed">
      <div>Your email address has not been confirmed. You cannot accept league invites until you do so.</div>
      <div>Check your email account for an email from us.</div>
      <span>If you are having issues, check out our <a href="/faq#technical" class="text-secondary" target="_blank">FAQ</a> page.</span>
    </div>

    <div class="row">
      <div class="col-lg-6 col-md-12">
        <b-button variant="info" :to="{ name: 'howtoplay' }" v-if="shouldShowLearnToPlay" class="learn-to-play-button">Learn to Play</b-button>
        <b-button variant="primary" :to="{ name: 'createLeague' }" class="create-league-button">Create a League</b-button>
        <div v-if="anyInvitedLeagues">
          <h2>League Invites</h2>
          <ul>
            <li v-for="league in invitedLeagues">
              <router-link v-show="userInfo.emailConfirmed" :to="{ name: 'league', params: { leagueid: league.leagueID, year: league.activeYear }}">{{league.leagueName}}</router-link>
              <div v-show="!userInfo.emailConfirmed">{{league.leagueName}}</div>
            </li>
          </ul>
          <hr />
        </div>

        <div v-if="anyManagedLeagues">
          <h2>Leagues I Manage</h2>
          <ul>
            <li v-for="league in myLeagues" v-if="league.isManager && !league.testLeague">
              <router-link :to="{ name: 'league', params: { leagueid: league.leagueID, year: league.activeYear }}">{{league.leagueName}}</router-link>
            </li>
          </ul>
          <hr />
        </div>

        <div v-if="anyPlayerLeagues">
          <h2>Leagues I Play In</h2>
          <ul>
            <li v-for="league in myLeagues" v-if="!league.isManager && !league.testLeague">
              <router-link :to="{ name: 'league', params: { leagueid: league.leagueID, year: league.activeYear }}">{{league.leagueName}}</router-link>
            </li>
          </ul>
          <hr />
        </div>
        <div>
          <h2>Leagues I'm Following</h2>
          <h5><router-link :to="{ name: 'publicLeagues' }">Full List of Public Leagues</router-link></h5>
          <div v-if="anyFollowedLeagues">
            <ul>
              <li v-for="league in myFollowedLeagues">
                <router-link :to="{ name: 'league', params: { leagueid: league.leagueID, year: league.activeYear }}">{{league.leagueName}}</router-link>
              </li>
            </ul>
          </div>
          <div v-else>
            <label>You are not following any public leagues!</label>
          </div>
          <hr />
        </div>

        <div v-if="anyTestLeagues">
          <h2>Test Leagues</h2>
          <ul>
            <li v-for="league in myLeagues" v-if="league.testLeague">
              <router-link :to="{ name: 'league', params: { leagueid: league.leagueID, year: league.activeYear }}">{{league.leagueName}}</router-link>
            </li>
          </ul>
          <hr />
        </div>

        <div v-if="!fetchingLeagues && noLeagues">
          <h3>You are not part of any leagues! Why not start one?</h3>
          <hr />
        </div>
        <h5 v-if="isAdmin"><router-link :to="{ name: 'adminConsole' }">Admin Console</router-link></h5>

      </div>
      <div class="col-lg-6 col-md-12">
        <tweets></tweets>
      </div>
    </div>
  </div>
</template>

<script>
  import Vue from "vue";
  import axios from "axios";
  import _ from "lodash";
  import Tweets from "components/modules/tweets";

  export default {
    data() {
      return {
        errorInfo: "",
        myLeagues: [],
        invitedLeagues: [],
        myFollowedLeagues: [],
        fetchingLeagues: true
      }
    },
    components: {
      Tweets
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
      noLeagues() {
        return (!(this.anyPlayerLeagues || this.anyManagedLeagues));
      },
      userInfo() {
        return this.$store.getters.userInfo;
      },
      isAdmin() {
        return this.$store.getters.isAdmin;
      },
      shouldShowLearnToPlay() {
        if (this.fetchingLeagues) {
          return false;
        }

        if (this.noLeagues) {
          return true;
        }

        return false;
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
      }
    },
    async mounted() {
      await Promise.all([this.fetchMyLeagues(), this.fetchFollowedLeagues(), this.fetchInvitedLeagues()]);
    }
  }
</script>
<style scoped>
  .learn-to-play-button {
    width: 100%;
    margin-bottom: 8px;
  }
  .create-league-button {
    width: 100%;
  }
</style>
