<template>
  <div class="col-12">
    <h1>Admin Console</h1>
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">'{{ jobAttempted }}' failed: {{ errorInfo }}</b-alert>
    <b-alert v-else-if="isBusy" variant="info" :model-value="true">Request is processing...</b-alert>
    <b-alert v-else-if="jobAttempted && lastQueuedJob" variant="success" :model-value="true">'{{ jobAttempted }}' queued.</b-alert>
    <b-alert v-else-if="jobAttempted" variant="success" :model-value="true">'{{ jobAttempted }}' successfully run.</b-alert>

    <div class="row">
      <div :class="hasRightColumn ? 'col-lg-4 col-md-12' : 'col-12'">
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
            <b-button size="sm" class="mr-1 mb-1" variant="info" :to="{ name: 'pendingMasterGameUpdates' }">Pending Discord updates</b-button>
          </div>
          <div v-show="showMergeMasterGame" class="mt-2">
            <div class="form-group">
              <label for="removeMasterGameID" class="control-label">Master Game ID (To Remove)</label>
              <input id="removeMasterGameID" v-model="removeMasterGameID" type="text" class="form-control form-control-sm input" />
            </div>
            <div class="form-group">
              <label for="mergeIntoMasterGameID" class="control-label">Master Game ID (To Merge Into)</label>
              <input id="mergeIntoMasterGameID" v-model="mergeIntoMasterGameID" type="text" class="form-control form-control-sm input" />
            </div>
            <b-button variant="danger" size="sm" :disabled="isBusy || !removeMasterGameID.trim() || !mergeIntoMasterGameID.trim()" @click="mergeMasterGame">Merge Games</b-button>
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
            <toggleSwitch
              v-model="actionProcessingModeSwitch"
              class="toggle align-middle"
              :disabled="isBusy"
              :labels="{ checked: 'On', unchecked: 'Off' }"
              :height="24"
              @change="changeActionProcessingMode" />
            <span class="ml-2 align-middle">Action processing mode</span>
          </div>
          <div v-if="automatedActionProcessingLoaded" class="mb-2">
            <toggleSwitch
              v-model="automatedActionProcessingSwitch"
              class="toggle align-middle"
              :disabled="isBusy"
              :labels="{ checked: 'On', unchecked: 'Off' }"
              :height="24"
              @change="changeAutomatedActionProcessing" />
            <span class="ml-2 align-middle">Automated action processing</span>
          </div>
          <div>
            <b-button size="sm" class="mr-1 mb-1" variant="info" :to="{ name: 'actionProcessingDryRunResults' }">Dry Run</b-button>
            <b-button size="sm" class="mr-1 mb-1" variant="info" href="/api/ActionRunner/ComparableActionProcessingDryRun">Comparable Dry Run (CSV)</b-button>
            <b-button
              size="sm"
              class="mr-1 mb-1"
              variant="info"
              :disabled="isBusy"
              @click="runAction('Send Action Processing Pre-Check Email', () => actionRunnerClient.sendActionProcessingPreCheckEmail())">
              Send Action Processing Pre-Check Email
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
            <b-button size="sm" class="mr-1 mb-1" variant="warning" :disabled="isBusy" @click="enqueueJob('Archive Database', () => actionRunnerClient.archiveDatabase())">Archive Database</b-button>
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
                <b-dropdown-item-button @click="showGrantSuperDrops()">Grant Super Drops</b-dropdown-item-button>
                <b-dropdown-item-button @click="confirmOtherJob('Expire Trades', () => adminClient.expireTrades())">Expire Trades</b-dropdown-item-button>

                <b-dropdown-header>Year end</b-dropdown-header>
                <b-dropdown-item-button @click="confirmOtherJob('End Of Year Rollover', () => adminClient.endOfYearRollover())">End Of Year Rollover</b-dropdown-item-button>
                <b-dropdown-item-button @click="confirmOtherJob('Send Final Year Standings', () => adminClient.sendFinalYearStandings())">Send Final Year Standings</b-dropdown-item-button>

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
              <input id="resendConfirmationUserID" v-model="resendConfirmationUserID" type="text" class="form-control form-control-sm input" />
            </div>
            <b-button variant="info" size="sm" :disabled="isBusy || !resendConfirmationUserID.trim()" @click="resendConfirmationEmail">Send Confirmation</b-button>
          </div>
        </div>

        <div v-if="isAdmin">
          <h4>Build</h4>
          <b-alert v-if="buildInfoError" variant="danger" :model-value="true">Could not load build info: {{ buildInfoError }}</b-alert>
          <div v-else-if="buildInfo">
            <b-alert v-if="buildInfo.isLocalBuild" variant="info" :model-value="true">No release file found, so this is a local build. Deployed instances get one from the deploy pipeline.</b-alert>
            <table class="table table-sm table-bordered w-auto">
              <tbody>
                <tr v-if="buildInfo.commitHash">
                  <th>Commit</th>
                  <td>
                    <a v-if="buildInfo.commitUrl" :href="buildInfo.commitUrl" target="_blank" rel="noopener">{{ buildInfo.shortCommitHash }}</a>
                    <span v-else>{{ buildInfo.shortCommitHash }}</span>
                    <span v-if="gitRefName">({{ gitRefName }})</span>
                  </td>
                </tr>
                <tr v-if="buildInfo.commitDate">
                  <th>Commit date</th>
                  <td>{{ formatDateTime(buildInfo.commitDate) }}</td>
                </tr>
                <tr v-if="buildInfo.deployedAt">
                  <th>Deployed</th>
                  <td>{{ formatDateTime(buildInfo.deployedAt) }}</td>
                </tr>
                <tr>
                  <th>Running since</th>
                  <td>{{ formatDateTime(buildInfo.processStartedAt) }}</td>
                </tr>
                <tr v-if="buildInfo.builtAt">
                  <th>Built</th>
                  <td>{{ formatDateTime(buildInfo.builtAt) }}</td>
                </tr>
                <tr v-if="buildInfo.releaseID">
                  <th>Release</th>
                  <td>
                    {{ buildInfo.releaseID }}
                    <span v-if="buildInfo.deployedEnvironment">({{ buildInfo.deployedEnvironment }})</span>
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

      <div v-if="hasRightColumn" class="col-lg-8 col-md-12">
        <div v-if="isAdmin" class="mb-3">
          <h4>Services</h4>
          <service-monitor ref="serviceMonitor"></service-monitor>
        </div>

        <div v-if="isJobManager" class="mb-3">
          <h4>Recent Jobs</h4>
          <recent-jobs-table ref="jobsTable"></recent-jobs-table>
        </div>
      </div>
    </div>

    <b-modal
      id="grantSuperDropsModal"
      ref="grantSuperDropsModalRef"
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
<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import {
  actionRunnerClient,
  adminClient,
  factCheckerClient,
  type AdminResendConfirmationEmail,
  type BuildInfoViewModel,
  type DatabaseSnapshotInfoViewModel,
  type FantasyCriticJobViewModel,
  type MergeGameRequest
} from '@/api/clients';
import { useAdminStore } from '@/store/adminStore';
import { useAuthStore } from '@/store/authStore';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { useConfirm } from '@/utilities/confirm';
import { formatDateTime } from '@/utilities/formatters';
import RecentJobsTable from '@/components/recentJobsTable.vue';
import ServiceMonitor from '@/components/serviceMonitor.vue';
import ToggleSwitch from '@/components/toggleSwitch.vue';

interface AdminConsoleData {
  isBusy: boolean;
  errorInfo: string;
  jobAttempted: string;
  lastQueuedJob: FantasyCriticJobViewModel | null;
  recentSnapshots: DatabaseSnapshotInfoViewModel[] | null;
  showRecentConfirmationEmail: boolean;
  showMergeMasterGame: boolean;
  removeMasterGameID: string;
  mergeIntoMasterGameID: string;
  resendConfirmationUserID: string;
  superDropConfirmation: string;
  actionProcessingModeSwitch: boolean;
  automatedActionProcessingSwitch: boolean;
  automatedActionProcessingLoaded: boolean;
  buildInfo: BuildInfoViewModel | null;
  buildInfoError: string;
}

export default defineComponent({
  setup() {
    //The template calls the clients directly. Returned from setup, so Vue does not try to make the singletons reactive.
    return { confirm: useConfirm(), actionRunnerClient, adminClient, factCheckerClient, isDevelopment: import.meta.env.DEV };
  },
  components: {
    RecentJobsTable,
    ServiceMonitor,
    ToggleSwitch
  },
  data(): AdminConsoleData {
    return {
      isBusy: false,
      errorInfo: '',
      jobAttempted: '',
      lastQueuedJob: null,
      recentSnapshots: null,
      showRecentConfirmationEmail: false,
      showMergeMasterGame: false,
      removeMasterGameID: '',
      mergeIntoMasterGameID: '',
      resendConfirmationUserID: '',
      superDropConfirmation: '',
      actionProcessingModeSwitch: false,
      automatedActionProcessingSwitch: false,
      automatedActionProcessingLoaded: false,
      buildInfo: null,
      buildInfoError: ''
    };
  },
  computed: {
    ...mapState(useAuthStore, ['isAdmin', 'isFactChecker', 'isActionRunner', 'isJobManager']),
    ...mapState(useAdminStore, ['adminTaskCounts']),
    ...mapState(useInterLeagueStore, ['bidTimes']),
    masterGameRequestCount(): number | undefined {
      return this.adminTaskCounts.masterGameRequestCount;
    },
    masterGameChangeRequestCount(): number | undefined {
      return this.adminTaskCounts.masterGameChangeRequestCount;
    },
    supportTicketCount(): number | undefined {
      return this.adminTaskCounts.supportTicketCount;
    },
    //Jobs are for job managers and build info is for admins. Anyone else gets the buttons at full width.
    hasRightColumn(): boolean {
      return this.isJobManager || this.isAdmin;
    },
    superDropsConfirmed(): boolean {
      return this.superDropConfirmation === 'I want to grant super drops';
    },
    gitRefName(): string | null {
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
    const adminStore = useAdminStore();
    await adminStore.fetchAdminTaskCounts();
    await this.fetchBuildInfo();
    await this.fetchAutomatedActionProcessing();
  },
  methods: {
    formatDateTime,
    async fetchAutomatedActionProcessing(): Promise<void> {
      if (!this.isActionRunner) {
        return;
      }

      try {
        this.automatedActionProcessingSwitch = await actionRunnerClient.getEnableAutomatedActionProcessing();
        this.automatedActionProcessingLoaded = true;
      } catch (error) {
        this.jobAttempted = 'Get automated action processing';
        this.errorInfo = getApiErrorMessage(error);
      }
    },
    async fetchBuildInfo(): Promise<void> {
      //The endpoint is admin only, so don't bother calling it for fact checkers or action runners.
      if (!this.isAdmin) {
        return;
      }

      try {
        this.buildInfo = await adminClient.buildInfo();
      } catch (error) {
        this.buildInfoError = getApiErrorMessage(error);
      }
    },
    //For buttons that queue a background job. The call returns the queued job.
    async enqueueJob(label: string, call: () => Promise<FantasyCriticJobViewModel>): Promise<void> {
      const result = await this.runActionWithResult(label, call);
      if (!result.succeeded) {
        return;
      }

      this.lastQueuedJob = result.value;
      await (this.$refs.jobsTable as InstanceType<typeof RecentJobsTable> | undefined)?.showLatest(result.value);
    },
    //For the Other Jobs menu, where a stray click should not queue anything.
    async confirmOtherJob(label: string, call: () => Promise<FantasyCriticJobViewModel>): Promise<void> {
      const confirmed = await this.confirm(`Queue the '${label}' job?`, {
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
    //For everything that still does its work inside the request. Returns false if the request failed.
    async runAction(label: string, call: () => Promise<unknown>): Promise<boolean> {
      const result = await this.runActionWithResult(label, call);
      return result.succeeded;
    },
    //Like runAction, for a call whose response the console uses.
    async runActionWithResult<T>(label: string, call: () => Promise<T>): Promise<{ succeeded: true; value: T } | { succeeded: false }> {
      this.errorInfo = '';
      this.lastQueuedJob = null;
      this.jobAttempted = label;
      this.isBusy = true;

      try {
        return { succeeded: true, value: await call() };
      } catch (error) {
        //A 400 from the API carries its reason, e.g. "FullDataRefresh is already queued or running."
        this.errorInfo = getApiErrorMessage(error);
        return { succeeded: false };
      } finally {
        this.isBusy = false;
      }
    },
    syncActionProcessingModeSwitch(): void {
      this.actionProcessingModeSwitch = this.bidTimes.actionProcessingMode;
    },
    async changeActionProcessingMode(enabled: boolean): Promise<void> {
      const label = enabled ? 'Turn on action processing mode' : 'Turn off action processing mode';
      const call = enabled ? () => actionRunnerClient.turnOnActionProcessingMode() : () => actionRunnerClient.turnOffActionProcessingMode();
      await this.runAction(label, call);

      //The server is the truth. If the request failed, this also puts the switch back where it was.
      const interLeagueStore = useInterLeagueStore();
      await interLeagueStore.refreshBasicData();
      this.syncActionProcessingModeSwitch();
    },
    async changeAutomatedActionProcessing(enabled: boolean): Promise<void> {
      const label = enabled ? 'Turn on automated action processing' : 'Turn off automated action processing';
      const call = enabled ? () => actionRunnerClient.turnOnAutomatedActionProcessing() : () => actionRunnerClient.turnOffAutomatedActionProcessing();
      const succeeded = await this.runAction(label, call);
      if (!succeeded) {
        //Re-reading the setting would replace the failure's message, so the switch goes back by itself.
        this.automatedActionProcessingSwitch = !enabled;
        return;
      }

      //As with the mode switch, the server is the truth.
      await this.fetchAutomatedActionProcessing();
    },
    //Turning the worker off only stops it pulling new jobs. The service monitor shows it Draining, then Off.
    async turnOffWorker(): Promise<void> {
      const confirmed = await this.confirm(
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
      await this.refreshServiceMonitor();
    },
    async turnOnWorker(): Promise<void> {
      await this.runAction('Turn On Worker', () => adminClient.turnOnWorker());
      await this.refreshServiceMonitor();
    },
    async refreshServiceMonitor(): Promise<void> {
      await (this.$refs.serviceMonitor as InstanceType<typeof ServiceMonitor> | undefined)?.refresh();
    },
    showGrantSuperDrops(): void {
      (this.$refs.grantSuperDropsModalRef as InstanceType<typeof BModal> | undefined)?.show();
    },
    async grantSuperDrops(): Promise<void> {
      if (!this.superDropsConfirmed) {
        return;
      }

      await this.enqueueJob('Grant Super Drops', () => adminClient.grantSuperDrops());
    },
    async mergeMasterGame(): Promise<void> {
      const request: MergeGameRequest = {
        removeMasterGameID: this.removeMasterGameID.trim(),
        mergeIntoMasterGameID: this.mergeIntoMasterGameID.trim()
      };

      await this.runAction('Merge Master Games', () => factCheckerClient.mergeMasterGame(request));
    },
    async getRecentDatabaseSnapshots(): Promise<void> {
      const result = await this.runActionWithResult('Getting snapshots', () => actionRunnerClient.getRecentDatabaseSnapshots());
      if (result.succeeded) {
        this.recentSnapshots = result.value;
      }
    },
    async resendConfirmationEmail(): Promise<void> {
      const request: AdminResendConfirmationEmail = {
        userID: this.resendConfirmationUserID.trim()
      };

      await this.runAction('Resend Confirmation Email', () => adminClient.resendConfirmationEmail(request));
    }
  }
});
</script>
