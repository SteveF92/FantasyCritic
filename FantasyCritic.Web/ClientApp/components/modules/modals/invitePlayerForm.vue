<template>
  <b-modal id="invitePlayer" ref="invitePlayerRef" title="Invite a Player">
    <form class="form-horizontal" v-on:submit.prevent="invitePlayer">
      <div class="form-group">
        <label for="inviteEmail" class="control-label">Email Address</label>
        <input v-model="inviteEmail" id="inviteEmail" name="inviteEmail" type="text" class="form-control input" />
      </div>
    </form>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Send Invite" />
    </div>
  </b-modal>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    data() {
      return {
        inviteEmail: "",
        errorInfo: ""
      }
    },
    props: ['league'],
    methods: {
      invitePlayer() {
        var model = {
          leagueID: this.league.leagueID,
          inviteEmail: this.inviteEmail
        };
        axios
          .post('/api/league/InvitePlayer', model)
          .then(response => {
            this.$refs.invitePlayerRef.hide();
            this.$emit('playerInvited', this.inviteEmail);
            this.showInvitePlayer = false;
            this.inviteEmail = "";
          })
          .catch(response => {
            this.errorInfo = "Cannot find a player with that email address."
          });
      }
    }
  }
</script>
