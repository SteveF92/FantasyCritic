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
        <form class="form-horizontal" v-on:submit.prevent="removePublisherGame">
          <b-modal id="removePublisherGame" ref="removePublisherGameRef" title="Remove Publisher Game" hide-footer>
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

            <div>
              <input type="submit" class="btn btn-primary" value="Remove" />
              <div v-if="errorInfo" class="alert alert-danger remove-error">
                <h4 class="alert-heading">Error!</h4>
                <p>{{errorInfo}}</p>
              </div>
            </div>
          </b-modal>
        </form>
      </div>

    </div>
  </div>
</template>
<script>
  import Vue from "vue";
  import ManagerClaimGameForm from "components/modules/modals/managerClaimGameForm";
  import ManagerAssociateGameForm from "components/modules/modals/managerAssociateGameForm";
  import InvitePlayerForm from "components/modules/modals/invitePlayerForm";
  import axios from "axios";

  export default {
    data() {
      return {
        showAddGame: false,
        claimGameName: "",
        claimPublisher: "",
        removeGamePublisher: null,
        removeGame: null,
        errorInfo: ""
      }
    },
    props: ['league', 'leagueYear'],
    components: {
      ManagerClaimGameForm,
      ManagerAssociateGameForm,
      InvitePlayerForm
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
      removePublisherGame() {
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
            this.$refs.removePublisherGameRef.hide();
            this.$emit('gameRemoved', removeInfo);
            this.removeGamePublisher = null;
            this.removeGame = null;
          })
          .catch(response => {
            this.errorInfo = response.response.data;
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
