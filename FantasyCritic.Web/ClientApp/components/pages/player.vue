<template>
    <div v-if="player">
        <h2>{{player.userName}}</h2>
        <h5>League: {{player.leagueName}}</h5>

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
        props: ['leagueid', 'playerid', 'year'],
        methods: {
            fetchPlayer() {
                axios
                    .get('/api/League/GetPlayer?LeagueID=' + this.leagueid + '&PlayerID=' + this.playerid + '&Year=' + this.year)
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
