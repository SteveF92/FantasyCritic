<template>
  <div>
    <div v-show="errorResponse" class="alert alert-danger">{{ errorResponse }}</div>

    <div class="form-row align-items-end mb-2">
      <div class="form-group col-sm-5 mb-0">
        <label for="jobTypeFilter">Job type</label>
        <select id="jobTypeFilter" v-model="jobTypeFilter" class="form-control form-control-sm" @change="changeFilter">
          <option :value="null">All types</option>
          <option v-for="jobType in jobTypes" :key="jobType" :value="jobType">{{ jobTypeDisplayName(jobType) }}</option>
        </select>
      </div>
      <div class="form-group col-sm-7 mb-0">
        <b-button variant="info" size="sm" :disabled="isBusy" @click="refresh">Refresh</b-button>
        <b-button variant="secondary" size="sm" :disabled="isBusy || page === 1" @click="newerPage">Newer</b-button>
        <b-button variant="secondary" size="sm" :disabled="isBusy || !mayHaveOlderPage" @click="olderPage">Older</b-button>
        <span v-if="lastRefreshedAt" class="text-muted ml-2">Page {{ page }} &middot; refreshed {{ lastRefreshedAt.toLocaleString(DateTime.TIME_WITH_SECONDS) }}</span>
      </div>
    </div>

    <p v-if="jobs && !jobs.length" class="text-muted">No jobs yet.</p>
    <b-table v-else-if="jobs" :items="jobs" :fields="fields" :tbody-tr-class="rowClass" striped bordered responsive small>
      <template #cell(type)="row">
        {{ jobTypeDisplayName(row.item.type) }}
        <b-badge v-if="row.item.scheduledFor" variant="secondary" class="ml-1">cron</b-badge>
      </template>
      <template #cell(status)="row">
        <b-badge :variant="statusVariant(row.item.status)">
          <font-awesome-icon v-if="row.item.status === 'Running'" icon="circle-notch" spin class="mr-1" />
          {{ row.item.status }}
        </b-badge>
      </template>
      <template #cell(requestedBy)="row">{{ row.item.createdByUserDisplayName || 'Scheduler' }}</template>
      <template #cell(createdAt)="row">{{ formatTimestamp(row.item.createdAt) }}</template>
      <template #cell(duration)="row">{{ duration(row.item) }}</template>
      <template #cell(detail)="row">
        <font-awesome-icon v-if="isRunningSuspiciouslyLong(row.item)" icon="exclamation-triangle" class="text-warning mr-1" title="Running for over an hour. The worker may have died." />
        <span class="d-inline-block text-truncate align-bottom" style="max-width: 14rem">{{ detailSummary(row.item) }}</span>
      </template>
      <template #cell(actions)="row">
        <b-button v-if="isCancellable(row.item)" variant="danger" size="sm" :disabled="isBusy" @click="cancelJob(row.item)">Cancel</b-button>
        <b-button variant="secondary" size="sm" @click="row.toggleDetails">{{ row.detailsShowing ? 'Hide' : 'Details' }}</b-button>
      </template>
      <template #row-details="row">
        <dl class="row mb-0">
          <dt class="col-sm-3">Job ID</dt>
          <dd class="col-sm-9">{{ row.item.jobID }}</dd>
          <dt class="col-sm-3">Run type</dt>
          <dd class="col-sm-9">{{ row.item.runType }} ({{ row.item.severity }})</dd>
          <template v-if="row.item.scheduledFor">
            <dt class="col-sm-3">Scheduled for</dt>
            <dd class="col-sm-9">{{ formatFullTimestamp(row.item.scheduledFor) }}</dd>
          </template>
          <dt class="col-sm-3">Queued</dt>
          <dd class="col-sm-9">{{ formatFullTimestamp(row.item.createdAt) }}</dd>
          <template v-if="row.item.startedAt">
            <dt class="col-sm-3">Started</dt>
            <dd class="col-sm-9">{{ formatFullTimestamp(row.item.startedAt) }}</dd>
          </template>
          <template v-if="row.item.finishedAt">
            <dt class="col-sm-3">Finished</dt>
            <dd class="col-sm-9">{{ formatFullTimestamp(row.item.finishedAt) }}</dd>
          </template>
          <template v-if="row.item.cancelledAt">
            <dt class="col-sm-3">Cancel requested</dt>
            <dd class="col-sm-9">{{ formatFullTimestamp(row.item.cancelledAt) }} by {{ row.item.cancelledByUserDisplayName || 'the worker' }}</dd>
          </template>
          <template v-if="row.item.detailedStatus">
            <dt class="col-sm-3">Detailed status</dt>
            <dd class="col-sm-9" style="white-space: pre-wrap">{{ row.item.detailedStatus }}</dd>
          </template>
          <template v-if="row.item.errorMessage">
            <dt class="col-sm-3">Error</dt>
            <dd class="col-sm-9">
              <pre class="mb-0" style="max-height: 300px; overflow: auto">{{ row.item.errorMessage }}</pre>
            </dd>
          </template>
        </dl>
      </template>
    </b-table>
  </div>
</template>
<script>
import { DateTime } from 'luxon';

import { ApiException, jobManagerClient } from '@/api/clients';

//The values of FantasyCriticJobType. The API does not expose the list, so it lives here for the filter.
const jobTypes = [
  'AdvanceRoyaleQuarters',
  'EndOfYearRollover',
  'ExpireTrades',
  'FullDataRefresh',
  'GrantSuperDrops',
  'MakeSlotsConsistent',
  'PrepareForActionProcessing',
  'ProcessActions',
  'ProcessSpecialAuctions',
  'PushGameReleaseMessages',
  'RecalculateLastSeasonWinners',
  'RecalculateRoyaleWinners',
  'RecomputeRulesBasedRoyaleGroups',
  'RefreshCaches',
  'RefreshCriticScores',
  'RefreshGGInfo',
  'RefreshPatreonInfo',
  'SendAllPublicBiddingMessages',
  'SendPublicBiddingDiscordMessages',
  'SendPublicBiddingEmails',
  'SendReleasingThisWeekUpdate',
  'SnapshotDatabase',
  'UpdateDailyPublisherStatistics',
  'UpdateFantasyPoints',
  'UpdateTopBidsAndDrops'
];

const cancellableStatuses = ['Queued', 'Running'];

const statusVariants = {
  Queued: 'secondary',
  Running: 'primary',
  Cancelling: 'warning',
  Complete: 'success',
  Error: 'danger',
  Cancelled: 'dark',
  CancelledInProgress: 'warning'
};

export default {
  data() {
    return {
      DateTime,
      jobTypes,
      jobs: null,
      page: 1,
      count: 10,
      jobTypeFilter: null,
      isBusy: false,
      errorResponse: null,
      lastRefreshedAt: null,
      highlightedJobID: null,
      fields: [
        { key: 'type', label: 'Job', thClass: 'bg-primary' },
        { key: 'status', label: 'Status', thClass: 'bg-primary' },
        { key: 'requestedBy', label: 'Requested by', thClass: 'bg-primary' },
        { key: 'createdAt', label: 'Queued', thClass: 'bg-primary' },
        { key: 'duration', label: 'Duration', thClass: 'bg-primary' },
        { key: 'detail', label: 'Detail', thClass: 'bg-primary' },
        { key: 'actions', label: '', thClass: 'bg-primary' }
      ]
    };
  },
  computed: {
    mayHaveOlderPage() {
      return this.jobs && this.jobs.length === this.count;
    }
  },
  async created() {
    await this.refresh();
  },
  methods: {
    async refresh() {
      this.isBusy = true;
      this.errorResponse = null;

      try {
        this.jobs = await jobManagerClient.getJobs(this.page, this.count, this.jobTypeFilter);
        this.lastRefreshedAt = DateTime.now();
      } catch (error) {
        this.errorResponse = this.describeError(error);
      } finally {
        this.isBusy = false;
      }
    },
    //Jumps back to the newest jobs so a job the console just queued is on screen.
    async showLatest(jobID) {
      this.page = 1;
      this.jobTypeFilter = null;
      this.highlightedJobID = jobID;
      await this.refresh();
    },
    async changeFilter() {
      this.page = 1;
      await this.refresh();
    },
    async newerPage() {
      this.page = Math.max(this.page - 1, 1);
      await this.refresh();
    },
    async olderPage() {
      this.page += 1;
      await this.refresh();
    },
    async cancelJob(job) {
      const confirmed = await this.$bvModal.msgBoxConfirm(`Cancel the ${this.jobTypeDisplayName(job.type)} job queued ${this.formatTimestamp(job.createdAt)}?`, {
        title: 'Cancel Job',
        okTitle: 'Cancel job',
        okVariant: 'danger',
        cancelTitle: 'Keep it'
      });
      if (!confirmed) {
        return;
      }

      this.isBusy = true;
      this.errorResponse = null;

      try {
        await jobManagerClient.cancelJob({ jobID: job.jobID });
      } catch (error) {
        this.errorResponse = this.describeError(error);
      } finally {
        this.isBusy = false;
      }

      await this.refresh();
    },
    describeError(error) {
      if (ApiException.isApiException(error) && error.response) {
        return error.response;
      }

      return error.message || String(error);
    },
    jobTypeDisplayName(jobType) {
      //"RefreshGGInfo" -> "Refresh GG Info"
      return jobType.replace(/([a-z])([A-Z])|([A-Z])([A-Z][a-z])/g, '$1$3 $2$4');
    },
    statusVariant(status) {
      return statusVariants[status] || 'secondary';
    },
    isCancellable(job) {
      return cancellableStatuses.includes(job.status);
    },
    //Compact, for the grid. The year and the exact second are in the detail row.
    formatTimestamp(value) {
      if (!value) {
        return '';
      }

      return DateTime.fromISO(value).toFormat('M/d h:mm a');
    },
    formatFullTimestamp(value) {
      if (!value) {
        return '';
      }

      return DateTime.fromISO(value).toLocaleString(DateTime.DATETIME_SHORT_WITH_SECONDS);
    },
    duration(job) {
      if (!job.startedAt) {
        return '';
      }

      const started = DateTime.fromISO(job.startedAt);
      if (job.finishedAt) {
        return this.formatDuration(DateTime.fromISO(job.finishedAt).diff(started));
      }

      //Open jobs are measured as of the last refresh, not live, so the number matches the row it sits in.
      return `${this.formatDuration(this.lastRefreshedAt.diff(started))} so far`;
    },
    formatDuration(luxonDuration) {
      const totalSeconds = Math.max(Math.floor(luxonDuration.as('seconds')), 0);
      const hours = Math.floor(totalSeconds / 3600);
      const minutes = Math.floor((totalSeconds % 3600) / 60);
      const seconds = totalSeconds % 60;
      if (hours > 0) {
        return `${hours}h ${minutes}m ${seconds}s`;
      }
      if (minutes > 0) {
        return `${minutes}m ${seconds}s`;
      }
      return `${seconds}s`;
    },
    detailSummary(job) {
      if (job.status === 'Error' && job.errorMessage) {
        return job.errorMessage.split('\n')[0];
      }

      return job.detailedStatus || '';
    },
    //There is no worker heartbeat, so a job whose worker died stays Running forever. Flag it rather than hide it.
    isRunningSuspiciouslyLong(job) {
      if (job.status !== 'Running' || !job.startedAt || !this.lastRefreshedAt) {
        return false;
      }

      return this.lastRefreshedAt.diff(DateTime.fromISO(job.startedAt)).as('minutes') > 60;
    },
    rowClass(item, type) {
      if (!item || type !== 'row') {
        return;
      }

      if (item.jobID === this.highlightedJobID) {
        return 'table-info';
      }
    }
  }
};
</script>
