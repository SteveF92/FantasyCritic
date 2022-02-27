<template>
  <div>
    <div class="league-actions">
      <div v-if="leagueYear.userPublisher">
        <div class="publisher-section">
          <div v-if="leagueYear.userPublisher.publisherIcon && iconIsValid" class="publisher-icon">
            {{leagueYear.userPublisher.publisherIcon}}
          </div>
          <div class="publisher-name-section">
            <h2 class="publisher-name">{{leagueYear.userPublisher.publisherName}}</h2>
            <h5>{{leagueYear.userPublisher.playerName}}</h5>
          </div>
        </div>
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
          <li class="fake-link action" v-b-modal="'tagOverridesModal'">
            See Tag Overrides
          </li>
        </ul>
      </div>
      <div v-if="leagueYear.userPublisher">
        <div>
          <h4>Player Actions</h4>
          <ul class="actions-list">
            <li class="action">
              <router-link :to="{ name: 'publisher', params: { publisherid: leagueYear.userPublisher.publisherID }}">
                <template v-if="leagueYear.hasSpecialSlots">
                  My Publisher Details / Move Games
                </template>
                <template v-else>
                  My Publisher Details
                </template>
              </router-link>
            </li>
            <template v-if="!leagueYear.supportedYear.finished">
              <li class="fake-link action" v-b-modal="'playerDraftGameForm'" v-show="leagueYear.playStatus.draftIsActive && !leagueYear.playStatus.draftingCounterPicks && userIsNextInDraft">
                Draft Game
              </li>
              <li class="fake-link action" v-b-modal="'playerDraftCounterPickForm'" v-show="leagueYear.playStatus.draftIsActive && leagueYear.playStatus.draftingCounterPicks && userIsNextInDraft">
                Draft Counterpick
              </li>
              <li class="fake-link action" v-b-modal="'bidGameForm'" v-show="leagueYear.playStatus.draftFinished">
                Make a Bid
              </li>
              <li class="fake-link action" v-b-modal="'bidCounterPickForm'" v-show="leagueYear.playStatus.draftFinished">
                Make a Counter Pick Bid
              </li>
              <li class="fake-link action" v-b-modal="'currentBidsForm'" v-show="leagueYear.playStatus.draftFinished">
                My Current Bids
              </li>
              <li class="fake-link action" v-b-modal="'proposeTradeForm'" v-show="leagueYear.playStatus.draftFinished && leagueYear.tradingSystem !== 'NoTrades'">
                Propose a Trade
              </li>
              <li class="fake-link action" v-b-modal="'activeTradesModal'" v-show="leagueYear.playStatus.draftFinished && leagueYear.tradingSystem !== 'NoTrades'">
                Active Trades
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
            </template>
            <li class="fake-link action" v-b-modal="'changePublisherNameForm'">
              Change Publisher Name
            </li>
            <li class="fake-link action" v-b-modal="'changePublisherIconForm'" v-if="isPlusUser">
              Change Publisher Icon
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
            <li class="fake-link action" v-b-modal="'createPublisherForUserForm'">
              Create Publisher For User
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
              <router-link :to="{ name: 'editLeague', params: { leagueid: league.leagueID, year: leagueYear.year }}">Change Year-Specific Options</router-link>
            </li>
            <li class="fake-link action" v-b-modal="'manageEligibilityOverridesModal'">
              Override Game Eligibility
            </li>
            <li class="fake-link action" v-b-modal="'manageTagOverridesModal'">
              Override Game Tags
            </li>
            <li class="fake-link action" v-b-modal="'changeLeagueOptionsForm'">
              Change General League Options
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
      <tagOverridesModal :tagOverrides="leagueYear.tagOverrides"></tagOverridesModal>

      <div v-if="leagueYear.userPublisher">
        <playerDraftGameForm :leagueYear="leagueYear" :userPublisher="leagueYear.userPublisher" :isManager="league.isManager" :year="leagueYear.year" v-on:gameDrafted="gameDrafted"></playerDraftGameForm>
        <playerDraftCounterPickForm :userPublisher="leagueYear.userPublisher" v-on:counterPickDrafted="counterPickDrafted"></playerDraftCounterPickForm>

        <bidGameForm :leagueYear="leagueYear" :publisher="leagueYear.userPublisher" v-on:gameBid="gameBid"></bidGameForm>
        <bidCounterPickForm :leagueYear="leagueYear" :publisher="leagueYear.userPublisher" v-on:gameBid="gameBid"></bidCounterPickForm>
        <currentBidsForm :leagueYear="leagueYear" :currentBids="currentBids" :publisher="leagueYear.userPublisher" v-on:bidEdited="bidEdited" v-on:bidCanceled="bidCanceled" v-on:bidPriorityEdited="bidPriorityEdited"></currentBidsForm>

        <proposeTradeForm :leagueYear="leagueYear" :publisher="leagueYear.userPublisher" v-on:tradeProposed="tradeProposed"></proposeTradeForm>
        <activeTradesModal :leagueYear="leagueYear" :publisher="leagueYear.userPublisher" v-on:tradeActioned="tradeActioned"></activeTradesModal>

        <dropGameForm :publisher="leagueYear.userPublisher" v-on:dropRequestMade="dropRequestMade"></dropGameForm>
        <currentDropsForm :currentDrops="currentDrops" :publisher="leagueYear.userPublisher" v-on:dropCancelled="dropCancelled"></currentDropsForm>
        <gameQueueForm :leagueYear="leagueYear" :publisher="leagueYear.userPublisher" :year="leagueYear.year"></gameQueueForm>

        <changePublisherNameForm :publisher="leagueYear.userPublisher" v-on:publisherNameChanged="publisherNameChanged"></changePublisherNameForm>
        <changePublisherIconForm :publisher="leagueYear.userPublisher" v-on:publisherIconChanged="publisherIconChanged"></changePublisherIconForm>

        <addNewLeagueYearForm :league="league" :isManager="league.isManager" v-on:newYearAdded="newYearAdded"></addNewLeagueYearForm>
        <invitePlayerForm :league="league" v-on:playerInvited="playerInvited" v-on:linkCopied="linkCopied"></invitePlayerForm>
        <manageActivePlayersForm :league="league" :leagueYear="leagueYear" v-on:activePlayersEdited="activePlayersEdited"></manageActivePlayersForm>
        <createPublisherForUserForm :leagueYear="leagueYear" v-on:publisherCreated="publisherCreated"></createPublisherForUserForm>
        <editDraftOrderForm :leagueYear="leagueYear" v-on:draftOrderEdited="draftOrderEdited"></editDraftOrderForm>
        <editAutoDraftForm :publisher="leagueYear.userPublisher" v-on:autoDraftSet="autoDraftSet"></editAutoDraftForm>
        <managerDraftGameForm :leagueYear="leagueYear" :nextPublisherUp="nextPublisherUp" :year="leagueYear.year" v-on:gameDrafted="managerGameDrafted"></managerDraftGameForm>
        <managerDraftCounterPickForm :nextPublisherUp="nextPublisherUp" v-on:counterPickDrafted="managerCounterPickDrafted"></managerDraftCounterPickForm>
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
        <manageTagOverridesModal :leagueYear="leagueYear" v-on:gameEligibilitySet="gameTagsSet" v-on:gameTagsReset="gameTagsReset"></manageTagOverridesModal>
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
import BidCounterPickForm from '@/components/modules/modals/bidCounterPickForm';
import CurrentBidsForm from '@/components/modules/modals/currentBidsForm';
import DropGameForm from '@/components/modules/modals/dropGameForm';
import CurrentDropsForm from '@/components/modules/modals/currentDropsForm';
import GameQueueForm from '@/components/modules/modals/gameQueueForm';
import ProposeTradeForm from '@/components/modules/modals/proposeTradeForm';
import ActiveTradesModal from '@/components/modules/modals/activeTradesModal';

import EligibilityOverridesModal from '@/components/modules/modals/eligibilityOverridesModal';
import TagOverridesModal from '@/components/modules/modals/tagOverridesModal';
import ChangePublisherNameForm from '@/components/modules/modals/changePublisherNameForm';
import ChangePublisherIconForm from '@/components/modules/modals/changePublisherIconForm';
import PlayerDraftGameForm from '@/components/modules/modals/playerDraftGameForm';
import PlayerDraftCounterPickForm from '@/components/modules/modals/playerDraftCounterPickForm';
import EditAutoDraftForm from '@/components/modules/modals/editAutoDraftForm';
import ManagerSetAutoDraftForm from '@/components/modules/modals/managerSetAutoDraftForm';
import ManagerEditPublishersForm from '@/components/modules/modals/managerEditPublishersForm';

import ManagerClaimGameForm from '@/components/modules/modals/managerClaimGameForm';
import ManagerDraftGameForm from '@/components/modules/modals/managerDraftGameForm';
import ManagerAssociateGameForm from '@/components/modules/modals/managerAssociateGameForm';
import InvitePlayerForm from '@/components/modules/modals/invitePlayerForm';
import CreatePublisherForUserForm from '@/components/modules/modals/createPublisherForUserForm';
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
import ManageTagOverridesModal from '@/components/modules/modals/manageTagOverridesModal';
import RemovePlayerModal from '@/components/modules/modals/removePlayerModal';
import ManagerMessageModal from '@/components/modules/modals/managerMessageModal';
import TransferManagerModal from '@/components/modules/modals/transferManagerModal';

import GlobalFunctions from '@/globalFunctions';

export default {
  data() {
    return {
      errorInfo: ''
    };
  },
  props: ['league', 'leagueYear', 'leagueActions', 'currentBids', 'currentDrops', 'userIsNextInDraft', 'nextPublisherUp'],
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
  computed: {
    isPlusUser() {
      return this.$store.getters.isPlusUser;
    },
    iconIsValid() {
      return GlobalFunctions.publisherIconIsValid(this.leagueYear.userPublisher.publisherIcon);
    }
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
    bidEdited(bidInfo) {
      let actionInfo = {
        message: 'Bid for ' + bidInfo.gameName + ' for $' + bidInfo.bidAmount + ' was made.',
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
    tradeProposed(tradeInfo) {
      let actionInfo = {
        message: 'Trade proposal has been made.',
        fetchLeagueYear: true
      };
      this.$emit('actionTaken', actionInfo);
    },
    tradeActioned(tradeInfo) {
      let actionInfo = {
        message: tradeInfo.message,
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
    publisherIconChanged(changeInfo) {
      let actionInfo = {
        message: 'Publisher icon changed.',
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
    gameTagsSet(gameInfo) {
      let message = message = gameInfo.gameName + ' had it\'s tags overriden.';
      let actionInfo = {
        message: message,
        fetchLeagueYear: true
      };
      this.$emit('actionTaken', actionInfo);
    },
    gameTagsReset(gameInfo) {
      let message = message = gameInfo.gameName + ' had it\'s tags reset.';
      let actionInfo = {
        message: message,
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
    publisherCreated(createdInfo) {
      let actionInfo = {
        message: 'Publisher ' + createdInfo.publisherName + ' has been created.',
        fetchLeague: true,
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

  .publisher-section {
    text-align: center;
    margin-top: 5px;
    padding: 5px;
  }

  .publisher-icon {
    font-size: 75px;
    padding: 5px;
  }

  .publisher-name-section{
      margin-top: 10px;
      margin-left: 10px;
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
