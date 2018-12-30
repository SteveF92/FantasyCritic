<template>
  <div v-if="league">
    <div v-if="errorInfo" class="alert alert-danger" role="alert">
      {{errorInfo}}
    </div>

    <div class="row league-header">
      <h2>{{ league.leagueName }}</h2>
      <div class="year-selector">
        <div>
          <b-button v-if="league.isManager" variant="info" v-b-modal="'addNewLeagueYear'">Start new Year</b-button>
        </div>
        <div>
          <b-form-select v-model="selectedYear" :options="league.years" v-on:change="changeLeagueYear" />
        </div>
      </div>
    </div>
    <addNewLeagueYearForm v-if="league.isManager" :league="league" v-on:actionTaken="actionTaken"></addNewLeagueYearForm>

    <b-modal id="draftFinishedModal" ref="draftFinishedModalRef" title="Draft Complete!">
      <p>
        The draft is complete! From here you can make bids for games that were not drafted, however, you may want to hold onto your available budget until later in the year!
      </p>
    </b-modal>

    <div v-if="leagueYear && !leagueYear.playStatus.readyToDraft" class="alert alert-warning">
      <h3>
        This year is not active yet!
      </h3>
      <ul>
        <li v-for="error in leagueYear.playStatus.startDraftErrors">{{error}}</li>
      </ul>
    </div>

    <div v-if="league.outstandingInvite">
      You have been invited to join this league. Do you wish to join?
      <div class="row">
        <div class="btn-toolbar">
          <b-button variant="primary" v-on:click="acceptInvite" class="mx-2">Join</b-button>
          <b-button variant="secondary" v-on:click="declineInvite" class="mx-2">Decline</b-button>
        </div>
      </div>
    </div>
    <div v-if="leagueYear && !leagueYear.userPublisher && !league.outstandingInvite">
      You need to create your publisher for this year.
      <b-button variant="primary" v-b-modal="'createPublisher'" class="mx-2">Create Publisher</b-button>
      <createPublisherForm :leagueYear="leagueYear" v-on:actionTaken="actionTaken"></createPublisherForm>
    </div>

    <div v-if="leagueYear && !leagueYear.playStatus.playStarted && leagueYear.playStatus.readyToDraft && !league.outstandingInvite" class="alert alert-success">
      <span v-if="league.isManager">
        Things are all set to get started! <b-button variant="primary" v-b-modal="'startDraft'" class="mx-2">Start Drafting!</b-button>
        <startDraftModal v-on:draftStarted="startDraft"></startDraftModal>
      </span>
      <span v-if="!league.isManager">
        Things are all set to get started! Your league manager can choose when to begin the draft.
      </span>
    </div>
    <div v-if="leagueYear && leagueYear.playStatus.draftIsPaused">
      <div class="alert alert-danger">
        <div v-show="!league.isManager">The draft has been paused. Speak to your league manager for details.</div>
        <div v-show="league.isManager">The draft has been paused. You can undo games that have been drafted. Press 'Resume Draft' to go back to picking games.</div>
      </div>
    </div>
    <div v-if="leagueYear && leagueYear.playStatus.draftIsActive && nextPublisherUp">
      <div v-if="!userIsNextInDraft">
        <div class="alert alert-info">
          <div v-show="!leagueYear.playStatus.draftingCounterPicks">The draft is currently in progress!</div>
          <div v-show="leagueYear.playStatus.draftingCounterPicks">It's time to draft Counter-Picks!</div>
          <div>Next to draft: <strong>{{nextPublisherUp.publisherName}}</strong></div>
        </div>
      </div>
      <div v-else>
        <div class="alert alert-success">
          <div v-show="!leagueYear.playStatus.draftingCounterPicks">The draft is currently in progress!</div>
          <div v-show="leagueYear.playStatus.draftingCounterPicks">It's time to draft counter picks!</div>
          <div><strong>It is your turn to draft!</strong></div>
        </div>
      </div>
    </div>

    <div class="league-manager-info">
      <h5 class="league-manager-info-item">League Manager:</h5>
      <span class="league-manager-info-item">{{ league.leagueManager.displayName }}</span>
      <div class="league-manager-info-item" v-if="league.isManager">
        <router-link :to="{ name: 'editLeague', params: { leagueid: league.leagueID, year: year }}">Edit League Settings</router-link>
      </div>
    </div>

    <div v-if="league.invitedPlayers.length > 0">
      <h5>Invited Players</h5>
      <ul>
        <li v-for="player in league.invitedPlayers">
          <span>{{ player }}</span>
          <b-button v-if="league.isManager" variant="danger" v-on:click="rescindInvite(player)" class="mx-2">Rescind Invite</b-button>
        </li>
      </ul>
    </div>

    <div class="row" v-if="league && leagueYear && leagueYear.userPublisher">
      <div class="col-xl-2 col-lg-3 col-md-12">
        <leagueActions ref="leagueActionsRef" :league="league" :leagueYear="leagueYear" :leagueActions="leagueActions"
                       :currentBids="currentBids" :userIsNextInDraft="userIsNextInDraft" :nextPublisherUp="nextPublisherUp" v-on:actionTaken="actionTaken"></leagueActions>
        <!--<playerActions :league="league" :leagueYear="leagueYear" :currentBids="currentBids" :leagueActions="leagueActions" :userIsNextInDraft="userIsNextInDraft" v-on:actionTaken="actionTaken"></playerActions>
        <managerActions :league="league" :leagueYear="leagueYear" :nextPublisherUp="nextPublisherUp" v-on:actionTaken="actionTaken"></managerActions>-->
      </div>
      <div class="col-xl-10 col-lg-9 col-md-12">
        <leagueYearStandings :standings="leagueYear.standings"></leagueYearStandings>
        <h3>Summary</h3>
        <leagueGameSummary :leagueYear="leagueYear"></leagueGameSummary>
      </div>
    </div>
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
  import PlayerActions from "components/modules/playerActions";
  import CreatePublisherForm from "components/modules/modals/createPublisherForm";
  import ManagerActions from "components/modules/managerActions";
  import StartDraftModal from "components/modules/modals/startDraftModal";
  import AddNewLeagueYearForm from "components/modules/modals/addNewLeagueYearForm";

  export default {
    data() {
      return {
        errorInfo: "",
        league: null,
        leagueYear: null,
        currentBids: [],
        leagueActions: []
      }
    },
    props: ['leagueid', 'year'],
    components: {
      LeagueGameSummary,
      LeagueYearStandings,
      ManagerActions,
      PlayerActions,
      CreatePublisherForm,
      StartDraftModal,
      AddNewLeagueYearForm,
      LeagueActions
    },
    computed: {
      nextPublisherUp() {
        let next = _.find(this.leagueYear.publishers, ['nextToDraft', true]);
        return next;
      },
      userIsNextInDraft() {
        if (this.nextPublisherUp) {
          return this.nextPublisherUp.publisherID === this.leagueYear.userPublisher.publisherID
        }

        return false;
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
              })
              .catch(returnedError => (this.error = returnedError));
      },
      fetchLeagueYear() {
          axios
              .get('/api/League/GetLeagueYear?leagueID=' + this.leagueid + '&year=' + this.year)
              .then(response => {
                this.leagueYear = response.data;
                this.fetchCurrentBids();
                this.fetchLeagueActions();
              })
            .catch(returnedError => (this.error = returnedError));
      },
      fetchLeagueActions() {
        axios
          .get('/api/League/GetLeagueActions?leagueID=' + this.leagueid + '&year=' + this.year)
          .then(response => {
            this.leagueActions = response.data;
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
      rescindInvite(emailAddress) {
        var model = {
          leagueID: this.league.leagueID,
          inviteEmail: emailAddress
        };
        axios
          .post('/api/leagueManager/RescindInvite', model)
          .then(response => {
            let actionInfo = {
              message: 'The invite to ' + emailAddress + ' has been rescinded.',
              fetchLeague: true,
              fetchLeagueYear: true
            };
            this.actionTaken(actionInfo);
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
            leagueID: this.league.leagueID
          };
          axios
            .post('/api/league/DeclineInvite', model)
            .then(response => {
                this.$router.push({ name: "home" });
            })
            .catch(response => {

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
      async startHubConnection() {
        let token = this.$store.getters.token;
        let hubConnection = new signalR.HubConnectionBuilder()
          .withUrl("/updatehub", { accessTokenFactory: () => token })
          .configureLogging(signalR.LogLevel.Error)
          .build();

        hubConnection.start().catch(err => console.error(err.toString()));
        hubConnection.on('RefreshLeagueYear', data => {
          this.fetchLeagueYear();
        });
        hubConnection.on('DraftFinished', data => {
          this.$refs.draftFinishedModalRef.show();
        });
        hubConnection.onclose(async () => {
          await startHubConnection();
        })
      }
    },
    async mounted() {
      this.selectedYear = this.year;
      this.fetchLeague();
      this.fetchLeagueYear();
      await this.startHubConnection();
    },
    watch: {
      '$route'(to, from) {
          this.fetchLeagueYear();
        }
    }
  }
</script>
<style>
  .year-selector {
    position: absolute;
    right: 0px;
  }
  .year-selector div {
    float:left;
  }
  .league-header {
    margin-left: 0px;
  }
  .league-manager-info {
    display: flex;
    flex-direction: row;
  }
  .league-manager-info-item {
    margin-right: 5px;
  }
</style>
