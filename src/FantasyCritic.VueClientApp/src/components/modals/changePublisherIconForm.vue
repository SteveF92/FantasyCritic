<template>
  <b-modal id="change-publisher-icon-form" ref="changePublisherIconRef" title="Change Publisher Icon" @hidden="clearData" @show="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div v-show="!newPublisherIcon || !iconIsValid" class="alert alert-info">
      Your icon must be a single character, ideally an emoji.
      <br />
      <br />
      On mobile your keyboard probably has emoji built in, on desktop you can go to:
      <a href="https://emojipedia.org/" target="_blank">
        Emojipedia
        <font-awesome-icon icon="external-link-alt" size="sm" />
      </a>
    </div>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newPublisherIcon" class="control-label">Publisher Icon</label>
        <input id="newPublisherIcon" v-model="newPublisherIcon" name="newPublisherIcon" type="text" class="form-control input" />
      </div>
    </div>
    <template #footer>
      <input type="submit" class="btn btn-primary" value="Change Icon" :disabled="!iconIsValid || isBusy" @click="changePublisherIcon" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueClient, type ChangePublisherIconRequest } from '@/api/clients';
import { publisherIconIsValid } from '@/globalFunctions';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface ChangePublisherIconFormData {
  newPublisherIcon: string | undefined;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): ChangePublisherIconFormData {
    return {
      newPublisherIcon: '',
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredUserPublisher']),
    iconIsValid(): boolean {
      return publisherIconIsValid(this.newPublisherIcon);
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async changePublisherIcon(): Promise<void> {
      const request: ChangePublisherIconRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        publisherIcon: this.newPublisherIcon
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueClient.changePublisherIcon(request);

        (this.$refs.changePublisherIconRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('Publisher icon changed.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when changing your publisher icon.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.newPublisherIcon = this.requiredUserPublisher.publisherIcon;
      this.errorInfo = '';
    }
  }
});
</script>
