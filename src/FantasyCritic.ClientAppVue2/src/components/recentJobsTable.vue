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
      <span v-if="lastRefreshedAt" class="ml-2">Page {{ page }} &middot; refreshed {{ lastRefreshedAt.toLocaleString(DateTime.TIME_WITH_SECONDS) }}</span>
    </div>

    <p v-if="jobs && !jobs.length">{{ hasFilters ? 'No jobs match these filters.' : 'No jobs yet.' }}</p>
    <b-table v-else-if="jobs" :items="jobs" :fields="fields" :tbody-tr-class="rowClass" striped bordered responsive small>
      <template #cell(type)="row">
        <div class="row-filter-cell">
          {{ jobTypeDisplayName(row.item.type) }}
          <span class="row-filters">
            <font-awesome-icon :disabled="isBusy" @click="filterJobType('only', row.item.type)" icon="eye" title="Show only this job type" />
            <font-awesome-icon :disabled="isBusy" @click="filterJobType('hide', row.item.type)" icon="eye-slash" title="Hide this job type" />
          </span>
        </div>
      </template>
      <template #cell(status)="row">
        <div class="row-filter-cell">
          <b-badge :variant="statusVariant(row.item.status)">
            <font-awesome-icon v-if="row.item.status === 'Running'" icon="circle-notch" spin class="mr-1" />
            {{ row.item.status }}
          </b-badge>
          <span class="row-filters">
            <font-awesome-icon :disabled="isBusy" @click="filterStatus('only', row.item.status)" icon="eye" title="Show only this status" />
            <font-awesome-icon :disabled="isBusy" @click="filterStatus('hide', row.item.status)" icon="eye-slash" title="Hide this status" />
          </span>
        </div>
      </template>
      <template #cell(requestedBy)="row">{{ row.item.createdByUserDisplayName || 'Scheduler' }}</template>
      <template #cell(createdAt)="row">{{ formatTimestamp(row.item.createdAt) }}</template>
      <template #cell(duration)="row">{{ duration(row.item) }}</template>
      <template #cell(detail)="row">
        <div class="d-flex">
          <font-awesome-icon v-if="isRunningSuspiciouslyLong(row.item)" icon="exclamation-triangle" class="text-warning mr-1 mt-1" title="Running for over an hour. The worker may have died." />
          <span class="job-detail-summary" :title="detailSummary(row.item)">{{ detailSummary(row.item) }}</span>
        </div>
      </template>
      <template #cell(actions)="row">
        <b-button v-if="isCancellable(row.item)" variant="danger" size="sm" :disabled="isBusy" @click="cancelJob(row.item)">Cancel</b-button>
        <b-button variant="secondary" size="sm" @click="row.toggleDetails">{{ row.detailsShowing ? 'Hide' : 'Details' }}</b-button>
        <b-button v-if="row.item.logsUrl" variant="info" size="sm" :href="row.item.logsUrl" target="_blank" rel="noopener">Logs</b-button>
      </template>
      <template #row-details="row">
        <dl class="row mx-0 mb-0">
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
              <div class="job-error">
                <div class="d-flex align-items-start">
                  <div class="job-error-message flex-grow-1">{{ errorParts(row.item).message }}</div>
                  <b-button v-clipboard:copy="row.item.errorMessage" v-clipboard:success="errorCopied" variant="secondary" size="sm" class="ml-2">Copy</b-button>
                </div>
                <pre v-if="errorParts(row.item).trace" class="job-error-trace">{{ errorParts(row.item).trace }}</pre>
              </div>
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
  'ArchiveDatabase',
  'EndOfYearRollover',
  'ExpireTrades',
  'FullAutomatedActionsProcess',
  'FullDataRefresh',
  'GrantSuperDrops',
  'MakeSlotsConsistent',
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
  'SendFinalYearStandings',
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
        { key: 'createdAt', label: 'Queued', thClass: 'bg-primary', tdClass: 'text-nowrap' },
        { key: 'duration', label: 'Duration', thClass: 'bg-primary', tdClass: 'text-nowrap' },
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
    //The icons on each row. "only" narrows to that one value; "hide" adds the value to what is hidden.
    async filterJobType(mode, jobType) {
      const option = this.jobTypeOptions.find((x) => x.value === jobType) || { value: jobType, label: this.jobTypeDisplayName(jobType) };
      this.selectedJobTypes = this.narrowFilter(this.jobTypeMode, this.selectedJobTypes, mode, option);
      this.jobTypeMode = mode;
      await this.changeFilter();
    },
    async filterStatus(mode, status) {
      const option = this.statusOptions.find((x) => x.value === status) || { value: status, label: spaceOutWords(status) };
      this.selectedStatuses = this.narrowFilter(this.statusMode, this.selectedStatuses, mode, option);
      this.statusMode = mode;
      await this.changeFilter();
    },
    narrowFilter(currentMode, currentSelected, mode, option) {
      const alreadyHiding = currentMode === 'hide' && mode === 'hide';
      const others = alreadyHiding ? currentSelected.filter((x) => x.value !== option.value) : [];
      return [...others, option];
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
      //"Amazon.RDS.Model.DBInstanceNotFoundException: DBInstance not found" -> "DBInstanceNotFoundException: DBInstance not found".
      //The namespace takes most of the room the message needs. The detail row has the whole thing.
      if (job.status === 'Error' && job.errorMessage) {
        return job.errorMessage
          .split('\n')[0]
          .trim()
          .replace(/^[\w.`]+\.(\w+):/, '$1:');
      }

      return job.detailedStatus || '';
    },
    //The first line of an exception's text is its type and message; everything after is the stack trace.
    errorParts(job) {
      const firstLineEnd = job.errorMessage.indexOf('\n');
      if (firstLineEnd === -1) {
        return { message: job.errorMessage.trim(), trace: '' };
      }

      return { message: job.errorMessage.slice(0, firstLineEnd).trim(), trace: job.errorMessage.slice(firstLineEnd + 1) };
    },
    errorCopied() {
      this.makeToast('Error copied to clipboard.');
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
        return 'highlighted-job';
      }
    }
  }
};
</script>

<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>
<style scoped>
/*An open job and its detail row read as one block on one background. Striping would otherwise give them different shades.*/
div >>> tr.b-table-has-details > td,
div >>> tr.b-table-details > td {
  background-color: #333333;
}

/*A fainter, dashed line than between jobs, so the column borders end somewhere without cutting the job off from its details.*/
div >>> tr.b-table-has-details > td {
  border-bottom: 1px dashed #6c757d;
}

.row-filters svg {
  cursor: pointer;
}

/*The filter icons cost the job name a line on every row and widen the status column, so where there is a mouse they only appear over the cell being pointed at.*/
@media (hover: hover) {
  .row-filter-cell {
    position: relative;
  }

  .row-filters {
    display: none;
    position: absolute;
    top: 0;
    right: 0;
    padding: 0 0.25rem;
    border-radius: 0.25rem;
    background-color: #414141;
  }

  td:hover .row-filters {
    display: inline;
  }
}

div >>> tr.highlighted-job > td:first-child {
  box-shadow: inset 4px 0 0 #d6993a;
}

/*Up to three lines at a bounded width, so an exception message says something before it is cut off without widening the table.
  The minimum stops an open detail row from squeezing it to a few characters.*/
.job-detail-summary {
  display: -webkit-box;
  min-width: 10rem;
  max-width: 14rem;
  overflow: hidden;
  overflow-wrap: anywhere;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 3;
}

.job-error {
  padding: 0.5rem;
  border-left: 4px solid #aa1e1e;
  background-color: rgba(0, 0, 0, 0.4);
}

.job-error-message {
  font-weight: bold;
  overflow-wrap: anywhere;
}

/*Stack traces wrap rather than scroll sideways. Unwrapped, one long line stretches the whole table.*/
.job-error-trace {
  max-height: 300px;
  margin: 0.5rem 0 0;
  overflow-y: auto;
  color: white;
  font-size: 12px;
  white-space: pre-wrap;
  overflow-wrap: anywhere;
}
</style>
