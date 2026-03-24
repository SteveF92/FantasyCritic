<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Support tickets</h1>
      <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
    </div>
    <hr />

    <div v-show="errorInfo" class="alert alert-danger">Request for '{{ jobAttempted }}' returned: {{ errorInfo }}</div>
    <div v-show="errorResponse" class="alert alert-danger">{{ errorResponse }}</div>
    <div v-show="lastJobFailed" class="alert alert-danger">'{{ jobAttempted }}' failed.</div>
    <div v-show="isBusy" class="alert alert-info">Request is processing...</div>
    <div v-show="jobAttempted && !lastJobFailed && !isBusy" class="alert alert-success">'{{ jobAttempted }}' successfully run.</div>

    <h2>Find user</h2>
    <p class="text-muted">Search by user ID, display name (exact match), or email (exact).</p>
    <div class="form-row align-items-end">
      <div class="form-group col-md-3">
        <label for="searchKind">Search by</label>
        <select id="searchKind" v-model="searchKind" class="form-control">
          <option :value="0">User ID</option>
          <option :value="1">Display name</option>
          <option :value="2">Email</option>
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

    <div v-if="searchMatches && searchMatches.length === 0 && searchWasRun" class="alert alert-warning">No users found.</div>

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
    <b-table v-if="activeSupportTickets" class="mt-2" :items="activeSupportTickets" :fields="activeSupportTicketFields" striped bordered responsive>
      <template #cell(actions)="row">
        <b-button variant="primary" size="sm" @click="prefillCloseForm(row.item)">Close ticket</b-button>
      </template>
    </b-table>

    <div v-if="selectedCloseTicket">
      <h2 class="mt-5">Close ticket</h2>
      <div class="card mb-3">
        <div class="card-body">
          <div><strong>Display name:</strong> {{ selectedCloseTicket.userDisplayName }}</div>
          <div><strong>Email address:</strong> {{ selectedCloseTicket.emailAddress }}</div>
          <div><strong>Verification code:</strong> {{ selectedCloseTicket.verificationCode }}</div>
          <div><strong>Opened:</strong> {{ selectedCloseTicket.openedAt }}</div>
          <div><strong>Issue:</strong> {{ selectedCloseTicket.issueDescription }}</div>
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

<script>
import axios from 'axios';

export default {
  data() {
    return {
      isBusy: false,
      errorInfo: null,
      errorResponse: null,
      lastJobFailed: false,
      jobAttempted: '',
      searchKind: 0,
      searchValue: '',
      searchMatches: null,
      searchWasRun: false,
      selectedUserId: null,
      supportTicketIssueDescription: null,
      closeSupportTicketID: null,
      selectedCloseTicket: null,
      supportTicketResolutionNotes: null,
      activeSupportTickets: null,
      activeSupportTicketFields: [
        { key: 'userDisplayName', label: 'Display name', thClass: 'bg-primary' },
        { key: 'emailAddress', label: 'Email address', thClass: 'bg-primary' },
        { key: 'verificationCode', label: 'Code', thClass: 'bg-primary' },
        { key: 'issueDescription', label: 'Issue', thClass: 'bg-primary' },
        { key: 'openedAt', label: 'Opened', thClass: 'bg-primary' },
        { key: 'actions', label: '', thClass: 'bg-primary' }
      ]
    };
  },
  async mounted() {
    await this.fetchActiveSupportTickets();
  },
  methods: {
    selectUserForTicket(userId) {
      this.selectedUserId = userId;
    },
    prefillCloseForm(ticket) {
      this.selectedCloseTicket = ticket;
      this.closeSupportTicketID = ticket.supportTicketID;
      this.supportTicketResolutionNotes = null;
    },
    async searchUsers() {
      this.lastJobFailed = false;
      this.jobAttempted = 'Search users';
      this.isBusy = true;
      this.searchWasRun = true;

      try {
        const response = await axios.post('/api/admin/SearchSupportUsers', {
          SearchKind: this.searchKind,
          SearchValue: this.searchValue
        });
        this.searchMatches = response.data;
      } catch (error) {
        this.errorInfo = error;
        this.errorResponse = error.response;
        this.lastJobFailed = true;
      } finally {
        this.isBusy = false;
      }
    },
    async fetchActiveSupportTickets() {
      const response = await axios.get('/api/admin/GetActiveSupportTickets');
      this.activeSupportTickets = response.data.map((ticket) => ({
        ...ticket,
        emailAddress: ticket.user?.emailAddress ?? ''
      }));
      if (this.closeSupportTicketID) {
        this.selectedCloseTicket = this.activeSupportTickets.find((x) => x.supportTicketID === this.closeSupportTicketID) ?? null;
      }
    },
    async getActiveSupportTickets() {
      this.lastJobFailed = false;
      this.jobAttempted = 'Get active support tickets';
      this.isBusy = true;

      try {
        await this.fetchActiveSupportTickets();
      } catch (error) {
        this.errorInfo = error;
        this.errorResponse = error.response;
        this.lastJobFailed = true;
      } finally {
        this.isBusy = false;
      }
    },
    async openSupportTicket() {
      if (!this.selectedUserId) {
        return;
      }

      this.lastJobFailed = false;
      this.jobAttempted = 'Open support ticket';
      this.isBusy = true;

      try {
        await axios.post('/api/admin/OpenSupportTicket', {
          UserID: this.selectedUserId,
          IssueDescription: this.supportTicketIssueDescription
        });
        await this.fetchActiveSupportTickets();
      } catch (error) {
        this.errorInfo = error;
        this.errorResponse = error.response;
        this.lastJobFailed = true;
      } finally {
        this.isBusy = false;
      }
    },
    async closeSupportTicket() {
      this.lastJobFailed = false;
      this.jobAttempted = 'Close support ticket';
      this.isBusy = true;

      try {
        await axios.post('/api/admin/CloseSupportTicket', {
          SupportTicketID: this.closeSupportTicketID,
          ResolutionNotes: this.supportTicketResolutionNotes
        });
        await this.fetchActiveSupportTickets();
        this.closeSupportTicketID = null;
        this.selectedCloseTicket = null;
        this.supportTicketResolutionNotes = null;
      } catch (error) {
        this.errorInfo = error;
        this.errorResponse = error.response;
        this.lastJobFailed = true;
      } finally {
        this.isBusy = false;
      }
    }
  }
};
</script>
