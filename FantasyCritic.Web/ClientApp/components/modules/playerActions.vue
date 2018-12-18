<template>
  <div>
    <div v-if="leagueYear && leagueYear.userPublisher">
      <h4>Player Actions</h4>
      <div class="player-actions" role="group" aria-label="Basic example">
        <b-button variant="info" class="nav-link" v-b-modal="'playerDraftGameForm'" v-if="leagueYear.playStatus.draftIsActive" v-show="!leagueYear.playStatus.draftingCounterPicks" :disabled="!userIsNextInDraft">Draft Game</b-button>
        <b-button variant="info" class="nav-link" v-b-modal="'playerDraftCounterPickForm'" v-if="leagueYear.playStatus.draftIsActive" v-show="leagueYear.playStatus.draftingCounterPicks" :disabled="!userIsNextInDraft">Draft Counter-Pick</b-button>
        <b-button variant="info" class="nav-link" v-b-modal="'leaguePlayersForm'">See Players</b-button>
        <b-button variant="info" class="nav-link" v-b-modal="'bidGameForm'" v-if="leagueYear.playStatus.draftFinished">Bid on a Game</b-button>
        <b-button variant="info" class="nav-link" v-b-modal="'currentBidsForm'" v-if="leagueYear.playStatus.draftFinished">Current Bids</b-button>
        <b-button variant="info" class="nav-link" v-b-modal="'leagueActionsModal'" v-if="leagueYear.playStatus.draftFinished">See League History</b-button>
        <b-button variant="warning" class="nav-link" v-b-modal="'changePublisherNameForm'">Change Publisher Name</b-button>
      </div>
      <br />
      <div v-if="leagueYear">
        <leaguePlayersForm :players="leagueYear.players" :league="league" v-on:playerRemoved="playerRemoved" ></leaguePlayersForm>
        <playerDraftGameForm :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" :userPublisher="leagueYear.userPublisher" :isManager="league.isManager" v-on:gameDrafted="gameDrafted"></playerDraftGameForm>
        <playerDraftCounterPickForm :userPublisher="leagueYear.userPublisher" :availableCounterPicks="leagueYear.availableCounterPicks" v-on:counterPickDrafted="counterPickDrafted"></playerDraftCounterPickForm>
        <bidGameForm :leagueYear="leagueYear" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" v-on:gameBid="gameBid"></bidGameForm>
        <currentBidsForm :currentBids="currentBids" v-on:bidCanceled="bidCanceled"></currentBidsForm>
        <leagueHistoryModal :leagueActions="leagueActions"></leagueHistoryModal>
        <changePublisherNameForm :publisher="leagueYear.userPublisher" v-on:publisherNameChanged="publisherNameChanged"></changePublisherNameForm>
      </div>
    </div>
  </div>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";
  import BidGameForm from "components/modules/modals/bidGameForm";
  import CurrentBidsForm from "components/modules/modals/currentBidsForm";
  import LeagueHistoryModal from "components/modules/modals/leagueHistoryModal";
  import ChangePublisherNameForm from "components/modules/modals/changePublisherNameForm";
  import LeaguePlayersForm from "components/modules/modals/leaguePlayersForm";
  import PlayerDraftGameForm from "components/modules/modals/playerDraftGameForm";
  import PlayerDraftCounterPickForm from "components/modules/modals/playerDraftCounterPickForm";

  export default {
    data() {
      return {
        errorInfo: ""
      }
    },
    props: ['league', 'leagueYear', 'currentBids', 'leagueActions', 'userIsNextInDraft'],
    components: {
      BidGameForm,
      CurrentBidsForm,
      LeagueHistoryModal,
      ChangePublisherNameForm,
      LeaguePlayersForm,
      PlayerDraftGameForm,
      PlayerDraftCounterPickForm
    },
    methods: {
      gameBid(bidInfo) {
        let actionInfo = {
          message: 'Bid for ' + bidInfo.gameName + ' for $' + bidInfo.bidAmount + ' was made.',
          fetchLeagueYear: true
        };
        this.$emit('actionTaken', actionInfo);
      },
      bidCanceled(bidInfo) {
        let actionInfo = {
          message: 'Bid for ' + bidInfo.gameName + ' for $' + bidInfo.bidAmount + ' was canceled.',
          fetchLeagueYear: true
        };
        this.$emit('actionTaken', actionInfo);
      },
      publisherNameChanged(changeInfo) {
        let actionInfo = {
          message: 'Publisher name changed from ' + changeInfo.oldName + ' to ' + changeInfo.newName,
          fetchLeagueYear: true
        };
        this.$emit('actionTaken', actionInfo);
      },
      playerRemoved(removeInfo) {
        let actionInfo = {
          message: removeInfo.displayName + ' has been removed from the league.',
          fetchLeague: true,
          fetchLeagueYear: true
        };
        this.$emit('actionTaken', actionInfo);
      },
      gameDrafted(draftInfo) {
        let actionInfo = {
          message: 'You have drafted: ' + draftInfo.gameName,
          fetchLeagueYear: true
        };
        this.$emit('actionTaken', actionInfo);
      },
      counterPickDrafted(gameInfo) {
        let actionInfo = {
          message: 'You have selected ' + gameInfo.gameName + ' as a counter pick.',
          fetchLeagueYear: true
        };
        this.$emit('actionTaken', actionInfo);
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
