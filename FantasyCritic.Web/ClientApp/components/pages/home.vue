<template>
  <div>
    <div class="alert alert-warning" v-show="userInfo && !userInfo.emailConfirmed">
      <div>Your email address has not been confirmed. You cannot accept league invites until you do so.</div>
      <div>Check your email account for an email from us.</div>
      <span>If you are having issues, check out our <a href="/faq#technical" class="text-secondary" target="_blank">FAQ</a> page.</span>
    </div>

    <div class="col-md-10 offset-md-1 col-sm-12 text-well welcome-area">
      <div class="row welcome-header">
        <h1>Welcome {{userInfo.displayName}}!</h1>
      </div>
      <div class="row main-buttons">
        <b-button variant="primary" :to="{ name: 'createLeague' }" class="main-button">Create a League</b-button>
        <b-button variant="info" :to="{ name: 'howtoplay' }" class="main-button">Learn to Play</b-button>
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
  .welcome-area{
    margin-top: 10px;
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
</style>
