<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Current Failing Bids</h1>

      <div v-if="failingBids && failingBids.length === 0" class="alert alert-info">No failing bids.</div>
      <div class="row" v-if="failingBids && failingBids.length !== 0">
        <b-table striped :items="failingBids"></b-table>
      </div>
    </div>
  </div>
</template>
<script>
  import axios from 'axios';

  export default {
    data() {
      return {
        failingBids: null
      }
    },
    computed: {

    },
    methods: {
      fetchFailingBids() {
        axios
          .get('/api/admin/GetCurrentFailingBids')
          .then(response => {
            this.failingBids = response.data;
          })
          .catch(response => {

          });
      }
    },
    mounted() {
      this.fetchFailingBids();
    }
  }
</script>
