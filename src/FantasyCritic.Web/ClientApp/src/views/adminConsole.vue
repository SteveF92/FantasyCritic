<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Admin Console</h1>
      <div v-show="errorInfo" class="alert alert-danger">Request for '{{ jobAttempted }}' returned: {{ errorInfo }}</div>
      <div v-show="errorResponse" class="alert alert-danger">{{ errorResponse }}</div>
      <div v-show="lastJobFailed" class="alert alert-danger">'{{ jobAttempted }}' failed.</div>
      <div v-show="isBusy" class="alert alert-info">Request is processing...</div>
      <div v-show="jobAttempted && !lastJobFailed && !isBusy" class="alert alert-success">'{{ jobAttempted }}' sucessfully run.</div>

      <h2>Requests</h2>
      <div>
        <b-button variant="info" :to="{ name: 'activeMasterGameRequests' }">View master game requests</b-button>
        <b-button variant="info" :to="{ name: 'activeMasterGameChangeRequests' }">View master game change requests</b-button>
        <b-button variant="info" :to="{ name: 'masterGameCreator' }">Add new master game</b-button>
      </div>

      <h2>Data Actions</h2>
      <div>
        <b-button variant="info" @click="takePostAction('FullDataRefresh')">Full Refresh</b-button>
        <b-button variant="info" @click="takePostAction('RefreshCriticInfo')">Refresh Critic Scores</b-button>
        <b-button variant="info" @click="takePostAction('RefreshGGInfo')">Refresh GG Info</b-button>
        <b-button variant="info" @click="takePostAction('UpdateFantasyPoints')">Update Fantasy Points</b-button>
        <b-button variant="info" @click="takePostAction('RefreshCaches')">Refresh Caches</b-button>
        <b-button variant="info" @click="takePostAction('RefreshPatreonInfo')">Refresh Patreon</b-button>
      </div>

      <h2>Bids</h2>
      <div>
        <b-button variant="info" :to="{ name: 'actionProcessingDryRunResults' }">Action Processing Dry Run</b-button>
        <b-button variant="info" href="/api/Admin/ComparableActionProcessingDryRun">Comparable Action Processing Dry Run</b-button>
        <b-button variant="warning" @click="takePostAction('TurnOnActionProcessingMode')">Turn on action processing mode</b-button>
        <b-button variant="info" @click="takePostAction('TurnOffActionProcessingMode')">Turn off action processing mode</b-button>
        <b-button variant="danger" @click="takePostAction('ProcessActions')">Process Actions</b-button>
      </div>

      <h2>Other</h2>
      <div>
        <b-button variant="info" @click="showRecentConfirmationEmail = true">Resend Confirmation Email</b-button>
        <b-button variant="danger" @click="takePostAction('SendPublicBiddingEmails')">Send Public Bidding Emails</b-button>
        <b-button variant="danger" @click="takePostAction('MakePublisherSlotsConsistent')">Make Slots Consistent</b-button>
      </div>

      <h2>Database</h2>
      <div>
        <b-button variant="info" @click="takePostAction('FullDataRefresh')">Get Recent Database Snapshots</b-button>
        <b-button variant="warning" @click="getRecentDatabaseSnapshots">Snapshot Database</b-button>
      </div>
    </div>

    <b-table v-if="recentSnapshots" :items="recentSnapshots" striped bordered responsive></b-table>

    <div v-show="showRecentConfirmationEmail">
      <div class="form-group">
        <label for="resendConfirmationUserID" class="control-label">User ID</label>
        <input v-model="resendConfirmationUserID" type="text" class="form-control input" />
      </div>
      <b-button variant="info" @click="resendConfirmationEmail">Send Confirmation</b-button>
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
      recentSnapshots: null,
      showRecentConfirmationEmail: false,
      resendConfirmationUserID: null
    };
  },
  methods: {
    async takePostAction(endPoint) {
      this.lastJobFailed = false;
      this.jobAttempted = endPoint;
      this.isBusy = true;
      try {
        await axios.post(`/api/admin/${endPoint}`);
      } catch (error) {
        this.errorInfo = error;
        this.errorResponse = error.response;
        this.lastJobFailed = true;
      } finally {
        this.isBusy = false;
      }
    },
    getRecentDatabaseSnapshots() {
      this.isBusy = true;
      axios
        .get('/api/admin/GetRecentDatabaseSnapshots')
        .then((response) => {
          this.recentSnapshots = response.data;
          this.isBusy = false;
          this.jobSuccess = 'Getting snapshots';
        })
        .catch((returnedError) => {
          this.isBusy = false;
          this.errorInfo = returnedError.response.data;
        });
    },
    resendConfirmationEmail() {
      this.isBusy = true;
      let request = {
        UserID: this.resendConfirmationUserID
      };
      axios
        .post('/api/admin/ResendConfirmationEmail', request)
        .then(() => {
          this.isBusy = false;
          this.jobSuccess = 'Recent Confirmation Email';
        })
        .catch((returnedError) => {
          this.isBusy = false;
          this.errorInfo = returnedError.response.data;
        });
    }
  }
};
</script>
