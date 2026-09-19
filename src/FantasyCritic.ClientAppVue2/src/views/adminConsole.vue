<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Admin Console</h1>
      <div v-show="errorInfo" class="alert alert-danger">Request for '{{ jobAttempted }}' returned: {{ errorInfo }}</div>
      <div v-show="errorResponse" class="alert alert-danger">{{ errorResponse }}</div>
      <div v-show="lastJobFailed" class="alert alert-danger">'{{ jobAttempted }}' failed.</div>
      <div v-show="isBusy" class="alert alert-info">Request is processing...</div>
      <div v-show="jobAttempted && !lastJobFailed && !isBusy" class="alert alert-success">'{{ jobAttempted }}' successfully run.</div>

      <div class="row">
        <div class="col-12">
          <div v-if="isFactChecker" class="mb-3">
            <h4>Master Games</h4>
            <div>
              <b-button size="sm" class="mr-1 mb-1" variant="info" :to="{ name: 'activeMasterGameRequests' }">
                Requests
                <b-badge v-if="masterGameRequestCount" variant="danger" class="ml-1">{{ masterGameRequestCount }}</b-badge>
              </b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="info" :to="{ name: 'activeMasterGameChangeRequests' }">
                Change requests
                <b-badge v-if="masterGameChangeRequestCount" variant="danger" class="ml-1">{{ masterGameChangeRequestCount }}</b-badge>
              </b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="info" :to="{ name: 'masterGameCreator' }">Add new</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="warning" @click="showMergeMasterGame = true">Merge</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="warning" @click="takePostAction('FactChecker', 'ClearMasterGameEditDiscordQueue')">Clear Edit Queue</b-button>
            </div>
            <div v-show="showMergeMasterGame" class="mt-2">
              <div class="form-group">
                <label for="removeMasterGameID" class="control-label">Master Game ID (To Remove)</label>
                <input v-model="removeMasterGameID" type="text" class="form-control form-control-sm input" />
              </div>
              <div class="form-group">
                <label for="mergeIntoMasterGameID" class="control-label">Master Game ID (To Merge Into)</label>
                <input v-model="mergeIntoMasterGameID" type="text" class="form-control form-control-sm input" />
              </div>
              <b-button variant="danger" size="sm" @click="mergeMasterGame">Merge Games</b-button>
            </div>
          </div>

          <div v-if="isAdmin" class="mb-3">
            <h4>Site Management</h4>
            <div>
              <b-button size="sm" class="mr-1 mb-1" variant="info" :to="{ name: 'adminSupportTickets' }">
                Support tickets
                <b-badge v-if="supportTicketCount" variant="danger" class="ml-1">{{ supportTicketCount }}</b-badge>
              </b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="info" :to="{ name: 'adminSiteAnnouncements' }">Site announcements</b-button>
            </div>
          </div>

          <div v-if="isFactChecker" class="mb-3">
            <h4>Data Refresh Jobs</h4>
            <div>
              <b-button size="sm" class="mr-1 mb-1" variant="info" @click="takePostAction('FactChecker', 'FullDataRefresh')">Full Refresh</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="info" @click="takePostAction('FactChecker', 'RefreshCriticInfo')">Critic Scores</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="info" @click="takePostAction('FactChecker', 'RefreshGGInfo')">GG Info</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="info" @click="takePostAction('FactChecker', 'UpdateFantasyPoints')">Fantasy Points</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="info" @click="takePostAction('FactChecker', 'RefreshCaches')">Caches</b-button>
            </div>
          </div>

          <div v-if="isActionRunner" class="mb-3">
            <h4>Action Processing</h4>
            <div>
              <b-button size="sm" class="mr-1 mb-1" variant="info" :to="{ name: 'actionProcessingDryRunResults' }">Dry Run</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="info" href="/api/ActionRunner/ComparableActionProcessingDryRun">Comparable Dry Run (CSV)</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="warning" @click="takePostAction('ActionRunner', 'TurnOnActionProcessingMode')">Turn on action processing mode</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="info" @click="takePostAction('ActionRunner', 'TurnOffActionProcessingMode')">Turn off action processing mode</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="danger" @click="takePostAction('ActionRunner', 'ProcessActions')">Process Actions</b-button>
            </div>
          </div>

          <div v-if="isActionRunner" class="mb-3">
            <h4>Database</h4>
            <div>
              <b-button size="sm" class="mr-1 mb-1" variant="info" @click="getRecentDatabaseSnapshots">Show Snapshots</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="warning" @click="takePostAction('ActionRunner', 'SnapshotDatabase')">Snapshot Database</b-button>
            </div>
            <b-table v-if="recentSnapshots" :items="recentSnapshots" class="mt-2" striped bordered responsive small></b-table>
          </div>

          <div v-if="isAdmin || isActionRunner" class="mb-3">
            <h4>Other Actions</h4>
            <div>
              <b-button v-if="isAdmin" size="sm" class="mr-1 mb-1" variant="info" @click="showRecentConfirmationEmail = true">Resend Confirmation Email</b-button>

              <template v-if="isDevelopment && isAdmin">
                <b-button size="sm" class="mr-1 mb-1" variant="danger" @click="takePostAction('Admin', 'SendSpoofScoreUpdate')">Spoof Score Update</b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" @click="takePostAction('Admin', 'SendSpoofEditUpdate')">Spoof Edit Update</b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" @click="takePostAction('Admin', 'SendSpoofNewUpdate')">Spoof NewGame Update</b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" @click="takePostAction('Admin', 'SendSpoofReleasedUpdate')">Spoof Released Update</b-button>
              </template>

              <template v-if="isActionRunner">
                <b-button size="sm" class="mr-1 mb-1" variant="danger" @click="takePostAction('ActionRunner', 'ProcessSpecialAuctions')">Process Special Auctions</b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" @click="takePostAction('ActionRunner', 'UpdateTopBidsAndDrops')">Update Top Bids And Drops</b-button>
              </template>
              <template v-if="isAdmin">
                <b-button size="sm" class="mr-1 mb-1" variant="danger" @click="takePostAction('Admin', 'SendPublicBiddingEmails')">Send Public Bidding Emails</b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" @click="takePostAction('Admin', 'PushPublicBiddingDiscordMessages')">Push Public Bidding Messages</b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" @click="takePostAction('Admin', 'SendReleasingThisWeekUpdate')">Send Releasing This Week Update</b-button>

                <b-button size="sm" class="mr-1 mb-1" variant="danger" @click="takePostAction('Admin', 'MakePublisherSlotsConsistent')">Make Slots Consistent</b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" @click="showGrantSuperDrops = true">Grant Super Drops</b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" @click="takePostAction('Admin', 'ExpireTrades')">Expire Trades</b-button>

                <b-button size="sm" class="mr-1 mb-1" variant="danger" @click="takePostAction('Admin', 'RecalculateWinners')">Recalculate Last Season Winners</b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" @click="takePostAction('Admin', 'RecalculateRoyaleWinners')">Recalculate Royale Winners</b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="info" @click="takePostAction('Admin', 'RecomputeRulesBasedRoyaleGroups')">Recompute Rules Based Royale Groups</b-button>

                <b-button size="sm" class="mr-1 mb-1" variant="info" @click="takePostAction('Admin', 'RefreshPatreonInfo')">Refresh Patreon</b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="info" @click="takePostAction('Admin', 'UpdateDailyPublisherStatistics')">Update Daily Publisher Statistics</b-button>
              </template>
            </div>
            <div v-show="showRecentConfirmationEmail" class="mt-2">
              <div class="form-group">
                <label for="resendConfirmationUserID" class="control-label">User ID</label>
                <input v-model="resendConfirmationUserID" type="text" class="form-control form-control-sm input" />
              </div>
              <b-button variant="info" size="sm" @click="resendConfirmationEmail">Send Confirmation</b-button>
            </div>
            <div v-show="showGrantSuperDrops" class="mt-2">
              <div class="form-group">
                <label for="superDropConfirmation" class="control-label">Type 'I want to grant super drops'</label>
                <input v-model="superDropConfirmation" type="text" class="form-control form-control-sm input" />
              </div>
              <b-button variant="info" size="sm" @click="grantSuperDrops">Send Confirmation</b-button>
            </div>
          </div>
        </div>
      </div>

      <div v-if="isAdmin">
        <h4>Build</h4>
        <div v-if="buildInfoError" class="alert alert-danger">Could not load build info: {{ buildInfoError }}</div>
        <div v-else-if="buildInfo">
          <div v-if="buildInfo.isLocalBuild" class="alert alert-info">No release file found, so this is a local build. Deployed instances get one from the deploy pipeline.</div>
          <table class="table table-sm table-bordered w-auto">
            <tbody>
              <tr v-if="buildInfo.commitHash">
                <th>Commit</th>
                <td>
                  <a v-if="buildInfo.commitUrl" :href="buildInfo.commitUrl" target="_blank" rel="noopener">{{ buildInfo.shortCommitHash }}</a>
                  <span v-else>{{ buildInfo.shortCommitHash }}</span>
                  <span v-if="gitRefName" class="text-muted">({{ gitRefName }})</span>
                </td>
              </tr>
              <tr v-if="buildInfo.commitDate">
                <th>Commit date</th>
                <td>{{ buildInfo.commitDate | dateTime }}</td>
              </tr>
              <tr v-if="buildInfo.deployedAt">
                <th>Deployed</th>
                <td>{{ buildInfo.deployedAt | dateTime }}</td>
              </tr>
              <tr>
                <th>Running since</th>
                <td>{{ buildInfo.processStartedAt | dateTime }}</td>
              </tr>
              <tr v-if="buildInfo.builtAt">
                <th>Built</th>
                <td>{{ buildInfo.builtAt | dateTime }}</td>
              </tr>
              <tr v-if="buildInfo.releaseID">
                <th>Release</th>
                <td>
                  {{ buildInfo.releaseID }}
                  <span v-if="buildInfo.deployedEnvironment" class="text-muted">({{ buildInfo.deployedEnvironment }})</span>
                </td>
              </tr>
              <tr v-if="buildInfo.buildRunUrl">
                <th>Build log</th>
                <td><a :href="buildInfo.buildRunUrl" target="_blank" rel="noopener">GitHub Actions run</a></td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
import axios from 'axios';
import { mapGetters } from 'vuex';

import { adminClient } from '@/api/clients';

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
      showMergeMasterGame: false,
      removeMasterGameID: null,
      mergeIntoMasterGameID: null,
      resendConfirmationUserID: null,
      showGrantSuperDrops: false,
      superDropConfirmation: null,
      buildInfo: null,
      buildInfoError: null
    };
  },
  computed: {
    ...mapGetters(['adminTaskCounts']),
    masterGameRequestCount() {
      return this.adminTaskCounts ? this.adminTaskCounts.masterGameRequestCount : null;
    },
    masterGameChangeRequestCount() {
      return this.adminTaskCounts ? this.adminTaskCounts.masterGameChangeRequestCount : null;
    },
    supportTicketCount() {
      return this.adminTaskCounts ? this.adminTaskCounts.supportTicketCount : null;
    },
    gitRefName() {
      if (!this.buildInfo || !this.buildInfo.gitRef) {
        return null;
      }

      return this.buildInfo.gitRef.replace(/^refs\/(heads|tags)\//, '');
    }
  },
  async created() {
    await this.$store.dispatch('fetchAdminTaskCounts');
    await this.fetchBuildInfo();
  },
  methods: {
    async fetchBuildInfo() {
      //The endpoint is admin only, so don't bother calling it for fact checkers or action runners.
      if (!this.isAdmin) {
        return;
      }

      try {
        this.buildInfo = await adminClient.buildInfo();
      } catch (error) {
        this.buildInfoError = error;
      }
    },
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
    async mergeMasterGame() {
      this.lastJobFailed = false;
      this.jobAttempted = 'Merge Master Games';
      this.isBusy = true;

      let request = {
        removeMasterGameID: this.removeMasterGameID,
        mergeIntoMasterGameID: this.mergeIntoMasterGameID
      };

      try {
        await axios.post('/api/factchecker/MergeMasterGame', request);
      } catch (error) {
        this.errorInfo = error;
        this.errorResponse = error.response;
        this.lastJobFailed = true;
      } finally {
        this.isBusy = false;
      }
    },
    async getRecentDatabaseSnapshots() {
      this.lastJobFailed = false;
      this.jobAttempted = 'Getting snapshots';
      this.isBusy = true;

      try {
        const response = await axios.get('/api/ActionRunner/GetRecentDatabaseSnapshots');
        this.recentSnapshots = response.data;
      } catch (error) {
        this.errorInfo = error;
        this.errorResponse = error.response;
        this.lastJobFailed = true;
      } finally {
        this.isBusy = false;
      }
    },
    async resendConfirmationEmail() {
      this.lastJobFailed = false;
      this.jobAttempted = 'Recent Confirmation Email';
      this.isBusy = true;

      let request = {
        UserID: this.resendConfirmationUserID
      };

      try {
        await axios.post('/api/admin/ResendConfirmationEmail', request);
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
