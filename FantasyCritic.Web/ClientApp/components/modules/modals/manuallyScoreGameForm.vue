<template>
  <div>
    <form class="form-horizontal" v-on:submit.prevent="manuallyScorePublisherGame" hide-footer>
      <b-modal id="manuallyScorePublisherGame" ref="manuallyScorePublisherGameRef" title="Manually Score Publisher Game" hide-footer @hidden="clearData">
        <div class="form-group">
          <label for="claimPublisher" class="control-label">Publisher</label>
          <b-form-select v-model="manuallyScoreGamePublisher">
            <option v-for="publisher in leagueYear.publishers" v-bind:value="publisher">
              {{ publisher.publisherName }}
            </option>
          </b-form-select>
          <div v-if="manuallyScoreGamePublisher">
            <label for="manuallyScoreGame" class="control-label">Game</label>
            <b-form-select v-model="manuallyScoreGame">
              <option v-for="publisherGame in manuallyScoreGamePublisher.games" v-bind:value="publisherGame">
                {{ publisherGame.gameName }}
              </option>
            </b-form-select>
          </div>
          <div v-if="manuallyScoreGame">
            <div v-if="manuallyScoreGame.manualCriticScore" class="form-check">
              <span>
                <label class="form-check-label">
                  Remove Manual Score?
                </label>
                <input class="form-check-input remove-manual-score-checkbox" type="checkbox" v-model="removeManualScore">
              </span>
            </div>
            <div v-if="!removeManualScore">
              <label for="manualScore" class="control-label">Score</label>
              <input v-model="manualScore" name="manualScore" type="text" class="form-control input"/>
            </div>
          </div>
        </div>

        <div v-if="manuallyScoreGame && (manualScore || removeManualScore)">
          <input type="submit" class="btn btn-primary add-game-button" :value="buttonText" />
          <div v-if="errorInfo" class="alert alert-danger manuallyScore-error">
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
        manuallyScoreGamePublisher: null,
        manuallyScoreGame: null,
        manualScore: null,
        removeManualScore: false,
        errorInfo: ""
      }
    },
    props: ['leagueYear'],
    computed: {
      buttonText() {
        if (this.removeManualScore) {
          return "Remove Manual Score";
        }

        return "Set Manual Score";
      }
    },
    methods: {
      manuallyScorePublisherGame() {
        if (this.removeManualScore) {
          this.removeManualPublisherGameScore();
          return;
        }
        var model = {
          publisherGameID: this.manuallyScoreGame.publisherGameID,
          publisherID: this.manuallyScoreGamePublisher.publisherID,
          manualCriticScore: this.manualScore
        };
        var manualScoreInfo = {
          gameName: this.manuallyScoreGame.gameName,
          score: this.manualScore
        };
        axios
          .post('/api/leagueManager/ManuallyScorePublisherGame', model)
          .then(response => {
            this.$refs.manuallyScorePublisherGameRef.hide();
            this.$emit('gameManuallyScored', manualScoreInfo);
            this.manuallyScoreGamePublisher = null;
            this.manuallyScoreGame = null;
          })
          .catch(response => {
            this.errorInfo = response.response.data;
          });
      },
      removeManualPublisherGameScore() {
        var model = {
          publisherGameID: this.manuallyScoreGame.publisherGameID,
          publisherID: this.manuallyScoreGamePublisher.publisherID
        };
        axios
          .post('/api/leagueManager/RemoveManualPublisherGameScore', model)
          .then(response => {
            this.$refs.manuallyScorePublisherGameRef.hide();
            this.$emit('manualScoreRemoved', this.manuallyScoreGame.gameName);
            this.manuallyScoreGamePublisher = null;
            this.manuallyScoreGame = null;
          })
          .catch(response => {
            this.errorInfo = response.response.data;
          });
      },
      clearData() {
        this.manuallyScoreGamePublisher = null;
        this.manuallyScoreGame = null;
        this.manualScore = null;
        this.removeManualScore = false;
      }
    },
    watch: {
      manuallyScoreGame: function (val, oldVal) {
        if (val) {
          this.manualScore = val.criticScore;
        }
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

.remove-manual-score-checkbox {
  margin-left: 15px;
  margin-top: 8px;
}
</style>
