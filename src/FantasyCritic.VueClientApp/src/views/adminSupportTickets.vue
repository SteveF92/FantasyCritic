<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Support tickets</h1>
      <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
    </div>
    <hr />

    <b-alert v-if="errorInfo" variant="danger" :model-value="true">'{{ jobAttempted }}' failed: {{ errorInfo }}</b-alert>
    <b-alert v-else-if="isBusy" variant="info" :model-value="true">Request is processing...</b-alert>
    <b-alert v-else-if="jobAttempted" variant="success" :model-value="true">'{{ jobAttempted }}' successfully run.</b-alert>

    <h2>Find user</h2>
    <p class="text-muted">Search by user ID, display name (exact match), or email (exact).</p>
    <div class="form-row align-items-end">
      <div class="form-group col-md-3">
        <label for="searchKind">Search by</label>
        <select id="searchKind" v-model="searchKind" class="form-control">
          <option :value="SupportUserSearchKind.UserId">User ID</option>
          <option :value="SupportUserSearchKind.DisplayName">Display name</option>
          <option :value="SupportUserSearchKind.Email">Email</option>
        </select>
      </div>
      <div class="form-group col-md-6">
        <label for="searchValue">Value</label>
        <input id="searchValue" v-model="searchValue" type="text" class="form-control" @keyup.enter="searchUsers" />
      </div>
      <div class="form-group col-md-3">
        <b-button variant="primary" :disabled="isBusy" @click="searchUsers">Search</b-button>
      </div>
    </div>

    <b-alert v-if="searchMatches && searchMatches.length === 0" variant="warning" :model-value="true">No users found.</b-alert>

    <div v-if="searchMatches && searchMatches.length > 0">
      <h3 class="mt-4">Results</h3>
      <div v-for="match in searchMatches" :key="match.user.userID" class="card mb-3">
        <div class="card-body">
          <div class="d-flex flex-wrap justify-content-between align-items-start">
            <div>
              <strong>{{ match.user.displayName }}</strong>
              <span class="text-muted">#{{ match.user.displayNumber }}</span>
              <div class="small text-muted">{{ match.user.emailAddress }}</div>
              <div class="small font-monospace">User ID: {{ match.user.userID }}</div>
            </div>
            <b-button :variant="selectedUserId === match.user.userID ? 'success' : 'outline-primary'" @click="selectUserForTicket(match.user.userID)">
              {{ selectedUserId === match.user.userID ? 'Selected for ticket' : 'Use for ticket' }}
            </b-button>
          </div>

          <div v-if="match.leagues.length === 0" class="mt-2 text-muted small">No league publishers on file for this user.</div>
          <table class="table table-sm table-responsive-sm table-bordered table-striped">
            <thead>
              <tr class="bg-primary">
                <th>League</th>
                <th>Year</th>
                <th>Publisher name</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in match.leagues" :key="row.leagueID + '-' + row.year">
                <td>{{ row.leagueName }}</td>
                <td>{{ row.year }}</td>
                <td>{{ row.publisherName }}</td>
                <td>
                  <router-link :to="{ name: 'league', params: { leagueid: row.leagueID, year: row.year } }">Open league</router-link>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <div v-if="selectedUserId" class="alert alert-info mt-4">
      Creating ticket for user ID:
      <strong class="font-monospace">{{ selectedUserId }}</strong>
    </div>

    <h2 class="mt-5">Open ticket</h2>
    <div class="form-group">
      <label for="issueDescription">Issue description (private)</label>
      <textarea id="issueDescription" v-model="supportTicketIssueDescription" class="form-control" rows="3"></textarea>
    </div>
    <b-button variant="info" :disabled="!selectedUserId || isBusy" @click="openSupportTicket">Open support ticket</b-button>

    <h2 class="mt-5">Active tickets</h2>
    <div>
      <b-button variant="info" class="mb-2" :disabled="isBusy" @click="getActiveSupportTickets">Refresh</b-button>
    </div>
    <b-table v-if="activeSupportTickets" class="mt-2" :items="activeSupportTickets" :fields="activeSupportTicketFields" primary-key="supportTicketID" striped bordered responsive>
      <template #cell(emailAddress)="row">{{ row.item.user.emailAddress }}</template>
      <template #cell(openedAt)="row">{{ formatDateTime(row.item.openedAt) }}</template>
      <template #cell(actions)="row">
        <b-button variant="primary" size="sm" @click="prefillCloseForm(row.item)">Close ticket</b-button>
      </template>
    </b-table>

    <div v-if="selectedCloseTicket">
      <h2 class="mt-5">Close ticket</h2>
      <div class="card mb-3">
        <div class="card-body">
          <div>
            <strong>Display name:</strong>
            {{ selectedCloseTicket.userDisplayName }}
          </div>
          <div>
            <strong>Email address:</strong>
            {{ selectedCloseTicket.user.emailAddress }}
          </div>
          <div>
            <strong>Verification code:</strong>
            {{ selectedCloseTicket.verificationCode }}
          </div>
          <div>
            <strong>Opened:</strong>
            {{ formatDateTime(selectedCloseTicket.openedAt) }}
          </div>
          <div>
            <strong>Issue:</strong>
            {{ selectedCloseTicket.issueDescription }}
          </div>
        </div>
      </div>
      <div class="form-group">
        <label for="supportTicketResolutionNotes">Resolution notes</label>
        <textarea id="supportTicketResolutionNotes" v-model="supportTicketResolutionNotes" class="form-control"></textarea>
      </div>
      <b-button variant="warning" :disabled="isBusy" @click="closeSupportTicket">Close support ticket</b-button>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import type { TableField } from 'bootstrap-vue-next';

import {
  adminClient,
  SupportUserSearchKind,
  type CloseSupportTicketRequest,
  type OpenSupportTicketRequest,
  type SupportTicketAdminListEntryViewModel,
  type SupportUserSearchMatchViewModel,
  type SupportUserSearchRequest
} from '@/api/clients';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatDateTime } from '@/utilities/formatters';

interface AdminSupportTicketsData {
  isBusy: boolean;
  errorInfo: string;
  jobAttempted: string;
  searchKind: SupportUserSearchKind;
  searchValue: string;
  searchMatches: SupportUserSearchMatchViewModel[] | null;
  selectedUserId: string | null;
  supportTicketIssueDescription: string;
  selectedCloseTicket: SupportTicketAdminListEntryViewModel | null;
  supportTicketResolutionNotes: string;
  activeSupportTickets: SupportTicketAdminListEntryViewModel[] | null;
}

export default defineComponent({
  setup() {
    const activeSupportTicketFields: TableField<SupportTicketAdminListEntryViewModel>[] = [
      { key: 'userDisplayName', label: 'Display name', thClass: 'bg-primary' },
      { key: 'emailAddress', label: 'Email address', thClass: 'bg-primary' },
      { key: 'verificationCode', label: 'Code', thClass: 'bg-primary' },
      { key: 'issueDescription', label: 'Issue', thClass: 'bg-primary' },
      { key: 'openedAt', label: 'Opened', thClass: 'bg-primary' },
      { key: 'actions', label: '', thClass: 'bg-primary' }
    ];

    return { activeSupportTicketFields, SupportUserSearchKind };
  },
  data(): AdminSupportTicketsData {
    return {
      isBusy: false,
      errorInfo: '',
      jobAttempted: '',
      searchKind: SupportUserSearchKind.UserId,
      searchValue: '',
      searchMatches: null,
      selectedUserId: null,
      supportTicketIssueDescription: '',
      selectedCloseTicket: null,
      supportTicketResolutionNotes: '',
      activeSupportTickets: null
    };
  },
  async mounted() {
    //Load quietly: the initial load isn't something the admin asked for, so it gets a banner only if it fails.
    try {
      await this.fetchActiveSupportTickets();
    } catch (error) {
      this.jobAttempted = 'Get active support tickets';
      this.errorInfo = getApiErrorMessage(error);
    }
  },
  methods: {
    formatDateTime,
    selectUserForTicket(userId: string): void {
      this.selectedUserId = userId;
    },
    prefillCloseForm(ticket: SupportTicketAdminListEntryViewModel): void {
      this.selectedCloseTicket = ticket;
      this.supportTicketResolutionNotes = '';
    },
    //Runs one request, with its result in the banner at the top of the page.
    async runAction(label: string, call: () => Promise<void>): Promise<void> {
      this.errorInfo = '';
      this.jobAttempted = label;
      this.isBusy = true;

      try {
        await call();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error);
      } finally {
        this.isBusy = false;
      }
    },
    async searchUsers(): Promise<void> {
      const request: SupportUserSearchRequest = {
        searchKind: this.searchKind,
        searchValue: this.searchValue
      };

      await this.runAction('Search users', async () => {
        this.searchMatches = await adminClient.searchSupportUsers(request);
      });
    },
    async fetchActiveSupportTickets(): Promise<void> {
      this.activeSupportTickets = await adminClient.getActiveSupportTickets();
      const selectedTicketID = this.selectedCloseTicket?.supportTicketID;
      if (selectedTicketID) {
        this.selectedCloseTicket = this.activeSupportTickets.find((x) => x.supportTicketID === selectedTicketID) ?? null;
      }
    },
    async getActiveSupportTickets(): Promise<void> {
      await this.runAction('Get active support tickets', () => this.fetchActiveSupportTickets());
    },
    async openSupportTicket(): Promise<void> {
      if (!this.selectedUserId) {
        return;
      }

      const request: OpenSupportTicketRequest = {
        userID: this.selectedUserId,
        issueDescription: this.supportTicketIssueDescription
      };

      await this.runAction('Open support ticket', async () => {
        await adminClient.openSupportTicket(request);
        await this.fetchActiveSupportTickets();
      });
    },
    async closeSupportTicket(): Promise<void> {
      if (!this.selectedCloseTicket) {
        return;
      }

      const request: CloseSupportTicketRequest = {
        supportTicketID: this.selectedCloseTicket.supportTicketID,
        resolutionNotes: this.supportTicketResolutionNotes
      };

      await this.runAction('Close support ticket', async () => {
        await adminClient.closeSupportTicket(request);
        this.selectedCloseTicket = null;
        this.supportTicketResolutionNotes = '';
        await this.fetchActiveSupportTickets();
      });
    }
  }
});
</script>
