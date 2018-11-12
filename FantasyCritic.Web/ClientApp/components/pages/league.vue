<template>
  <div v-if="league">
    <div v-if="errorInfo" class="alert alert-danger" role="alert">
      {{errorInfo}}
    </div>

    <div class="row">
      <h2>{{ league.leagueName }}</h2>
      <b-form-select v-model="activeYear" :options="league.years" class="year-selector" />
    </div>

    <div v-if="leagueYear && !leagueYear.playStatus.readyToDraft" class="alert alert-warning">
      <h3>
        This year is not active yet!
      </h3>
      <ul>
        <li v-for="error in leagueYear.playStatus.startDraftErrors">{{error}}</li>
      </ul>
    </div>

    <div v-if="league && league.isManager">
      <router-link :to="{ name: 'editLeague', params: { leagueid: league.leagueID, year: year }}">Edit League Settings</router-link>
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

    <div v-if="leagueYear && !leagueYear.playStatus.playStarted && leagueYear.playStatus.readyToDraft" class="alert alert-success">
      <span v-if="league.isManager">
        Things are all set to get started! <b-button variant="primary" v-b-modal="'startDraft'" class="mx-2">Start Drafting!</b-button>
        <startDraftModal v-on:draftStarted="startDraft"></startDraftModal>
      </span>
      <span v-if="!league.isManager">
        Things are all set to get started! Your league manager can choose when to begin the draft.
      </span>
    </div>
    <div v-if="leagueYear && leagueYear.playStatus.draftIsActive && nextPublisherUp">
      <div v-if="!userIsNextInDraft">
        <div class="alert alert-info">
          <div>The draft is currently in progress!</div>
          <div>Next to draft: <strong>{{nextPublisherUp.publisherName}}</strong></div>
        </div>
      </div>
      <div v-else>
        <div class="alert alert-success">
          <div>The draft is currently in progress!</div>
          <div><strong>It is your turn to draft!</strong></div>
        </div>
      </div>
    </div>

    <div>
      <h3>League Manager</h3>
      {{ league.leagueManager.userName }}
    </div>

    <div class="row" v-if="leagueYear && leagueYear.userPublisher">
      <div class="col-lg-6 col-12">
        <leagueYearStandings :standings="leagueYear.standings"></leagueYearStandings>
      </div>
      <div class="col-lg-3 col-12">
        <playerActions :league="league" :leagueYear="leagueYear" :currentBids="currentBids" :leagueActions="leagueActions" :userIsNextInDraft="userIsNextInDraft" v-on:actionTaken="actionTaken"></playerActions>
      </div>
      <div class="col-lg-3 col-12">
        <leagueActions :league="league" :leagueYear="leagueYear" :nextPublisherUp="nextPublisherUp" v-on:actionTaken="actionTaken"></leagueActions>
      </div>
    </div>

    <div v-if="league.invitedPlayers.length > 0">
      <h5>Invited Players</h5>
      <ul>
        <li v-for="player in league.invitedPlayers">
          {{ player.userName }}
        </li>
      </ul>
    </div>

    <div v-if="leagueYear">
      <h3>Summary</h3>
      <div class="league-summary">
        <leagueGameSummary :leagueYear="leagueYear"></leagueGameSummary>
      </div>
    </div>

  </div>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    import moment from "moment";
    import LeagueGameSummary from "components/modules/leagueGameSummary";
    import LeagueYearStandings from "components/modules/leagueYearStandings";
    import PlayerActions from "components/modules/playerActions";
    import CreatePublisherForm from "components/modules/modals/createPublisherForm";
    import LeagueActions from "components/modules/leagueActions";
    import StartDraftModal from "components/modules/modals/startDraftModal";

    export default {
        data() {
            return {
                errorInfo: "",
                league: null,
                leagueYear: null,
                activeYear: null,
                currentBids: [],
                leagueActions: []
            }
        },
        props: ['leagueid', 'year'],
        components: {
            LeagueGameSummary,
            LeagueYearStandings,
            LeagueActions,
            PlayerActions,
            CreatePublisherForm,
            StartDraftModal
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
                    .get('/api/League/GetLeagueYear?leagueID=' + this.leagueid + '&year=' + this.activeYear)
                    .then(response => {
                      this.leagueYear = response.data;
                      this.fetchCurrentBids();
                      this.fetchLeagueActions();
                    })
                  .catch(returnedError => (this.error = returnedError));
            },
            fetchLeagueActions() {
              axios
                .get('/api/League/GetLeagueActions?leagueID=' + this.leagueid + '&year=' + this.activeYear)
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
            acceptInvite() {
                var model = {
                    leagueID: this.league.leagueID
                };
                axios
                    .post('/api/league/AcceptInvite', model)
                    .then(response => {
                        this.fetchLeague();
                    })
                    .catch(response => {

                    });
            },
            declineInvite() {
                var model = {
                    leagueID: this.leagueID
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
            }
        },
        mounted() {
            this.activeYear = this.year;
            this.fetchLeague();
            this.fetchLeagueYear();
        },
        watch: {
            '$route'(to, from) {
                this.fetchLeagueYear();
            },
            'activeYear'(oldVal, newVal) {
                var parameters = {
                    leagueid: this.leagueid,
                    year: this.activeYear
                };
                this.$router.push({ name: "league", params: parameters });
            }
        }
    }
</script>
<style>
  .league-summary {
    margin: auto;
    width: 80%;
    left: 10%;
  }
  .year-selector {
    width: 100px;
    position: absolute;
    right: 0px;
  }
</style>
