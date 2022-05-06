<template>
  <div>
    <form class="form-horizontal" hide-footer>
      <b-modal id="manuallyScorePublisherGame" ref="manuallyScorePublisherGameRef" title="Manually Score Publisher Game" hide-footer @hidden="clearData">
        <div class="alert alert-warning">
          Warning! This feature is intended to deal with exceptional circumstances. See the
          <a href="/faq#scoring" class="text-secondary" target="_blank">FAQ</a>
          page for more info.
        </div>
        <div class="form-group">
          <label for="claimPublisher" class="control-label">Publisher</label>
          <b-form-select v-model="manuallyScoreGamePublisher">
            <option v-for="publisher in leagueYear.publishers" :key="publisher.publisherID" :value="publisher">
              {{ publisher.publisherName }}
            </option>
          </b-form-select>
          <div v-if="manuallyScoreGamePublisher">
            <label for="manuallyScoreGame" class="control-label">Game</label>
            <b-form-select v-model="manuallyScoreGame">
              <option v-for="publisherGame in manuallyScoreGamePublisher.games" :key="publisherGame.publisherGameID" :value="publisherGame">
                {{ publisherGame.gameName }}
              </option>
            </b-form-select>
          </div>
          <div v-if="manuallyScoreGame">
            <div v-if="manuallyScoreGame.manualCriticScore" class="form-check">
              <span>
                <label class="form-check-label">Remove Manual Score?</label>
                <input v-model="removeManualScore" class="form-check-input remove-manual-score-checkbox" type="checkbox" />
              </span>
            </div>
            <div v-if="!removeManualScore">
              <label for="manualScore" class="control-label">Score</label>
              <input v-model="manualScore" name="manualScore" type="text" class="form-control input" />
            </div>
          </div>
        </div>

        <div v-if="manuallyScoreGame && (manualScore || removeManualScore)">
          <b-button variant="primary" class="full-width-button" @click="manuallyScorePublisherGame">{{ buttonText }}</b-button>
          <div v-if="errorInfo" class="alert alert-danger manuallyScore-error">
            <h3 class="alert-heading">Error!</h3>
            <p>{{ errorInfo }}</p>
          </div>
        </div>
      </b-modal>
    </form>
  </div>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      manuallyScoreGamePublisher: null,
      manuallyScoreGame: null,
      manualScore: null,
      removeManualScore: false,
      errorInfo: ''
    };
  },
  computed: {
    buttonText() {
      if (this.removeManualScore) {
        return 'Remove Manual Score';
      }

      return 'Set Manual Score';
    }
  },
  watch: {
    manuallyScoreGame: function (val) {
      if (val) {
        this.manualScore = val.criticScore;
      }
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
      axios
        .post('/api/leagueManager/ManuallyScorePublisherGame', model)
        .then(() => {
          this.$refs.manuallyScorePublisherGameRef.hide();
          this.notifyAction(this.manuallyScoreGame.gameName + ' was given a score of ' + this.manualScore + '.');
          this.manuallyScoreGamePublisher = null;
          this.manuallyScoreGame = null;
        })
        .catch((response) => {
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
        .then(() => {
          this.$refs.manuallyScorePublisherGameRef.hide();
          this.notifyAction(this.manuallyScoreGame.gameName + "'s manual score was removed.");
          this.manuallyScoreGamePublisher = null;
          this.manuallyScoreGame = null;
        })
        .catch((response) => {
          this.errorInfo = response.response.data;
        });
    },
    clearData() {
      this.manuallyScoreGamePublisher = null;
      this.manuallyScoreGame = null;
      this.manualScore = null;
      this.removeManualScore = false;
    }
  }
};
</script>
<style scoped>
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
