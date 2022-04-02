<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Admin Console</h1>
      <div v-show="errorInfo" class="alert alert-danger">{{ errorInfo }}</div>
      <div v-show="isBusy" class="alert alert-info">Request is processing...</div>
      <div v-show="jobSuccess" class="alert alert-success">'{{ jobSuccess }}' sucessfully run.</div>

      <h2>Requests</h2>
      <div>
        <b-button variant="info" :to="{ name: 'activeMasterGameRequests' }">View master game requests</b-button>
        <b-button variant="info" :to="{ name: 'activeMasterGameChangeRequests' }">View master game change requests</b-button>
        <b-button variant="info" :to="{ name: 'masterGameCreator' }">Add new master game</b-button>
      </div>

      <h2>Data Actions</h2>
      <div>
        <b-button variant="info" @click="fullRefresh">Full Refresh</b-button>
        <b-button variant="info" @click="refreshCriticScores">Refresh Critic Scores</b-button>
        <b-button variant="info" @click="refreshGGInfo">Refresh GG Info</b-button>
        <b-button variant="info" @click="updateFantasyPoints">Update Fantasy Points</b-button>
        <b-button variant="info" @click="refreshCaches">Refresh Caches</b-button>
        <b-button variant="info" @click="refreshPatreonInfo">Refresh Patreon</b-button>
      </div>

      <h2>Bids</h2>
      <div>
        <b-button variant="info" :to="{ name: 'actionProcessingDryRunResults' }">Action Processing Dry Run</b-button>
        <b-button variant="info" href="/api/Admin/ComparableActionProcessingDryRun">Comparable Action Processing Dry Run</b-button>
        <b-button variant="warning" @click="turnOnActionProcessing">Turn on action processing mode</b-button>
        <b-button variant="info" @click="turnOffActionProcessing">Turn off action processing mode</b-button>
        <b-button variant="danger" @click="processActions">Process Actions</b-button>
      </div>

      <h2>Other</h2>
      <div>
        <b-button variant="info" @click="showRecentConfirmationEmail = true">Resend Confirmation Email</b-button>
        <b-button variant="danger" @click="sendPublicBiddingEmails">Send Public Bidding Emails</b-button>
      </div>

      <h2>Database</h2>
      <div>
        <b-button variant="info" @click="getRecentDatabaseSnapshots">Get Recent Database Snapshots</b-button>
        <b-button variant="warning" @click="snapshotDatabase">Snapshot Database</b-button>
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
      errorInfo: '',
      jobSuccess: '',
      recentSnapshots: null,
      showRecentConfirmationEmail: false,
      resendConfirmationUserID: null
    };
  },
  methods: {
    fullRefresh() {
      this.isBusy = true;
      axios
        .post('/api/admin/FullDataRefresh')
        .then(() => {
          this.isBusy = false;
          this.jobSuccess = 'Full Data Refresh';
        })
        .catch((returnedError) => {
          this.isBusy = false;
          this.errorInfo = returnedError.response.data;
        });
    },
    refreshCriticScores() {
      this.isBusy = true;
      axios
        .post('/api/admin/RefreshCriticInfo')
        .then(() => {
          this.isBusy = false;
          this.jobSuccess = 'Refresh Critic Scores';
        })
        .catch((returnedError) => {
          this.isBusy = false;
          this.errorInfo = returnedError.response.data;
        });
    },
    refreshGGInfo() {
      this.isBusy = true;
      axios
        .post('/api/admin/RefreshGGInfo')
        .then(() => {
          this.isBusy = false;
          this.jobSuccess = 'Refresh GG Info';
        })
        .catch((returnedError) => {
          this.isBusy = false;
          this.errorInfo = returnedError.response.data;
        });
    },
    updateFantasyPoints() {
      this.isBusy = true;
      axios
        .post('/api/admin/updateFantasyPoints')
        .then(() => {
          this.isBusy = false;
          this.jobSuccess = 'Update Fantasy Points';
        })
        .catch((returnedError) => {
          this.isBusy = false;
          this.errorInfo = returnedError.response.data;
        });
    },
    processActions() {
      this.isBusy = true;
      axios
        .post('/api/admin/ProcessActions')
        .then(() => {
          this.isBusy = false;
          this.jobSuccess = 'Process Actions';
        })
        .catch((returnedError) => {
          this.isBusy = false;
          this.errorInfo = returnedError.response.data;
        });
    },
    turnOnActionProcessing() {
      this.isBusy = true;
      axios
        .post('/api/admin/TurnOnActionProcessingMode')
        .then(() => {
          this.isBusy = false;
          this.jobSuccess = 'Action Processing Mode ON';
        })
        .catch((returnedError) => {
          this.isBusy = false;
          this.errorInfo = returnedError.response.data;
        });
    },
    turnOffActionProcessing() {
      this.isBusy = true;
      axios
        .post('/api/admin/TurnOffActionProcessingMode')
        .then(() => {
          this.isBusy = false;
          this.jobSuccess = 'Action Processing Mode OFF';
        })
        .catch((returnedError) => {
          this.isBusy = false;
          this.errorInfo = returnedError.response.data;
        });
    },
    refreshCaches() {
      this.isBusy = true;
      axios
        .post('/api/admin/refreshCaches')
        .then(() => {
          this.isBusy = false;
          this.jobSuccess = 'Refresh Caches';
        })
        .catch((returnedError) => {
          this.isBusy = false;
          this.errorInfo = returnedError.response.data;
        });
    },
    refreshPatreonInfo() {
      this.isBusy = true;
      axios
        .post('/api/admin/refreshPatreonInfo')
        .then(() => {
          this.isBusy = false;
          this.jobSuccess = 'Refresh Patreon Info';
        })
        .catch((returnedError) => {
          this.isBusy = false;
          this.errorInfo = returnedError.response.data;
        });
    },
    snapshotDatabase() {
      this.isBusy = true;
      axios
        .post('/api/admin/snapshotDatabase')
        .then(() => {
          this.isBusy = false;
          this.jobSuccess = 'Database snapshot started';
        })
        .catch((returnedError) => {
          this.isBusy = false;
          this.errorInfo = returnedError.response.data;
        });
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
    },
    sendPublicBiddingEmails() {
      this.isBusy = true;
      axios
        .post('/api/admin/SendPublicBiddingEmails')
        .then(() => {
          this.isBusy = false;
          this.jobSuccess = 'Send Public Bid Emails';
        })
        .catch((returnedError) => {
          this.isBusy = false;
          this.errorInfo = returnedError.response.data;
        });
    }
  }
};
</script>
