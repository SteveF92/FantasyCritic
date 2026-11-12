<template>
  <b-modal id="createRoyalePublisher" ref="createRoyalePublisherRef" title="Create Publisher" @hidden="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="publisherName" class="control-label">Publisher Name</label>
        <input id="publisherName" v-model="publisherName" name="publisherName" type="text" class="form-control input" />
      </div>
    </div>
    <template #footer>
      <input type="submit" class="btn btn-primary" value="Create Publisher" :disabled="!publisherName" @click="createRoyalePublisher" />
    </template>
  </b-modal>
</template>
<script lang="ts">
import { defineComponent, type PropType } from 'vue';

import { royaleClient, type CreateRoyalePublisherRequest, type RoyaleYearQuarterViewModel } from '@/api/clients';

interface CreateRoyalePublisherFormData {
  publisherName: string;
  errorInfo: string;
}

export default defineComponent({
  props: {
    royaleYearQuarter: { type: Object as PropType<RoyaleYearQuarterViewModel>, required: true }
  },
  data(): CreateRoyalePublisherFormData {
    return {
      publisherName: '',
      errorInfo: ''
    };
  },
  methods: {
    createRoyalePublisher(): void {
      const request: CreateRoyalePublisherRequest = {
        year: this.royaleYearQuarter.year,
        quarter: this.royaleYearQuarter.quarter,
        publisherName: this.publisherName.trim()
      };
      royaleClient
        .createRoyalePublisher(request)
        .then((publisherid) => {
          this.$router.push({ name: 'royalePublisher', params: { publisherid: publisherid } });
        })
        .catch(() => {});
    },
    clearData(): void {
      this.publisherName = '';
    }
  }
});
</script>
