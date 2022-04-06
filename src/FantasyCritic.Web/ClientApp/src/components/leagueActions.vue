<template>
  <div>
    <div class="league-actions">
      <div v-if="userPublisher">
        <div class="publisher-section">
          <div v-if="userPublisher.publisherIcon && iconIsValid" class="publisher-icon">
            {{ userPublisher.publisherIcon }}
          </div>
          <div class="publisher-name-section">
            <h2 class="publisher-name">{{ userPublisher.publisherName }}</h2>
            <h5>{{ userPublisher.playerName }}</h5>
          </div>
        </div>
        <hr />
      </div>
      <div>
        <h4>Public Actions</h4>
        <ul class="actions-list">
          <li v-if="leagueYear.playStatus.playStarted" class="fake-link action">
            <router-link :to="{ name: 'leagueHistory', params: { leagueid: league.leagueID, year: leagueYear.year } }">See League History</router-link>
          </li>
          <li v-b-modal="'leagueOptionsModal'" class="fake-link action">See League Options</li>
          <li v-b-modal="'eligibilityOverridesModal'" class="fake-link action">See Eligibility Overrides</li>
          <li v-b-modal="'tagOverridesModal'" class="fake-link action">See Tag Overrides</li>
        </ul>
      </div>
      <div v-if="userPublisher">
        <div>
          <h4>Player Actions</h4>
          <ul class="actions-list">
            <li class="action">
              <router-link :to="{ name: 'publisher', params: { publisherid: userPublisher.publisherID } }">
                <template v-if="leagueYear.hasSpecialSlots">My Publisher Details / Move Games</template>
                <template v-else>My Publisher Details</template>
              </router-link>
            </li>
            <template v-if="!leagueYear.supportedYear.finished">
              <li v-show="leagueYear.playStatus.draftIsActive && !leagueYear.playStatus.draftingCounterPicks && userIsNextInDraft" v-b-modal="'playerDraftGameForm'" class="fake-link action">
                Draft Game
              </li>
              <li v-show="leagueYear.playStatus.draftIsActive && leagueYear.playStatus.draftingCounterPicks && userIsNextInDraft" v-b-modal="'playerDraftCounterPickForm'" class="fake-link action">
                Draft Counterpick
              </li>
              <li v-show="leagueYear.playStatus.draftFinished" v-b-modal="'bidGameForm'" class="fake-link action">Make a Bid</li>
              <li v-show="leagueYear.playStatus.draftFinished" v-b-modal="'bidCounterPickForm'" class="fake-link action">Make a Counter Pick Bid</li>
              <li v-show="leagueYear.playStatus.draftFinished" v-b-modal="'currentBidsForm'" class="fake-link action">My Current Bids</li>
              <li v-show="leagueYear.playStatus.draftFinished && leagueYear.tradingSystem !== 'NoTrades'" v-b-modal="'proposeTradeForm'" class="fake-link action">Propose a Trade</li>
              <li v-show="leagueYear.playStatus.draftFinished && leagueYear.tradingSystem !== 'NoTrades'" v-b-modal="'activeTradesModal'" class="fake-link action">Active Trades</li>
              <li v-show="leagueYear.playStatus.draftFinished" v-b-modal="'dropGameForm'" class="fake-link action">Drop a Game</li>
              <li v-show="leagueYear.playStatus.draftFinished" v-b-modal="'currentDropsForm'" class="fake-link action">My Pending Drops</li>
              <li v-b-modal="'gameQueueForm'" class="fake-link action">Watchlist</li>
              <li v-show="!leagueYear.playStatus.draftFinished" v-b-modal="'editAutoDraftForm'" class="fake-link action">Set Auto Draft</li>
            </template>
            <li v-b-modal="'changePublisherNameForm'" class="fake-link action">Change Publisher Name</li>
            <li v-if="isPlusUser" v-b-modal="'changePublisherIconForm'" class="fake-link action">Change Publisher Icon</li>
          </ul>
        </div>

        <div v-if="league.isManager">
          <div v-if="leagueYear.playStatus.draftIsActive || leagueYear.playStatus.draftIsPaused">
            <h4>Draft Management</h4>
            <ul class="actions-list">
              <li v-show="!leagueYear.playStatus.draftingCounterPicks && leagueYear.playStatus.draftIsActive" v-b-modal="'managerDraftGameForm'" class="fake-link action">
                Draft Game for Next Player
              </li>
              <li v-show="leagueYear.playStatus.draftingCounterPicks && leagueYear.playStatus.draftIsActive" v-b-modal="'managerDraftCounterPickForm'" class="fake-link action">
                Draft Game for Next Player
              </li>
              <li v-b-modal="'setPauseModal'" class="fake-link action">
                <span v-show="leagueYear.playStatus.draftIsActive">Pause Draft</span>
                <span v-show="leagueYear.playStatus.draftIsPaused">Resume Draft</span>
              </li>
              <li v-b-modal="'resetDraftModal'" class="fake-link action">Reset Draft</li>
              <li v-show="leagueYear.playStatus.draftIsPaused" v-b-modal="'undoLastDraftActionModal'" class="fake-link action">Undo Last Drafted Game</li>
              <li v-show="!leagueYear.playStatus.draftIsPaused">
                Undo Last Drafted Game
                <br />
                <span class="action-note">(Pause Draft First)</span>
              </li>
            </ul>
          </div>
          <h4>Manage League</h4>
          <ul class="actions-list">
            <li v-show="!leagueYear.playStatus.playStarted" v-b-modal="'invitePlayer'" class="fake-link action">Invite a Player</li>
            <li v-show="!leagueYear.playStatus.playStarted" v-b-modal="'manageActivePlayers'" class="fake-link action">Manage Active Players</li>
            <li v-b-modal="'managerMessageForm'" class="fake-link action">Post new Message to League</li>
            <li v-b-modal="'createPublisherForUserForm'" class="fake-link action">Create Publisher For User</li>
            <li v-show="leagueYear.playStatus.readyToSetDraftOrder && !leagueYear.playStatus.playStarted" v-b-modal="'editDraftOrderForm'" class="fake-link action">Edit Draft Order</li>
            <li v-b-modal="'managerEditPublishersForm'" class="fake-link action">Edit Publishers</li>
            <li v-show="!leagueYear.playStatus.draftFinished" v-b-modal="'managerSetAutoDraftForm'" class="fake-link action">Edit Player Auto Draft</li>
            <li v-show="leagueYear.playStatus.draftFinished" v-b-modal="'claimGameForm'" class="fake-link action">Add Publisher Game</li>
            <li v-show="leagueYear.playStatus.draftFinished" v-b-modal="'associateGameForm'" class="fake-link action">Associate Unlinked Game</li>
            <li v-show="leagueYear.playStatus.draftFinished" v-b-modal="'removePublisherGame'" class="fake-link action">Remove Publisher Game</li>
            <li v-show="leagueYear.playStatus.draftFinished" v-b-modal="'manuallyScorePublisherGame'" class="fake-link action">Score a Game Manually</li>
            <li v-show="leagueYear.playStatus.draftFinished" v-b-modal="'manuallySetWillNotRelease'" class="fake-link action">Override "Will not Release"</li>
            <li class="fake-link action">
              <router-link :to="{ name: 'editLeague', params: { leagueid: league.leagueID, year: leagueYear.year } }">Change Year-Specific Options</router-link>
            </li>
            <li v-b-modal="'manageEligibilityOverridesModal'" class="fake-link action">Override Game Eligibility</li>
            <li v-b-modal="'manageTagOverridesModal'" class="fake-link action">Override Game Tags</li>
            <li v-b-modal="'changeLeagueOptionsForm'" class="fake-link action">Change General League Options</li>
            <li v-b-modal="'removePlayerForm'" class="fake-link action">Remove a Player</li>
            <li v-b-modal="'transferManagerForm'" class="fake-link action">Promote new League Manager</li>
            <li v-b-modal="'addNewLeagueYear'" class="fake-link action">Start New Year</li>
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
      return GlobalFunctions.publisherIconIsValid(this.userPublisher.publisherIcon);
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
