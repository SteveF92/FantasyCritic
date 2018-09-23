<template>
  <div>
    <div v-if="league.isManager">
      <h4>Manager Actions</h4>
      <div>
        <div v-if="!showInvitePlayer">
          <b-button variant="info" class="nav-link" v-on:click="showInvitePlayerForm">Invite a Player</b-button>
        </div>
        <div v-if="showInvitePlayer">
          <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="invitePlayer">
            <div class="form-group">
              <label for="inviteEmail" class="control-label">Email Address</label>
              <input v-model="inviteEmail" id="inviteEmail" name="inviteEmail" type="text" class="form-control input" />
            </div>
            <div class="form-group">
              <input type="submit" class="btn btn-primary" value="Send Invite" />
            </div>
          </form>
        </div>
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

  export default {
    data() {
      return {
        showInvitePlayer: false,
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
      showInvitePlayerForm() {
        this.showInvitePlayer = true;
      },
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
        var model = {
          leagueID: this.leagueID,
          inviteEmail: this.inviteEmail
        };
        axios
          .post('/api/league/InvitePlayer', model)
          .then(response => {
            this.showInvitePlayer = false;
            this.invitedEmail = this.inviteEmail;
            this.inviteEmail = "";
            this.fetchLeague();
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
