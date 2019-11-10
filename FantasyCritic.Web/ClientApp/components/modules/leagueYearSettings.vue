<template>
  <div>
    <div class="form-group">
      <label for="intendedNumberOfPlayers" class="control-label">How many players do you think will be in this league?</label>
      <input v-model="intendedNumberOfPlayers" v-validate="'required|min_value:2|max_value:14'" id="intendedNumberOfPlayers" name="intendedNumberOfPlayers" type="text" class="form-control input" />
      <span class="text-danger">{{ errors.first('intendedNumberOfPlayers') }}</span>
      <p>You aren't locked into this number of people. This is just to recommend how many games to have per person.</p>
    </div>

    <div v-if="readyToChooseNumbers">
      <hr />
      <label>Based on your number of players, we recommend the following settings. However, you are free to change this.</label>
      <div class="form-group">
        <label for="standardGames" class="control-label">Total Number of Games</label>
        <p>
          This is the total number of games that each player will have on their roster.
        </p>

        <input v-model="standardGames" v-validate="'required|min_value:1|max_value:30'" id="standardGames" name="standardGames" type="text" class="form-control input" />
        <span class="text-danger">{{ errors.first('standardGames') }}</span>
      </div>

      <div class="form-group">
        <label for="gamesToDraft" class="control-label">Number of Games to Draft</label>
        <p>
          This is the number of games that will be chosen by each player at the draft.
          If this number is lower than the "Total Number of Games", the remainder will be
          <a href="/faq#bidding-system" target="_blank">
            Pickup Games.
          </a>
        </p>
        <input v-model="gamesToDraft" v-validate="'required|min_value:1|max_value:30'" id="gamesToDraft" name="gamesToDraft" type="text" class="form-control input" />
        <span class="text-danger">{{ errors.first('gamesToDraft') }}</span>
      </div>

      <div class="form-group">
        <label for="counterPicks" class="control-label">Number of Counter Picks</label>
        <p>
          Counter picks are essentially bets against a game. For more details,
          <a href="/faq#scoring" target="_blank">
            click here.
          </a>
        </p>
        <input v-model="counterPicks" v-validate="'required|max_value:5'" id="counterPicks" name="counterPicks" type="text" class="form-control input" />
        <span class="text-danger">{{ errors.first('counterPicks') }}</span>
      </div>
    </div>
  </div>
</template>
<script>
  export default {
    props: ['year', 'value'],
    data() {
      return {
        leagueYearSettings: null,
        intendedNumberOfPlayers: "",
        standardGames: "",
        gamesToDraft: "",
        counterPicks: ""
      }
    },
    computed: {
      readyToChooseNumbers() {
        let intendedNumberOfPlayersValid = this.veeFields['intendedNumberOfPlayers'] && this.veeFields['intendedNumberOfPlayers'].valid;
        return intendedNumberOfPlayersValid;
      }
    },
    watch: {
      intendedNumberOfPlayers: function (val) {
        let recommendedNumberOfGames = 72;
        this.standardGames = Math.floor(recommendedNumberOfGames / val);
        if (this.standardGames > 25) {
          this.standardGames = 25;
        }
        if (this.standardGames < 10) {
          this.standardGames = 10;
        }
        this.gamesToDraft = Math.floor(this.standardGames / 2);
        this.counterPicks = Math.floor(this.gamesToDraft / 6);
        if (this.counterPicks === 0) {
          this.counterPicks = 1;
        } 
      }
    },
  }
</script>
<style scoped>
  .eligibility-explanation {
    margin-bottom: 50px;
    max-width: 1300px;
  }

  .eligibility-section {
    margin-bottom: 10px;
  }

  .eligibility-description {
    margin-top: 25px;
  }

  .checkbox-label {
    padding-left: 25px;
  }

  .disclaimer {
    margin-top: 10px;
  }

  label {
    font-size: 18px;
  }
</style>
<style>
  .vue-slider-piecewise-label {
    color: white !important;
  }
</style>
