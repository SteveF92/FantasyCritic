<template>
  <div>
    <div v-if="league.isManager">
      <h4>Manager Actions</h4>
      <div class="publisher-actions" role="group" aria-label="Basic example">
        <b-button variant="info" class="nav-link" v-b-modal="'invitePlayer'">Invite a Player</b-button>
        <b-button variant="info" class="nav-link" v-b-modal="'claimGameForm'">Add Publisher Game</b-button>
        <b-button variant="info" class="nav-link" v-if="leagueYear && leagueYear.unlinkedGameExists" v-b-modal="'associateGameForm'">Associate Unlinked Game</b-button>
        <b-button variant="warning" class="nav-link" v-b-modal="'removePublisherGame'">Remove Publisher Game</b-button>
      </div>

      <div>
        <b-modal id="invitePlayer" ref="invitePlayerRef" title="Invite a Player" hide-footer>
          <invitePlayerForm :league="league" v-on:playerInvited="playerInvited"></invitePlayerForm>
        </b-modal>
      </div>
      <br />

      <div v-if="leagueYear">
        <b-modal id="claimGameForm" ref="claimGameFormRef" title="Add Publisher Game" hide-footer>
          <managerClaimGameForm :publishers="leagueYear.publishers" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" v-on:gameClaimed="gameClaimed"></managerClaimGameForm>
        </b-modal>
      </div>
      <br />

      <div v-if="leagueYear">
        <b-modal id="associateGameForm" ref="associateGameFormRef" title="Associate Publisher Game" hide-footer>
          <managerAssociateGameForm :publishers="leagueYear.publishers" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" v-on:gameAssociated="gameAssociated"></managerAssociateGameForm>
        </b-modal>
      </div>
      <br />

      <div v-if="leagueYear">
        <removeGameForm :leagueYear="leagueYear" v-on:gameRemoved="gameRemoved"></removeGameForm>
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
      RemoveGameForm
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
        this.$refs.invitePlayerRef.hide();
        this.$emit('playerInvited', inviteEmail);
      },
      gameClaimed(gameName, publisher) {
        this.$refs.claimGameFormRef.hide();
        var claimInfo = {
          gameName,
          publisher
        }
        this.$emit('gameClaimed', claimInfo);
      },
      gameAssociated(gameName) {
        this.$refs.claimGameFormRef.hide();
        this.$emit('gameAssociated', gameName);
      },
      gameRemoved(removeInfo) {
        
        this.$emit('gameRemoved', removeInfo);
      }
    }
  }
</script>
<style scoped>
.publisher-actions button{
  margin-bottom: 5px;
  width: 210px;
}

.remove-error{
  margin-top: 15px;
}
</style>
