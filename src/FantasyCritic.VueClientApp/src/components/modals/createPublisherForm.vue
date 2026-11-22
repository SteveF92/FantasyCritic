<template>
  <b-modal id="create-publisher" ref="createPublisherRef" title="Create Publisher" @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="publisherName" class="control-label">Publisher Name</label>
        <input id="publisherName" v-model="publisherName" name="publisherName" type="text" class="form-control input" />
      </div>
    </div>
    <template #footer>
      <input type="submit" class="btn btn-primary" value="Create Publisher" :disabled="!publisherName.trim() || isBusy" @click="createPublisher" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import { BModal } from 'bootstrap-vue-next';

import { leagueClient, type CreatePublisherRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface CreatePublisherFormData {
  publisherName: string;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear'])
  },
  data(): CreatePublisherFormData {
    return {
      publisherName: '',
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async createPublisher(): Promise<void> {
      const request: CreatePublisherRequest = {
        leagueID: this.requiredLeagueYear.leagueID,
        year: this.requiredLeagueYear.year,
        publisherName: this.publisherName.trim()
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueClient.createPublisher(request);

        (this.$refs.createPublisherRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction(request.publisherName + ' created.');
        this.clearData();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when creating your publisher.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.publisherName = '';
      this.errorInfo = '';
    }
  }
});
</script>
