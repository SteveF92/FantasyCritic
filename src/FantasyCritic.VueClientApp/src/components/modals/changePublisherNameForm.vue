<template>
  <b-modal id="change-publisher-name-form" ref="changePublisherNameRef" title="Change Publisher Name" @hidden="clearData" @show="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newPublisherName" class="control-label">Publisher Name</label>
        <input id="newPublisherName" v-model="newPublisherName" name="newPublisherName" type="text" class="form-control input" />
      </div>
    </div>
    <template #footer>
      <input type="submit" class="btn btn-primary" value="Change Name" :disabled="!newPublisherName.trim() || isBusy" @click="changePublisherName" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueClient, type ChangePublisherNameRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface ChangePublisherNameFormData {
  newPublisherName: string;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  computed: {
    ...mapState(useLeagueStore, ['requiredUserPublisher'])
  },
  data(): ChangePublisherNameFormData {
    return {
      newPublisherName: '',
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async changePublisherName(): Promise<void> {
      const oldPublisherName = this.requiredUserPublisher.publisherName;
      const request: ChangePublisherNameRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        publisherName: this.newPublisherName.trim()
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueClient.changePublisherName(request);

        (this.$refs.changePublisherNameRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('Publisher name changed from ' + oldPublisherName + ' to ' + request.publisherName);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when changing your publisher name.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.newPublisherName = this.requiredUserPublisher.publisherName;
      this.errorInfo = '';
    }
  }
});
</script>
