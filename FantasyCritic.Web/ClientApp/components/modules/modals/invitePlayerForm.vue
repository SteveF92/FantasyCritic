<template>
  <b-modal id="invitePlayer" ref="invitePlayerRef" title="Invite a Player" @hidden="clearData">
    <div class="form-horizontal">
      <div class="form-group email-form">
        <label for="inviteEmail" class="control-label">Email Address</label>
        <input v-model="inviteEmail" id="inviteEmail" name="inviteEmail" type="text" class="form-control input" />
      </div>
    </div>
    <h3 class="text-black">OR</h3>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="inviteDisplayName" class="control-label">Display Name</label>
        <input v-model="inviteDisplayName" id="inviteDisplayName" name="inviteDisplayName" type="text" class="form-control input" />
      </div>
      <label for="inviteDisplayNumber" class="control-label">Display Number (Found in the username dropdown)</label>
      <div class="form-group form-inline">
        <label for="inviteDisplayNumber" class="display-number-label">#</label>
        <input v-model="inviteDisplayNumber" id="inviteDisplayNumber" name="inviteDisplayNumber" type="text" class="form-control"/>
      </div>
    </div>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Send Invite" v-on:click="invitePlayer" :disabled="!emailIsValid && !displayNameIsValid" />
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
        inviteDisplayName: "",
        inviteDisplayNumber: "",
        errorInfo: ""
      }
    },
    computed: {
      emailIsValid() {
        var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(String(this.inviteEmail).toLowerCase());
      },
      displayNameIsValid() {
        return (this.inviteDisplayName && this.inviteDisplayNumber);
      }
    },
    props: ['league'],
    methods: {
      invitePlayer() {
        var model = {
          leagueID: this.league.leagueID,
          inviteEmail: this.inviteEmail,
          inviteDisplayName: this.inviteDisplayName,
          inviteDisplayNumber: this.inviteDisplayNumber
        };
        axios
          .post('/api/leagueManager/InvitePlayer', model)
          .then(response => {
            this.$refs.invitePlayerRef.hide();
            let inviteName = this.inviteEmail;
            if (this.inviteDisplayName) {
              inviteName = this.inviteDisplayName;
            }
            this.$emit('playerInvited', inviteName);
            this.inviteEmail = "";
          })
          .catch(response => {
            this.errorInfo = "Cannot find a player with that email address."
          });
      },
      clearData() {
        this.inviteEmail = "";
        this.inviteDisplayName = "";
        this.inviteDisplayNumber = "";
      }
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
