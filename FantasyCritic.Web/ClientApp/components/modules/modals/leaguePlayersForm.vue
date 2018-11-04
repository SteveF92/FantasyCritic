<template>
  <b-modal id="leaguePlayersForm" ref="leaguePlayersFormRef" title="League Players" size="lg" hide-footer>
    <table class="table table-sm table-responsive-sm table-bordered">
      <thead>
        <tr class="table-secondary">
          <th scope="col" class="game-column">User Name</th>
          <th scope="col">Publisher Name</th>
          <th scope="col">Draft Position</th>
          <th scope="col">Budget</th>
          <th scope="col" v-if="showRemove">Remove Player</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="player in players">
          <td>{{player.user.userName}}</td>
          <td v-if="player.publisher">{{player.publisher.publisherName}}</td>
          <td v-else class="not-created-publisher">Not Created Yet</td>
          <td v-if="player.publisher">{{player.publisher.draftPosition}}</td>
          <td v-else></td>
          <td v-if="player.publisher">{{player.publisher.budget}}</td>
          <td v-else></td>
          <td v-if="showRemove">
            <b-button variant="danger" size="sm" v-on:click="cancelBid(bid)">Remove</b-button>
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
          //cancelBid(bid) {
          //  var model = {
          //    bidID: bid.bidID
          //  };
          //  axios
          //    .post('/api/league/DeletePickupBid', model)
          //    .then(response => {
          //      var bidInfo = {
          //        gameName: bid.masterGame.gameName,
          //        bidAmount: bid.bidAmount
          //      };
          //      this.$emit('bidCanceled', bidInfo);
          //    })
          //    .catch(response => {

          //    });
          //}
        }
    }
</script>
<style scoped>
  .not-created-publisher {
    color: #B1B1B1;
    font-style: italic;
  }
</style>
