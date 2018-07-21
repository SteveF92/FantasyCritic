<template>
    <div>
        <h2>{{ league.leagueName }}</h2>
        <div class="col-md-8" v-if="league.outstandingInvite">
            You have been invited to join this league. Do you wish to join?
        </div>
        <h3>Players</h3>
        <ul>
            <li v-for="player in league.players">
                {{ player.userName }}
            </li>
        </ul>
        <div class="col-md-8" v-if="league.isManager">
            <div v-if="!showInvitePlayer">
                <b-button variant="info" class="nav-link" v-on:click="showInvite">Invite a Player</b-button>
            </div>
            <div v-if="showInvitePlayer">
                <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="invitePlayer">
                    <div class="form-group col-md-4">
                        <label for="inviteEmail" class="control-label">Email Address</label>
                        <input v-model="inviteEmail" id="inviteEmail" name="inviteEmail" type="text" class="form-control input" />
                    </div>
                    <div class="form-group col-md-2">
                        <input type="submit" class="btn btn-primary" value="Send Invite" />
                    </div>
                </form>
            </div>
        </div>
    </div>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    export default {
        data() {
            return {
                errorInfo: "",
                leagueID: "",
                league: {},
                showInvitePlayer: false,
                inviteEmail: ""
            }
        },
        methods: {
            fetchLeague() {
                this.leagueID = this.$route.params.id;
                axios
                    .get('/api/League/GetLeague/' + this.leagueID)
                    .then(response => {
                        this.league = response.data;
                    })
                    .catch(returnedError => (this.error = returnedError));
            },
            showInvite() {
                this.showInvitePlayer = true;
            },
            invitePlayer() {
                var model = {
                    leagueID: this.leagueID,
                    inviteEmail: this.inviteEmail
                };
                axios
                    .post('/api/league/InvitePlayer', model)
                    .then(this.responseHandler)
                    .catch(this.catchHandler);
            },
            responseHandler(response) {
                this.$router.push({ name: "login" });
            },
            catchHandler(returnedError) {

            }
        },
        mounted() {
            this.fetchLeague();
        },
        watch: {
            '$route'(to, from) {
                this.fetchLeague();
            }
        }
    }
</script>
