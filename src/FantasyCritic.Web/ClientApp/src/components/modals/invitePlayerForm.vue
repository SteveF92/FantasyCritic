<template>
  <ValidationObserver v-slot="{ invalid }">
    <b-modal id="invitePlayer" ref="invitePlayerRef" title="Invite a Player" @hidden="clearData" @show="fetchInviteLinks">
      <div v-show="errorInfo" class="alert alert-danger">
        {{ errorInfo }}
      </div>
      <div v-if="playStarted" class="alert alert-warning">
        This year has already been started. Any players invited now will be marked as "inactive". The only reason to do this is if you intend to use the "Reassign Publisher" option.
      </div>
      <div v-if="!playStarted">
        <h3 class="text-black">Invite Link</h3>
        <label>This is the easiest way to invite players. Just send one link to anyone you want to invite.</label>
        <div v-for="inviteLink in inviteLinks" :key="inviteLink.inviteID" class="invite-link">
          <input type="text" class="form-control input" :value="inviteLink.fullInviteLink" readonly />
          <b-button v-clipboard:copy="inviteLink.fullInviteLink" v-clipboard:success="inviteLinkCopied" variant="info" size="sm">Copy</b-button>
          <b-button variant="danger" size="sm" @click="deleteInviteLink(inviteLink)">Delete</b-button>
        </div>
        <br />
        <b-button variant="primary" size="sm" @click="createInviteLink()">Create Invite Link</b-button>
      </div>
      <hr />
      <div>
        <h3 class="text-black">Invite Single Player</h3>
        <div class="form-horizontal">
          <div class="form-group email-form">
            <label for="inviteEmail" class="control-label">Email Address</label>
            <ValidationProvider v-slot="{ errors }" rules="email" name="Email Address">
              <input id="inviteEmail" v-model="inviteEmail" name="inviteEmail" type="text" class="form-control input" />
              <span class="text-danger">{{ errors[0] }}</span>
            </ValidationProvider>
          </div>
        </div>

        <h3 class="text-black">OR</h3>
        <div class="form-horizontal">
          <div class="form-group">
            <label for="inviteDisplayName" class="control-label">Display Name</label>
            <input id="inviteDisplayName" v-model="inviteDisplayName" name="inviteDisplayName" type="text" class="form-control input" />
          </div>
          <div class="form-group">
            <label for="inviteDisplayNumber" class="control-label">Display Number (Found in the username dropdown)</label>
            <ValidationProvider v-slot="{ errors }" rules="min_value:1000|max_value:9999" name="Display Number">
              <input id="inviteDisplayNumber" v-model="inviteDisplayNumber" name="inviteDisplayNumber" type="number" class="form-control input" />
              <span class="text-danger">{{ errors[0] }}</span>
            </ValidationProvider>
          </div>
        </div>
      </div>
      <template #modal-footer>
        <input type="submit" class="btn btn-primary" value="Send Invite" :disabled="invalid || !valuesEntered" @click="invitePlayer" />
      </template>
    </b-modal>
  </ValidationObserver>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
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
    }
  },
  methods: {
    invitePlayer() {
      const model = {
        leagueID: this.league.leagueID,
        inviteEmail: this.inviteEmail,
        inviteDisplayName: this.inviteDisplayName,
        inviteDisplayNumber: this.inviteDisplayNumber
      };
      axios
        .post('/api/leagueManager/InvitePlayer', model)
        .then(() => {
          this.$refs.invitePlayerRef.hide();
          let inviteName = this.inviteEmail;
          if (this.inviteDisplayName) {
            inviteName = this.inviteDisplayName;
          }
          this.notifyAction('Invite was sent to ' + inviteName);
          this.inviteEmail = '';
        })
        .catch((error) => {
          this.errorInfo = error.response.data;
        });
    },
    createInviteLink() {
      const model = {
        leagueID: this.league.leagueID
      };
      axios
        .post('/api/leagueManager/CreateInviteLink', model)
        .then(() => {
          this.fetchInviteLinks();
        })
        .catch((error) => {
          this.errorInfo = error.response.data;
        });
    },
    inviteLinkCopied() {
      this.makeToast('Invite Link copied to clipboard.');
    },
    deleteInviteLink(inviteLink) {
      var model = {
        leagueID: this.league.leagueID,
        inviteID: inviteLink.inviteID
      };
      axios
        .post('/api/leagueManager/DeleteInviteLink', model)
        .then(() => {
          this.fetchInviteLinks();
        })
        .catch((error) => {
          this.errorInfo = error.response.data;
        });
    },
    fetchInviteLinks() {
      axios
        .get('/api/leagueManager/InviteLinks/' + this.league.leagueID)
        .then((response) => {
          this.inviteLinks = response.data;
        })
        .catch(() => {});
    },
    clearData() {
      this.inviteEmail = '';
      this.inviteDisplayName = '';
      this.inviteDisplayNumber = '';
      this.errorInfo = '';
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
.invite-link {
  display: flex;
}
</style>
