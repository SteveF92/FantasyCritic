<template>
  <div>
    <div class="league-actions">
      <div class="publisher-image">
        <font-awesome-icon icon="user-circle" size="4x" />
      </div>
      <h5>{{leagueYear.userPublisher.publisherName}}</h5>
      <span>User: {{leagueYear.userPublisher.playerName}}</span>
      <hr />
      <h6>Drafting</h6>
      <h6>Game Bidding</h6>
      <h6>
        <a class="fake-link action-info" v-b-modal="'leagueActionsModal'" v-if="leagueYear.playStatus.draftFinished">
          League History
        </a>
      </h6>
      <h6>Manage League</h6>
    </div>
    <div>
      <leagueHistoryModal :leagueActions="leagueActions"></leagueHistoryModal>
    </div>
  </div>
</template>
<script>
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
    props: ['league', 'leagueYear', 'leagueActions'],
    components: {
      BidGameForm,
      CurrentBidsForm,
      LeagueHistoryModal,
      ChangePublisherNameForm,
      LeaguePlayersForm,
      PlayerDraftGameForm,
      PlayerDraftCounterPickForm
    }
  }
</script>
<style scoped>
  .league-actions {
    border: 2px;
    border-color: #D6993A;
    border-style: solid;
    background-color: #414141;
    padding-left: 5px;
  }
  .publisher-image {
    text-align: center;
  }
  .action-info {
    color: #D6993A !important;
  }
</style>
