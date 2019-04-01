<template>
  <div>
    <h1>Active Master Game Requests</h1>
    <div class="col-xl-8 col-lg-10 col-md-12" v-if="myRequests.length !== 0">
      <div class="row">
        <table class="table table-sm table-responsive-sm table-bordered table-striped">
          <thead>
            <tr class="bg-primary">
              <th scope="col" class="game-column">Game Name</th>
              <th scope="col"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="request in myRequests">
              <td>{{request.gameName}}</td>
              <td class="select-cell">
                <b-button variant="danger" size="sm" v-on:click="assignGame(request)">Assign Game</b-button>
              </td>
              <td class="select-cell">
                <b-button variant="danger" size="sm" v-on:click="createGame(request)">Create Game</b-button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
<script>
  import axios from 'axios';

  export default {
    data() {
      return {
        myRequests: []
      }
    },
    computed: {

    },
    methods: {
      fetchMyRequests() {
        axios
          .get('/api/admin/ActiveMasterGameRequests')
          .then(response => {
            this.activeRequests = response.data;
          })
          .catch(response => {

          });
      },
      createGame() {
        
      },
      assignGame() {

      }
    },
    mounted() {
      this.fetchMyRequests();
    }
  }
</script>
