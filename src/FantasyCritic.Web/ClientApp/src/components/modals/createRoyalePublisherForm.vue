<template>
  <b-modal id="createRoyalePublisher" ref="createRoyalePublisherRef" v-model="showModal" title="Create Publisher" @hidden="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="publisherName" class="control-label">Publisher Name</label>
        <input id="publisherName" v-model="publisherName" name="publisherName" type="text" class="form-control input" />
      </div>
    </div>
    <template #modal-footer>
      <input type="submit" class="btn btn-primary" value="Create Publisher" :disabled="!publisherName" @click="createRoyalePublisher" />
    </template>
  </b-modal>
</template>
<script>
import axios from 'axios';

export default {
  props: {
    royaleYearQuarter: { type: Object, required: true }
  },
  data() {
    return {
      publisherName: '',
      errorInfo: ''
    };
  },
  computed: {
    showModal() {
      return !!this.$store.getters['modal/modals'].createRoyalePublisher;
    }
  },
  methods: {
    createRoyalePublisher() {
      var model = {
        year: this.royaleYearQuarter.year,
        quarter: this.royaleYearQuarter.quarter,
        publisherName: this.publisherName
      };
      axios
        .post('/api/royale/createRoyalePublisher', model)
        .then((response) => {
          let publisherid = response.data;
          this.$router.push({ name: 'royalePublisher', params: { publisherid: publisherid } });
        })
        .catch(() => {});
    },
    clearData() {
      this.publisherName = '';
    }
  }
};
</script>
