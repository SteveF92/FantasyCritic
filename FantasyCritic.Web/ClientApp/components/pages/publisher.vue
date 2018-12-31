<template>
    <div v-if="publisher">
        <h1>{{publisher.publisherName}}</h1>
        <h3>{{publisher.playerName}}</h3>
        <h5>
            <router-link :to="{ name: 'league', params: { leagueid: publisher.leagueID, year: publisher.year }}">League: {{publisher.leagueName}}</router-link>
        </h5>
        <p>Budget: {{publisher.budget | money}}</p>
        <playerGameTable v-if="leagueYear" :publisher="publisher" :options="options"></playerGameTable>
    </div>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    import PlayerGameTable from "components/modules/gameTables/playerGameTable";

    export default {
        data() {
            return {
                errorInfo: "",
                publisher: null,
                leagueYear: null,
            }
        },
        components: {
            PlayerGameTable
        },
        props: ['publisherid'],
        computed: {
          options() {
            var options = {
                standardGameSlots: this.leagueYear.standardGames,
                counterPickSlots: this.leagueYear.counterPicks
            };

            return options;
          }
        },
        methods: {
            fetchPublisher() {
                axios
                    .get('/api/League/GetPublisher/' + this.publisherid)
                    .then(response => {
                      this.publisher = response.data;
                      this.fetchLeagueYear()
                    })
                    .catch(returnedError => (this.error = returnedError));
            },
            fetchLeagueYear() {
              axios
                .get('/api/League/GetLeagueYear?leagueID=' + this.publisher.leagueID + '&year=' + this.publisher.year)
                .then(response => {
                  this.leagueYear = response.data;
                })
                .catch(returnedError => (this.error = returnedError));
            }
        },
        mounted() {
            this.fetchPublisher();
        },
        watch: {
            '$route'(to, from) {
                this.fetchPublisher();
            }
        }
    }
</script>
