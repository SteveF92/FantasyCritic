<template>
  <div class="league-actions">
    <h5>{{leagueYear.userPublisher.publisherName}}</h5>
    <label>{{leagueYear.userPublisher.playerName}}</label>
    <h6>Drafting</h6>
    <h6>Game Bidding</h6>
    <h6>League History</h6>
    <h6>Manage League</h6>
  </div>
</template>
<script>
  export default {
    data() {
      return {
        errorInfo: ""
      }
    },
    props: ['league', 'leagueYear'],
    components: {
    }
  }
</script>
<style scoped>
  .league-actions {
    border: 2px;
    border-color: #D6993A;
    border-style: solid;
    background-color: #414141;
  }
</style>
