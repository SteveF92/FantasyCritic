<template>
  <div>
    <div class="league-actions">
      <div class="publisher-image">
        <font-awesome-icon icon="user-circle" size="4x" />
      </div>
      <h5>{{leagueYear.userPublisher.publisherName}}</h5>
      <span>User: {{leagueYear.userPublisher.playerName}}</span>
      <hr />

      <h5>Player Actions</h5>
      <ul class="actions-list">
        <li class="fake-link action" v-b-modal="'playerDraftGameForm'" v-show="leagueYear.playStatus.draftIsActive && !leagueYear.playStatus.draftingCounterPicks" :disabled="!userIsNextInDraft">
          Draft Game
        </li>
        <li class="fake-link action" v-b-modal="'playerDraftCounterPickForm'" v-show="leagueYear.playStatus.draftIsActive && leagueYear.playStatus.draftingCounterPicks" :disabled="!userIsNextInDraft">
          Draft Counterpick
        </li>
        <li class="fake-link action" v-b-modal="'bidGameForm'" v-show="leagueYear.playStatus.draftFinished">
          Make a Bid
        </li>
        <li class="fake-link action" v-b-modal="'currentBidsForm'" v-show="leagueYear.playStatus.draftFinished">
          See Current Bids
        </li>
        <li class="fake-link action" v-b-modal="'leagueActionsModal'" v-if="leagueYear.playStatus.draftFinished">
          See League History
        </li>
        <li class="fake-link action" v-b-modal="'changePublisherNameForm'">
          Change Publisher Name
        </li>
      </ul>

      <div v-if="league.isManager">
        <div v-if="leagueYear.playStatus.draftIsActive || leagueYear.playStatus.draftIsPaused">
          <h5>Draft Management</h5>
          <ul class="actions-list">
            <li class="fake-link action" v-b-modal="'managerDraftGameForm'" v-show="!leagueYear.playStatus.draftingCounterPicks">
              Select Next Game
            </li>
            <li class="fake-link action" v-b-modal="'managerDraftCounterPickForm'" v-show="leagueYear.playStatus.draftingCounterPicks">
              Select Next Game
            </li>
            <li class="fake-link action" v-b-modal="'setPauseModal'">
              <span v-show="leagueYear.playStatus.draftIsActive">Pause Draft</span>
              <span v-show="leagueYear.playStatus.draftIsPaused">Resume Draft</span>
            </li>
            <li class="fake-link action" v-b-modal="'undoLastDraftActionModal'" v-show="leagueYear.playStatus.draftIsPaused">
              Undo Last Drafted Game
            </li>
          </ul>
        </div>
        <h5>Manage League</h5>
        <ul class="actions-list">
          <li class="fake-link action" v-b-modal="'invitePlayer'" v-show="!leagueYear.playStatus.playStarted">
            Invite a Player
          </li>
          <li class="fake-link action" v-b-modal="'editDraftOrderForm'" v-show="leagueYear.playStatus.readyToSetDraftOrder && !leagueYear.playStatus.playStarted">
            Edit Draft Order
          </li>
          <li class="fake-link action" v-b-modal="'claimGameForm'" v-show="leagueYear.playStatus.draftFinished">
            Add Publisher Game
          </li>
          <li class="fake-link action" v-b-modal="'associateGameForm'" v-show="leagueYear.playStatus.draftFinished">
            Associate Unlinked Game
          </li>
          <li class="fake-link action" v-b-modal="'removePublisherGame'" v-show="leagueYear.playStatus.draftFinished">
            Remove Publisher Game
          </li>
          <li class="fake-link action" v-b-modal="'manuallyScorePublisherGame'" v-show="leagueYear.playStatus.draftFinished">
            Score a Game Manually
          </li>
          <li class="fake-link action" v-b-modal="'changeLeagueNameForm'">
            Change League Name
          </li>
        </ul>
      </div>
    </div>
    <div>
      <playerDraftGameForm :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" :userPublisher="leagueYear.userPublisher" :isManager="league.isManager" v-on:gameDrafted="gameDrafted"></playerDraftGameForm>
      <playerDraftCounterPickForm :userPublisher="leagueYear.userPublisher" :availableCounterPicks="leagueYear.availableCounterPicks" v-on:counterPickDrafted="counterPickDrafted"></playerDraftCounterPickForm>

      <bidGameForm :leagueYear="leagueYear" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" v-on:gameBid="gameBid"></bidGameForm>
      <currentBidsForm :currentBids="currentBids" v-on:bidCanceled="bidCanceled"></currentBidsForm>

      <leagueHistoryModal :leagueActions="leagueActions"></leagueHistoryModal>
      <changePublisherNameForm ref="changePublisherComponentRef" :publisher="leagueYear.userPublisher" v-on:publisherNameChanged="publisherNameChanged"></changePublisherNameForm>

      <invitePlayerForm :league="league" v-on:playerInvited="playerInvited"></invitePlayerForm>
      <editDraftOrderForm :leagueYear="leagueYear" v-on:draftOrderEdited="draftOrderEdited"></editDraftOrderForm>
      <managerDraftGameForm :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" :nextPublisherUp="nextPublisherUp" v-on:gameDrafted="managerGameDrafted"></managerDraftGameForm>
      <managerDraftCounterPickForm :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" :availableCounterPicks="leagueYear.availableCounterPicks"
                                   :nextPublisherUp="nextPublisherUp" v-on:counterPickDrafted="managerCounterPickDrafted"></managerDraftCounterPickForm>
      <undoLastDraftActionModal v-on:undoLastDraftAction="undoLastDraftAction"></undoLastDraftActionModal>
      <setPauseModal v-on:setPause="setPause" :paused="leagueYear.playStatus.draftIsPaused"></setPauseModal>
      <managerClaimGameForm :publishers="leagueYear.publishers" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" v-on:gameClaimed="gameClaimed"></managerClaimGameForm>
      <managerAssociateGameForm :publishers="leagueYear.publishers" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" v-on:gameAssociated="gameAssociated"></managerAssociateGameForm>
      <removeGameForm :leagueYear="leagueYear" v-on:gameRemoved="gameRemoved"></removeGameForm>
      <manuallyScoreGameForm :leagueYear="leagueYear" v-on:gameManuallyScored="gameManuallyScored" v-on:manualScoreRemoved="manualScoreRemoved"></manuallyScoreGameForm>
      <changeLeagueNameForm :league="league" v-on:leagueNameChanged="leagueNameChanged"></changeLeagueNameForm>

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

  import ManagerClaimGameForm from "components/modules/modals/managerClaimGameForm";
  import ManagerDraftGameForm from "components/modules/modals/managerDraftGameForm";
  import ManagerAssociateGameForm from "components/modules/modals/managerAssociateGameForm";
  import InvitePlayerForm from "components/modules/modals/invitePlayerForm";
  import RemoveGameForm from "components/modules/modals/removeGameForm";
  import ManuallyScoreGameForm from "components/modules/modals/manuallyScoreGameForm";
  import ChangeLeagueNameForm from "components/modules/modals/changeLeagueNameForm";
  import EditDraftOrderForm from "components/modules/modals/editDraftOrderForm";
  import SetPauseModal from "components/modules/modals/setPauseModal";
  import UndoLastDraftActionModal from "components/modules/modals/undoLastDraftActionModal";
  import ManagerDraftCounterPickForm from "components/modules/modals/managerDraftCounterPickForm";

  export default {
    data() {
      return {
        errorInfo: ""
      }
    },
    props: ['league', 'leagueYear', 'leagueActions', 'currentBids', 'userIsNextInDraft', 'nextPublisherUp'],
    components: {
      BidGameForm,
      CurrentBidsForm,
      LeagueHistoryModal,
      ChangePublisherNameForm,
      LeaguePlayersForm,
      PlayerDraftGameForm,
      PlayerDraftCounterPickForm,
      ManagerClaimGameForm,
      ManagerDraftGameForm,
      ManagerAssociateGameForm,
      InvitePlayerForm,
      RemoveGameForm,
      ManuallyScoreGameForm,
      ChangeLeagueNameForm,
      EditDraftOrderForm,
      SetPauseModal,
      UndoLastDraftActionModal,
      ManagerDraftCounterPickForm
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
      },
      setPause(pauseInfo) {
        var model = {
          leagueID: this.league.leagueID,
          year: this.leagueYear.year,
          pause: pauseInfo.pause
        };
        axios
          .post('/api/leagueManager/SetDraftPause', model)
          .then(response => {
            let pauseMessage = "Draft has been paused.";
            if (!pauseInfo.pause) {
              pauseMessage = "Draft has been un-paused."
            }
            let actionInfo = {
              message: pauseMessage,
              fetchLeague: true,
              fetchLeagueYear: true
            };
            this.$emit('actionTaken', actionInfo);
          })
          .catch(response => {

          });
      },
      undoLastDraftAction(pauseInfo) {
        var model = {
          leagueID: this.league.leagueID,
          year: this.leagueYear.year
        };
        axios
          .post('/api/leagueManager/UndoLastDraftAction', model)
          .then(response => {
            let actionInfo = {
              message: 'Last action was undone.',
              fetchLeagueYear: true
            };
            this.$emit('actionTaken', actionInfo);
          })
          .catch(response => {

          });
      },
      playerInvited(inviteEmail) {
        let actionInfo = {
          message: 'Invite was sent to ' + inviteEmail,
          fetchLeagueYear: true,
          fetchLeague: true
        };
        this.$emit('actionTaken', actionInfo);
      },
      gameClaimed(claimInfo) {
        let actionInfo = {
          message: claimInfo.gameName + ' added to ' + claimInfo.publisherName,
          fetchLeagueYear: true
        };
        this.$emit('actionTaken', actionInfo);
      },
      managerGameDrafted(draftInfo) {
        let actionInfo = {
          message: draftInfo.gameName + ' drafted by ' + draftInfo.publisherName,
          fetchLeagueYear: true
        };
        this.$emit('actionTaken', actionInfo);
      },
      managerCounterPickDrafted(draftInfo) {
        let actionInfo = {
          message: draftInfo.gameName + ' selected as a counter pick by ' + draftInfo.publisherName,
          fetchLeagueYear: true
        };
        this.$emit('actionTaken', actionInfo);
      },
      gameAssociated(associateInfo) {
        let actionInfo = {
          message: associateInfo.gameName + ' sucessfully associated.',
          fetchLeagueYear: true
        };
        this.$emit('actionTaken', actionInfo);
      },
      gameRemoved(removeInfo) {
        let actionInfo = {
          message: removeInfo.gameName + ' removed from ' + removeInfo.publisherName,
          fetchLeagueYear: true
        };
        this.$emit('actionTaken', actionInfo);
      },
      gameManuallyScored(manualScoreInfo) {
        let actionInfo = {
          message: manualScoreInfo.gameName + ' was given a score of ' + manualScoreInfo.score + '.',
          fetchLeagueYear: true
        };
        this.$emit('actionTaken', actionInfo);
      },
      manualScoreRemoved(gameName) {
        let actionInfo = {
          message: gameName + "'s manual score was removed.",
          fetchLeagueYear: true
        };
        this.$emit('actionTaken', actionInfo);
      },
      leagueNameChanged(changeInfo) {
        let actionInfo = {
          message: 'League name changed from ' + changeInfo.oldName + ' to ' + changeInfo.newName,
          fetchLeague: true,
          fetchLeagueYear: true
        };
        this.$emit('actionTaken', actionInfo);
      },
      draftOrderEdited() {
        let actionInfo = {
          message: 'Draft order has been changed',
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
    margin-top: 5px;
  }
  .actions-list {
    list-style: square;
    padding-left: 20px;
  }
  .action {
    color: #D6993A !important;
  }
</style>
