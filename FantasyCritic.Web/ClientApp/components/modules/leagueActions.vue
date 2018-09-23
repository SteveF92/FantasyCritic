<template>
  <div>
    <div v-if="league.isManager">
      <h4>Manager Actions</h4>
      <div>
        <b-button variant="info" class="nav-link" v-b-modal="'invitePlayer'">Invite a Player</b-button>
        <form class="form-horizontal" v-on:submit.prevent="invitePlayer">
          <b-modal id="invitePlayer" ref="invitePlayerRef" title="Invite a Player">
            <div class="form-group">
              <label for="inviteEmail" class="control-label">Email Address</label>
              <input v-model="inviteEmail" id="inviteEmail" name="inviteEmail" type="text" class="form-control input" />
            </div>
            <div slot="modal-footer">
              <input type="submit" class="btn btn-primary" value="Send Invite" />
            </div>
          </b-modal>
        </form>
      </div>
      <br />
      <div>
        <div v-if="!showAddGame">
          <b-button variant="info" class="nav-link" v-on:click="showAddGameForm">Add Publisher Game</b-button>
        </div>
        <div v-if="showAddGame">
          <managerClaimGameForm :publishers="leagueYear.publishers" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" v-on:claim-game-success="showGameClaimed"></managerClaimGameForm>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
  import Vue from "vue";
  import ManagerClaimGameForm from "components/modules/managerClaimGameForm";
  import axios from "axios";

  export default {
    data() {
      return {
        showAddGame: false,
        inviteEmail: "",
        invitedEmail: "",
        claimGameName: "",
        claimPublisher: ""
      }
    },
    props: ['league', 'leagueYear'],
    components: {
      ManagerClaimGameForm
    },
    methods: {
      showAddGameForm() {
        this.showAddGame = true;
      },
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
      invitePlayer() {
        this.$refs.invitePlayerRef.hide();
        var model = {
          leagueID: this.league.leagueID,
          inviteEmail: this.inviteEmail
        };
        axios
          .post('/api/league/InvitePlayer', model)
          .then(response => {
            this.showInvitePlayer = false;
            this.invitedEmail = this.inviteEmail;
            this.inviteEmail = "";
            this.$emit('playerInvited', this.invitedEmail);
          })
          .catch(response => {
            this.errorInfo = "Cannot find a player with that email address."
          });
      },
      showGameClaimed(gameName, publisher) {
        this.showAddGame = false;
        var claimInfo = {
          gameName,
          publisher
        }
        this.$emit('gameClaimed', claimInfo);
      }
    }
  }
</script>
