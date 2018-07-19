<template>
    <div>
        <h2>{{ league.leagueName }}</h2>
        <h3>Players</h3>
        <ul>
            <li v-for="player in league.players">
                {{ player.userName }}
            </li>
        </ul>
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
                league: {}
            }
        },
        methods: {
            fetchLeague() {
                let leagueID = this.$route.params.id;
                axios
                    .get('/api/League/GetLeague/' + leagueID)
                    .then(response => {
                        this.league = response.data;
                    })
                    .catch(returnedError => (this.error = returnedError));
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
