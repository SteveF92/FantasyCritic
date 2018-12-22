<template>
  <b-modal id="leaguePlayersForm" ref="leaguePlayersFormRef" title="League Players" size="lg" hide-footer>
    <table class="table table-sm table-responsive-sm table-bordered table-striped">
      <thead>
        <tr class="bg-primary">
          <th scope="col" class="game-column">Display Name</th>
          <th scope="col">Publisher Name</th>
          <th scope="col">Draft Position</th>
          <th scope="col">Budget</th>
          <th scope="col" v-if="showRemove">Remove Player</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="player in players">
          <td>{{player.user.displayName}}</td>
          <td v-if="player.publisher">{{player.publisher.publisherName}}</td>
          <td v-else class="not-created-publisher">Not Created Yet</td>
          <td v-if="player.publisher">{{player.publisher.draftPosition}}</td>
          <td v-else></td>
          <td v-if="player.publisher">{{player.publisher.budget}}</td>
          <td v-else></td>
          <td v-if="showRemove">
            <b-button variant="danger" size="sm" v-on:click="removePlayer(player)">Remove</b-button>
          </td>
        </tr>
      </tbody>
    </table>
  </b-modal>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    export default {
    props: ['players', 'league'],
        computed: {
          showRemove() {
            return (this.league.isManager && this.league.neverStarted);
          }
        },
        methods: {
          removePlayer(player) {
            var model = {
              leagueID: this.league.leagueID,
              userID: player.user.userID
            };
            axios
              .post('/api/leagueManager/RemovePlayer', model)
              .then(response => {
                var removeInfo = {
                  emailAddress: player.user.emailAddress
                };
                this.$emit('playerRemoved', removeInfo);
              })
              .catch(response => {

              });
          }
        }
    }
</script>
<style scoped>
  .not-created-publisher {
    color: #B1B1B1;
    font-style: italic;
  }
</style>
