<template>
    <div>
        <h2>Welcome to Fantasy Critic!</h2>
        <div class="col-md-4">
            <b-button variant="primary" :to="{ name: 'createLeague' }" class="nav-link">Create a League</b-button>

            <div v-if="anyInvitedLeagues">
                <h3>League Invites</h3>
                <ul>
                    <li v-for="league in invitedLeagues">
                        <router-link :to="{ name: 'league', params: { leagueid: league.leagueID }}">{{league.leagueName}}</router-link>
                    </li>
                </ul>
                <hr />
            </div>

            <div v-if="anyManagedLeagues">
                <h3>Leagues I Manage</h3>
                <ul>
                    <li v-for="league in myLeagues" v-if="league.isManager">
                        <router-link :to="{ name: 'league', params: { leagueid: league.leagueID }}">{{league.leagueName}}</router-link>
                    </li>
                </ul>
                <hr />
            </div>

            <div v-if="anyPlayerLeagues">
                <h3>Leagues I Play In</h3>
                <ul>
                    <li v-for="league in myLeagues" v-if="!league.isManager">
                        <router-link :to="{ name: 'league', params: { leagueid: league.leagueID }}">{{league.leagueName}}</router-link>
                    </li>
                </ul>
                <hr />
            </div>

            <div v-if="noLeagues">
                You are not part of any leagues! Why not start one?
            </div>
        </div>
    </div>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    import _ from "lodash";

    export default {
        data() {
            return {
                errorInfo: "",
                myLeagues: [],
                invitedLeagues: []
            }
        },
        computed: {
            anyManagedLeagues() {
                return _(this.myLeagues).some('isManager');
            },
            anyPlayerLeagues() {
                return _.some(this.myLeagues, ['isManager', false]);
            },
            anyInvitedLeagues() {
                return this.invitedLeagues.length > 0;
            },
            noLeagues() {
                return (!(this.anyPlayerLeagues || this.anyManagedLeagues || this.anyInvitedLeagues));
            }
        },
        methods: {
            fetchMyLeagues() {
                axios
                    .get('/api/League/MyLeagues')
                    .then(response => {
                        this.myLeagues = response.data;
                    })
                    .catch(returnedError => (this.error = returnedError));
            },
            fetchInvitedLeagues() {
                axios
                    .get('/api/League/MyInvites')
                    .then(response => {
                        this.invitedLeagues = response.data;
                    })
                    .catch(returnedError => (this.error = returnedError));
            }
        },
        mounted() {
            this.fetchMyLeagues();
            this.fetchInvitedLeagues();
        }
    }
</script>
