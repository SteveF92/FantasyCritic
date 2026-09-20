<template>
  <div>
    <div v-show="errorResponse" class="alert alert-danger">{{ errorResponse }}</div>

    <div class="mb-2">
      <b-button variant="info" size="sm" :disabled="isBusy" @click="refresh">Refresh</b-button>
      <span v-if="lastRefreshedAt" class="text-muted ml-2">refreshed {{ lastRefreshedAt.toLocaleString(DateTime.TIME_WITH_SECONDS) }}</span>
    </div>

    <b-table v-if="monitor" :items="services" :fields="fields" striped bordered responsive small>
      <template #cell(state)="row">
        <b-badge :variant="stateVariant(row.item.state)">
          <font-awesome-icon v-if="row.item.state === 'Draining'" icon="circle-notch" spin class="mr-1" />
          {{ row.item.state }}
        </b-badge>
      </template>
      <template #cell(details)="row">
        <div v-for="(value, key) in row.item.details" :key="key" class="small text-muted">{{ key }}: {{ value }}</div>
      </template>
    </b-table>
  </div>
</template>
<script>
import { DateTime } from 'luxon';

import { ApiException, adminClient } from '@/api/clients';

//The worker's states come from WorkerState on the server; the bot only has its health status.
const stateVariants = {
  Running: 'success',
  Draining: 'warning',
  Off: 'secondary',
  Healthy: 'success',
  Degraded: 'warning',
  Unhealthy: 'danger',
  Unreachable: 'dark'
};

export default {
  data() {
    return {
      DateTime,
      monitor: null,
      isBusy: false,
      errorResponse: null,
      lastRefreshedAt: null,
      fields: [
        { key: 'name', label: 'Service', thClass: 'bg-primary' },
        { key: 'state', label: 'State', thClass: 'bg-primary' },
        { key: 'description', label: 'Description', thClass: 'bg-primary' },
        { key: 'details', label: 'Details', thClass: 'bg-primary' }
      ]
    };
  },
  computed: {
    services() {
      if (!this.monitor) {
        return [];
      }

      return [
        { name: this.monitor.worker.name, state: this.monitor.workerState, description: this.monitor.worker.description, details: this.monitor.worker.details },
        { name: this.monitor.discordBot.name, state: this.monitor.discordBot.status, description: this.monitor.discordBot.description, details: this.monitor.discordBot.details }
      ];
    }
  },
  async created() {
    await this.refresh();
  },
  methods: {
    //There is no timer. Watching the worker go from Draining to Off is a matter of pressing Refresh.
    async refresh() {
      this.isBusy = true;
      this.errorResponse = null;

      try {
        this.monitor = await adminClient.getServiceMonitor();
        this.lastRefreshedAt = DateTime.now();
      } catch (error) {
        this.errorResponse = this.describeError(error);
      } finally {
        this.isBusy = false;
      }
    },
    stateVariant(state) {
      return stateVariants[state] || 'secondary';
    },
    describeError(error) {
      if (ApiException.isApiException(error) && error.response) {
        return error.response;
      }

      return error.message || String(error);
    }
  }
};
</script>
