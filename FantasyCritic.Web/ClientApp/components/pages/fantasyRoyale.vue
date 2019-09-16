<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Fantasy Royale</h1>

      <div v-if="!userPublisher" class="alert alert-info">
        Create your publisher to start playing!
        <b-button variant="primary" v-b-modal="'createRoyalePublisher'">Create Publisher</b-button>
        <createRoyalePublisherForm :royaleYearQuarter="royaleYearQuarter"></createRoyalePublisherForm>
      </div>
      <h2>Leaderboards {{year}}-Q{{quarter}}</h2>
    </div>
  </div>
</template>

<script>
  import Vue from 'vue';
  import axios from "axios";

  import CreateRoyalePublisherForm from "components/modules/modals/createRoyalePublisherForm";

  export default {
    props: ['year', 'quarter'],
    components: {
      CreateRoyalePublisherForm
    },
    data() {
      return {
        userPublisher: null,
        royaleYearQuarter: null
      }
    },
    methods: {
      async fetchRoyaleYearQuarter() {
        axios
          .get('/api/royale/RoyaleQuarter/' + this.year + '/' + this.quarter)
          .then(response => {
            this.royaleYearQuarter = response.data;
          })
          .catch(response => {

          });
      },
    },
    async mounted() {
      await this.fetchRoyaleYearQuarter();
    },
  }
</script>
