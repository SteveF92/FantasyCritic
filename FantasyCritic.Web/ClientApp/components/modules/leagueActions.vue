<template>
  <div>
    <div v-if="league.isManager">
      <h4>Manager Actions</h4>
      <div class="publisher-actions" role="group" aria-label="Basic example">
        <b-button variant="info" class="nav-link" v-b-modal="'invitePlayer'">Invite a Player</b-button>
        <b-button variant="info" class="nav-link" v-b-modal="'claimGameForm'">Add Publisher Game</b-button>
        <b-button variant="warning" class="nav-link" v-b-modal="'removePublisherGame'">Remove Publisher Game</b-button>
      </div>

      <div>
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

      <div v-if="leagueYear">
        <b-modal id="claimGameForm" ref="claimGameFormRef" title="Add Publisher Game" hide-footer>
          <managerClaimGameForm :publishers="leagueYear.publishers" :maximumEligibilityLevel="leagueYear.maximumEligibilityLevel" v-on:claim-game-success="gameClaimed"></managerClaimGameForm>
        </b-modal>
      </div>
      <br />

      <div v-if="leagueYear">
        <form class="form-horizontal" v-on:submit.prevent="removePublisherGame">
          <b-modal id="removePublisherGame" ref="removePublisherGameRef" title="Remove Publisher Game">
            <div class="form-group">
              <label for="claimPublisher" class="control-label">Publisher</label>
              <b-form-select v-model="removeGamePublisher">
                <option v-for="publisher in leagueYear.publishers" v-bind:value="publisher">
                  {{ publisher.publisherName }}
                </option>
              </b-form-select>
              <div v-if="removeGamePublisher">
                <label for="removeGame" class="control-label">Game</label>
                <b-form-select v-model="removeGame">
                  <option v-for="publisherGame in removeGamePublisher.games" v-bind:value="publisherGame">
                    {{ publisherGame.gameName }}
                  </option>
                </b-form-select>
              </div>
              
            </div>
            <div slot="modal-footer">
              <input type="submit" class="btn btn-primary" value="Remove" />
            </div>
          </b-modal>
        </form>
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
        claimGameName: "",
        claimPublisher: "",
        removeGamePublisher: null,
        removeGame: null
      }
    },
    props: ['league', 'leagueYear'],
    components: {
      ManagerClaimGameForm
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
      invitePlayer() {
        this.$refs.invitePlayerRef.hide();
        var model = {
          leagueID: this.league.leagueID,
          inviteEmail: this.inviteEmail
        };
        axios
          .post('/api/league/InvitePlayer', model)
          .then(response => {
            this.$emit('playerInvited', this.inviteEmail);
            this.showInvitePlayer = false;
            this.inviteEmail = "";
          })
          .catch(response => {
            this.errorInfo = "Cannot find a player with that email address."
          });
      },
      removePublisherGame() {
        this.$refs.removePublisherGameRef.hide();
        var model = {
          publisherGameID: this.removeGame.publisherGameID,
          publisherID: this.removeGamePublisher.publisherID
        };
        var removeInfo = {
          gameName: this.removeGame.gameName,
          publisherName: this.removeGamePublisher.publisherName
        };
        axios
          .post('/api/league/RemovePublisherGame', model)
          .then(response => {
            this.$emit('gameRemoved', removeInfo);
            this.removeGamePublisher = null;
            this.removeGame = null;
          })
          .catch(response => {
            this.errorInfo = "Cannot find a player with that email address."
          });
      },
      gameClaimed(gameName, publisher) {
        this.$refs.claimGameFormRef.hide();
        var claimInfo = {
          gameName,
          publisher
        }
        this.$emit('gameClaimed', claimInfo);
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
