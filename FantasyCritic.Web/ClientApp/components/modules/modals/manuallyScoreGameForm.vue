<template>
  <div>
    <form class="form-horizontal" v-on:submit.prevent="removePublisherGame" hide-footer>
      <b-modal id="removePublisherGame" ref="removePublisherGameRef" title="Remove Publisher Game" hide-footer @hidden="clearData">
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

        <div v-if="removeGame">
          <input type="submit" class="btn btn-primary add-game-button" value="Remove Game" />
          <div v-if="errorInfo" class="alert alert-danger remove-error">
            <h4 class="alert-heading">Error!</h4>
            <p>{{errorInfo}}</p>
          </div>
        </div>
      </b-modal>
    </form>
  </div>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    data() {
      return {
        removeGamePublisher: null,
        removeGame: null,
        errorInfo: ""
      }
    },
    props: ['leagueYear'],
    methods: {
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
          .post('/api/leagueManager/RemovePublisherGame', model)
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
      clearData() {
        this.removeGamePublisher = null;
        this.removeGame = null;
      }
    }
  }
</script>
<style scoped>

.add-game-button {
  width: 100%;
}

.claim-error {
  margin-top: 10px;
}

.game-search-input {
  margin-bottom: 15px;
}

.remove-error {
  margin-top: 15px;
}
</style>
