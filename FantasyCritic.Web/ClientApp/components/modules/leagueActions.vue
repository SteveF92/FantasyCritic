<template>
  <div class="league-actions">
    <div class="publisher-image">
      <font-awesome-icon icon="user-circle" size="4x" />
    </div>
    <h5>{{leagueYear.userPublisher.publisherName}}</h5>
    <span>User: {{leagueYear.userPublisher.playerName}}</span>
    <hr/>
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
  .publisher-image {
    text-align: center;
  }
</style>
