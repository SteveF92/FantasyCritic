<template>
  <b-modal id="manageActivePlayers" ref="manageActivePlayersRef" title="Manage Active Players" @show="setData">
    <h4 class="text-black">Active Players for {{ leagueYear.year }}</h4>
    <div class="alert alert-danger" v-show="errorInfo">
      {{ errorInfo }}
    </div>
    <table class="table table-bordered table-striped table-sm" v-if="showTable">
      <thead>
        <tr class="bg-primary">
          <th scope="col">User</th>
          <th scope="col">Active?</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(value, name) in internalPlayerActive">
          <td>{{ value.displayName }}</td>
          <td>
            <input type="checkbox" v-model="value.active" :disabled="value.manager" />
          </td>
        </tr>
      </tbody>
    </table>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Set Active Players" v-on:click="confirmActivePlayers" />
    </div>
  </b-modal>
</template>
<script>
import axios from 'axios';

export default {
  data() {
    return {
      showTable: false,
      internalPlayerActive: {},
      errorInfo: ''
    };
  },
  props: ['league', 'leagueYear'],
  methods: {
    userIsManager(user) {
      return this.league.leagueManager.userID === user.userID;
    },
    userIsActive(user) {
      let matchingPlayer = _.find(this.leagueYear.players, function (item) {
        return item.user && item.user.userID === user.userID;
      });

      return !!matchingPlayer;
    },
    setCurrentActivePlayers() {
      this.internalPlayerActive = {};
      let outerScope = this;
      this.league.players.forEach(function (player) {
        let playerIsActive = outerScope.userIsActive(player);
        let playerIsManager = outerScope.userIsManager(player);
        outerScope.internalPlayerActive[player.userID] = {
          displayName: player.displayName,
          active: playerIsActive,
          manager: playerIsManager
        };
      });
      this.showTable = true;
    },
    confirmActivePlayers() {
      let playerStatus = {};
      let playerActiveDict = this.internalPlayerActive;

      Object.keys(playerActiveDict).forEach(function (key) {
        playerStatus[key] = playerActiveDict[key].active;
      });

      var model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year,
        activeStatus: playerStatus
      };

      axios
        .post('/api/leagueManager/SetPlayerActiveStatus', model)
        .then(() => {
          this.$emit('activePlayersEdited');
          this.$refs.manageActivePlayersRef.hide();
        })
        .catch((response) => {
          this.errorInfo = response.response.data;
        });
    },
    setData() {
      this.setCurrentActivePlayers();
    }
  },
  mounted() {
    this.setCurrentActivePlayers();
  }
};
</script>
<style scoped>
.email-form {
  margin-bottom: 10px;
}
.text-black {
  color: black !important;
}
.display-number-label {
  font-size: 35px;
  margin-right: 3px;
}
</style>
