<template>
  <div>
    <div v-if="league.isManager && leagueYear">
      <h4>Manager Actions</h4>
      <div class="publisher-actions" role="group">

        <b-button variant="info" class="nav-link" v-b-modal="'invitePlayer'" v-show="!leagueYear.playStatus.playStarted">Invite a Player</b-button>
        <invitePlayerForm :league="league" v-on:playerInvited="playerInvited"></invitePlayerForm>

        <b-button variant="info" class="nav-link" v-b-modal="'editDraftOrderForm'" v-show="leagueYear.playStatus.readyToSetDraftOrder && !leagueYear.playStatus.playStarted">Edit Draft Order</b-button>
        <editDraftOrderForm :leagueYear="leagueYear" v-on:draftOrderEdited="draftOrderEdited"></editDraftOrderForm>

        <b-button id="managerDraftButton" variant="info" class="nav-link" v-b-modal="'managerDraftGameForm'" v-if="leagueYear.playStatus.draftIsActive" v-show="!leagueYear.playStatus.draftingCounterPicks">Select Next Game</b-button>
        <managerDraftGameForm :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" :nextPublisherUp="nextPublisherUp" v-on:gameDrafted="gameDrafted" v-if="leagueYear.playStatus.draftIsActive"></managerDraftGameForm>

        <b-button id="managerDraftCounterPickButton" variant="info" class="nav-link" v-b-modal="'managerDraftCounterPickForm'"
                  v-if="leagueYear.playStatus.draftIsActive" v-show="leagueYear.playStatus.draftingCounterPicks">Select Next Game</b-button>
        <managerDraftCounterPickForm :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" :availableCounterPicks="leagueYear.availableCounterPicks"
                                     :nextPublisherUp="nextPublisherUp" v-on:counterPickDrafted="counterPickDrafted" v-if="leagueYear.playStatus.draftIsActive"></managerDraftCounterPickForm>

        <b-button id="managerUndoButton" variant="warning" class="nav-link" v-b-modal="'undoLastDraftActionModal'" v-show="leagueYear.playStatus.draftIsPaused">Undo Last Drafted Game</b-button>
        <undoLastDraftActionModal v-on:undoLastDraftAction="undoLastDraftAction"></undoLastDraftActionModal>

        <b-button id="managerPauseButton" variant="warning" class="nav-link" v-b-modal="'setPauseModal'" v-show="leagueYear.playStatus.draftIsActive || leagueYear.playStatus.draftIsPaused">
          <span v-show="leagueYear.playStatus.draftIsActive">Pause Draft</span>
          <span v-show="leagueYear.playStatus.draftIsPaused">Resume Draft</span>
        </b-button>
        <setPauseModal v-on:setPause="setPause" :paused="leagueYear.playStatus.draftIsPaused"></setPauseModal>

        <b-button variant="info" class="nav-link" v-b-modal="'claimGameForm'" v-show="leagueYear.playStatus.draftFinished">Add Publisher Game</b-button>
        <managerClaimGameForm :publishers="leagueYear.publishers" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" v-on:gameClaimed="gameClaimed"></managerClaimGameForm>

        <b-button variant="info" class="nav-link" v-b-modal="'associateGameForm'" v-show="leagueYear.playStatus.draftFinished">Associate Unlinked Game</b-button>
        <managerAssociateGameForm :publishers="leagueYear.publishers" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" v-on:gameAssociated="gameAssociated"></managerAssociateGameForm>

        <b-button variant="warning" class="nav-link" v-b-modal="'removePublisherGame'" v-show="leagueYear.playStatus.draftFinished">Remove Publisher Game</b-button>
        <removeGameForm :leagueYear="leagueYear" v-on:gameRemoved="gameRemoved"></removeGameForm>

        <b-button variant="warning" class="nav-link" v-b-modal="'manuallyScorePublisherGame'" v-show="leagueYear.playStatus.draftFinished">Set a Score Manually</b-button>
        <manuallyScoreGameForm :leagueYear="leagueYear" v-on:gameManuallyScored="gameManuallyScored" v-on:manualScoreRemoved="manualScoreRemoved"></manuallyScoreGameForm>

        <b-button variant="warning" class="nav-link" v-b-modal="'changeLeagueNameForm'">Change League Name</b-button>
        <changeLeagueNameForm :league="league" v-on:leagueNameChanged="leagueNameChanged"></changeLeagueNameForm>

      </div>
    </div>
  </div>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";

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
    props: ['league', 'leagueYear', 'nextPublisherUp'],
    components: {
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
      acceptInvite() {
        var model = {
          leagueID: this.leagueID
        };
        axios
          .post('/api/league/AcceptInvite', model)
          .then(response => {
            this.fetchLeague();
          })
          .catch(response => {

          });
      },
      declineInvite() {
        var model = {
          leagueID: this.leagueID
        };
        axios
          .post('/api/league/DeclineInvite', model)
          .then(response => {
            this.$router.push({ name: "home" });
          })
          .catch(response => {

          });
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
      gameDrafted(draftInfo) {
        let actionInfo = {
          message: draftInfo.gameName + ' drafted by ' + draftInfo.publisherName,
          fetchLeagueYear: true
        };
        this.$emit('actionTaken', actionInfo);
      },
      counterPickDrafted(draftInfo) {
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
.publisher-actions button{
  margin-bottom: 5px;
  width: 210px;
}
</style>
