<template>
  <b-modal id="royaleChangePublisherSloganForm" ref="changeRoyalePublisherSloganRef" title="Change Publisher Slogan" @hidden="clearData" @show="clearData">
    <div class="alert alert-info">Your publisher slogan is a 'tagline' that you can choose to add to your publisher. Think of it like a "corporate motto".</div>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newPublisherSlogan" class="control-label">Publisher Slogan</label>
        <input id="newPublisherSlogan" v-model="newPublisherSlogan" name="newPublisherSlogan" type="text" class="form-control input" />
      </div>
    </div>
    <template #footer>
      <input type="submit" class="btn btn-primary" value="Change Slogan" @click="changePublisherSlogan" />
    </template>
  </b-modal>
</template>
<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import type { BModal } from 'bootstrap-vue-next';

import { royaleClient, type ChangeRoyalePublisherSloganRequest, type RoyalePublisherViewModel } from '@/api/clients';

interface RoyaleChangePublisherSloganFormData {
  newPublisherSlogan: string | undefined;
  errorInfo: string;
}

export default defineComponent({
  props: {
    userRoyalePublisher: { type: Object as PropType<RoyalePublisherViewModel>, required: true }
  },
  emits: {
    publisherSloganChanged: () => true
  },
  data(): RoyaleChangePublisherSloganFormData {
    return {
      newPublisherSlogan: '',
      errorInfo: ''
    };
  },
  created() {
    this.clearData();
  },
  methods: {
    changePublisherSlogan(): void {
      const request: ChangeRoyalePublisherSloganRequest = {
        publisherID: this.userRoyalePublisher.publisherID,
        publisherSlogan: this.newPublisherSlogan
      };
      royaleClient
        .changePublisherSlogan(request)
        .then(() => {
          (this.$refs.changeRoyalePublisherSloganRef as InstanceType<typeof BModal> | undefined)?.hide();
          this.$emit('publisherSloganChanged');
        })
        .catch(() => {});
    },
    clearData(): void {
      this.newPublisherSlogan = this.userRoyalePublisher.publisherSlogan;
    }
  }
});
</script>
