<template>
  <div>
    <div v-if="league.isManager">
      <h4>Player Actions</h4>
      <div class="player-actions" role="group" aria-label="Basic example">
        <b-button variant="info" class="nav-link" v-b-modal="'invitePlayer'">Bid on a Game</b-button>
        <b-button variant="info" class="nav-link">Edit Current Bids</b-button>
        <b-button variant="info" class="nav-link">See League History</b-button>
      </div>

      <invitePlayerForm :league="league" v-on:playerInvited="playerInvited"></invitePlayerForm>
      <br />

      <div v-if="leagueYear">
        <managerClaimGameForm :publishers="leagueYear.publishers" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" v-on:gameClaimed="gameClaimed"></managerClaimGameForm>
        <br />
        <managerAssociateGameForm :publishers="leagueYear.publishers" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" v-on:gameAssociated="gameAssociated"></managerAssociateGameForm>
        <br />
        <removeGameForm :leagueYear="leagueYear" v-on:gameRemoved="gameRemoved"></removeGameForm>
        <br />
        <manuallyScoreGameForm :leagueYear="leagueYear" v-on:gameManuallyScored="gameManuallyScored" v-on:manualScoreRemoved="manualScoreRemoved"></manuallyScoreGameForm>
        <br />
      </div>
    </div>
  </div>
</template>
<script>
  import Vue from "vue";
  import ManagerClaimGameForm from "components/modules/modals/managerClaimGameForm";
  import ManagerAssociateGameForm from "components/modules/modals/managerAssociateGameForm";
  import InvitePlayerForm from "components/modules/modals/invitePlayerForm";
  import RemoveGameForm from "components/modules/modals/removeGameForm";
  import ManuallyScoreGameForm from "components/modules/modals/manuallyScoreGameForm";
  import axios from "axios";

  export default {
    data() {
      return {
        errorInfo: ""
      }
    },
    props: ['league', 'leagueYear'],
    components: {
      ManagerClaimGameForm,
      ManagerAssociateGameForm,
      InvitePlayerForm,
      RemoveGameForm,
      ManuallyScoreGameForm
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
      
      playerInvited(inviteEmail) {
        this.$emit('playerInvited', inviteEmail);
      },
      gameClaimed(claimInfo) {
        this.$emit('gameClaimed', claimInfo);
      },
      gameAssociated(gameName) {
        this.$emit('gameAssociated', gameName);
      },
      gameRemoved(removeInfo) {
        this.$emit('gameRemoved', removeInfo);
      },
      gameManuallyScored(manualScoreInfo) {
        this.$emit('gameManuallyScored', manualScoreInfo);
      },
      manualScoreRemoved(gameName) {
        this.$emit('manualScoreRemoved', gameName);
      }
    }
  }
</script>
<style scoped>
.player-actions button{
  margin-bottom: 5px;
  width: 210px;
}
</style>
