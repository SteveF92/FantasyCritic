<template>
  <div class="col-12">
    <h1>Admin Console</h1>
    <div v-show="errorResponse" class="alert alert-danger">Request for '{{ jobAttempted }}' returned: {{ errorResponse }}</div>
    <div v-show="lastJobFailed" class="alert alert-danger">'{{ jobAttempted }}' failed.</div>
    <div v-show="isBusy" class="alert alert-info">Request is processing...</div>
    <div v-show="jobAttempted && !lastJobFailed && !isBusy && lastQueuedJob" class="alert alert-success">'{{ jobAttempted }}' queued.</div>
    <div v-show="jobAttempted && !lastJobFailed && !isBusy && !lastQueuedJob" class="alert alert-success">'{{ jobAttempted }}' successfully run.</div>

    <div class="row">
      <div :class="hasRightColumn ? 'col-lg-5 col-md-12' : 'col-12'">
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
          <div v-if="bidTimes" class="mb-2">
            <toggle-button
              v-model="actionProcessingModeSwitch"
              class="toggle align-middle"
              :sync="true"
              :disabled="isBusy"
              :labels="{ checked: 'On', unchecked: 'Off' }"
              :css-colors="true"
              :font-size="13"
              :width="60"
              :height="24"
              @change="changeActionProcessingMode" />
            <span class="ml-2 align-middle">Action processing mode</span>
          </div>
          <div>
            <b-button size="sm" class="mr-1 mb-1" variant="info" :to="{ name: 'actionProcessingDryRunResults' }">Dry Run</b-button>
            <b-button size="sm" class="mr-1 mb-1" variant="info" href="/api/ActionRunner/ComparableActionProcessingDryRun">Comparable Dry Run (CSV)</b-button>
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

        <div v-if="isAdmin" class="mb-3">
          <h4>Worker</h4>
          <div>
            <b-button size="sm" class="mr-1 mb-1" variant="danger" :disabled="isBusy" @click="turnOffWorker">Turn Off Worker</b-button>
            <b-button size="sm" class="mr-1 mb-1" variant="info" :disabled="isBusy" @click="turnOnWorker">Turn On Worker</b-button>
          </div>
        </div>

        <div v-if="isAdmin || isActionRunner" class="mb-3">
          <h4>Other Actions</h4>
          <div>
            <b-button v-if="isAdmin" size="sm" class="mr-1 mb-1" variant="info" @click="showRecentConfirmationEmail = true">Resend Confirmation Email</b-button>

            <b-dropdown v-if="isDevelopment && isAdmin" text="Discord Test Pushes" size="sm" variant="danger" class="mr-1 mb-1" :disabled="isBusy">
              <b-dropdown-item-button @click="runAction('Send Spoof Score Update', () => adminClient.sendSpoofScoreUpdate())">Spoof Score Update</b-dropdown-item-button>
              <b-dropdown-item-button @click="runAction('Send Spoof Edit Update', () => adminClient.sendSpoofEditUpdate())">Spoof Edit Update</b-dropdown-item-button>
              <b-dropdown-item-button @click="runAction('Send Spoof NewGame Update', () => adminClient.sendSpoofNewUpdate())">Spoof NewGame Update</b-dropdown-item-button>
              <b-dropdown-item-button @click="runAction('Send Spoof Released Update', () => adminClient.sendSpoofReleasedUpdate())">Spoof Released Update</b-dropdown-item-button>
            </b-dropdown>

            <b-dropdown text="Other Jobs" size="sm" variant="secondary" class="mr-1 mb-1" :disabled="isBusy">
              <template v-if="isActionRunner">
                <b-dropdown-header>Bids and auctions</b-dropdown-header>
                <b-dropdown-item-button @click="confirmOtherJob('Process Special Auctions', () => actionRunnerClient.processSpecialAuctions())">Process Special Auctions</b-dropdown-item-button>
                <b-dropdown-item-button @click="confirmOtherJob('Update Top Bids And Drops', () => actionRunnerClient.updateTopBidsAndDrops())">Update Top Bids And Drops</b-dropdown-item-button>
              </template>
              <template v-if="isAdmin">
                <b-dropdown-header>Notifications</b-dropdown-header>
                <b-dropdown-item-button @click="confirmOtherJob('Send Public Bidding Emails', () => adminClient.sendPublicBiddingEmails())">Send Public Bidding Emails</b-dropdown-item-button>
                <b-dropdown-item-button @click="confirmOtherJob('Push Public Bidding Messages', () => adminClient.pushPublicBiddingDiscordMessages())">
                  Push Public Bidding Messages
                </b-dropdown-item-button>
                <b-dropdown-item-button @click="confirmOtherJob('Send Releasing This Week Update', () => adminClient.sendReleasingThisWeekUpdate())">
                  Send Releasing This Week Update
                </b-dropdown-item-button>

                <b-dropdown-header>League maintenance</b-dropdown-header>
                <b-dropdown-item-button @click="confirmOtherJob('Make Slots Consistent', () => adminClient.makePublisherSlotsConsistent())">Make Slots Consistent</b-dropdown-item-button>
                <b-dropdown-item-button @click="$bvModal.show('grantSuperDropsModal')">Grant Super Drops</b-dropdown-item-button>
                <b-dropdown-item-button @click="confirmOtherJob('Expire Trades', () => adminClient.expireTrades())">Expire Trades</b-dropdown-item-button>

                <b-dropdown-header>Winners and Royale</b-dropdown-header>
                <b-dropdown-item-button @click="confirmOtherJob('Recalculate Last Season Winners', () => adminClient.recalculateWinners())">Recalculate Last Season Winners</b-dropdown-item-button>
                <b-dropdown-item-button @click="confirmOtherJob('Recalculate Royale Winners', () => adminClient.recalculateRoyaleWinners())">Recalculate Royale Winners</b-dropdown-item-button>
                <b-dropdown-item-button @click="confirmOtherJob('Recompute Rules Based Royale Groups', () => adminClient.recomputeRulesBasedRoyaleGroups())">
                  Recompute Rules Based Royale Groups
                </b-dropdown-item-button>

                <b-dropdown-header>Scheduled data</b-dropdown-header>
                <b-dropdown-item-button @click="confirmOtherJob('Refresh Patreon', () => adminClient.refreshPatreonInfo())">Refresh Patreon</b-dropdown-item-button>
                <b-dropdown-item-button @click="confirmOtherJob('Update Daily Publisher Statistics', () => adminClient.updateDailyPublisherStatistics())">
                  Update Daily Publisher Statistics
                </b-dropdown-item-button>
              </template>
            </b-dropdown>
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

      <div v-if="hasRightColumn" class="col-lg-7 col-md-12">
        <div v-if="isAdmin" class="mb-3">
          <h4>Services</h4>
          <service-monitor ref="serviceMonitor"></service-monitor>
        </div>

        <div v-if="isJobManager" class="mb-3">
          <h4>Recent Jobs</h4>
          <recent-jobs-table ref="jobsTable"></recent-jobs-table>
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
import { ToggleButton } from 'vue-js-toggle-button';

import { ApiException, actionRunnerClient, adminClient, factCheckerClient } from '@/api/clients';
import RecentJobsTable from '@/components/recentJobsTable.vue';
import ServiceMonitor from '@/components/serviceMonitor.vue';

export default {
  components: {
    RecentJobsTable,
    ServiceMonitor,
    ToggleButton
  },
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
      actionProcessingModeSwitch: false,
      buildInfo: null,
      buildInfoError: null
    };
  },
  computed: {
    ...mapGetters(['adminTaskCounts', 'bidTimes']),
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
    //Jobs are for job managers and build info is for admins. Anyone else gets the buttons at full width.
    hasRightColumn() {
      return this.isJobManager || this.isAdmin;
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
  watch: {
    bidTimes: {
      immediate: true,
      handler() {
        this.syncActionProcessingModeSwitch();
      }
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
      if (!job) {
        return;
      }

      this.lastQueuedJob = job;
      if (this.$refs.jobsTable) {
        await this.$refs.jobsTable.showLatest(job);
      }
    },
    //For the Other Jobs menu, where a stray click should not queue anything.
    async confirmOtherJob(label, call) {
      const confirmed = await this.$bvModal.msgBoxConfirm(`Queue the '${label}' job?`, {
        title: 'Queue Job',
        okTitle: 'Queue job',
        okVariant: 'danger',
        cancelTitle: 'Cancel'
      });
      if (!confirmed) {
        return;
      }

      await this.enqueueJob(label, call);
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
    syncActionProcessingModeSwitch() {
      this.actionProcessingModeSwitch = !!this.bidTimes && this.bidTimes.actionProcessingMode;
    },
    async changeActionProcessingMode(event) {
      const enabled = event.value;
      const label = enabled ? 'Turn on action processing mode' : 'Turn off action processing mode';
      const call = enabled ? () => actionRunnerClient.turnOnActionProcessingMode() : () => actionRunnerClient.turnOffActionProcessingMode();
      await this.runAction(label, call);

      //The server is the truth. If the request failed, this also puts the switch back where it was.
      await this.$store.dispatch('fetchBasicData');
      this.syncActionProcessingModeSwitch();
    },
    //Turning the worker off only stops it pulling new jobs. The service monitor shows it Draining, then Off.
    async turnOffWorker() {
      const confirmed = await this.$bvModal.msgBoxConfirm(
        'The worker will finish the job it is running, then pick up nothing new until it is turned back on. Scheduled jobs keep queuing in the meantime and run when it is.',
        {
          title: 'Turn Off Worker',
          okTitle: 'Turn off',
          okVariant: 'danger',
          cancelTitle: 'Cancel'
        }
      );
      if (!confirmed) {
        return;
      }

      await this.runAction('Turn Off Worker', () => adminClient.turnOffWorker());
      await this.$refs.serviceMonitor.refresh();
    },
    async turnOnWorker() {
      await this.runAction('Turn On Worker', () => adminClient.turnOnWorker());
      await this.$refs.serviceMonitor.refresh();
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
