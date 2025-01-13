<template>
  <b-modal id="manageActivePlayers" ref="manageActivePlayersRef" title="Manage Active Players" @show="setData">
    <h4 class="text-black">Active Players for {{ leagueYear.year }}</h4>
    <div v-show="errorInfo" class="alert alert-danger">
      {{ errorInfo }}
    </div>
    <table v-if="showTable" class="table table-bordered table-striped table-sm">
      <thead>
        <tr class="bg-primary">
          <th scope="col">User</th>
          <th scope="col">Active?</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(value, name) in internalPlayerActive" :key="name">
          <td>{{ value.displayName }}</td>
          <td>
            <input v-model="value.active" type="checkbox" />
          </td>
        </tr>
      </tbody>
    </table>
    <template #modal-footer>
      <input type="submit" class="btn btn-primary" value="Set Active Players" @click="confirmActivePlayers" />
    </template>
  </b-modal>
</template>
<script>
import axios from 'axios';

import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      showTable: false,
      internalPlayerActive: {},
      errorInfo: ''
    };
  },
  created() {
    this.setCurrentActivePlayers();
  },
  methods: {
    userIsActive(user) {
      let matchingPlayer = this.leagueYear.players.find((item) => item.user && item.user.userID === user.userID);
      return !!matchingPlayer;
    },
    setCurrentActivePlayers() {
      this.internalPlayerActive = {};
      let outerScope = this;
      this.league.players.forEach(function (player) {
        let playerIsActive = outerScope.userIsActive(player);
        outerScope.internalPlayerActive[player.userID] = {
          displayName: player.displayName,
          active: playerIsActive
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

      const model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year,
        activeStatus: playerStatus
      };

      axios
        .post('/api/leagueManager/SetPlayerActiveStatus', model)
        .then(() => {
          this.$refs.manageActivePlayersRef.hide();
          this.notifyAction('Active players were changed.');
        })
        .catch((response) => {
          this.errorInfo = response.response.data;
        });
    },
    setData() {
      this.errorInfo = '';
      this.setCurrentActivePlayers();
    }
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
