<template>
    <div>
        <h2>{{player.userName}}</h2>
        <h5>League: {{player.leagueName}}</h5>

        <h3>Games</h3>
        <ul>
            <li v-for="game in player.games" v-if="!game.antiPick">
                {{ game.gameName }}
            </li>
        </ul>
        <h3>Anti Picks</h3>
        <ul>
            <li v-for="game in player.games" v-if="game.antiPick">
                {{ game.gameName }}
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
                player: null
            }
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
