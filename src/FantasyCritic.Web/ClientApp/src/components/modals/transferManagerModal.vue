<template>
  <b-modal id="transferManagerForm" ref="transferManagerFormRef" size="lg" title="Transfer Manager" hide-footer @hidden="clearData">
    <div class="alert alert-warning">Warning! If you promote a new player to be League Manager, you will no longer be League Manager! There can only be one!</div>
    <div class="form-group">
      <label for="newManager" class="control-label">New Manager</label>
      <b-form-select v-model="newManager">
        <option v-for="user in players" v-bind:value="user" :key="user.userID">
          {{ user.displayName }}
        </option>
      </b-form-select>
      <br />
      <br />
      <b-button variant="danger" v-on:click="promoteNewManager">Transfer League Manager</b-button>
    </div>
  </b-modal>
</template>

<script>
import axios from 'axios';
export default {
  data() {
    return {
      newManager: null
    };
  },
  props: ['league'],
  computed: {
    players() {
      return this.league.players;
    }
  },
  methods: {
    promoteNewManager() {
      var model = {
        leagueID: this.league.leagueID,
        newManagerUserID: this.newManager.userID
      };
      axios
        .post('/api/leagueManager/PromoteNewLeagueManager', model)
        .then(() => {
          this.$refs.transferManagerFormRef.hide();
          this.$emit('managerTransferred');
        })
        .catch(() => {});
    },
    clearData() {
      this.newManager = null;
    }
  }
};
</script>
