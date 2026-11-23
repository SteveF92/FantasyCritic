<template>
  <ValidationForm v-slot="{ meta }" as="">
    <b-modal id="invite-player-form" ref="invitePlayerFormRef" title="Invite a Player" @hidden="clearData" @show="fetchInviteLinks">
      <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
      <div v-if="playStarted" class="alert alert-warning">
        This year has already been started. Any players invited now will be marked as "inactive". The only reason to do this is if you intend to use the "Reassign Publisher" option.
      </div>
      <div v-if="!playStarted">
        <h3 class="text-black">Invite Links</h3>
        <label>This is the easiest way to invite players. Just send one link to anyone you want to invite.</label>
        <div v-for="inviteLink in inviteLinks" :key="inviteLink.inviteID" class="invite-link">
          <input type="text" class="form-control input" :value="inviteLink.fullInviteLink" readonly aria-label="Invite link" />
          <b-button variant="info" size="sm" @click="copyInviteLink(inviteLink)">Copy</b-button>
          <b-button variant="danger" size="sm" :disabled="isBusy" @click="deleteInviteLink(inviteLink)">Delete</b-button>
        </div>
        <br />
        <b-button variant="primary" size="sm" :disabled="isBusy" @click="createInviteLink()">Create Invite Link</b-button>
      </div>
      <hr />
      <div>
        <h3 class="text-black">Invite Single Player</h3>
        <div class="form-horizontal">
          <div class="form-group email-form">
            <label for="inviteEmail" class="control-label">Email Address</label>
            <ValidationField v-slot="{ errors }" :model-value="inviteEmail" name="Email Address" rules="email">
              <input id="inviteEmail" v-model="inviteEmail" name="inviteEmail" type="text" class="form-control input" aria-describedby="inviteEmail-error" />
              <span id="inviteEmail-error" class="text-danger">{{ errors[0] }}</span>
            </ValidationField>
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
            <ValidationField v-slot="{ errors }" :model-value="inviteDisplayNumber" name="Display Number" rules="min_value:1000|max_value:9999">
              <input id="inviteDisplayNumber" v-model="inviteDisplayNumber" name="inviteDisplayNumber" type="number" class="form-control input" aria-describedby="inviteDisplayNumber-error" />
              <span id="inviteDisplayNumber-error" class="text-danger">{{ errors[0] }}</span>
            </ValidationField>
          </div>
        </div>
      </div>
      <template #footer>
        <input type="submit" class="btn btn-primary" value="Send Invite" :disabled="!meta.valid || !valuesEntered || isBusy" @click="invitePlayer" />
      </template>
    </b-modal>
  </ValidationForm>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueManagerClient, type CreateInviteLinkRequest, type CreateInviteRequest, type DeleteInviteLinkRequest, type LeagueInviteLinkViewModel } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { makeToast } from '@/utilities/toast';

interface InvitePlayerFormData {
  inviteEmail: string;
  inviteDisplayName: string;
  // A number input's v-model gives '' when the box is empty.
  inviteDisplayNumber: number | '';
  inviteLinks: LeagueInviteLinkViewModel[];
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): InvitePlayerFormData {
    return {
      inviteEmail: '',
      inviteDisplayName: '',
      inviteDisplayNumber: '',
      inviteLinks: [],
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeague', 'playStarted']),
    valuesEntered(): boolean {
      return !!this.inviteEmail || (!!this.inviteDisplayName && this.inviteDisplayNumber !== '');
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async invitePlayer(): Promise<void> {
      const request: CreateInviteRequest = {
        leagueID: this.requiredLeague.leagueID,
        inviteEmail: this.inviteEmail,
        inviteDisplayName: this.inviteDisplayName,
        inviteDisplayNumber: this.inviteDisplayNumber === '' ? undefined : this.inviteDisplayNumber
      };
      const inviteName = this.inviteDisplayName || this.inviteEmail;

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.invitePlayer(request);

        (this.$refs.invitePlayerFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('Invite was sent to ' + inviteName);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when sending the invite.');
      } finally {
        this.isBusy = false;
      }
    },
    async createInviteLink(): Promise<void> {
      const request: CreateInviteLinkRequest = {
        leagueID: this.requiredLeague.leagueID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.createInviteLink(request);
        await this.fetchInviteLinks();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when creating the invite link.');
      } finally {
        this.isBusy = false;
      }
    },
    async copyInviteLink(inviteLink: LeagueInviteLinkViewModel): Promise<void> {
      await navigator.clipboard.writeText(inviteLink.fullInviteLink);
      makeToast('Invite Link copied to clipboard.');
    },
    async deleteInviteLink(inviteLink: LeagueInviteLinkViewModel): Promise<void> {
      const request: DeleteInviteLinkRequest = {
        leagueID: this.requiredLeague.leagueID,
        inviteID: inviteLink.inviteID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.deleteInviteLink(request);
        await this.fetchInviteLinks();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when deleting the invite link.');
      } finally {
        this.isBusy = false;
      }
    },
    async fetchInviteLinks(): Promise<void> {
      try {
        this.inviteLinks = await leagueManagerClient.inviteLinks(this.requiredLeague.leagueID);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to load the invite links.');
      }
    },
    clearData(): void {
      this.inviteEmail = '';
      this.inviteDisplayName = '';
      this.inviteDisplayNumber = '';
      this.errorInfo = '';
    }
  }
});
</script>

<style scoped>
.email-form {
  margin-bottom: 10px;
}
.text-black {
  color: black !important;
}
.invite-link {
  display: flex;
}
</style>
