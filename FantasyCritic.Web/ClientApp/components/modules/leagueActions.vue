<template>
  <div>
    <div class="league-actions">
      <div class="publisher-image">
        <font-awesome-icon icon="user-circle" size="4x" />
      </div>
      <h5>{{leagueYear.userPublisher.publisherName}}</h5>
      <span>User: {{leagueYear.userPublisher.playerName}}</span>
      <hr />
      <h6>Game Bidding</h6>
      <ul class="actions-list">
        <li>
          <a class="fake-link action" v-b-modal="'bidGameForm'" v-if="leagueYear.playStatus.draftFinished">
            Make a bid
          </a>
        </li>
        <li>
          <a class="fake-link action" v-b-modal="'currentBidsForm'" v-if="leagueYear.playStatus.draftFinished">
            See Current Bids
          </a>
        </li>
      </ul>

      <h6>
        <a class="fake-link action" v-b-modal="'leagueActionsModal'" v-if="leagueYear.playStatus.draftFinished">
          League History
        </a>
      </h6>
      <h6>
        <a class="fake-link action" v-b-modal="'changePublisherNameForm'">
          Change Publisher Name
        </a>
      </h6>
      <h6>Manage League</h6>
    </div>
    <div>
      <leagueHistoryModal :leagueActions="leagueActions"></leagueHistoryModal>
      <bidGameForm :leagueYear="leagueYear" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" v-on:gameBid="gameBid"></bidGameForm>
      <currentBidsForm :currentBids="currentBids" v-on:bidCanceled="bidCanceled"></currentBidsForm>
      <changePublisherNameForm ref="changePublisherComponentRef" :publisher="leagueYear.userPublisher" v-on:publisherNameChanged="publisherNameChanged"></changePublisherNameForm>
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
    props: ['league', 'leagueYear', 'leagueActions', 'currentBids'],
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
  .actions-list {
    list-style: none;
    padding-left: 10px;
  }
  .action {
    color: #D6993A !important;
  }
</style>
