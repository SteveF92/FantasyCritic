<template>
  <b-modal id="invitePlayer" ref="invitePlayerRef" title="Invite a Player">
    <form class="form-horizontal" v-on:submit.prevent="invitePlayer" @hidden="clearData">
      <div class="form-group">
        <label for="inviteEmail" class="control-label">Email Address</label>
        <input v-model="inviteEmail" id="inviteEmail" name="inviteEmail" type="text" class="form-control input" />
      </div>
    </form>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Send Invite" :disabled="!emailIsValid"/>
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
    computed: {
      emailIsValid() {
        var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(String(this.inviteEmail).toLowerCase());
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
          .post('/api/leagueManager/InvitePlayer', model)
          .then(response => {
            this.$refs.invitePlayerRef.hide();
            this.$emit('playerInvited', this.inviteEmail);
            this.inviteEmail = "";
          })
          .catch(response => {
            this.errorInfo = "Cannot find a player with that email address."
          });
      },
      clearData() {
        this.inviteEmail = "";
      }
    }
  }
</script>
