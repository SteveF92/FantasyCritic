<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div v-if="forbidden">
        <div class="alert alert-danger" role="alert">
          You do not have permission to view this league.
        </div>
      </div>
      <div v-if="league">
        <div v-if="errorInfo" class="alert alert-danger" role="alert">
          {{errorInfo}}
        </div>

        <div class="row">
          <div class="league-header-row">
            <div class="league-header-flex">
              <div class="league-name">
                <h1>{{ league.leagueName }}</h1>
              </div>

              <div class="selector-area">
                <div v-if="!league.userIsInLeague && isAuth">
                  <b-button v-if="!league.userIsFollowingLeague" variant="primary" v-on:click="followLeague">Follow</b-button>
                  <b-button v-if="league.userIsFollowingLeague" variant="secondary" v-on:click="unfollowLeague">Unfollow</b-button>
                </div>
                <b-form-select v-model="selectedYear" :options="league.years" v-on:change="changeLeagueYear" class="year-selector" />
              </div>
            </div>
          </div>
        </div>
        <hr />

        <div class="league-manager-info">
          <h4>League Manager:</h4>
          <span class="league-manager-info-item">{{ league.leagueManager.displayName }}</span>
        </div>

        <div>
          <label>Followers: </label> {{league.numberOfFollowers}}
        </div>

        <div v-if="league.publicLeague && !(league.userIsInLeague || league.outstandingInvite)" class="alert alert-info" role="info">
          You are viewing a public league.
        </div>

        <b-modal id="draftFinishedModal" ref="draftFinishedModalRef" title="Draft Complete!">
          <p v-if="league.publicLeague && !(league.userIsInLeague || league.outstandingInvite)">
            The draft is complete!
          </p>
          <p v-else>
            The draft is complete! From here you can make bids for games that were not drafted, however, you may want to hold onto your available budget until later in the year!
          </p>
        </b-modal>

        <div v-if="inviteCode && !league.userIsInLeague" class="alert alert-info">
          You have been invited to join this league.
          <b-button variant="primary" v-if="isAuth" v-on:click="joinWithInviteLink()" class="mx-2">Join League</b-button>
          <template v-else>
            <b-button variant="primary" :to="{ name: 'login', query: { leagueid: league.leagueID, year: year, inviteCode: inviteCode }}">
              <span>Log In</span>
              <font-awesome-icon class="full-nav" icon="sign-in-alt" />
            </b-button>
            <b-button variant="primary" :to="{ name: 'register', query: { leagueid: league.leagueID, year: year, inviteCode: inviteCode }}">
              <span>Sign Up</span>
              <font-awesome-icon class="full-nav" icon="user-plus" />
            </b-button>
          </template>
        </div>

        <div v-if="league.outstandingInvite" class="alert alert-info">
          You have been invited to join this league. Do you wish to join?
          <div class="row">
            <div class="btn-toolbar">
              <b-button variant="primary" v-on:click="acceptInvite" class="mx-2">Join</b-button>
              <b-button variant="secondary" v-on:click="declineInvite" class="mx-2">Decline</b-button>
            </div>
          </div>
        </div>

        <div v-if="leagueYear">
          <div v-if="leagueYear.playStatus.playStarted && leagueYear.supportedYear.finished">
            <div class="alert alert-success" role="alert">
              This year is finished! The winner is <strong>{{topPublisher.publisherName}}</strong>!
            </div>
          </div>
          <div v-if="!leagueYear.userIsActive && league.userIsInLeague">
            <div class="alert alert-warning" role="alert">
              You are set to inactive for this year.
            </div>
          </div>

          <div v-if="leagueYear.userIsActive && !leagueYear.playStatus.readyToDraft" class="alert alert-warning">
            <h2>
              This year is not active yet!
            </h2>
            <ul>
              <li v-for="error in leagueYear.playStatus.startDraftErrors">{{error}}</li>
            </ul>
          </div>

          <div v-if="leagueYear.userIsActive && !leagueYear.userPublisher" class="alert alert-info">
            You need to create your publisher for this year.
            <b-button variant="primary" v-b-modal="'createPublisher'" class="mx-2">Create Publisher</b-button>
            <createPublisherForm :leagueYear="leagueYear" v-on:actionTaken="actionTaken"></createPublisherForm>
          </div>

          <div v-if="!leagueYear.playStatus.playStarted && leagueYear.playStatus.readyToDraft && !league.outstandingInvite">
            <div class="alert alert-success">
              <span v-if="league.isManager">
                Things are all set to get started! <b-button variant="primary" v-b-modal="'startDraft'" class="mx-2">Start Drafting!</b-button>
              </span>
              <span v-if="!league.isManager">
                Things are all set to get started! Your league manager can choose when to begin the draft.
              </span>
            </div>
            <startDraftModal v-on:draftStarted="startDraft"></startDraftModal>
          </div>

          <div v-if="leagueYear.playStatus.draftIsPaused">
            <div class="alert alert-danger">
              <div v-show="!league.isManager">The draft has been paused. Speak to your league manager for details.</div>
              <div v-show="league.isManager">The draft has been paused. You can undo games that have been drafted. Press 'Resume Draft' to go back to picking games.</div>
            </div>
          </div>
          <div v-if="leagueYear.playStatus.draftIsActive && nextPublisherUp">
            <div v-if="!userIsNextInDraft">
              <div class="alert alert-info">
                <div v-show="!leagueYear.playStatus.draftingCounterPicks">The draft is currently in progress!</div>
                <div v-show="leagueYear.playStatus.draftingCounterPicks">It's time to draft Counter-Picks!</div>
                <div>Next to draft: <strong>{{nextPublisherUp.publisherName}}</strong></div>
                <div v-show="league.isManager">To select the next player's game for them, Select 'Select Next Game' under 'Draft Management' in the sidebar!</div>
              </div>
            </div>
            <div v-else>
              <div class="alert alert-success">
                <div v-show="!leagueYear.playStatus.draftingCounterPicks">The draft is currently in progress!</div>
                <div v-show="leagueYear.playStatus.draftingCounterPicks">It's time to draft counter picks!</div>
                <div><strong>It is your turn to draft!</strong></div>
                <div v-show="!leagueYear.playStatus.draftingCounterPicks">Select 'Draft Game' under 'Player Actions' in the sidebar!</div>
                <div v-show="leagueYear.playStatus.draftingCounterPicks">Select 'Draft Counterpick' under 'Player Actions' in the sidebar!</div>
              </div>
            </div>
          </div>

          <div class="row">
            <div class="col-xl-3 col-lg-4 col-md-12">
              <leagueActions ref="leagueActionsRef" :league="league" :leagueYear="leagueYear"
                             :currentBids="currentBids" :userIsNextInDraft="userIsNextInDraft" :nextPublisherUp="nextPublisherUp" v-on:actionTaken="actionTaken"></leagueActions>
            </div>
            <div class="col-xl-9 col-lg-8 col-md-12">
              <leagueYearStandings :league="league" :leagueYear="leagueYear" v-on:actionTaken="actionTaken"></leagueYearStandings>
              <h2>Summary</h2>
              <leagueGameSummary :leagueYear="leagueYear"></leagueGameSummary>
            </div>
          </div>
        </div>
      </div>
    </div>
    <audio src="/sounds/quite-impressed.mp3" id="draft-notification-sound"></audio>
  </div>
</template>

<script>
  import Vue from "vue";
  import axios from "axios";
  import moment from "moment";
  import { HubConnection } from '@aspnet/signalr';
  import * as signalR from '@aspnet/signalr';

  import LeagueGameSummary from "components/modules/leagueGameSummary";
  import LeagueYearStandings from "components/modules/leagueYearStandings";
  import LeagueActions from "components/modules/leagueActions";
  import CreatePublisherForm from "components/modules/modals/createPublisherForm";
  import StartDraftModal from "components/modules/modals/startDraftModal";

  export default {
    data() {
      return {
        errorInfo: "",
        league: null,
        leagueYear: null,
        currentBids: [],
        leagueActions: [],
        forbidden: false,
        advancedProjections: false,
        inviteCode: null
      }
    },
    props: ['leagueid', 'year'],
    components: {
      LeagueGameSummary,
      LeagueYearStandings,
      CreatePublisherForm,
      StartDraftModal,
      LeagueActions
    },
    computed: {
      nextPublisherUp() {
        if (!this.leagueYear || !this.leagueYear.publishers) {
          return null;
        }
        let next = _.find(this.leagueYear.publishers, ['nextToDraft', true]);
        return next;
      },
      userIsNextInDraft() {
        if (this.nextPublisherUp && this.leagueYear && this.leagueYear.userPublisher) {
          return this.nextPublisherUp.publisherID === this.leagueYear.userPublisher.publisherID
        }

        return false;
      },
      isAuth() {
        return this.$store.getters.tokenIsCurrent();
      },
      topPublisher() {
        if (this.leagueYear.publishers && this.leagueYear.publishers.length > 0) {
          return _.maxBy(this.leagueYear.publishers, 'totalFantasyPoints');
        }
      }
    },
    methods: {
      formatDate(date) {
        return moment(date).format('MMMM Do, YYYY');
      },
      fetchLeague() {
        axios
          .get('/api/League/GetLeague/' + this.leagueid)
          .then(response => {
            this.league = response.data;
            document.title = this.league.leagueName + " - Fantasy Critic";
          })
          .catch(returnedError => {
            this.error = returnedError;
            this.forbidden = (returnedError.response.status === 403);
          });
      },
      fetchLeagueYear() {
          axios
              .get('/api/League/GetLeagueYear?leagueID=' + this.leagueid + '&year=' + this.year)
              .then(response => {
                this.leagueYear = response.data;
                this.selectedYear = this.leagueYear.year;
                this.fetchCurrentBids();
                this.fetchLeagueActions();
              })
            .catch(returnedError => (this.error = returnedError));
      },
      fetchCurrentBids() {
        axios
          .get('/api/league/CurrentBids/' + this.leagueYear.userPublisher.publisherID)
          .then(response => {
            this.currentBids = response.data;
          })
          .catch(response => {

          });
      },
      acceptInvite() {
          var model = {
              leagueID: this.league.leagueID
          };
          axios
            .post('/api/league/AcceptInvite', model)
            .then(response => {
              this.fetchLeague();
              this.fetchLeagueYear();
            })
            .catch(response => {

            });
      },
      declineInvite() {
          var model = {
            inviteID: this.league.outstandingInvite.inviteID
          };
          axios
            .post('/api/league/DeclineInvite', model)
            .then(response => {
                this.$router.push({ name: "home" });
            })
            .catch(response => {

            });
      },
      joinWithInviteLink() {
        var model = {
            leagueID: this.league.leagueID,
            inviteCode: this.inviteCode
          };
          axios
            .post('/api/league/JoinWithInviteLink', model)
            .then(response => {
              this.fetchLeague();
              this.fetchLeagueYear();
            })
            .catch(response => {
              this.errorInfo = "Something went wrong joining the league";
            });
      },
      startDraft() {
        var model = {
          leagueID: this.league.leagueID,
          year: this.leagueYear.year
        };
        axios
          .post('/api/leagueManager/startDraft', model)
          .then(response => {
            this.fetchLeague();
            this.fetchLeagueYear();
          })
          .catch(response => {

          });
      },
      actionTaken(actionInfo) {
        if (actionInfo.fetchLeagueYear) {
          this.fetchLeagueYear();
        }
        if (actionInfo.fetchLeague) {
          this.fetchLeague();
        }
        if (actionInfo.fetchCurrentBids) {
          this.fetchCurrentBids();
        }
        let toast = this.$toasted.show(actionInfo.message, {
          theme: "primary",
          position: "top-right",
          duration: 5000
        });
      },
      changeLeagueYear(newVal) {
        var parameters = {
          leagueid: this.leagueid,
          year: newVal
        };
        this.$router.push({ name: "league", params: parameters });
      },
      followLeague() {
        var model = {
          leagueID: this.league.leagueID
        };
        axios
          .post('/api/league/FollowLeague', model)
          .then(response => {
            this.fetchLeague();
          })
          .catch(response => {

          });
      },
      unfollowLeague() {
        var model = {
          leagueID: this.league.leagueID
        };
        axios
          .post('/api/league/UnfollowLeague', model)
          .then(response => {
            this.fetchLeague();
          })
          .catch(response => {

          });
      },
      getInviteCode() {
        let inviteCode = this.$route.query.inviteCode;
        if (inviteCode) {
          this.inviteCode = inviteCode;
        }
      },
      async startHubConnection() {
        let token = this.$store.getters.token;
        let hubConnection = new signalR.HubConnectionBuilder()
          .withUrl("/updatehub", { accessTokenFactory: () => token })
          .configureLogging(signalR.LogLevel.Error)
          .build();

        await hubConnection.start().catch(err => console.error(err.toString()));

        hubConnection.invoke("Subscribe", this.leagueid, this.year).catch(err => console.error(err.toString()));

        hubConnection.on('RefreshLeagueYear', data => {
          this.fetchLeagueYear();
        });
        hubConnection.on('DraftFinished', data => {
          this.$refs.draftFinishedModalRef.show();
        });
        hubConnection.onclose(async () => {
          await this.startHubConnection();
        });
      }
    },
    async mounted() {
      this.selectedYear = this.year;
      this.fetchLeague();
      this.fetchLeagueYear();
      await this.startHubConnection();
      this.getInviteCode();
    },
    watch: {
      '$route'(to, from) {
          this.fetchLeagueYear();
      },
      userIsNextInDraft: function (val, oldVal) {
        if (val && val !== oldVal) {
          document.getElementById('draft-notification-sound').play();
        }
      }
    }
  }
</script>
<style scoped>
  
  .league-manager-info {
    display: flex;
    flex-direction: row;
  }
  .league-manager-info-item {
    padding-left: 5px;
    padding-top: 3px;
  }

  .league-header-row {
    width: 100%;
  }

  .league-header-flex {
    display: flex;
    justify-content: space-between;
    flex-wrap: wrap;
  }

  .selector-area{
    display: flex;
    align-items: flex-start;
  }

  .year-selector {
    width: 100px;
  }

  .league-name {
    display: block;
    max-width: 100%;
    word-wrap: break-word; 
  }
</style>
