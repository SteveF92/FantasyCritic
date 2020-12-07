<template>
  <div>
    <div>
      <div v-show="!editMode">
        <h2>Game Settings</h2>
        <p>Settings in this section can be different from year to year for your league.</p>
      </div>

      <div v-if="freshSettings">
        <div class="form-group">
          <label for="intendedNumberOfPlayers" class="control-label">How many players do you think will be in this league?</label>
          <ValidationProvider rules="required|min_value:2|max_value:20" v-slot="{ errors }">
            <input v-model="intendedNumberOfPlayers" id="intendedNumberOfPlayers" name="Intended Number of Players" type="text" class="form-control input" />
            <span class="text-danger">{{ errors[0] }}</span>
          </ValidationProvider>
          <p>You aren't locked into this number of people. This is just to recommend how many games to have per person.</p>
        </div>

        <div class="form-group">
          <label for="gameMode" class="control-label">Game Mode</label>
          <p>
            This slider changes the recommended number of games per player. If you've played Fantasy Critic before, consider using Advanced.
            <br />
            If you're playing with people new to video games in general, consider using Beginner.
          </p>
          <div class="mode-slider">
            <vue-slider v-model="gameMode" :data="gameModeOptions" :marks="gameModeMarks">
            </vue-slider>
          </div>
          <p>These modes only change the recommended settings. You are free to customize any value you want.</p>
        </div>
      </div>
    </div>

    <div v-if="intendedNumberOfPlayersEverValid || editMode">
      <div v-show="freshSettings">
        <hr />
        <label>Based on your number of players and selected game mode, we recommend the following settings. However, you are free to change this.</label>
      </div>

      <div class="form-group">
        <label for="standardGames" class="control-label">Total Number of Games</label>
        <p>
          This is the total number of games that each player will have on their roster.
        </p>

        <ValidationProvider rules="required|min_value:1|max_value:50" v-slot="{ errors }">
          <input v-model="local.standardGames" @input="update('standardGames', $event.target.value)" id="standardGames" name="Total Number of Games" type="text" class="form-control input" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationProvider>
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

        <ValidationProvider rules="required|min_value:1|max_value:50" v-slot="{ errors }">
          <input v-model="local.gamesToDraft" @input="update('gamesToDraft', $event.target.value)" id="gamesToDraft" name="Games to Draft" type="text" class="form-control input" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationProvider>
      </div>

      <div class="form-group">
        <label for="counterPicks" class="control-label">Number of Counter Picks</label>
        <p>
          Counter picks are essentially bets against a game. For more details,
          <a href="/faq#scoring" target="_blank">
            click here.
          </a>
        </p>

        <ValidationProvider rules="required|max_value:5" v-slot="{ errors }">
          <input v-model="local.counterPicks" @input="update('counterPicks', $event.target.value)" id="counterPicks" name="Number of Counterpicks" type="text" class="form-control input" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationProvider>
      </div>

      <hr />
      <h3>Game Dropping Settings</h3>
      <div class="alert alert-info">
        New for 2020, players can now choose to drop a game before it releases. This setting allows you to choose how many such games can be dropped, if any.
        You can customize how many games are droppable after the game is confirmed to be delayed, as well as how many are droppable that are still scheduled to release.
        <br />
        You can also use the "Any Unreleased" setting, which applies to all unreleased games, delayed or not.
        <br />
        For more details, check out the <a href="/faq#dropping-games" target="_blank" class="text-secondary">FAQ.</a>
      </div>

      <table class="table table-small table-bordered">
        <thead>
          <tr>
            <th scope="col"></th>
            <th scope="col">Number</th>
            <th scope="col">Unlimited?</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <th scope="row">Will Release</th>
            <td>
              <ValidationProvider rules="required|max_value:100" v-slot="{ errors }" v-if="!local.unlimitedWillReleaseDroppableGames">
                <input v-model="local.willReleaseDroppableGames" @input="update('willReleaseDroppableGames', $event.target.value)" 
                       id="willReleaseDroppableGames" name="Will Release Droppable Games" type="text" class="form-control input drop-number" />
                <span class="text-danger">{{ errors[0] }}</span>
              </ValidationProvider>
            </td>
            <td>
              <b-form-checkbox class="unlimited-checkbox" v-model="local.unlimitedWillReleaseDroppableGames" @input="update('unlimitedWillReleaseDroppableGames', local.unlimitedWillReleaseDroppableGames)">
              </b-form-checkbox>
            </td>
          </tr>
          <tr>
            <th scope="row">Will Not Release</th>
            <td>
              <ValidationProvider rules="required|max_value:100" v-slot="{ errors }" v-if="!local.unlimitedWillNotReleaseDroppableGames">
                <input v-model="local.willNotReleaseDroppableGames" @input="update('willNotReleaseDroppableGames', $event.target.value)"
                       id="willNotReleaseDroppableGames" name="Will Not Release Droppable Games" type="text" class="form-control input drop-number" />
                <span class="text-danger">{{ errors[0] }}</span>
              </ValidationProvider>
            </td>
            <td>
              <b-form-checkbox class="unlimited-checkbox" v-model="local.unlimitedWillNotReleaseDroppableGames" @input="update('unlimitedWillNotReleaseDroppableGames', local.unlimitedWillNotReleaseDroppableGames)">
              </b-form-checkbox>
            </td>
          </tr>
          <tr>
            <th scope="row">Any Unreleased</th>
            <td>
              <ValidationProvider rules="required|max_value:100" v-slot="{ errors }"  v-if="!local.unlimitedFreeDroppableGames">
                <input v-model="local.freeDroppableGames" @input="update('freeDroppableGames', $event.target.value)"
                       id="freeDroppableGames" name="Unrestricted Droppable Games" type="text" class="form-control input drop-number" />
                <span class="text-danger">{{ errors[0] }}</span>
              </ValidationProvider>
            </td>
            <td>
              <b-form-checkbox class="unlimited-checkbox" v-model="local.unlimitedFreeDroppableGames" @input="update('unlimitedFreeDroppableGames', local.unlimitedFreeDroppableGames)">
              </b-form-checkbox>
            </td>
          </tr>
        </tbody>
      </table>

      <div>
        <b-form-checkbox v-model="local.dropOnlyDraftGames" @input="update('dropOnlyDraftGames', local.dropOnlyDraftGames)">
          <span class="checkbox-label">Only allow drafted to be dropped</span>
          <p>If this is checked, pickup games will not be droppable, no matter what the above settings are. Counter picks are never droppable.</p>
        </b-form-checkbox>
      </div>

      <hr />
      <h3>Eligibility Settings</h3>
      <div class="alert alert-info">
        These options let you choose what games are available in your league. These settings can be overriden on a game by game basis, and I recommend you lean towards being more restrictive,
        and allow specific exemptions if your entire league decides on one. The default options are the recommended settings.
        <br />
        <br />
        All games have a number of tags associated with them. If you place a tag in the "Banned" column, any game with that tag will not be selectable.
      </div>
      <leagueTagSelector v-model="local.tags"></leagueTagSelector>
    </div>
  </div>
</template>
<script>
import vueSlider from 'vue-slider-component';
import Popper from 'vue-popperjs';
import 'vue-slider-component/theme/antd.css';
import { cloneDeep, tap, set } from 'lodash';
import LeagueTagSelector from '@/components/modules/leagueTagSelector';

export default {
  props: ['year', 'possibleLeagueOptions', 'editMode', 'value', 'currentNumberOfPlayers', 'freshSettings'],
  data() {
    return {
      intendedNumberOfPlayers: '',
      intendedNumberOfPlayersEverValid: false,
      gameMode: 'Standard',
      gameModeOptions: [
        'Beginner',
        'Standard',
        'Advanced'
      ],
      gameModeMarks: {
        'Beginner': {
          label: 'Beginner',
          style: {
            width: '8px',
            height: '8px',
            display: 'block',
            transform: 'translate(-2px, -2px)'
          },
          labelStyle: {
            color: 'white',
            fontWeight: 'bolder',
            fontSize: '15px'
          }
        },
        'Standard': {
          label: 'Standard',
          style: {
            width: '8px',
            height: '8px',
            display: 'block',
            transform: 'translate(-2px, -2px)'
          },
          labelStyle: {
            color: 'white',
            fontWeight: 'bolder',
            fontSize: '15px'
          }
        },
        'Advanced': {
          label: 'Advanced',
          style: {
            width: '8px',
            height: '8px',
            display: 'block',
            transform: 'translate(-2px, -2px)'
          },
          labelStyle: {
            color: 'white',
            fontWeight: 'bolder',
            fontSize: '15px'
          }
        }
      },
      markStyle: {
        width: '8px',
        height: '8px',
        display: 'block',
        transform: 'translate(-2px, -2px)'
      },
      labelStyle: {
        color: 'white',
        fontWeight: 'bolder',
        fontSize: '15px'
      }
    };
  },
  components: {
    vueSlider,
    'popper': Popper,
    LeagueTagSelector
  },
  computed: {
    minimumPossibleEligibilityLevel() {
      return 0;
    },
    maximumPossibleEligibilityLevel() {
      if (!this.possibleLeagueOptions.eligibilityLevels) {
        return 0;
      }
      let maxEligibilityLevel = _.maxBy(this.possibleLeagueOptions.eligibilityLevels, 'level');
      return maxEligibilityLevel.level;
    },
    selectedEligibilityLevel() {
      let matchingLevel = _.filter(this.possibleLeagueOptions.eligibilityLevels, { 'level': this.value.maximumEligibilityLevel });
      return matchingLevel[0];
    },
    marks() {
      if (!this.possibleLeagueOptions.eligibilityLevels) {
        return [];
      }
  
      let levels =  this.possibleLeagueOptions.eligibilityLevels.map(function (v) {
        return v.level;
      });
  
      return levels;
    },
    local() {
      return this.value;
    },
    maxFreeDroppableGamesRule() {
      return 'required|max_value:' + this.maxFreeDroppableGamesValue;
    },
    maxFreeDroppableGamesValue() {
      if (this.value.unlimitedWillNotReleaseDroppableGames) {
        return 100;
      }

      return this.value.willNotReleaseDroppableGames;
    }
  },
  methods: {
    showDetailedDropOptions() {
      this.shouldShowDetailedDropOptions = true;
    },
    update(key, value) {
      this.$emit('input', tap(cloneDeep(this.local), v => set(v, key, value)));
    },
    autoUpdateOptions() {
      if (this.intendedNumberOfPlayers >= 2 && this.intendedNumberOfPlayers <= 20) {
        this.intendedNumberOfPlayersEverValid = true;
      }

      let recommendedNumberOfGames = 72;
      let draftGameRatio = (1 / 2);

      if (this.gameMode === 'Beginner') {
        recommendedNumberOfGames = 42;
        draftGameRatio = (4 / 7);
      } else if (this.gameMode === 'Advanced') {
        recommendedNumberOfGames = 108;
        draftGameRatio = (4 / 9);
      }

      let averageSizeLeagueStandardGames = Math.floor(recommendedNumberOfGames / 6);
      let averageSizeLeagueGamesToDraft = Math.floor(averageSizeLeagueStandardGames * draftGameRatio);
      let averageSizeLeagueCounterPicks = Math.floor(averageSizeLeagueGamesToDraft / 4);

      let thisSizeLeagueStandardGames = Math.floor(recommendedNumberOfGames / this.intendedNumberOfPlayers);
      let thisSizeLeagueGamesToDraft = Math.floor(thisSizeLeagueStandardGames * draftGameRatio);
      let thisSizeLeagueCounterPicks = Math.floor(thisSizeLeagueGamesToDraft / 4);

      this.value.standardGames = Math.floor((averageSizeLeagueStandardGames + thisSizeLeagueStandardGames) / 2);
      this.value.gamesToDraft = Math.floor((averageSizeLeagueGamesToDraft + thisSizeLeagueGamesToDraft) / 2);
      this.value.counterPicks = Math.floor((averageSizeLeagueCounterPicks + thisSizeLeagueCounterPicks) / 2);

      if (this.value.counterPicks === 0) {
        this.value.counterPicks = 1;
      }

      if (this.gameMode === 'Beginner') {
        this.value.counterPicks = 0;
      }

      this.value.freeDroppableGames = 0;
      this.value.willNotReleaseDroppableGames = 0;
      this.value.willReleaseDroppableGames = 1;
      this.value.unlimitedFreeDroppableGames = false;
      this.value.unlimitedWillNotReleaseDroppableGames = true;
      this.value.unlimitedWillReleaseDroppableGames = false;

      this.$emit('input', this.local);
    }
  },
  watch: {
    intendedNumberOfPlayers: function (val) {
      this.autoUpdateOptions();
    },
    gameMode: function (val) {
      this.autoUpdateOptions();
    }
  },
  mounted() {
    if (this.currentNumberOfPlayers && this.freshSettings) {
      this.intendedNumberOfPlayers = this.currentNumberOfPlayers;
    }
  }
};
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

  .mode-slider {
    margin-left: 25px;
    margin-right: 25px;
    margin-bottom: 40px;
  }

  .unlimited-checkbox{
    margin-bottom: 10px;
  }

  .drop-number{
    width: 100px;
  }
</style>
