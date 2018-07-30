<template>
    <div v-if="player">
        <h2>{{player.userName}}</h2>
        <h5>
            <router-link :to="{ name: 'league', params: { leagueid: player.leagueID }}">League: {{player.leagueName}}</router-link>
        </h5>
        <playerGameTable :games="player.games"></playerGameTable>
    </div>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    import PlayerGameTable from "components/modules/playerGameTable";

    export default {
        data() {
            return {
                errorInfo: "",
                player: null
            }
        },
        components: {
            PlayerGameTable
        },
        props: ['publisherid'],
        methods: {
            fetchPlayer() {
                axios
                    .get('/api/League/GetPublisher/' + this.publisherid)
                    .then(response => {
                        this.player = response.data;
                    })
                    .catch(returnedError => (this.error = returnedError));
            }
        },
        mounted() {
            this.fetchPlayer();
        },
        watch: {
            '$route'(to, from) {
                this.fetchPlayer();
            }
        }
    }
</script>
