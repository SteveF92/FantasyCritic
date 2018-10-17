<template>
  <div>
    <div v-if="leagueYear && leagueYear.userPublisher">
      <h4>Player Actions</h4>
      <div class="player-actions" role="group" aria-label="Basic example">
        <b-button variant="info" class="nav-link" v-b-modal="'bidGameForm'">Bid on a Game</b-button>
        <b-button variant="info" class="nav-link" v-b-modal="'currentBidsForm'">Current Bids</b-button>
        <b-button variant="info" class="nav-link">See League History</b-button>
      </div>
      <br />
      <div v-if="leagueYear">
        <bidGameForm :leagueYear="leagueYear" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" v-on:gameBid="gameBid"></bidGameForm>
        <currentBidsForm :currentBids="currentBids"></currentBidsForm>
      </div>
    </div>
  </div>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";
  import BidGameForm from "components/modules/modals/bidGameForm";
  import CurrentBidsForm from "components/modules/modals/currentBidsForm";

  export default {
    data() {
      return {
        errorInfo: ""
      }
    },
    props: ['league', 'leagueYear', 'currentBids'],
    components: {
      BidGameForm,
      CurrentBidsForm
    },
    methods: {
      gameBid(bidInfo) {
        this.$emit('gameBid', bidInfo);
      }
    }
  }
</script>
<style scoped>
.player-actions button{
  margin-bottom: 5px;
  width: 210px;
}
</style>
