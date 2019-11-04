<template>
  <b-modal id="manageActivePlayers" ref="manageActivePlayersRef" title="Manage Active Players" @ok="confirmActivePlayers" @hidden="clearData">
    <h4 class="text-black">Active Players for {{leagueYear.year}}</h4>
    <table class="table table-bordered table-striped table-sm">
      <thead>
        <tr class="bg-primary">
          <th scope="col">User</th>
          <th scope="col">Active?</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(value, name) in internalPlayerActive">
          <td>{{value.displayName}}</td>
          <td>
            <input type="checkbox" v-model="value.active" :disabled="value.manager">
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
    data() {
      return {
        internalPlayerActive: {}
      }
    },
    props: ['league', 'leagueYear'],
    methods: {
      userIsManager(user) {
        return this.league.leagueManager.userID === user.userID;
      },
      userIsActive(user) {
        let matchingPlayer = _.find(this.leagueYear.players, function(item){
          return item.user.userID === user.userID;
        });

        return !!matchingPlayer;
      },
      setCurrentActivePlayers() {
        let outerScope = this;
        this.league.players.forEach(function(player) {
          let playerIsActive = outerScope.userIsActive(player);
          let playerIsManager = outerScope.userIsManager(player);
          outerScope.internalPlayerActive[player.userID] = {
              displayName: player.displayName,
              active: playerIsActive,
              manager: playerIsManager
            };
        });
      },
      confirmActivePlayers() {
        var model = {
          leagueID: this.league.leagueID,
          year: this.leagueYear.year,
          userID: player.userID,
          activeStatus: this.internalPlayerActive
        };

        axios
          .post('/api/leagueManager/SetPlayerActiveStatus', model)
          .then(response => {
            this.$emit('activePlayersEdited');
          });
      },
      clearData() {
        this.setCurrentActivePlayers();
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
