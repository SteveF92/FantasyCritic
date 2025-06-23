<template>
  <div>
    <div class="league-actions bg-secondary">
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
          <li class="fake-link action">
            <router-link :to="{ name: 'leagueHistory', params: { leagueid: league.leagueID, year: leagueYear.year } }">See League History</router-link>
          </li>
          <li v-b-modal="`leagueOptionsModal_${league.leagueID}-${leagueYear.year}`" class="fake-link action">See League Options</li>
          <li v-b-modal="'eligibilityOverridesModal'" class="fake-link action">See Eligibility Overrides</li>
          <li v-b-modal="'tagOverridesModal'" class="fake-link action">See Tag Overrides</li>
        </ul>
      </div>
      <div v-if="userPublisher">
        <h4>Player Actions</h4>
        <ul class="actions-list">
          <li class="action">
            <router-link :to="{ name: 'publisher', params: { publisherid: userPublisher.publisherID } }">
              <template v-if="leagueYear.settings.hasSpecialSlots && !leagueYear.supportedYear.finished">My Publisher Details / Move Games</template>
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
            <template v-if="!oneShotMode">
              <li v-if="draftFinished" v-b-modal="'bidGameForm'" class="fake-link action">Make a Bid</li>
              <li v-if="draftFinished" v-b-modal="'bidCounterPickForm'" class="fake-link action">Make a Counter Pick Bid</li>
              <li v-if="draftFinished" v-b-modal="'currentBidsForm'" class="fake-link action">My Current Bids</li>
              <li v-if="draftFinished && leagueYear.settings.tradingSystem !== 'NoTrades'" v-b-modal="'proposeTradeForm'" class="fake-link action">Propose a Trade</li>
              <li v-if="draftFinished && leagueYear.settings.tradingSystem !== 'NoTrades'" v-b-modal="'activeTradesModal'" class="fake-link action">Active Trades</li>
              <li v-if="draftFinished" v-b-modal="'dropGameForm'" class="fake-link action">Drop a Game</li>
              <li v-if="draftFinished && userPublisher.superDropsAvailable > 0" v-b-modal="'superDropGameForm'" class="fake-link action">Use a Super Drop</li>
              <li v-if="draftFinished" v-b-modal="'currentDropsForm'" class="fake-link action">My Pending Drops</li>
            </template>

            <li v-b-modal="'gameQueueForm'" class="fake-link action">Watchlist</li>
            <li v-if="!draftFinished" v-b-modal="'editAutoDraftForm'" class="fake-link action">Set Auto Draft</li>
          </template>
          <li v-b-modal="'changePublisherNameForm'" class="fake-link action">Change Publisher Name</li>
          <li v-if="isPlusUser" v-b-modal="'changePublisherIconForm'" class="fake-link action">Change Publisher Icon</li>
          <li v-if="isPlusUser" v-b-modal="'changePublisherSloganForm'" class="fake-link action">Change Publisher Slogan</li>
        </ul>
      </div>

      <div v-if="league.isManager">
        <div v-if="leagueYear.playStatus.draftIsActive || leagueYear.playStatus.draftIsPaused">
          <h4>Draft Management</h4>
          <ul class="actions-list">
            <li v-show="!leagueYear.playStatus.draftingCounterPicks && leagueYear.playStatus.draftIsActive" v-b-modal="'managerDraftGameForm'" class="fake-link action">Draft Game for Next Player</li>
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
          <li v-b-modal="'managerMessageForm'" class="fake-link action">Post new Message to League</li>
          <li v-b-modal="'invitePlayer'" class="fake-link action">Invite a Player</li>
          <li v-if="!playStarted" v-b-modal="'createPublisherForUserForm'" class="fake-link action">Create Publisher For User</li>
          <li v-if="!playStarted" v-b-modal="'removePublisherForm'" class="fake-link action">Delete A User's Publisher</li>
          <li v-if="!playStarted" v-b-modal="'manageActivePlayers'" class="fake-link action">Manage Active Players</li>
          <li v-if="readyToSetDraftOrder && !playStarted" v-b-modal="'editDraftOrderForm'" class="fake-link action">Edit Draft Order</li>
          <li v-if="!draftFinished" v-b-modal="'managerSetAutoDraftForm'" class="fake-link action">Edit Player Auto Draft</li>
          <li v-if="draftFinished" v-b-modal="'specialAuctionsModal'" class="fake-link action">Special Auctions</li>
          <li v-b-modal="'managerEditPublishersForm'" class="fake-link action">Edit Publishers</li>
          <li v-if="draftFinished" v-b-modal="'claimGameForm'" class="fake-link action">Add Publisher Game</li>
          <li v-if="draftFinished" v-b-modal="'associateGameForm'" class="fake-link action">Associate Unlinked Game</li>
          <li v-if="draftFinished" v-b-modal="'removePublisherGame'" class="fake-link action">Remove Publisher Game</li>
          <li v-if="draftFinished" v-b-modal="'manuallyScorePublisherGame'" class="fake-link action">Score a Game Manually</li>
          <li v-if="draftFinished" v-b-modal="'manuallySetWillNotRelease'" class="fake-link action">Override "Will not Release"</li>
          <li class="fake-link action">
            <router-link :to="{ name: 'editLeague', params: { leagueid: league.leagueID, year: leagueYear.year } }">Change Year-Specific Options</router-link>
          </li>
          <li v-b-modal="'manageEligibilityOverridesModal'" class="fake-link action">Override Game Eligibility</li>
          <li v-b-modal="'manageTagOverridesModal'" class="fake-link action">Override Game Tags</li>
          <li v-b-modal="'changeLeagueOptionsForm'" class="fake-link action">Change General League Options</li>
          <li v-if="!leagueYear.supportedYear.finished" v-b-modal="'removePlayerForm'" class="fake-link action">Remove a Player</li>
          <li v-if="draftFinished && !leagueYear.supportedYear.finished" v-b-modal="'reassignPublisherModal'" class="fake-link action">Reassign a Publisher</li>
          <li v-b-modal="'transferManagerForm'" class="fake-link action">Promote new League Manager</li>
          <li v-b-modal="'addNewLeagueYear'" class="fake-link action">Start New Year</li>
        </ul>
      </div>
    </div>
    <div>
      <leagueOptionsModal :league="league" :league-year-options="leagueYear.settings" :possible-league-options="possibleLeagueOptions" :supported-year="supportedYear"></leagueOptionsModal>
      <eligibilityOverridesModal></eligibilityOverridesModal>
      <tagOverridesModal></tagOverridesModal>

      <template v-if="userPublisher">
        <playerDraftGameForm></playerDraftGameForm>
        <playerDraftCounterPickForm></playerDraftCounterPickForm>

        <bidGameForm></bidGameForm>
        <bidCounterPickForm></bidCounterPickForm>
        <currentBidsForm></currentBidsForm>

        <proposeTradeForm></proposeTradeForm>

        <dropGameForm></dropGameForm>
        <superDropGameForm></superDropGameForm>
        <currentDropsForm></currentDropsForm>
        <gameQueueForm></gameQueueForm>

        <changePublisherNameForm></changePublisherNameForm>
        <changePublisherIconForm></changePublisherIconForm>
        <changePublisherSloganForm></changePublisherSloganForm>
        <editAutoDraftForm></editAutoDraftForm>
      </template>

      <activeTradesModal></activeTradesModal>
      <addNewLeagueYearForm></addNewLeagueYearForm>
      <invitePlayerForm></invitePlayerForm>
      <manageActivePlayersForm></manageActivePlayersForm>
      <createPublisherForUserForm></createPublisherForUserForm>
      <editDraftOrderForm></editDraftOrderForm>
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
      <reassignPublisherModal></reassignPublisherModal>
      <removePublisherModal></removePublisherModal>
      <transferManagerModal></transferManagerModal>
      <managerMessageModal></managerMessageModal>
      <specialAuctionsModal></specialAuctionsModal>
    </div>
  </div>
</template>
<script>
import LeagueMixin from '@/mixins/leagueMixin.js';

import BidGameForm from '@/components/modals/bidGameForm.vue';
import BidCounterPickForm from '@/components/modals/bidCounterPickForm.vue';
import CurrentBidsForm from '@/components/modals/currentBidsForm.vue';
import DropGameForm from '@/components/modals/dropGameForm.vue';
import SuperDropGameForm from '@/components/modals/superDropGameForm.vue';
import CurrentDropsForm from '@/components/modals/currentDropsForm.vue';
import GameQueueForm from '@/components/modals/gameQueueForm.vue';
import ProposeTradeForm from '@/components/modals/proposeTradeForm.vue';
import ActiveTradesModal from '@/components/modals/activeTradesModal.vue';

import EligibilityOverridesModal from '@/components/modals/eligibilityOverridesModal.vue';
import TagOverridesModal from '@/components/modals/tagOverridesModal.vue';
import ChangePublisherNameForm from '@/components/modals/changePublisherNameForm.vue';
import ChangePublisherIconForm from '@/components/modals/changePublisherIconForm.vue';
import ChangePublisherSloganForm from '@/components/modals/changePublisherSloganForm.vue';
import PlayerDraftGameForm from '@/components/modals/playerDraftGameForm.vue';
import PlayerDraftCounterPickForm from '@/components/modals/playerDraftCounterPickForm.vue';
import EditAutoDraftForm from '@/components/modals/editAutoDraftForm.vue';
import ManagerSetAutoDraftForm from '@/components/modals/managerSetAutoDraftForm.vue';
import ManagerEditPublishersForm from '@/components/modals/managerEditPublishersForm.vue';

import ManagerClaimGameForm from '@/components/modals/managerClaimGameForm.vue';
import ManagerDraftGameForm from '@/components/modals/managerDraftGameForm.vue';
import ManagerAssociateGameForm from '@/components/modals/managerAssociateGameForm.vue';
import InvitePlayerForm from '@/components/modals/invitePlayerForm.vue';
import CreatePublisherForUserForm from '@/components/modals/createPublisherForUserForm.vue';
import ManageActivePlayersForm from '@/components/modals/manageActivePlayersForm.vue';
import RemoveGameForm from '@/components/modals/removeGameForm.vue';
import ManuallyScoreGameForm from '@/components/modals/manuallyScoreGameForm.vue';
import ManuallySetWillNotReleaseForm from '@/components/modals/manuallySetWillNotReleaseForm.vue';
import ChangeLeagueOptionsForm from '@/components/modals/changeLeagueOptionsForm.vue';
import EditDraftOrderForm from '@/components/modals/editDraftOrderForm.vue';
import SetPauseModal from '@/components/modals/setPauseModal.vue';
import ResetDraftModal from '@/components/modals/resetDraftModal.vue';
import UndoLastDraftActionModal from '@/components/modals/undoLastDraftActionModal.vue';
import ManagerDraftCounterPickForm from '@/components/modals/managerDraftCounterPickForm.vue';
import AddNewLeagueYearForm from '@/components/modals/addNewLeagueYearForm.vue';
import LeagueOptionsModal from '@/components/modals/leagueOptionsModal.vue';
import ManageEligibilityOverridesModal from '@/components/modals/manageEligibilityOverridesModal.vue';
import ManageTagOverridesModal from '@/components/modals/manageTagOverridesModal.vue';
import RemovePlayerModal from '@/components/modals/removePlayerModal.vue';
import ReassignPublisherModal from '@/components/modals/reassignPublisherModal.vue';
import RemovePublisherModal from '@/components/modals/removePublisherModal.vue';
import ManagerMessageModal from '@/components/modals/managerMessageModal.vue';
import TransferManagerModal from '@/components/modals/transferManagerModal.vue';
import SpecialAuctionsModal from '@/components/modals/specialAuctionsModal.vue';

import { publisherIconIsValid } from '@/globalFunctions';

export default {
  components: {
    BidGameForm,
    BidCounterPickForm,
    CurrentBidsForm,
    GameQueueForm,
    DropGameForm,
    SuperDropGameForm,
    CurrentDropsForm,
    EligibilityOverridesModal,
    TagOverridesModal,
    ChangePublisherNameForm,
    ChangePublisherIconForm,
    ChangePublisherSloganForm,
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
    ReassignPublisherModal,
    RemovePublisherModal,
    ManagerMessageModal,
    TransferManagerModal,
    CreatePublisherForUserForm,
    ProposeTradeForm,
    ActiveTradesModal,
    SpecialAuctionsModal
  },
  mixins: [LeagueMixin],
  computed: {
    iconIsValid() {
      return publisherIconIsValid(this.userPublisher.publisherIcon);
    }
  }
};
</script>
<style scoped>
.league-actions {
  border: 2px;
  border-color: #d6993a;
  border-style: solid;
  padding-left: 5px;
  padding-right: 5px;
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

.action-note {
  padding-left: 15px;
}
</style>
