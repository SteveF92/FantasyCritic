<template>
  <b-modal id="manageActivePlayers" ref="manageActivePlayersRef" title="Manage Active Players">
    <h4 class="text-black">Active Players for {{leagueYear.year}}</h4>
    <table class="table table-bordered table-striped table-sm">
      <thead>
        <tr class="bg-primary">
          <th scope="col">User</th>
          <th scope="col">Active?</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="player in league.players">
          <td>{{player.displayName}}</td>
          <td><input type="checkbox" v-model="internalPlayerActive[player.userID]"></td>
        </tr>
      </tbody>
    </table>
</b-modal>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    data() {
      return {
        internalPlayerActive: {}
      }
    },
    props: ['league', 'leagueYear'],
    methods: {
      setPlayerActiveStatus(player, activeStatus) {
        var model = {
          leagueID: this.league.leagueID,
          year: this.leagueYear.year,
          userID: player.userID,
          activeStatus: activeStatus
        };

        axios
          .post('/api/leagueManager/SetPlayerActiveStatus', model)
          .then(response => {
            this.$emit('activePlayersEdited');
          });
      },
      userIsActive(user) {
        let matchingPlayer = _.find(this.leagueYear.players, function(item){
          return item.user.userID === user.userID;
        });

        return matchingPlayer;
      },
      setCurrentActivePlayers() {
        let outerScope = this;
        this.league.players.forEach(function(player) {
          let playerIsActive = outerScope.userIsActive(player);
          if (playerIsActive) {
            outerScope.internalPlayerActive[player.userID] = true;
          } else {
            outerScope.internalPlayerActive[player.userID] = false;
          }
        });
      }
    },
    mounted() {
      this.setCurrentActivePlayers();
    }
  }
</script>
<style scoped>
  .email-form {
    margin-bottom: 10px;
  }
  .text-black{
    color:black !important;
  }
  .display-number-label {
    font-size: 35px;
    margin-right:3px;
  }
</style>
