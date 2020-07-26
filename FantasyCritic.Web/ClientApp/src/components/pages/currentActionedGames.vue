<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Current Actioned Games</h1>

      <div v-if="actionedGames && actionedGames.length === 0" class="alert alert-info">No actioned games.</div>
      <div class="row" v-if="actionedGames && actionedGames.length !== 0">
        <h2>Drops</h2>
        <masterGamesTable :masterGames="actionedGames.dropActions"></masterGamesTable>
        <h2>Pickups</h2>
        <masterGamesTable :masterGames="actionedGames.pickupActions"></masterGamesTable>
      </div>
      <div v-else class="spinner">
        <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
      </div>
    </div>
  </div>
</template>
<script>
  import axios from 'axios';
  import MasterGamesTable from "@/components/modules/gameTables/masterGamesTable";

  export default {
    data() {
      return {
        actionedGames: null
      }
    },
    components: {
      MasterGamesTable
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
<style scoped>
  .spinner {
    display: flex;
    justify-content: space-around;
  }
</style>
