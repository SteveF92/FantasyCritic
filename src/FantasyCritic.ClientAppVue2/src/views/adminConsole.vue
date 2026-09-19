<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Admin Console</h1>
      <div v-show="errorResponse" class="alert alert-danger">Request for '{{ jobAttempted }}' returned: {{ errorResponse }}</div>
      <div v-show="lastJobFailed" class="alert alert-danger">'{{ jobAttempted }}' failed.</div>
      <div v-show="isBusy" class="alert alert-info">Request is processing...</div>
      <div v-show="jobAttempted && !lastJobFailed && !isBusy && lastQueuedJob" class="alert alert-success">'{{ jobAttempted }}' queued.</div>
      <div v-show="jobAttempted && !lastJobFailed && !isBusy && !lastQueuedJob" class="alert alert-success">'{{ jobAttempted }}' successfully run.</div>

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
              <b-button size="sm" class="mr-1 mb-1" variant="warning" :disabled="isBusy" @click="runAction('Clear Edit Game Discord Queue', () => factCheckerClient.clearMasterGameEditDiscordQueue())">
                Clear Edit Queue
              </b-button>
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
              <b-button variant="danger" size="sm" :disabled="isBusy" @click="mergeMasterGame">Merge Games</b-button>
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
              <b-button size="sm" class="mr-1 mb-1" variant="info" :disabled="isBusy" @click="enqueueJob('Full Refresh', () => factCheckerClient.fullDataRefresh())">Full Refresh</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="info" :disabled="isBusy" @click="enqueueJob('Refresh Critic Scores', () => factCheckerClient.refreshCriticInfo())">Critic Scores</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="info" :disabled="isBusy" @click="enqueueJob('Refresh GG Info', () => factCheckerClient.refreshGGInfo())">GG Info</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="info" :disabled="isBusy" @click="enqueueJob('Update Fantasy Points', () => factCheckerClient.updateFantasyPoints())">
                Fantasy Points
              </b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="info" :disabled="isBusy" @click="enqueueJob('Refresh Caches', () => factCheckerClient.refreshCaches())">Caches</b-button>
            </div>
          </div>

          <div v-if="isActionRunner" class="mb-3">
            <h4>Action Processing</h4>
            <div>
              <b-button size="sm" class="mr-1 mb-1" variant="info" :to="{ name: 'actionProcessingDryRunResults' }">Dry Run</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="info" href="/api/ActionRunner/ComparableActionProcessingDryRun">Comparable Dry Run (CSV)</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="warning" :disabled="isBusy" @click="runAction('Turn on action processing mode', () => actionRunnerClient.turnOnActionProcessingMode())">
                Turn on action processing mode
              </b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="info" :disabled="isBusy" @click="runAction('Turn off action processing mode', () => actionRunnerClient.turnOffActionProcessingMode())">
                Turn off action processing mode
              </b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="danger" :disabled="isBusy" @click="enqueueJob('Process Actions', () => actionRunnerClient.processActions())">Process Actions</b-button>
            </div>
          </div>

          <div v-if="isActionRunner" class="mb-3">
            <h4>Database</h4>
            <div>
              <b-button size="sm" class="mr-1 mb-1" variant="info" :disabled="isBusy" @click="getRecentDatabaseSnapshots">Show Snapshots</b-button>
              <b-button size="sm" class="mr-1 mb-1" variant="warning" :disabled="isBusy" @click="enqueueJob('Snapshot Database', () => actionRunnerClient.snapshotDatabase())">
                Snapshot Database
              </b-button>
            </div>
            <b-table v-if="recentSnapshots" :items="recentSnapshots" class="mt-2" striped bordered responsive small></b-table>
          </div>

          <div v-if="isAdmin || isActionRunner" class="mb-3">
            <h4>Other Actions</h4>
            <div>
              <b-button v-if="isAdmin" size="sm" class="mr-1 mb-1" variant="info" @click="showRecentConfirmationEmail = true">Resend Confirmation Email</b-button>

              <template v-if="isDevelopment && isAdmin">
                <b-button size="sm" class="mr-1 mb-1" variant="danger" :disabled="isBusy" @click="runAction('Send Spoof Score Update', () => adminClient.sendSpoofScoreUpdate())">
                  Spoof Score Update
                </b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" :disabled="isBusy" @click="runAction('Send Spoof Edit Update', () => adminClient.sendSpoofEditUpdate())">
                  Spoof Edit Update
                </b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" :disabled="isBusy" @click="runAction('Send Spoof NewGame Update', () => adminClient.sendSpoofNewUpdate())">
                  Spoof NewGame Update
                </b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" :disabled="isBusy" @click="runAction('Send Spoof Released Update', () => adminClient.sendSpoofReleasedUpdate())">
                  Spoof Released Update
                </b-button>
              </template>

              <template v-if="isActionRunner">
                <b-button size="sm" class="mr-1 mb-1" variant="danger" :disabled="isBusy" @click="enqueueJob('Process Special Auctions', () => actionRunnerClient.processSpecialAuctions())">
                  Process Special Auctions
                </b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" :disabled="isBusy" @click="enqueueJob('Update Top Bids And Drops', () => actionRunnerClient.updateTopBidsAndDrops())">
                  Update Top Bids And Drops
                </b-button>
              </template>
              <template v-if="isAdmin">
                <b-button size="sm" class="mr-1 mb-1" variant="danger" :disabled="isBusy" @click="enqueueJob('Send Public Bidding Emails', () => adminClient.sendPublicBiddingEmails())">
                  Send Public Bidding Emails
                </b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" :disabled="isBusy" @click="enqueueJob('Push Public Bidding Messages', () => adminClient.pushPublicBiddingDiscordMessages())">
                  Push Public Bidding Messages
                </b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" :disabled="isBusy" @click="enqueueJob('Send Releasing This Week Update', () => adminClient.sendReleasingThisWeekUpdate())">
                  Send Releasing This Week Update
                </b-button>

                <b-button size="sm" class="mr-1 mb-1" variant="danger" :disabled="isBusy" @click="enqueueJob('Make Slots Consistent', () => adminClient.makePublisherSlotsConsistent())">
                  Make Slots Consistent
                </b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" :disabled="isBusy" @click="$bvModal.show('grantSuperDropsModal')">Grant Super Drops</b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" :disabled="isBusy" @click="enqueueJob('Expire Trades', () => adminClient.expireTrades())">Expire Trades</b-button>

                <b-button size="sm" class="mr-1 mb-1" variant="danger" :disabled="isBusy" @click="enqueueJob('Recalculate Last Season Winners', () => adminClient.recalculateWinners())">
                  Recalculate Last Season Winners
                </b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="danger" :disabled="isBusy" @click="enqueueJob('Recalculate Royale Winners', () => adminClient.recalculateRoyaleWinners())">
                  Recalculate Royale Winners
                </b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="info" :disabled="isBusy" @click="enqueueJob('Recompute Rules Based Royale Groups', () => adminClient.recomputeRulesBasedRoyaleGroups())">
                  Recompute Rules Based Royale Groups
                </b-button>

                <b-button size="sm" class="mr-1 mb-1" variant="info" :disabled="isBusy" @click="enqueueJob('Refresh Patreon', () => adminClient.refreshPatreonInfo())">Refresh Patreon</b-button>
                <b-button size="sm" class="mr-1 mb-1" variant="info" :disabled="isBusy" @click="enqueueJob('Update Daily Publisher Statistics', () => adminClient.updateDailyPublisherStatistics())">
                  Update Daily Publisher Statistics
                </b-button>
              </template>
            </div>
            <div v-show="showRecentConfirmationEmail" class="mt-2">
              <div class="form-group">
                <label for="resendConfirmationUserID" class="control-label">User ID</label>
                <input v-model="resendConfirmationUserID" type="text" class="form-control form-control-sm input" />
              </div>
              <b-button variant="info" size="sm" :disabled="isBusy" @click="resendConfirmationEmail">Send Confirmation</b-button>
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

    <b-modal
      id="grantSuperDropsModal"
      title="Grant Super Drops"
      :ok-disabled="!superDropsConfirmed"
      ok-variant="danger"
      ok-title="Queue job"
      @ok="grantSuperDrops"
      @hidden="superDropConfirmation = ''">
      <p>This queues a job that grants super drops to every eligible publisher that has not already received them.</p>
      <p>
        If you are sure, type
        <strong>I want to grant super drops</strong>
        into the box below and click the button.
      </p>
      <input v-model="superDropConfirmation" type="text" class="form-control input" />
    </b-modal>
  </div>
</template>
<script>
import { mapGetters } from 'vuex';

import { ApiException, actionRunnerClient, adminClient, factCheckerClient } from '@/api/clients';

export default {
  data() {
    return {
      isBusy: false,
      errorResponse: null,
      lastJobFailed: false,
      jobAttempted: '',
      lastQueuedJob: null,
      recentSnapshots: null,
      showRecentConfirmationEmail: false,
      showMergeMasterGame: false,
      removeMasterGameID: null,
      mergeIntoMasterGameID: null,
      resendConfirmationUserID: null,
      superDropConfirmation: '',
      buildInfo: null,
      buildInfoError: null
    };
  },
  computed: {
    ...mapGetters(['adminTaskCounts']),
    //The template calls the clients directly. Computed rather than data so Vue does not try to make the singletons reactive.
    actionRunnerClient() {
      return actionRunnerClient;
    },
    adminClient() {
      return adminClient;
    },
    factCheckerClient() {
      return factCheckerClient;
    },
    masterGameRequestCount() {
      return this.adminTaskCounts ? this.adminTaskCounts.masterGameRequestCount : null;
    },
    masterGameChangeRequestCount() {
      return this.adminTaskCounts ? this.adminTaskCounts.masterGameChangeRequestCount : null;
    },
    supportTicketCount() {
      return this.adminTaskCounts ? this.adminTaskCounts.supportTicketCount : null;
    },
    superDropsConfirmed() {
      return this.superDropConfirmation === 'I want to grant super drops';
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
    //For buttons that queue a background job. The call returns the queued FantasyCriticJobViewModel.
    async enqueueJob(label, call) {
      const job = await this.runAction(label, call);
      if (job) {
        this.lastQueuedJob = job;
      }
    },
    //For everything that still does its work inside the request.
    async runAction(label, call) {
      this.lastJobFailed = false;
      this.errorResponse = null;
      this.lastQueuedJob = null;
      this.jobAttempted = label;
      this.isBusy = true;

      try {
        return await call();
      } catch (error) {
        this.errorResponse = this.describeError(error);
        this.lastJobFailed = true;
        return null;
      } finally {
        this.isBusy = false;
      }
    },
    describeError(error) {
      //A 400 from the API carries its reason as a plain string body, e.g. "FullDataRefresh is already queued or running."
      if (ApiException.isApiException(error) && error.response) {
        return error.response;
      }

      return error.message || String(error);
    },
    async grantSuperDrops() {
      if (!this.superDropsConfirmed) {
        return;
      }

      await this.enqueueJob('Grant Super Drops', () => adminClient.grantSuperDrops());
    },
    async mergeMasterGame() {
      const request = {
        removeMasterGameID: this.removeMasterGameID,
        mergeIntoMasterGameID: this.mergeIntoMasterGameID
      };

      await this.runAction('Merge Master Games', () => factCheckerClient.mergeMasterGame(request));
    },
    async getRecentDatabaseSnapshots() {
      const snapshots = await this.runAction('Getting snapshots', () => actionRunnerClient.getRecentDatabaseSnapshots());
      if (snapshots) {
        this.recentSnapshots = snapshots;
      }
    },
    async resendConfirmationEmail() {
      const request = {
        userID: this.resendConfirmationUserID
      };

      await this.runAction('Resend Confirmation Email', () => adminClient.resendConfirmationEmail(request));
    }
  }
};
</script>
