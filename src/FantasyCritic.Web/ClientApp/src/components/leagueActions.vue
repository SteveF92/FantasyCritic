<template>
  <div>
    <div class="league-actions">
      <div v-if="leagueYear.userPublisher">
        <div class="publisher-section">
          <div v-if="leagueYear.userPublisher.publisherIcon && iconIsValid" class="publisher-icon">
            {{ leagueYear.userPublisher.publisherIcon }}
          </div>
          <div class="publisher-name-section">
            <h2 class="publisher-name">{{ leagueYear.userPublisher.publisherName }}</h2>
            <h5>{{ leagueYear.userPublisher.playerName }}</h5>
          </div>
        </div>
        <hr />
      </div>
      <div>
        <h4>Public Actions</h4>
        <ul class="actions-list">
          <li class="fake-link action" v-if="leagueYear.playStatus.playStarted">
            <router-link :to="{ name: 'leagueHistory', params: { leagueid: league.leagueID, year: leagueYear.year } }">See League History</router-link>
          </li>
          <li class="fake-link action" v-b-modal="'leagueOptionsModal'">See League Options</li>
          <li class="fake-link action" v-b-modal="'eligibilityOverridesModal'">See Eligibility Overrides</li>
          <li class="fake-link action" v-b-modal="'tagOverridesModal'">See Tag Overrides</li>
        </ul>
      </div>
      <div v-if="leagueYear.userPublisher">
        <div>
          <h4>Player Actions</h4>
          <ul class="actions-list">
            <li class="action">
              <router-link :to="{ name: 'publisher', params: { publisherid: leagueYear.userPublisher.publisherID } }">
                <template v-if="leagueYear.hasSpecialSlots">My Publisher Details / Move Games</template>
                <template v-else>My Publisher Details</template>
              </router-link>
            </li>
            <template v-if="!leagueYear.supportedYear.finished">
              <li class="fake-link action" v-b-modal="'playerDraftGameForm'" v-show="leagueYear.playStatus.draftIsActive && !leagueYear.playStatus.draftingCounterPicks && userIsNextInDraft">
                Draft Game
              </li>
              <li class="fake-link action" v-b-modal="'playerDraftCounterPickForm'" v-show="leagueYear.playStatus.draftIsActive && leagueYear.playStatus.draftingCounterPicks && userIsNextInDraft">
                Draft Counterpick
              </li>
              <li class="fake-link action" v-b-modal="'bidGameForm'" v-show="leagueYear.playStatus.draftFinished">Make a Bid</li>
              <li class="fake-link action" v-b-modal="'bidCounterPickForm'" v-show="leagueYear.playStatus.draftFinished">Make a Counter Pick Bid</li>
              <li class="fake-link action" v-b-modal="'currentBidsForm'" v-show="leagueYear.playStatus.draftFinished">My Current Bids</li>
              <li class="fake-link action" v-b-modal="'proposeTradeForm'" v-show="leagueYear.playStatus.draftFinished && leagueYear.tradingSystem !== 'NoTrades'">Propose a Trade</li>
              <li class="fake-link action" v-b-modal="'activeTradesModal'" v-show="leagueYear.playStatus.draftFinished && leagueYear.tradingSystem !== 'NoTrades'">Active Trades</li>
              <li class="fake-link action" v-b-modal="'dropGameForm'" v-show="leagueYear.playStatus.draftFinished">Drop a Game</li>
              <li class="fake-link action" v-b-modal="'currentDropsForm'" v-show="leagueYear.playStatus.draftFinished">My Pending Drops</li>
              <li class="fake-link action" v-b-modal="'gameQueueForm'">Watchlist</li>
              <li class="fake-link action" v-b-modal="'editAutoDraftForm'" v-show="!leagueYear.playStatus.draftFinished">Set Auto Draft</li>
            </template>
            <li class="fake-link action" v-b-modal="'changePublisherNameForm'">Change Publisher Name</li>
            <li class="fake-link action" v-b-modal="'changePublisherIconForm'" v-if="isPlusUser">Change Publisher Icon</li>
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
              <li class="fake-link action" v-b-modal="'resetDraftModal'">Reset Draft</li>
              <li class="fake-link action" v-b-modal="'undoLastDraftActionModal'" v-show="leagueYear.playStatus.draftIsPaused">Undo Last Drafted Game</li>
              <li v-show="!leagueYear.playStatus.draftIsPaused">
                Undo Last Drafted Game
                <br />
                <span class="action-note">(Pause Draft First)</span>
              </li>
            </ul>
          </div>
          <h4>Manage League</h4>
          <ul class="actions-list">
            <li class="fake-link action" v-b-modal="'invitePlayer'" v-show="!leagueYear.playStatus.playStarted">Invite a Player</li>
            <li class="fake-link action" v-b-modal="'manageActivePlayers'" v-show="!leagueYear.playStatus.playStarted">Manage Active Players</li>
            <li class="fake-link action" v-b-modal="'managerMessageForm'">Post new Message to League</li>
            <li class="fake-link action" v-b-modal="'createPublisherForUserForm'">Create Publisher For User</li>
            <li class="fake-link action" v-b-modal="'editDraftOrderForm'" v-show="leagueYear.playStatus.readyToSetDraftOrder && !leagueYear.playStatus.playStarted">Edit Draft Order</li>
            <li class="fake-link action" v-b-modal="'managerEditPublishersForm'">Edit Publishers</li>
            <li class="fake-link action" v-b-modal="'managerSetAutoDraftForm'" v-show="!leagueYear.playStatus.draftFinished">Edit Player Auto Draft</li>
            <li class="fake-link action" v-b-modal="'claimGameForm'" v-show="leagueYear.playStatus.draftFinished">Add Publisher Game</li>
            <li class="fake-link action" v-b-modal="'associateGameForm'" v-show="leagueYear.playStatus.draftFinished">Associate Unlinked Game</li>
            <li class="fake-link action" v-b-modal="'removePublisherGame'" v-show="leagueYear.playStatus.draftFinished">Remove Publisher Game</li>
            <li class="fake-link action" v-b-modal="'manuallyScorePublisherGame'" v-show="leagueYear.playStatus.draftFinished">Score a Game Manually</li>
            <li class="fake-link action" v-b-modal="'manuallySetWillNotRelease'" v-show="leagueYear.playStatus.draftFinished">Override "Will not Release"</li>
            <li class="fake-link action">
              <router-link :to="{ name: 'editLeague', params: { leagueid: league.leagueID, year: leagueYear.year } }">Change Year-Specific Options</router-link>
            </li>
            <li class="fake-link action" v-b-modal="'manageEligibilityOverridesModal'">Override Game Eligibility</li>
            <li class="fake-link action" v-b-modal="'manageTagOverridesModal'">Override Game Tags</li>
            <li class="fake-link action" v-b-modal="'changeLeagueOptionsForm'">Change General League Options</li>
            <li class="fake-link action" v-b-modal="'removePlayerForm'">Remove a Player</li>
            <li class="fake-link action" v-b-modal="'transferManagerForm'">Promote new League Manager</li>
            <li class="fake-link action" v-b-modal="'addNewLeagueYear'">Start New Year</li>
          </ul>
        </div>
      </div>
    </div>
    <div>
      <leagueOptionsModal></leagueOptionsModal>
      <eligibilityOverridesModal></eligibilityOverridesModal>
      <tagOverridesModal></tagOverridesModal>

      <div v-if="userPublisher">
        <playerDraftGameForm></playerDraftGameForm>
        <playerDraftCounterPickForm></playerDraftCounterPickForm>

        <bidGameForm></bidGameForm>
        <bidCounterPickForm></bidCounterPickForm>
        <currentBidsForm></currentBidsForm>

        <proposeTradeForm></proposeTradeForm>
        <activeTradesModal></activeTradesModal>

        <dropGameForm></dropGameForm>
        <currentDropsForm></currentDropsForm>
        <gameQueueForm></gameQueueForm>

        <changePublisherNameForm></changePublisherNameForm>
        <changePublisherIconForm></changePublisherIconForm>

        <addNewLeagueYearForm></addNewLeagueYearForm>
        <invitePlayerForm></invitePlayerForm>
        <manageActivePlayersForm></manageActivePlayersForm>
        <createPublisherForUserForm></createPublisherForUserForm>
        <editDraftOrderForm></editDraftOrderForm>
        <editAutoDraftForm></editAutoDraftForm>
        <managerDraftGameForm></managerDraftGameForm>
        <managerDraftCounterPickForm></managerDraftCounterPickForm>
        <undoLastDraftActionModal></undoLastDraftActionModal>
        <setPauseModal></setPauseModal>
        <resetDraftModal></resetDraftModal>
        <managerClaimGameForm></managerClaimGameForm>
        <managerAssociateGameForm></managerAssociateGameForm>
        <managerEditPublishersForm></managerEditPublishersForm>
        <managerSetAutoDraftForm></managerSetAutoDraftForm>
        <removeGameForm></removeGameForm>
        <manuallyScoreGameForm></manuallyScoreGameForm>
        <manuallySetWillNotReleaseForm></manuallySetWillNotReleaseForm>
        <changeLeagueOptionsForm></changeLeagueOptionsForm>
        <manageEligibilityOverridesModal></manageEligibilityOverridesModal>
        <manageTagOverridesModal></manageTagOverridesModal>
        <removePlayerModal></removePlayerModal>
        <transferManagerModal></transferManagerModal>
        <managerMessageModal></managerMessageModal>
      </div>
    </div>
  </div>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin';

import BidGameForm from '@/components/modals/bidGameForm';
import BidCounterPickForm from '@/components/modals/bidCounterPickForm';
import CurrentBidsForm from '@/components/modals/currentBidsForm';
import DropGameForm from '@/components/modals/dropGameForm';
import CurrentDropsForm from '@/components/modals/currentDropsForm';
import GameQueueForm from '@/components/modals/gameQueueForm';
import ProposeTradeForm from '@/components/modals/proposeTradeForm';
import ActiveTradesModal from '@/components/modals/activeTradesModal';

import EligibilityOverridesModal from '@/components/modals/eligibilityOverridesModal';
import TagOverridesModal from '@/components/modals/tagOverridesModal';
import ChangePublisherNameForm from '@/components/modals/changePublisherNameForm';
import ChangePublisherIconForm from '@/components/modals/changePublisherIconForm';
import PlayerDraftGameForm from '@/components/modals/playerDraftGameForm';
import PlayerDraftCounterPickForm from '@/components/modals/playerDraftCounterPickForm';
import EditAutoDraftForm from '@/components/modals/editAutoDraftForm';
import ManagerSetAutoDraftForm from '@/components/modals/managerSetAutoDraftForm';
import ManagerEditPublishersForm from '@/components/modals/managerEditPublishersForm';

import ManagerClaimGameForm from '@/components/modals/managerClaimGameForm';
import ManagerDraftGameForm from '@/components/modals/managerDraftGameForm';
import ManagerAssociateGameForm from '@/components/modals/managerAssociateGameForm';
import InvitePlayerForm from '@/components/modals/invitePlayerForm';
import CreatePublisherForUserForm from '@/components/modals/createPublisherForUserForm';
import ManageActivePlayersForm from '@/components/modals/manageActivePlayersForm';
import RemoveGameForm from '@/components/modals/removeGameForm';
import ManuallyScoreGameForm from '@/components/modals/manuallyScoreGameForm';
import ManuallySetWillNotReleaseForm from '@/components/modals/manuallySetWillNotReleaseForm';
import ChangeLeagueOptionsForm from '@/components/modals/changeLeagueOptionsForm';
import EditDraftOrderForm from '@/components/modals/editDraftOrderForm';
import SetPauseModal from '@/components/modals/setPauseModal';
import ResetDraftModal from '@/components/modals/resetDraftModal';
import UndoLastDraftActionModal from '@/components/modals/undoLastDraftActionModal';
import ManagerDraftCounterPickForm from '@/components/modals/managerDraftCounterPickForm';
import AddNewLeagueYearForm from '@/components/modals/addNewLeagueYearForm';
import LeagueOptionsModal from '@/components/modals/leagueOptionsModal';
import ManageEligibilityOverridesModal from '@/components/modals/manageEligibilityOverridesModal';
import ManageTagOverridesModal from '@/components/modals/manageTagOverridesModal';
import RemovePlayerModal from '@/components/modals/removePlayerModal';
import ManagerMessageModal from '@/components/modals/managerMessageModal';
import TransferManagerModal from '@/components/modals/transferManagerModal';

import GlobalFunctions from '@/globalFunctions';

export default {
  components: {
    BidGameForm,
    BidCounterPickForm,
    CurrentBidsForm,
    GameQueueForm,
    DropGameForm,
    CurrentDropsForm,
    EligibilityOverridesModal,
    TagOverridesModal,
    ChangePublisherNameForm,
    ChangePublisherIconForm,
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
    ManageTagOverridesModal,
    RemovePlayerModal,
    ManagerMessageModal,
    TransferManagerModal,
    CreatePublisherForUserForm,
    ProposeTradeForm,
    ActiveTradesModal
  },
  mixins: [LeagueMixin],
  computed: {
    iconIsValid() {
      return GlobalFunctions.publisherIconIsValid(this.leagueYear.userPublisher.publisherIcon);
    }
  },
  methods: {
    gameBid(bidInfo) {
      this.notifyAction('Bid for ' + bidInfo.gameName + ' for $' + bidInfo.bidAmount + ' was made.');
    },
    bidCanceled(bidInfo) {
      this.notifyAction('Bid for ' + bidInfo.gameName + ' for $' + bidInfo.bidAmount + ' was canceled.');
    },
    bidEdited(bidInfo) {
      this.notifyAction('Bid for ' + bidInfo.gameName + ' for $' + bidInfo.bidAmount + ' was made.');
    },
    dropRequestMade(dropInfo) {
      this.notifyAction('Drop Request for ' + dropInfo.gameName + ' was made.');
    },
    dropCancelled(dropInfo) {
      this.notifyAction('Drop Request for ' + dropInfo.gameName + ' was cancelled.');
    },
    tradeProposed() {
      this.notifyAction('Trade proposal has been made.');
    },
    publisherIconChanged() {
      this.notifyAction();
    },
    gameDrafted(draftInfo) {
      this.notifyAction('You have drafted: ' + draftInfo.gameName);
    },
    counterPickDrafted(gameInfo) {
      this.notifyAction('You have selected ' + gameInfo.gameName + ' as a counter pick.');
    },
    setPause(pauseInfo) {
      var model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year,
        pause: pauseInfo.pause
      };
      axios
        .post('/api/leagueManager/SetDraftPause', model)
        .then(() => {
          let pauseMessage = 'Draft has been paused.';
          if (!pauseInfo.pause) {
            pauseMessage = 'Draft has been un-paused.';
          }
          this.notifyAction(pauseMessage);
        })
        .catch(() => {});
    },
    resetDraft() {
      var model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year
      };
      axios
        .post('/api/leagueManager/ResetDraft', model)
        .then(() => {
          this.notifyAction('Draft has been reset.');
        })
        .catch(() => {});
    },
    undoLastDraftAction() {
      var model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year
      };
      axios
        .post('/api/leagueManager/UndoLastDraftAction', model)
        .then(() => {
          this.notifyAction('Last action was undone.');
        })
        .catch(() => {});
    },
    playerInvited(inviteEmail) {
      this.notifyAction('Invite was sent to ' + inviteEmail);
    },
    linkCopied() {
      this.makeToast('Invite Link copied to clipboard.');
    },
    activePlayersEdited() {
      this.notifyAction('Active players were changed.', true);
    },
    gameClaimed(claimInfo) {
      this.notifyAction(claimInfo.gameName + ' added to ' + claimInfo.publisherName);
    },
    managerGameDrafted(draftInfo) {
      this.notifyAction(draftInfo.gameName + ' drafted by ' + draftInfo.publisherName);
    },
    managerCounterPickDrafted(draftInfo) {
      this.notifyAction(draftInfo.gameName + ' selected as a counter pick by ' + draftInfo.publisherName);
    },
    gameAssociated(associateInfo) {
      this.notifyAction(associateInfo.gameName + ' sucessfully associated.');
    },
    gameRemoved(removeInfo) {
      this.notifyAction(removeInfo.gameName + ' removed from ' + removeInfo.publisherName);
    },
    gameManuallyScored(manualScoreInfo) {
      this.notifyAction(manualScoreInfo.gameName + ' was given a score of ' + manualScoreInfo.score + '.');
    },
    manualScoreRemoved(gameName) {
      this.notifyAction(gameName + "'s manual score was removed.");
    },
    gameWillNotReleaseSet() {
      this.notifyAction('Will not release status updated.');
    },
    leagueOptionsChanged() {
      this.notifyAction('League options have been updated.');
    },
    draftOrderEdited() {
      this.notifyAction('Draft order has been changed.');
    },
    bidPriorityEdited() {
      this.notifyAction('Bid priority has been changed.');
    },
    gameEligibilitySet(gameInfo) {
      let message = '';
      if (gameInfo.eligible) {
        message = gameInfo.gameName + ' was marked as eligible.';
      } else {
        message = gameInfo.gameName + ' was marked as ineligible.';
      }

      this.notifyAction(message);
    },
    gameEligiblityReset(gameInfo) {
      this.notifyAction(gameInfo.gameName + "'s eligibility was reset to normal.");
    },
    gameTagsSet(gameInfo) {
      this.notifyAction(gameInfo.gameName + " had it's tags overriden.");
    },
    gameTagsReset(gameInfo) {
      this.notifyAction(gameInfo.gameName + " had it's tags reset.");
    },
    autoDraftSet(autoDraftInfo) {
      let autoDraftStatus = 'off.';
      if (autoDraftInfo.autoDraft) {
        autoDraftStatus = 'on.';
      }
      this.notifyAction('Auto draft set to ' + autoDraftStatus);
    },
    publishersAutoDraftSet() {
      this.notifyAction('Auto draft changed.');
    },
    publishersEdited() {
      this.notifyAction('Publisher has been edited.');
    },
    publisherRemoved(removeInfo) {
      this.notifyAction('Publisher ' + removeInfo.publisherName + ' has been removed from the league.');
    },
    playerRemoved(removeInfo) {
      this.notifyAction('Player ' + removeInfo.playerName + ' has been removed from the league.');
    },
    managerTransferred() {
      this.notifyAction('You have transferred league manager status.');
    },
    managerMessagePosted() {
      this.notifyAction("New manager's message posted.");
    }
  }
};
</script>
<style scoped>
.league-actions {
  border: 2px;
  border-color: #d6993a;
  border-style: solid;
  background-color: #414141;
  padding-left: 5px;
}

.publisher-name {
  word-break: break-word;
}

.publisher-section {
  text-align: center;
  margin-top: 5px;
  padding: 5px;
}

.publisher-icon {
  font-size: 75px;
  padding: 5px;
}

.publisher-name-section {
  margin-top: 10px;
  margin-left: 10px;
}

.actions-list {
  list-style: square;
  padding-left: 20px;
}

.action {
  color: #d6993a !important;
}

.action-note {
  padding-left: 15px;
}
</style>
