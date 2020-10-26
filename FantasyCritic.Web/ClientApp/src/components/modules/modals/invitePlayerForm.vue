<template>
  <b-modal id="invitePlayer" ref="invitePlayerRef" title="Invite a Player" @hidden="clearData">
    <div class="alert alert-danger" v-show="errorInfo">
      {{errorInfo}}
    </div>
    <div>
      <h3 class="text-black">Invite Link</h3>
      <label>This is the easiest way to invite players. Just send one link to anyone you want to invite.</label>
      <div v-for="inviteLink in inviteLinks" class="invite-link">
        <input type="text" class="form-control input" :value="inviteLink.fullInviteLink" readonly>
        <b-button variant="info" size="sm" v-clipboard:copy="inviteLink.fullInviteLink" v-clipboard:success="inviteLinkCopied">Copy</b-button>
        <b-button variant="danger" size="sm" v-on:click="deleteInviteLink(inviteLink)">Delete</b-button>
      </div>
      <br />
      <b-button variant="primary" size="sm" v-on:click="createInviteLink()">Create Invite Link</b-button>
    </div>
    <hr />
    <div>
      <h3 class="text-black">Invite Single Player</h3>
      <ValidationObserver v-slot="{ invalid }">
        <div class="form-horizontal">
          <div class="form-group email-form">
            <label for="inviteEmail" class="control-label">Email Address</label>
            <ValidationProvider rules="email" v-slot="{ errors }" name="Email Address">
              <input v-model="inviteEmail" id="inviteEmail" name="inviteEmail" type="text" class="form-control input" />
              <span class="text-danger">{{ errors[0] }}</span>
            </ValidationProvider>
          </div>
        </div>
        
        <h3 class="text-black">OR</h3>
        <div class="form-horizontal">
          <div class="form-group">
            <label for="inviteDisplayName" class="control-label">Display Name</label>
            <input v-model="inviteDisplayName" id="inviteDisplayName" name="inviteDisplayName" type="text" class="form-control input" />
          </div>
          <div class="form-group">
          <label for="inviteDisplayNumber" class="control-label">Display Number (Found in the username dropdown)</label>
            <ValidationProvider rules="min_value:1000|max_value:9999" v-slot="{ errors }" name="Display Number">
              <input v-model="inviteDisplayNumber" id="inviteDisplayNumber" name="inviteDisplayNumber" type="number" class="form-control input" />
              <span class="text-danger">{{ errors[0] }}</span>
            </ValidationProvider>
          </div>
        </div>
        <div slot="modal-footer">
          <input type="submit" class="btn btn-primary" value="Send Invite" v-on:click="invitePlayer" :disabled="invalid || !valuesEntered" />
        </div>
      </ValidationObserver>
    </div>
  </b-modal>
</template>
<script>
import Vue from 'vue';
import axios from 'axios';

export default {
    data() {
        return {
            inviteEmail: '',
            inviteDisplayName: '',
            inviteDisplayNumber: '',
            inviteLinks: null,
            errorInfo: ''
        };
    },
    computed: {
        valuesEntered() {
            return this.inviteEmail || (this.inviteDisplayName && this.inviteDisplayNumber);
        },
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
                    this.inviteEmail = '';
                })
                .catch(error => {
                    this.errorInfo = error.response.data;
                });
        },
        createInviteLink() {
            var model = {
                leagueID: this.league.leagueID
            };
            axios
                .post('/api/leagueManager/CreateInviteLink', model)
                .then(response => {
                    this.fetchInviteLinks();
                })
                .catch(error => {
                    this.errorInfo = error.response.data;
                });
        },
        inviteLinkCopied() {
            this.$emit('linkCopied');
        },
        deleteInviteLink(inviteLink) {
            var model = {
                leagueID: this.league.leagueID,
                inviteID: inviteLink.inviteID
            };
            axios
                .post('/api/leagueManager/DeleteInviteLink', model)
                .then(response => {
                    this.fetchInviteLinks();
                })
                .catch(error => {
                    this.errorInfo = error.response.data;
                });
        },
        fetchInviteLinks() {
            axios
                .get('/api/leagueManager/InviteLinks/' + this.league.leagueID)
                .then(response => {
                    this.inviteLinks = response.data;
                })
                .catch(response => {

                });
        },
        clearData() {
            this.inviteEmail = '';
            this.inviteDisplayName = '';
            this.inviteDisplayNumber = '';
            this.errorInfo = '';
        }
    },
    mounted() {
        this.fetchInviteLinks();
    }
};
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
  .invite-link {
    display: flex;
  }
</style>
