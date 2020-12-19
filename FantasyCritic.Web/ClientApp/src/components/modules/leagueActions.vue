<template>
  <div>
    <div class="league-actions">
      <div v-if="leagueYear.userPublisher">
        <div class="publisher-image">
          <font-awesome-icon icon="user-circle" size="4x" />
        </div>
        <h4 class="publisher-name">{{leagueYear.userPublisher.publisherName}}</h4>
        <span>User: {{leagueYear.userPublisher.playerName}}</span>
        <hr />
      </div>
      <div>
        <h4>Public Actions</h4>
        <ul class="actions-list">
          <li class="fake-link action" v-if="leagueYear.playStatus.playStarted">
            <router-link :to="{ name: 'leagueHistory', params: { leagueid: league.leagueID, year: leagueYear.year }}">See League History</router-link>
          </li>
          <li class="fake-link action" v-b-modal="'leagueOptionsModal'">
            See League Options
          </li>
          <li class="fake-link action" v-b-modal="'eligibilityOverridesModal'">
            See Eligibility Overrides
          </li>
        </ul>
      </div>
      <div v-if="leagueYear.userPublisher">
        <div v-show="!leagueYear.supportedYear.finished">
          <h4>Player Actions</h4>
          <ul class="actions-list">
            <li class="fake-link action" v-b-modal="'playerDraftGameForm'" v-show="leagueYear.playStatus.draftIsActive && !leagueYear.playStatus.draftingCounterPicks && userIsNextInDraft">
              Draft Game
            </li>
            <li class="fake-link action" v-b-modal="'playerDraftCounterPickForm'" v-show="leagueYear.playStatus.draftIsActive && leagueYear.playStatus.draftingCounterPicks && userIsNextInDraft">
              Draft Counterpick
            </li>
            <li class="fake-link action" v-b-modal="'bidGameForm'" v-show="leagueYear.playStatus.draftFinished">
              Make a Bid
            </li>
            <li class="fake-link action" v-b-modal="'currentBidsForm'" v-show="leagueYear.playStatus.draftFinished">
              My Current Bids
            </li>
            <li class="fake-link action" v-b-modal="'dropGameForm'" v-show="leagueYear.playStatus.draftFinished">
              Drop a Game
            </li>
            <li class="fake-link action" v-b-modal="'currentDropsForm'" v-show="leagueYear.playStatus.draftFinished">
              My Pending Drops
            </li>
            <li class="fake-link action" v-b-modal="'gameQueueForm'">
              Watchlist
            </li>
            <li class="fake-link action" v-b-modal="'editAutoDraftForm'" v-show="!leagueYear.playStatus.draftFinished">
              Set Auto Draft
            </li>
            <li class="fake-link action" v-b-modal="'changePublisherNameForm'">
              Change Publisher Name
            </li>
          </ul>
        </div>

        <div v-if="league.isManager">
          <div v-if="leagueYear.playStatus.draftIsActive || leagueYear.playStatus.draftIsPaused">
            <h4>Draft Management</h4>
            <ul class="actions-list">
              <li class="fake-link action" v-b-modal="'managerDraftGameForm'" v-show="!leagueYear.playStatus.draftingCounterPicks && leagueYear.playStatus.draftIsActive">
                Draft Game for Next Player
              </li>
              <li class="fake-link action" v-b-modal="'managerDraftCounterPickForm'" v-show="leagueYear.playStatus.draftingCounterPicks && leagueYear.playStatus.draftIsActive">
                Draft Game for Next Player
              </li>
              <li class="fake-link action" v-b-modal="'setPauseModal'">
                <span v-show="leagueYear.playStatus.draftIsActive">Pause Draft</span>
                <span v-show="leagueYear.playStatus.draftIsPaused">Resume Draft</span>
              </li>
              <li class="fake-link action" v-b-modal="'resetDraftModal'">
                Reset Draft
              </li>
              <li class="fake-link action" v-b-modal="'undoLastDraftActionModal'" v-show="leagueYear.playStatus.draftIsPaused">
                Undo Last Drafted Game
              </li>
              <li v-show="!leagueYear.playStatus.draftIsPaused">
                Undo Last Drafted Game
                <br />
                <span class="action-note">(Pause Draft First)</span>
              </li>
            </ul>
          </div>
          <h4>Manage League</h4>
          <ul class="actions-list">
            <li class="fake-link action" v-b-modal="'invitePlayer'" v-show="!leagueYear.playStatus.playStarted">
              Invite a Player
            </li>
            <li class="fake-link action" v-b-modal="'manageActivePlayers'" v-show="!leagueYear.playStatus.playStarted">
              Manage Active Players
            </li>
            <li class="fake-link action" v-b-modal="'managerMessageForm'">
              Post new Message to League
            </li>
            <li class="fake-link action" v-b-modal="'editDraftOrderForm'" v-show="leagueYear.playStatus.readyToSetDraftOrder && !leagueYear.playStatus.playStarted">
              Edit Draft Order
            </li>
            <li class="fake-link action" v-b-modal="'managerEditPublishersForm'">
              Edit Publishers
            </li>
            <li class="fake-link action" v-b-modal="'managerSetAutoDraftForm'" v-show="!leagueYear.playStatus.draftFinished">
              Edit Player Auto Draft
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
            <li class="fake-link action" v-b-modal="'manuallySetWillNotRelease'" v-show="leagueYear.playStatus.draftFinished">
              Override "Will not Release"
            </li>
            <li class="fake-link action">
              <router-link :to="{ name: 'editLeague', params: { leagueid: league.leagueID, year: leagueYear.year }}">Edit Game Settings</router-link>
            </li>
            <li class="fake-link action" v-b-modal="'manageEligibilityOverridesModal'">
              Manually Set Game Eligibility
            </li>
            <li class="fake-link action" v-b-modal="'changeLeagueOptionsForm'">
              Change League Options
            </li>
            <li class="fake-link action" v-b-modal="'removePlayerForm'">
              Remove a Player
            </li>
            <li class="fake-link action" v-b-modal="'transferManagerForm'">
              Promote new League Manager
            </li>
            <li class="fake-link action" v-b-modal="'addNewLeagueYear'">
              Start New Year
            </li>
          </ul>
        </div>
      </div>
    </div>
    <div>
      <leagueOptionsModal :league="league" :leagueYear="leagueYear"></leagueOptionsModal>
      <eligibilityOverridesModal :eligibilityOverrides="leagueYear.eligibilityOverrides"></eligibilityOverridesModal>

      <div v-if="leagueYear.userPublisher">
        <playerDraftGameForm :userPublisher="leagueYear.userPublisher" :isManager="league.isManager" :year="leagueYear.year" v-on:gameDrafted="gameDrafted"></playerDraftGameForm>
        <playerDraftCounterPickForm :userPublisher="leagueYear.userPublisher" :availableCounterPicks="leagueYear.availableCounterPicks" v-on:counterPickDrafted="counterPickDrafted"></playerDraftCounterPickForm>

        <bidGameForm :leagueYear="leagueYear" v-on:gameBid="gameBid"></bidGameForm>
        <currentBidsForm :currentBids="currentBids" :publisher="leagueYear.userPublisher" v-on:bidCanceled="bidCanceled" v-on:bidPriorityEdited="bidPriorityEdited"></currentBidsForm>

        <dropGameForm :publisher="leagueYear.userPublisher" v-on:dropRequestMade="dropRequestMade"></dropGameForm>
        <currentDropsForm :currentDrops="currentDrops" :publisher="leagueYear.userPublisher" v-on:dropCancelled="dropCancelled"></currentDropsForm>
        <gameQueueForm :publisher="leagueYear.userPublisher" :year="leagueYear.year"></gameQueueForm>

        <changePublisherNameForm ref="changePublisherComponentRef" :publisher="leagueYear.userPublisher" v-on:publisherNameChanged="publisherNameChanged"></changePublisherNameForm>

        <addNewLeagueYearForm :league="league" :isManager="league.isManager" v-on:newYearAdded="newYearAdded"></addNewLeagueYearForm>
        <invitePlayerForm :league="league" v-on:playerInvited="playerInvited" v-on:linkCopied="linkCopied"></invitePlayerForm>
        <manageActivePlayersForm :league="league" :leagueYear="leagueYear" v-on:activePlayersEdited="activePlayersEdited"></manageActivePlayersForm>
        <editDraftOrderForm :leagueYear="leagueYear" v-on:draftOrderEdited="draftOrderEdited"></editDraftOrderForm>
        <editAutoDraftForm :publisher="leagueYear.userPublisher" v-on:autoDraftSet="autoDraftSet"></editAutoDraftForm>
        <managerDraftGameForm :nextPublisherUp="nextPublisherUp" :year="leagueYear.year" v-on:gameDrafted="managerGameDrafted"></managerDraftGameForm>
        <managerDraftCounterPickForm :availableCounterPicks="leagueYear.availableCounterPicks"
                                     :nextPublisherUp="nextPublisherUp" v-on:counterPickDrafted="managerCounterPickDrafted"></managerDraftCounterPickForm>
        <undoLastDraftActionModal v-on:undoLastDraftAction="undoLastDraftAction"></undoLastDraftActionModal>
        <setPauseModal v-on:setPause="setPause" :paused="leagueYear.playStatus.draftIsPaused"></setPauseModal>
        <resetDraftModal v-on:resetDraft="resetDraft"></resetDraftModal>
        <managerClaimGameForm :publishers="leagueYear.publishers" :year="leagueYear.year" v-on:gameClaimed="gameClaimed"></managerClaimGameForm>
        <managerAssociateGameForm :publishers="leagueYear.publishers" :year="leagueYear.year" v-on:gameAssociated="gameAssociated"></managerAssociateGameForm>
        <managerEditPublishersForm v-on:publishersEdited="publishersEdited" :leagueYear="leagueYear"></managerEditPublishersForm>
        <managerSetAutoDraftForm v-on:publishersAutoDraftSet="publishersAutoDraftSet" :leagueYear="leagueYear"></managerSetAutoDraftForm>
        <removeGameForm :leagueYear="leagueYear" v-on:gameRemoved="gameRemoved"></removeGameForm>
        <manuallyScoreGameForm :leagueYear="leagueYear" v-on:gameManuallyScored="gameManuallyScored" v-on:manualScoreRemoved="manualScoreRemoved"></manuallyScoreGameForm>
        <manuallySetWillNotReleaseForm :leagueYear="leagueYear" v-on:gameWillNotReleaseSet="gameWillNotReleaseSet"></manuallySetWillNotReleaseForm>
        <changeLeagueOptionsForm :league="league" v-on:leagueOptionsChanged="leagueOptionsChanged"></changeLeagueOptionsForm>
        <manageEligibilityOverridesModal :leagueYear="leagueYear" v-on:gameEligibilitySet="gameEligibilitySet" v-on:gameEligiblityReset="gameEligiblityReset"></manageEligibilityOverridesModal>
        <removePlayerModal v-on:playerRemoved="playerRemoved" v-on:publisherRemoved="publisherRemoved" :league="league" :leagueYear="leagueYear"></removePlayerModal>
        <transferManagerModal v-on:managerTransferred="managerTransferred" :league="league"></transferManagerModal>
        <managerMessageModal v-on:managerMessagePosted="managerMessagePosted" :league="league" :leagueYear="leagueYear"></managerMessageModal>
      </div>
    </div>
  </div>
</template>
<script>
import Vue from 'vue';
import axios from 'axios';

import BidGameForm from '@/components/modules/modals/bidGameForm';
import CurrentBidsForm from '@/components/modules/modals/currentBidsForm';
import DropGameForm from '@/components/modules/modals/dropGameForm';
import CurrentDropsForm from '@/components/modules/modals/currentDropsForm';
import GameQueueForm from '@/components/modules/modals/gameQueueForm';

import EligibilityOverridesModal from '@/components/modules/modals/eligibilityOverridesModal';
import ChangePublisherNameForm from '@/components/modules/modals/changePublisherNameForm';
import PlayerDraftGameForm from '@/components/modules/modals/playerDraftGameForm';
import PlayerDraftCounterPickForm from '@/components/modules/modals/playerDraftCounterPickForm';
import EditAutoDraftForm from '@/components/modules/modals/editAutoDraftForm';
import ManagerSetAutoDraftForm from '@/components/modules/modals/managerSetAutoDraftForm';
import ManagerEditPublishersForm from '@/components/modules/modals/managerEditPublishersForm';

import ManagerClaimGameForm from '@/components/modules/modals/managerClaimGameForm';
import ManagerDraftGameForm from '@/components/modules/modals/managerDraftGameForm';
import ManagerAssociateGameForm from '@/components/modules/modals/managerAssociateGameForm';
import InvitePlayerForm from '@/components/modules/modals/invitePlayerForm';
import ManageActivePlayersForm from '@/components/modules/modals/manageActivePlayersForm';
import RemoveGameForm from '@/components/modules/modals/removeGameForm';
import ManuallyScoreGameForm from '@/components/modules/modals/manuallyScoreGameForm';
import ManuallySetWillNotReleaseForm from '@/components/modules/modals/manuallySetWillNotReleaseForm';
import ChangeLeagueOptionsForm from '@/components/modules/modals/changeLeagueOptionsForm';
import EditDraftOrderForm from '@/components/modules/modals/editDraftOrderForm';
import SetPauseModal from '@/components/modules/modals/setPauseModal';
import ResetDraftModal from '@/components/modules/modals/resetDraftModal';
import UndoLastDraftActionModal from '@/components/modules/modals/undoLastDraftActionModal';
import ManagerDraftCounterPickForm from '@/components/modules/modals/managerDraftCounterPickForm';
import AddNewLeagueYearForm from '@/components/modules/modals/addNewLeagueYearForm';
import LeagueOptionsModal from '@/components/modules/modals/leagueOptionsModal';
import ManageEligibilityOverridesModal from '@/components/modules/modals/manageEligibilityOverridesModal';
import RemovePlayerModal from '@/components/modules/modals/removePlayerModal';
import ManagerMessageModal from '@/components/modules/modals/managerMessageModal';
import TransferManagerModal from '@/components/modules/modals/transferManagerModal';

export default {
  data() {
    return {
      errorInfo: ''
    };
  },
  props: ['league', 'leagueYear', 'leagueActions', 'currentBids', 'currentDrops', 'userIsNextInDraft', 'nextPublisherUp'],
  components: {
    BidGameForm,
    CurrentBidsForm,
    GameQueueForm,
    DropGameForm,
    CurrentDropsForm,
    EligibilityOverridesModal,
    ChangePublisherNameForm,
    PlayerDraftGameForm,
    PlayerDraftCounterPickForm,
    EditAutoDraftForm,
    ManageActivePlayersForm,
    ManagerAssociateGameForm,
    ManagerClaimGameForm,
    ManagerDraftCounterPickForm,
    ManagerDraftGameForm,
    ManagerSetAutoDraftForm,
    ManagerEditPublishersForm,
    InvitePlayerForm,
    RemoveGameForm,
    ManuallyScoreGameForm,
    ManuallySetWillNotReleaseForm,
    ChangeLeagueOptionsForm,
    EditDraftOrderForm,
    SetPauseModal,
    ResetDraftModal,
    UndoLastDraftActionModal,
    AddNewLeagueYearForm,
    LeagueOptionsModal,
    ManageEligibilityOverridesModal,
    RemovePlayerModal,
    ManagerMessageModal,
    TransferManagerModal
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
    dropRequestMade(dropInfo) {
      let actionInfo = {
        message: 'Drop Request for ' + dropInfo.gameName + ' was made.',
        fetchLeagueYear: true
      };
      this.$emit('actionTaken', actionInfo);
    },
    dropCancelled(dropInfo) {
      let actionInfo = {
        message: 'Drop Request for ' + dropInfo.gameName + ' was cancelled.',
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
    newYearAdded(year) {
      this.$router.push({ name: 'editLeague', params: { leagueid: this.league.leagueID, year: year }, query: { freshSettings: true }});
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
          let pauseMessage = 'Draft has been paused.';
          if (!pauseInfo.pause) {
            pauseMessage = 'Draft has been un-paused.';
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
    resetDraft() {
      var model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year
      };
      axios
        .post('/api/leagueManager/ResetDraft', model)
        .then(response => {
          let actionInfo = {
            message: 'Draft has been reset.',
            fetchLeague: true,
            fetchLeagueYear: true
          };
          this.$emit('actionTaken', actionInfo);
        })
        .catch(response => {

        });
    },
    undoLastDraftAction() {
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
    linkCopied() {
      let actionInfo = {
        message: 'Invite Link copied to clipboard.',
        fetchLeagueYear: false,
        fetchLeague: false
      };
      this.$emit('actionTaken', actionInfo);
    },
    activePlayersEdited() {
      let actionInfo = {
        message: 'Active players were changed.',
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
        message: gameName + '\'s manual score was removed.',
        fetchLeagueYear: true
      };
      this.$emit('actionTaken', actionInfo);
    },
    gameWillNotReleaseSet() {
      let actionInfo = {
        message: 'Will not release status updated.',
        fetchLeagueYear: true
      };
      this.$emit('actionTaken', actionInfo);
    },
    leagueOptionsChanged() {
      let actionInfo = {
        message: 'League options have been updated.',
        fetchLeague: true,
        fetchLeagueYear: true
      };
      this.$emit('actionTaken', actionInfo);
    },
    draftOrderEdited() {
      let actionInfo = {
        message: 'Draft order has been changed.',
        fetchLeagueYear: true
      };
      this.$emit('actionTaken', actionInfo);
    },
    bidPriorityEdited() {
      let actionInfo = {
        message: 'Bid priority has been changed.',
        fetchLeagueYear: true,
        fetchCurrentBids: true
      };
      this.$emit('actionTaken', actionInfo);
    },
    gameEligibilitySet(gameInfo) {
      let message = '';
      if (gameInfo.eligible) {
        message = gameInfo.gameName + ' was marked as eligible.';
      } else {
        message = gameInfo.gameName + ' was marked as ineligible.';
      }
      let actionInfo = {
        message: message,
        fetchLeagueYear: true
      };
      this.$emit('actionTaken', actionInfo);
    },
    gameEligiblityReset(gameInfo) {
      let actionInfo = {
        message: gameInfo.gameName + '\'s eligibility was reset to normal.',
        fetchLeagueYear: true
      };
      this.$emit('actionTaken', actionInfo);
    },
    autoDraftSet(autoDraftInfo) {
      let autoDraftStatus = 'off.';
      if (autoDraftInfo.autoDraft) {
        autoDraftStatus = 'on.';
      }
      let actionInfo = {
        message: 'Auto draft set to ' + autoDraftStatus,
        fetchLeagueYear: true
      };
      this.$emit('actionTaken', actionInfo);
    },
    publishersAutoDraftSet() {
      let actionInfo = {
        message: 'Auto draft changed.',
        fetchLeagueYear: true
      };
      this.$emit('actionTaken', actionInfo);
    },
    publishersEdited() {
      let actionInfo = {
        message: 'Publisher has been edited.',
        fetchLeagueYear: true
      };
      this.$emit('actionTaken', actionInfo);
    },
    publisherRemoved(removeInfo) {
      let actionInfo = {
        message: 'Publisher ' + removeInfo.publisherName + ' has been removed from the league.',
        fetchLeague: true,
        fetchLeagueYear: true
      };
      this.$emit('actionTaken', actionInfo);
    },
    playerRemoved(removeInfo) {
      let actionInfo = {
        message: 'Player ' + removeInfo.playerName + ' has been removed from the league.',
        fetchLeague: true,
        fetchLeagueYear: true
      };
      this.$emit('actionTaken', actionInfo);
    },
    managerTransferred() {
      let actionInfo = {
        message: 'You have transferred league manager status.',
        fetchLeague: true,
        fetchLeagueYear: true
      };
      this.$emit('actionTaken', actionInfo);
    },
    managerMessagePosted() {
      let actionInfo = {
        message: 'New manager\'s message posted.',
        fetchLeague: true,
        fetchLeagueYear: true
      };
      this.$emit('actionTaken', actionInfo);
    }
  }
};
</script>
<style scoped>
  .league-actions {
    border: 2px;
    border-color: #D6993A;
    border-style: solid;
    background-color: #414141;
    padding-left: 5px;
  }
  .publisher-name{
    word-break: break-word;
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
  .action-note {
    padding-left: 15px;
  }
</style>
