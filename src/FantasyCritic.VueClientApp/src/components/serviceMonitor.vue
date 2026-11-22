<template>
  <div>
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>

    <div class="mb-2">
      <b-button variant="info" size="sm" :disabled="isBusy" @click="refresh">Refresh</b-button>
      <span v-if="lastRefreshedAt" class="ml-2">refreshed {{ lastRefreshedAt.toLocaleString(DateTime.TIME_WITH_SECONDS) }}</span>
    </div>

    <b-table v-if="monitor" :items="services" :fields="fields" striped bordered responsive small>
      <template #cell(state)="row">
        <b-badge :variant="stateVariant(row.item.state)">
          <font-awesome-icon v-if="row.item.state === 'Draining'" icon="circle-notch" spin class="mr-1" />
          {{ row.item.state }}
        </b-badge>
      </template>
      <template #cell(details)="row">
        <div v-for="detail in row.item.details" :key="detail.label" class="small">
          <strong>{{ detail.label }}:</strong>
          {{ detailValue(detail) }}
        </div>
      </template>
      <template #cell(logLinks)="row">
        <b-button v-for="link in row.item.logLinks" :key="link.label" :variant="logLinkVariant(link)" size="sm" class="mr-1 mb-1" :href="link.url" target="_blank" rel="noopener">
          {{ link.label }}
        </b-button>
      </template>
    </b-table>
  </div>
</template>
<script lang="ts">
import { defineComponent } from 'vue';
import type { ColorVariant, TableField } from 'bootstrap-vue-next';
import { DateTime } from 'luxon';

import { adminClient, type LogLinkViewModel, type ServiceHealthDetailViewModel, type ServiceHealthViewModel, type ServiceMonitorViewModel } from '@/api/clients';
import { getApiErrorMessage } from '@/utilities/apiError';

interface ServiceRow {
  name: string;
  state: string;
  description?: string;
  details: ServiceHealthDetailViewModel[];
  logLinks: LogLinkViewModel[];
}

interface ServiceMonitorData {
  monitor: ServiceMonitorViewModel | null;
  isBusy: boolean;
  errorInfo: string;
  lastRefreshedAt: DateTime | null;
}

//The worker's states come from WorkerState on the server; the web app and the bot only have their health status.
const stateVariants: Record<string, ColorVariant> = {
  Running: 'success',
  Draining: 'warning',
  Off: 'secondary',
  Healthy: 'success',
  Degraded: 'warning',
  Unhealthy: 'danger',
  Unreachable: 'dark'
};

//Links that show only one level's lines have that level's colour. The rest are plain.
const logLevelVariants: Record<string, ColorVariant> = {
  Error: 'danger',
  Warning: 'warning'
};

function toRow(service: ServiceHealthViewModel, state: string): ServiceRow {
  return { name: service.name, state, description: service.description, details: service.details, logLinks: service.logLinks };
}

export default defineComponent({
  setup() {
    const fields: TableField<ServiceRow>[] = [
      { key: 'name', label: 'Service', thClass: 'bg-primary' },
      { key: 'state', label: 'State', thClass: 'bg-primary' },
      { key: 'description', label: 'Description', thClass: 'bg-primary' },
      { key: 'details', label: 'Details', thClass: 'bg-primary' },
      { key: 'logLinks', label: 'Logs', thClass: 'bg-primary' }
    ];

    return { DateTime, fields };
  },
  data(): ServiceMonitorData {
    return {
      monitor: null,
      isBusy: false,
      errorInfo: '',
      lastRefreshedAt: null
    };
  },
  computed: {
    services(): ServiceRow[] {
      if (!this.monitor) {
        return [];
      }

      return [toRow(this.monitor.web, this.monitor.web.status), toRow(this.monitor.worker, this.monitor.workerState), toRow(this.monitor.discordBot, this.monitor.discordBot.status)];
    }
  },
  async created() {
    await this.refresh();
  },
  methods: {
    //There is no timer. Watching the worker go from Draining to Off is a matter of pressing Refresh.
    async refresh(): Promise<void> {
      this.isBusy = true;
      this.errorInfo = '';

      try {
        this.monitor = await adminClient.getServiceMonitor();
        this.lastRefreshedAt = DateTime.now();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Could not load the service monitor.');
      } finally {
        this.isBusy = false;
      }
    },
    //Times are shown in the viewer's zone, with how long before the check they were, which is what says whether the worker is keeping up.
    detailValue(detail: ServiceHealthDetailViewModel): string {
      if (!detail.time || !this.monitor) {
        return detail.text ?? '';
      }

      const time = DateTime.fromISO(detail.time);
      const checkedAt = DateTime.fromISO(this.monitor.checkedAt);
      const format = time.hasSame(checkedAt, 'day') ? DateTime.TIME_WITH_SECONDS : DateTime.DATETIME_SHORT_WITH_SECONDS;
      return `${time.toLocaleString(format)} (${time.toRelative({ base: checkedAt })})`;
    },
    stateVariant(state: string): ColorVariant {
      return stateVariants[state] ?? 'secondary';
    },
    logLinkVariant(link: LogLinkViewModel): ColorVariant {
      return (link.level && logLevelVariants[link.level]) || 'info';
    }
  }
});
</script>
