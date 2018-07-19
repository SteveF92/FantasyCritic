<template>
    <div>
        <h2>Welcome to Fantasy Critic!</h2>
        <div class="col-md-4">
            <b-button variant="primary" :to="{ name: 'createLeague' }" class="nav-link">Create a League</b-button>
            <h3>Leagues I Manage</h3>
            <ul>
                <li v-for="league in myLeagues" v-if="league.isManager">
                    <router-link :to="{ name: 'league', params: { id: league.leagueID }}">{{league.leagueName}}</router-link>
                </li>
            </ul>
            <hr />
            <h3>Leagues I Play In</h3>
            <ul>
                <li v-for="league in myLeagues" v-if="!league.isManager">
                    <router-link :to="{ name: 'league', params: { id: league.leagueID }}">{{league.leagueName}}</router-link>
                </li>
            </ul>
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
                myLeagues: []
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
            }
        },
        mounted() {
            this.fetchMyLeagues();
        }
    }
</script>
