<template>
  <div>
    <div v-show="errorResponse" class="alert alert-danger">{{ errorResponse }}</div>

    <div class="form-row align-items-end">
      <div class="form-group col-sm-3 mb-1">
        <label for="jobTypeMode">Job types</label>
        <select id="jobTypeMode" v-model="jobTypeMode" class="form-control form-control-sm" @change="changeFilter">
          <option value="only">Show only</option>
          <option value="hide">Hide</option>
        </select>
      </div>
      <div class="form-group col-sm-9 mb-1">
        <multiselect
          v-model="selectedJobTypes"
          placeholder="All job types"
          label="label"
          track-by="value"
          :options="jobTypeOptions"
          :multiple="true"
          :close-on-select="false"
          @input="changeFilter"></multiselect>
      </div>
    </div>
    <div class="form-row align-items-end">
      <div class="form-group col-sm-3 mb-1">
        <label for="statusMode">Statuses</label>
        <select id="statusMode" v-model="statusMode" class="form-control form-control-sm" @change="changeFilter">
          <option value="only">Show only</option>
          <option value="hide">Hide</option>
        </select>
      </div>
      <div class="form-group col-sm-9 mb-1">
        <multiselect
          v-model="selectedStatuses"
          placeholder="All statuses"
          label="label"
          track-by="value"
          :options="statusOptions"
          :multiple="true"
          :close-on-select="false"
          @input="changeFilter"></multiselect>
      </div>
    </div>
    <div class="mb-2">
      <b-button variant="info" size="sm" :disabled="isBusy" @click="refresh">Refresh</b-button>
      <b-button variant="secondary" size="sm" :disabled="isBusy || page === 1" @click="newerPage">Newer</b-button>
      <b-button variant="secondary" size="sm" :disabled="isBusy || !mayHaveOlderPage" @click="olderPage">Older</b-button>
      <span v-if="lastRefreshedAt" class="text-muted ml-2">Page {{ page }} &middot; refreshed {{ lastRefreshedAt.toLocaleString(DateTime.TIME_WITH_SECONDS) }}</span>
    </div>

    <p v-if="jobs && !jobs.length" class="text-muted">{{ hasFilters ? 'No jobs match these filters.' : 'No jobs yet.' }}</p>
    <b-table v-else-if="jobs" :items="jobs" :fields="fields" :tbody-tr-class="rowClass" striped bordered responsive small>
      <template #cell(type)="row">
        <font-awesome-icon :disabled="isBusy" @click="filterJobType('only', row.item.type)" icon="eye" />
        <font-awesome-icon :disabled="isBusy" @click="filterJobType('hide', row.item.type)" icon="eye-slash" />
        {{ jobTypeDisplayName(row.item.type) }}
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
import Multiselect from 'vue-multiselect';

import { ApiException, jobManagerClient } from '@/api/clients';

//The values of FantasyCriticJobType. The API does not expose the list, so it lives here for the filter.
//A type missing from this list can still be hidden or shown by the server; it just cannot be picked here.
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

const savedFiltersKey = 'adminConsole.jobFilters';

const statusVariants = {
  Queued: 'secondary',
  Running: 'primary',
  Cancelling: 'warning',
  Complete: 'success',
  Error: 'danger',
  Cancelled: 'dark',
  CancelledInProgress: 'warning'
};

//"RefreshGGInfo" -> "Refresh GG Info"
function spaceOutWords(value) {
  return value.replace(/([a-z])([A-Z])|([A-Z])([A-Z][a-z])/g, '$1$3 $2$4');
}

export default {
  components: {
    Multiselect
  },
  data() {
    return {
      DateTime,
      jobTypeOptions: jobTypes.map((value) => ({ value, label: spaceOutWords(value) })),
      statusOptions: Object.keys(statusVariants).map((value) => ({ value, label: spaceOutWords(value) })),
      jobs: null,
      page: 1,
      count: 10,
      //Each filter is a list plus whether the list is what to show or what to hide. An empty list filters nothing.
      jobTypeMode: 'only',
      selectedJobTypes: [],
      statusMode: 'only',
      selectedStatuses: [],
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
    hasFilters() {
      return this.selectedJobTypes.length > 0 || this.selectedStatuses.length > 0;
    },
    mayHaveOlderPage() {
      return this.jobs && this.jobs.length === this.count;
    }
  },
  async created() {
    this.loadSavedFilters();
    await this.refresh();
  },
  methods: {
    async refresh() {
      this.isBusy = true;
      this.errorResponse = null;
      this.saveFilters();

      try {
        const jobTypeValues = this.selectedJobTypes.map((x) => x.value);
        const statusValues = this.selectedStatuses.map((x) => x.value);
        this.jobs = await jobManagerClient.getJobs(
          this.page,
          this.count,
          this.jobTypeMode === 'only' ? jobTypeValues : null,
          this.jobTypeMode === 'hide' ? jobTypeValues : null,
          this.statusMode === 'only' ? statusValues : null,
          this.statusMode === 'hide' ? statusValues : null
        );
        this.lastRefreshedAt = DateTime.now();
      } catch (error) {
        this.errorResponse = this.describeError(error);
      } finally {
        this.isBusy = false;
      }
    },
    //Jumps back to the newest jobs so a job the console just queued is on screen. A filter is only dropped if it would hide that job.
    async showLatest(job) {
      this.page = 1;
      this.highlightedJobID = job.jobID;
      if (this.isHidden(job.type, this.jobTypeMode, this.selectedJobTypes)) {
        this.selectedJobTypes = [];
      }
      if (this.isHidden(job.status, this.statusMode, this.selectedStatuses)) {
        this.selectedStatuses = [];
      }

      await this.refresh();
    },
    //Filters are remembered per browser, so a type hidden today is still hidden tomorrow. Every refresh saves them, since every filter change refreshes.
    saveFilters() {
      const filters = {
        jobTypeMode: this.jobTypeMode,
        jobTypes: this.selectedJobTypes.map((x) => x.value),
        statusMode: this.statusMode,
        statuses: this.selectedStatuses.map((x) => x.value)
      };
      localStorage.setItem(savedFiltersKey, JSON.stringify(filters));
    },
    loadSavedFilters() {
      const saved = localStorage.getItem(savedFiltersKey);
      if (!saved) {
        return;
      }

      //Values that no longer exist, such as a renamed job type, are dropped rather than sent to the server as a 400.
      const filters = JSON.parse(saved);
      this.jobTypeMode = filters.jobTypeMode === 'hide' ? 'hide' : 'only';
      this.selectedJobTypes = this.jobTypeOptions.filter((x) => (filters.jobTypes || []).includes(x.value));
      this.statusMode = filters.statusMode === 'hide' ? 'hide' : 'only';
      this.selectedStatuses = this.statusOptions.filter((x) => (filters.statuses || []).includes(x.value));
    },
    isHidden(value, mode, selected) {
      if (selected.length === 0) {
        return false;
      }

      const isSelected = selected.some((x) => x.value === value);
      return mode === 'only' ? !isSelected : isSelected;
    },
    //The links on each row. "only" narrows to that one type; "hide" adds the type to what is hidden.
    async filterJobType(mode, jobType) {
      const option = this.jobTypeOptions.find((x) => x.value === jobType) || { value: jobType, label: this.jobTypeDisplayName(jobType) };
      const alreadyHiding = this.jobTypeMode === 'hide' && mode === 'hide';
      const others = alreadyHiding ? this.selectedJobTypes.filter((x) => x.value !== jobType) : [];

      this.jobTypeMode = mode;
      this.selectedJobTypes = [...others, option];
      await this.changeFilter();
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
      return spaceOutWords(jobType);
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

<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>
