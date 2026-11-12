<template>
  <b-modal id="royaleChangePublisherNameForm" ref="changeRoyalePublisherNameRef" title="Change Publisher Name" @hidden="clearData" @show="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newPublisherName" class="control-label">Publisher Name</label>
        <input id="newPublisherName" v-model="newPublisherName" name="newPublisherName" type="text" class="form-control input" />
      </div>
    </div>
    <template #footer>
      <input type="submit" class="btn btn-primary" value="Change Name" :disabled="!newPublisherName" @click="changePublisherName" />
    </template>
  </b-modal>
</template>
<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import type { BModal } from 'bootstrap-vue-next';

import { royaleClient, type ChangeRoyalePublisherNameRequest, type RoyalePublisherViewModel } from '@/api/clients';

interface RoyaleChangePublisherNameFormData {
  newPublisherName: string;
  errorInfo: string;
}

export default defineComponent({
  props: {
    userRoyalePublisher: { type: Object as PropType<RoyalePublisherViewModel>, required: true }
  },
  emits: {
    publisherNameChanged: () => true
  },
  data(): RoyaleChangePublisherNameFormData {
    return {
      newPublisherName: '',
      errorInfo: ''
    };
  },
  created() {
    this.clearData();
  },
  methods: {
    changePublisherName(): void {
      const request: ChangeRoyalePublisherNameRequest = {
        publisherID: this.userRoyalePublisher.publisherID,
        publisherName: this.newPublisherName
      };
      royaleClient
        .changePublisherName(request)
        .then(() => {
          (this.$refs.changeRoyalePublisherNameRef as InstanceType<typeof BModal> | undefined)?.hide();
          this.$emit('publisherNameChanged');
        })
        .catch(() => {});
    },
    clearData(): void {
      this.newPublisherName = this.userRoyalePublisher.publisherName;
    }
  }
});
</script>
