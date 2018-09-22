<template>
  <div v-if="masterGame">
    <h2>{{masterGame.gameName}}</h2>
  </div>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    import PlayerGameTable from "components/modules/playerGameTable";

    export default {
        data() {
            return {
              masterGame: null,
              error: ""
            }
        },
        props: ['mastergameid'],
        methods: {
            fetchMasterGame() {
                axios
                  .get('/api/game/MasterGame/' + this.mastergameid)
                    .then(response => {
                      this.masterGame = response.data;
                    })
                    .catch(returnedError => (this.error = returnedError));
            }
        },
        mounted() {
          this.fetchMasterGame();
        },
        watch: {
            '$route'(to, from) {
              this.fetchMasterGame();
            }
        }
    }
</script>
