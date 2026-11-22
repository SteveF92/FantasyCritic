<template>
  <b-modal id="change-publisher-slogan-form" ref="changePublisherSloganRef" title="Change Publisher Slogan" @hidden="clearData" @show="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-info">Your publisher slogan is a 'tagline' that you can choose to add to your publisher. Think of it like a "corporate motto".</div>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newPublisherSlogan" class="control-label">Publisher Slogan</label>
        <input id="newPublisherSlogan" v-model="newPublisherSlogan" name="newPublisherSlogan" type="text" class="form-control input" />
      </div>
    </div>
    <template #footer>
      <input type="submit" class="btn btn-primary" value="Change Slogan" :disabled="isBusy" @click="changePublisherSlogan" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueClient, type ChangePublisherSloganRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface ChangePublisherSloganFormData {
  newPublisherSlogan: string | undefined;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  computed: {
    ...mapState(useLeagueStore, ['requiredUserPublisher'])
  },
  data(): ChangePublisherSloganFormData {
    return {
      newPublisherSlogan: '',
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async changePublisherSlogan(): Promise<void> {
      const request: ChangePublisherSloganRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        publisherSlogan: this.newPublisherSlogan?.trim()
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueClient.changePublisherSlogan(request);

        (this.$refs.changePublisherSloganRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('Publisher slogan changed.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when changing your publisher slogan.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.newPublisherSlogan = this.requiredUserPublisher.publisherSlogan;
      this.errorInfo = '';
    }
  }
});
</script>
