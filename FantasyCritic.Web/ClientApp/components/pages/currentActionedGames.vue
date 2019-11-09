<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Current Actioned Games</h1>

      <div v-if="actionedGames && actionedGames.length === 0" class="alert alert-info">No actioned games.</div>
      <div class="row" v-if="actionedGames && actionedGames.length !== 0">
        <b-table striped :items="actionedGames"></b-table>
      </div>
    </div>
  </div>
</template>
<script>
  import axios from 'axios';

  export default {
    data() {
      return {
        actionedGames: null
      }
    },
    computed: {

    },
    methods: {
      fetchActionedGames() {
        axios
          .get('/api/admin/GetCurrentActionedGames')
          .then(response => {
            this.actionedGames = response.data;
          })
          .catch(response => {

          });
      }
    },
    mounted() {
      this.fetchActionedGames();
    }
  }
</script>
