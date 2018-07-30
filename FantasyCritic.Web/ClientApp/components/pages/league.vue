<template>
    <div v-if="league">
        <div v-if="errorInfo" class="alert alert-danger" role="alert">
            {{errorInfo}}
        </div>

        <div v-if="invitedEmail" class="alert alert-success" role="alert">
            Sucessfully sent invite to {{ invitedEmail }}!
        </div>

        <div class="row">
            <h2 class="col-11">{{ league.leagueName }}</h2>
            <b-form-select v-model="activeYear" :options="league.years" class="col-1" />
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

        <div v-if="leagueYear">
            <h3>Publishers</h3>
            <ul>
                <li v-for="publisher in leagueYear.publishers">
                    <router-link :to="{ name: 'publisher', params: { publisherid: publisher.publisherID }}">{{ publisher.publisherName }}</router-link>
                </li>
            </ul>
        </div>

        <div v-if="leagueYear">
            <h3>Summary</h3>
            <leagueGameSummary  :leagueYear="leagueYear"></leagueGameSummary>
        </div>

        <div v-if="league.invitedPlayers.length > 0">
            <h5>Invited Players</h5>
            <ul>
                <li v-for="player in league.invitedPlayers">
                    {{ player.userName }}
                </li>
            </ul>
        </div>

        <div v-if="league.isManager" class="col-4">
            <h4>Manager Actions</h4>

            <div>
                <div v-if="!showInvitePlayer">
                    <b-button variant="info" class="nav-link" v-on:click="showInvitePlayerForm">Invite a Player</b-button>
                </div>
                <div v-if="showInvitePlayer">
                    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="invitePlayer">
                        <div class="form-group">
                            <label for="inviteEmail" class="control-label">Email Address</label>
                            <input v-model="inviteEmail" id="inviteEmail" name="inviteEmail" type="text" class="form-control input" />
                        </div>
                        <div class="form-group">
                            <input type="submit" class="btn btn-primary" value="Send Invite" />
                        </div>
                    </form>
                </div>
            </div>
            <br />
            <div>
                <div v-if="!showAddGame">
                    <b-button variant="info" class="nav-link" v-on:click="showAddGameForm">Add Publisher Game</b-button>
                </div>
                <div v-if="showAddGame">
                    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="addGame">
                        <div class="form-group">
                            <label for="claimGameName" class="control-label">Game Name</label>
                            <div class="input-group">
                                <input v-model="claimGameName" id="claimGameName" name="claimGameName" type="text" class="form-control input" />
                                <span class="input-group-btn">
                                    <b-button variant="info" class="nav-link" v-on:click="searchGame">Search Game</b-button>
                                </span>
                            </div>
                        </div>
                        <br />
                        <div class="form-group">
                            <div class="btn-group btn-group-toggle" data-toggle="buttons">
                                <label class="btn btn-secondary active">
                                    <input type="radio" name="options" id="draft" autocomplete="off" checked> Draft
                                </label>
                                <label class="btn btn-secondary">
                                    <input type="radio" name="options" id="antiPick" autocomplete="off"> Anti Pick
                                </label>
                                <label class="btn btn-secondary">
                                    <input type="radio" name="options" id="waiver" autocomplete="off"> Waiver
                                </label>
                            </div>
                        </div>
                        <div class="form-group col-2">
                            <input type="submit" class="btn btn-primary" value="Add game to publisher" />
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    import LeagueGameSummary from "components/modules/leagueGameSummary";

    export default {
        data() {
            return {
                errorInfo: "",
                league: null,
                leagueYear: null,
                showInvitePlayer: false,
                showAddGame: false,
                inviteEmail: "",
                invitedEmail: "",
                activeYear: null
            }
        },
        props: ['leagueid', 'year'],
        components: {
            LeagueGameSummary
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
            showInvitePlayerForm() {
                this.showInvitePlayer = true;
            },
            showAddGameForm() {
                this.showAddGame = true;
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
            invitePlayer() {
                var model = {
                    leagueID: this.leagueID,
                    inviteEmail: this.inviteEmail
                };
                axios
                    .post('/api/league/InvitePlayer', model)
                    .then(response => {
                        this.showInvitePlayer = false;
                        this.invitedEmail = this.inviteEmail;
                        this.inviteEmail = "";
                        this.fetchLeague();
                    })
                    .catch(response => {
                        this.errorInfo = "Cannot find a player with that email address."
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
                    leagueid: this.league.leagueID,
                    year: this.activeYear
                };
                this.$router.push({ name: "league", params: parameters });
            }
        }
    }
</script>
