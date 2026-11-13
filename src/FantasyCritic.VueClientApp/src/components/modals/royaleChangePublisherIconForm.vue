<template>
  <b-modal id="royaleChangePublisherIconForm" ref="changeRoyalePublisherIconRef" title="Change Publisher Icon" @hidden="clearData" @show="clearData">
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
      <input type="submit" class="btn btn-primary" value="Change Icon" :disabled="!iconIsValid" @click="changePublisherIcon" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import type { BModal } from 'bootstrap-vue-next';

import { royaleClient, type ChangeRoyalePublisherIconRequest, type RoyalePublisherViewModel } from '@/api/clients';
import { publisherIconIsValid } from '@/globalFunctions';
import { getApiErrorMessage } from '@/utilities/apiError';

interface RoyaleChangePublisherIconFormData {
  newPublisherIcon: string | undefined;
  errorInfo: string;
}

export default defineComponent({
  props: {
    userRoyalePublisher: { type: Object as PropType<RoyalePublisherViewModel>, required: true }
  },
  emits: {
    publisherIconChanged: () => true
  },
  data(): RoyaleChangePublisherIconFormData {
    return {
      newPublisherIcon: '',
      errorInfo: ''
    };
  },
  computed: {
    iconIsValid(): boolean {
      return publisherIconIsValid(this.newPublisherIcon ?? '');
    }
  },
  created() {
    this.clearData();
  },
  methods: {
    async changePublisherIcon(): Promise<void> {
      this.errorInfo = '';
      const request: ChangeRoyalePublisherIconRequest = {
        publisherID: this.userRoyalePublisher.publisherID,
        publisherIcon: this.newPublisherIcon
      };

      try {
        await royaleClient.changePublisherIcon(request);
        (this.$refs.changeRoyalePublisherIconRef as InstanceType<typeof BModal> | undefined)?.hide();
        this.$emit('publisherIconChanged');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error);
      }
    },
    clearData(): void {
      this.newPublisherIcon = this.userRoyalePublisher.publisherIcon;
      this.errorInfo = '';
    }
  }
});
</script>
