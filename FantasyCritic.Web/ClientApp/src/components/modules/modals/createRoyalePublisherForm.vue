<template>
  <b-modal id="createRoyalePublisher" ref="createRoyalePublisherRef" title="Create Publisher" @hidden="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="publisherName" class="control-label">Publisher Name</label>
        <input v-model="publisherName" id="publisherName" name="publisherName" type="text" class="form-control input" />
      </div>
    </div>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Create Publisher" v-on:click="createRoyalePublisher" :disabled="!publisherName" />
    </div>
  </b-modal>
</template>
<script>
import Vue from 'vue';
import axios from 'axios';

export default {
    data() {
        return {
            publisherName: '',
            errorInfo: ''
        };
    },
    props: ['royaleYearQuarter'],
    methods: {
        createRoyalePublisher() {
            var model = {
                year: this.royaleYearQuarter.year,
                quarter: this.royaleYearQuarter.quarter,
                publisherName: this.publisherName
            };
            axios
                .post('/api/royale/createRoyalePublisher', model)
                .then(response => {
                    let publisherid = response.data;
                    this.$router.push({ name: 'royalePublisher', params: { publisherid: publisherid } });
                })
                .catch(response => {

                });
        },
        clearData() {
            this.publisherName = '';
        }
    }
};
</script>
