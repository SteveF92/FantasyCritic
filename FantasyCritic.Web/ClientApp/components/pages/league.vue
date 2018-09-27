<template>
  <div v-if="league">
    <div v-if="errorInfo" class="alert alert-danger" role="alert">
      {{errorInfo}}
    </div>

    <div class="row">
      <h2>{{ league.leagueName }}</h2>
      <b-form-select v-model="activeYear" :options="league.years" class="year-selector" />
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

    <div>
      <h3>League Manager</h3>
      {{ league.leagueManager.userName }}
    </div>

    <div class="row">
      <div v-if="leagueYear" class="col-lg-6 col-12">
        <leagueYearStandings :standings="leagueYear.standings"></leagueYearStandings>
      </div>
      <div class="col-lg-6 col-12">
        <leagueActions :league="league" :leagueYear="leagueYear" v-on:gameClaimed="gameClaimed" v-on:playerInvited="playerInvited" v-on:gameRemoved="gameRemoved" v-on:gameAssociated="gameAssociated"></leagueActions>
      </div>
    </div>
    
    <div v-if="leagueYear">
      <h3>Summary</h3>
      <div class="league-summary">
        <leagueGameSummary :leagueYear="leagueYear"></leagueGameSummary>
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

  </div>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    import LeagueGameSummary from "components/modules/leagueGameSummary";
    import ManagerClaimGameForm from "components/modules/managerClaimGameForm";
    import LeagueYearStandings from "components/modules/leagueYearStandings";
    import LeagueActions from "components/modules/leagueActions";

    export default {
        data() {
            return {
                errorInfo: "",
                league: null,
                leagueYear: null,
                activeYear: null
            }
        },
        props: ['leagueid', 'year'],
        components: {
            LeagueGameSummary,
            ManagerClaimGameForm,
            LeagueYearStandings,
            LeagueActions
        },
        methods: {
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
                    })
                    .catch(returnedError => (this.error = returnedError));
            },
            acceptInvite() {
                var model = {
                    leagueID: this.leagueID
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
          gameClaimed(claimInfo) {
            this.fetchLeagueYear();
            let toast = this.$toasted.show(claimInfo.gameName + ' added to ' + claimInfo.publisher, {
              theme: "primary",
              position: "top-right",
              duration: 5000
            });
          },
          gameRemoved(removeInfo) {
            this.fetchLeagueYear();
            let toast = this.$toasted.show(removeInfo.gameName + ' removed from ' + removeInfo.publisherName, {
              theme: "primary",
              position: "top-right",
              duration: 5000
            });
          },
          gameAssociated(associationInfo) {
            this.fetchLeagueYear();
            let toast = this.$toasted.show(associationInfo.gameName + ' sucessfully associated.', {
              theme: "primary",
              position: "top-right",
              duration: 5000
            });
          },
          playerInvited(inviteEmail) {
            this.fetchLeague();
            let toast = this.$toasted.show('Invite was sent to ' + inviteEmail, {
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
