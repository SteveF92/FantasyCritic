<template>
  <div v-if="publisher">
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div class="publisher-name">
        <h1>{{publisher.publisherName}}</h1>
      </div>

      <div v-if="publisher.publicLeague && !(publisher.userIsInLeague || publisher.outstandingInvite)" class="alert alert-info" role="info">
        You are viewing a public league.
      </div>

      <h4>Player Name: {{publisher.playerName}}</h4>

      <h4>
        <router-link :to="{ name: 'league', params: { leagueid: publisher.leagueID, year: publisher.year }}">League: {{publisher.leagueName}}</router-link>
      </h4>
      <ul>
        <li>Budget: {{publisher.budget | money}}</li>
        <li>Will Release Games Dropped: {{getDropStatus(publisher.willReleaseGamesDropped, publisher.willReleaseDroppableGames)}}</li>
        <li>Will Not Release Games Dropped: {{getDropStatus(publisher.willNotReleaseGamesDropped, publisher.willNotReleaseDroppableGames)}}</li>
        <li>Unrestricted Games Dropped: {{getDropStatus(publisher.freeGamesDropped, publisher.freeDroppableGames)}}</li>
      </ul>
      <playerGameTable v-if="leagueYear" :publisher="publisher" :options="options"></playerGameTable>
    </div>
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
            },
            getDropStatus(dropped, droppable) {
              if (!droppable) {
                return 'N/A';
              }
              if (droppable === -1) {
                return dropped + '/' + '\u221E';
              }
              return dropped + '/' + droppable;
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
<style scoped>
  .publisher-name {
    display: block;
    max-width: 100%;
    word-wrap: break-word;
  }
</style>
