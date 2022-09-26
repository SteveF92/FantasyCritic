<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Admin Console</h1>
      <div v-show="errorInfo" class="alert alert-danger">Request for '{{ jobAttempted }}' returned: {{ errorInfo }}</div>
      <div v-show="errorResponse" class="alert alert-danger">{{ errorResponse }}</div>
      <div v-show="lastJobFailed" class="alert alert-danger">'{{ jobAttempted }}' failed.</div>
      <div v-show="isBusy" class="alert alert-info">Request is processing...</div>
      <div v-show="jobAttempted && !lastJobFailed && !isBusy" class="alert alert-success">'{{ jobAttempted }}' sucessfully run.</div>

      <div v-if="isFactChecker">
        <h2>Requests</h2>
        <div>
          <b-button variant="info" :to="{ name: 'activeMasterGameRequests' }">View master game requests</b-button>
          <b-button variant="info" :to="{ name: 'activeMasterGameChangeRequests' }">View master game change requests</b-button>
          <b-button variant="info" :to="{ name: 'masterGameCreator' }">Add new master game</b-button>
        </div>

        <h2>Data Actions</h2>
        <div>
          <b-button variant="info" @click="takePostAction('FactChecker', 'FullDataRefresh')">Full Refresh</b-button>
          <b-button variant="info" @click="takePostAction('FactChecker', 'RefreshCriticInfo')">Refresh Critic Scores</b-button>
          <b-button variant="info" @click="takePostAction('FactChecker', 'RefreshGGInfo')">Refresh GG Info</b-button>
          <b-button variant="info" @click="takePostAction('FactChecker', 'UpdateFantasyPoints')">Update Fantasy Points</b-button>
          <b-button variant="info" @click="takePostAction('FactChecker', 'RefreshCaches')">Refresh Caches</b-button>
          <b-button variant="info" @click="takePostAction('FactChecker', 'RefreshPatreonInfo')">Refresh Patreon</b-button>
        </div>
      </div>

      <div v-if="isAdmin">
        <h2>Bids</h2>
        <div>
          <b-button variant="info" :to="{ name: 'actionProcessingDryRunResults' }">Action Processing Dry Run</b-button>
          <b-button variant="info" href="/api/Admin/ComparableActionProcessingDryRun">Comparable Action Processing Dry Run</b-button>
          <b-button variant="warning" @click="takePostAction('Admin', 'TurnOnActionProcessingMode')">Turn on action processing mode</b-button>
          <b-button variant="info" @click="takePostAction('Admin', 'TurnOffActionProcessingMode')">Turn off action processing mode</b-button>
          <b-button variant="danger" @click="takePostAction('Admin', 'ProcessActions')">Process Actions</b-button>
          <b-button variant="danger" @click="takePostAction('Admin', 'ProcessSpecialAuctions')">Process Special Auctions</b-button>
        </div>

        <h2>Other</h2>
        <div>
          <b-button variant="info" @click="showRecentConfirmationEmail = true">Resend Confirmation Email</b-button>
          <b-button variant="danger" @click="takePostAction('Admin', 'SendPublicBiddingEmails')">Send Public Bidding Emails</b-button>
          <b-button variant="danger" @click="takePostAction('Admin', 'MakePublisherSlotsConsistent')">Make Slots Consistent</b-button>
          <b-button variant="danger" @click="showGrantSuperDrops = true">Grant Super Drops</b-button>
          <b-button variant="danger" @click="takePostAction('Admin', 'ExpireTrades')">Expire Trades</b-button>
        </div>

        <h2>Database</h2>
        <div>
          <b-button variant="info" @click="getRecentDatabaseSnapshots">Get Recent Database Snapshots</b-button>
          <b-button variant="warning" @click="takePostAction('Admin', 'SnapshotDatabase')">Snapshot Database</b-button>
        </div>
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
    <div v-show="showGrantSuperDrops">
      <div class="form-group">
        <label for="superDropConfirmation" class="control-label">Type 'I want to grant super drops'</label>
        <input v-model="superDropConfirmation" type="text" class="form-control input" />
      </div>
      <b-button variant="info" @click="grantSuperDrops">Send Confirmation</b-button>
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
      resendConfirmationUserID: null,
      showGrantSuperDrops: false,
      superDropConfirmation: null
    };
  },
  methods: {
    async takePostAction(controller, endPoint) {
      this.lastJobFailed = false;
      this.jobAttempted = endPoint;
      this.isBusy = true;
      try {
        await axios.post(`/api/${controller}/${endPoint}`);
      } catch (error) {
        this.errorInfo = error;
        this.errorResponse = error.response;
        this.lastJobFailed = true;
      } finally {
        this.isBusy = false;
      }
    },
    async grantSuperDrops() {
      if (this.superDropConfirmation !== 'I want to grant super drops') {
        return;
      }

      this.showGrantSuperDrops = false;
      this.superDropConfirmation = null;
      await this.takePostAction('Admin', 'GrantSuperDrops');
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
