<template>
  <div>
    <div v-if="league.isManager && leagueYear">
      <h4>Manager Actions</h4>
      <div class="publisher-actions" role="group" aria-label="Basic example">
        <b-button variant="info" class="nav-link" v-b-modal="'invitePlayer'" v-if="!leagueYear.playStatus.playStarted">Invite a Player</b-button>
        <b-button variant="info" class="nav-link" v-b-modal="'editDraftOrderForm'" v-if="leagueYear.playStatus.readyToSetDraftOrder && !leagueYear.playStatus.playStarted">Edit Draft Order</b-button>
        <b-button variant="info" class="nav-link" v-b-modal="'managerDraftGameForm'" v-if="leagueYear.playStatus.draftIsActive">Draft Game - Manager</b-button>
        <b-button variant="info" class="nav-link" v-b-modal="'claimGameForm'" v-if="leagueYear.playStatus.draftFinished">Add Publisher Game</b-button>
        <b-button variant="info" class="nav-link" v-b-modal="'associateGameForm'" v-if="leagueYear.playStatus.draftFinished">Associate Unlinked Game</b-button>
        <b-button variant="warning" class="nav-link" v-b-modal="'removePublisherGame'" v-if="leagueYear.playStatus.draftFinished">Remove Publisher Game</b-button>
        <b-button variant="warning" class="nav-link" v-b-modal="'manuallyScorePublisherGame'" v-if="leagueYear.playStatus.draftFinished">Set a Score Manually</b-button>
        <b-button variant="warning" class="nav-link" v-b-modal="'changeLeagueNameForm'">Change League Name</b-button>
        <b-button variant="warning" class="nav-link" v-b-modal="'setPause'" v-if="leagueYear.playStatus.draftIsActive || leagueYear.playStatus.draftIsPaused">
          <span v-if="leagueYear.playStatus.draftIsActive">Pause Draft</span>
          <span v-if="leagueYear.playStatus.draftIsPaused">Resume Draft</span>
        </b-button>
      </div>

      <invitePlayerForm :league="league" v-on:playerInvited="playerInvited"></invitePlayerForm>
      <br />

      <div v-if="leagueYear">
        <editDraftOrderForm :leagueYear="leagueYear" v-on:draftOrderEdited="draftOrderEdited"></editDraftOrderForm>
        <managerClaimGameForm :publishers="leagueYear.publishers" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" v-on:gameClaimed="gameClaimed"></managerClaimGameForm>
        <managerDraftGameForm v-if="leagueYear.playStatus.draftIsActive" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" :nextPublisherUp="nextPublisherUp" v-on:gameDrafted="gameDrafted"></managerDraftGameForm>
        <managerAssociateGameForm :publishers="leagueYear.publishers" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" v-on:gameAssociated="gameAssociated"></managerAssociateGameForm>
        <removeGameForm :leagueYear="leagueYear" v-on:gameRemoved="gameRemoved"></removeGameForm>
        <manuallyScoreGameForm :leagueYear="leagueYear" v-on:gameManuallyScored="gameManuallyScored" v-on:manualScoreRemoved="manualScoreRemoved"></manuallyScoreGameForm>
        <changeLeagueNameForm :league="league" v-on:leagueNameChanged="leagueNameChanged"></changeLeagueNameForm>
        <setPauseModal v-if="leagueYear.playStatus.draftIsActive || leagueYear.playStatus.draftIsPaused" v-on:setPause="setPause" :paused="leagueYear.playStatus.draftIsPaused"></setPauseModal>
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
      SetPauseModal
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
      gameAssociated(gameName) {
        let actionInfo = {
          message: gameName + ' sucessfully associated.',
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
