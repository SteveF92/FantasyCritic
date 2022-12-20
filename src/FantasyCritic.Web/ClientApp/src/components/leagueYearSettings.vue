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
          <ValidationProvider v-slot="{ errors }" rules="required|min_value:2|max_value:20|integer">
            <input id="intendedNumberOfPlayers" v-model="intendedNumberOfPlayers" name="Intended Number of Players" type="text" class="form-control input" @input="fullAutoUpdate()" />
            <span class="text-danger">{{ errors[0] }}</span>
          </ValidationProvider>
          <p>You aren't locked into this number of people. This is just to recommend how many games to have per person.</p>
        </div>

        <div class="form-group">
          <label for="gameMode" class="control-label">Game Mode</label>
          <p>If you've played Fantasy Critic before, consider using Advanced. If you're playing with people new to video games in general, consider using Beginner.</p>
          <p>
            If you're looking for less of a commitment, you can consider "one shot" mode. In this mode, the draft is final, there is no dropping or bidding for games. This can be a good option if you
            aren't sure you want to keep up with your league all year long.
          </p>
          <b-form-select v-model="gameMode" :options="gameModeOptions" @input="fullAutoUpdate()"></b-form-select>
          <p>These modes change the recommended number of games per player and a few other settings. This just provides a baseline, you are free to tweak anything you want.</p>
        </div>
      </div>
    </div>

    <div v-if="internalValue && (intendedNumberOfPlayersEverValid || editMode)">
      <div v-if="freshSettings">
        <hr />
        <label>Based on your number of players and selected game mode, we recommend the following settings. However, you are free to change this.</label>
      </div>

      <div class="form-group">
        <label for="standardGames" class="control-label">Total Number of Games</label>
        <p>This is the total number of games that each player will have on their roster.</p>

        <ValidationProvider v-slot="{ errors }" rules="required|min_value:1|max_value:50|integer">
          <input id="standardGames" v-model="internalValue.standardGames" name="Total Number of Games" type="text" class="form-control input" @input="autoUpdateSpecialSlotOptions()" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationProvider>
      </div>

      <div v-show="!oneShotMode" class="form-group">
        <label for="gamesToDraft" class="control-label">Number of Games to Draft</label>
        <p>
          This is the number of games that will be chosen by each player at the draft. If this number is lower than the "Total Number of Games", the remainder will be
          <a href="/faq#bidding-system" target="_blank">Pickup Games.</a>
        </p>

        <ValidationProvider v-slot="{ errors }" rules="required|min_value:1|max_value:50|integer">
          <input id="gamesToDraft" v-model="internalValue.gamesToDraft" name="Games to Draft" type="text" class="form-control input" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationProvider>
      </div>

      <div class="form-group">
        <label for="counterPicks" class="control-label">Total Number of Counter Picks</label>
        <p>
          Counter picks are essentially bets against a game. For more details,
          <a href="/faq#scoring" target="_blank">click here.</a>
        </p>

        <ValidationProvider v-slot="{ errors }" rules="required|max_value:50|integer">
          <input id="counterPicks" v-model="internalValue.counterPicks" name="Number of Counter picks" type="text" class="form-control input" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationProvider>
      </div>

      <div v-show="!oneShotMode" class="form-group">
        <label for="counterPicksToDraft" class="control-label">Number of Counter Picks to Draft</label>
        <p>
          This is the number of games that will be chosen by each player at the draft. If this number is lower than the "Total Number of Counter Picks", the remainder will be
          <a href="/faq#bidding-system" target="_blank">Pickup Counter picks.</a>
        </p>

        <ValidationProvider v-slot="{ errors }" rules="required|max_value:50|integer">
          <input id="counterPicksToDraft" v-model="internalValue.counterPicksToDraft" name="Counter picks to Draft" type="text" class="form-control input" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationProvider>
      </div>

      <div v-show="!oneShotMode" class="form-group">
        <label for="minimumBidAmount" class="control-label">Minimum Bid Amount</label>
        <ValidationProvider v-slot="{ errors }" rules="required|min_value:0|max_value:100|integer">
          <input id="minimumBidAmount" v-model="internalValue.minimumBidAmount" name="Minimum Bid Amount" type="text" class="form-control input" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationProvider>
        <p>The minimum dollar amount that a player can bid on a game. The default is $0. A minimum of $1 is probably the best option other than zero, and I don't recommend going above $10</p>
      </div>

      <div v-show="!oneShotMode">
        <hr />
        <h3>Bidding Settings</h3>
        <div class="alert alert-info">
          Choosing the "public bidding" option can help balance leagues where some players are more engaged/invested than others. You can read more about it on the
          <a href="/faq#bidding-system" target="_blank" class="text-secondary">FAQ page</a>
          .
          <br />
          If you want to keep playing the standard way, with fully secret bidding, you can chose the "secret bidding" option.
        </div>
        <label for="pickupSystem" class="control-label">Bidding System</label>
        <b-form-select v-model="internalValue.pickupSystem" :options="possibleLeagueOptions.pickupSystems"></b-form-select>
      </div>

      <div v-show="!oneShotMode">
        <hr />
        <h3>Trade Settings</h3>
        <label for="tradingSystem" class="control-label">Trading System</label>
        <b-form-select v-model="internalValue.tradingSystem" :options="possibleLeagueOptions.tradingSystems"></b-form-select>
      </div>

      <div v-show="!oneShotMode">
        <hr />
        <h3>Game Dropping Settings</h3>
        <div class="alert alert-info">
          If you like, you can allow players to drop a game before it releases. These settings allow you to choose how many such games can be dropped, if any. You can customize how many games are
          droppable after the game is confirmed to be delayed, as well as how many are droppable that are still scheduled to release.
          <br />
          You can also use the "Any Unreleased" setting, which applies to all unreleased games, delayed or not.
          <br />
          For more details, check out the
          <a href="/faq#dropping-games" target="_blank" class="text-secondary">FAQ.</a>
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
                <ValidationProvider v-if="!internalValue.unlimitedWillReleaseDroppableGames" v-slot="{ errors }" rules="required|max_value:100|integer">
                  <input id="willReleaseDroppableGames" v-model="internalValue.willReleaseDroppableGames" name="Will Release Droppable Games" type="text" class="form-control input drop-number" />
                  <span class="text-danger">{{ errors[0] }}</span>
                </ValidationProvider>
              </td>
              <td>
                <b-form-checkbox v-model="internalValue.unlimitedWillReleaseDroppableGames" class="unlimited-checkbox"></b-form-checkbox>
              </td>
            </tr>
            <tr>
              <th scope="row">Will Not Release</th>
              <td>
                <ValidationProvider v-if="!internalValue.unlimitedWillNotReleaseDroppableGames" v-slot="{ errors }" rules="required|max_value:100|integer">
                  <input
                    id="willNotReleaseDroppableGames"
                    v-model="internalValue.willNotReleaseDroppableGames"
                    name="Will Not Release Droppable Games"
                    type="text"
                    class="form-control input drop-number" />
                  <span class="text-danger">{{ errors[0] }}</span>
                </ValidationProvider>
              </td>
              <td>
                <b-form-checkbox v-model="internalValue.unlimitedWillNotReleaseDroppableGames" class="unlimited-checkbox"></b-form-checkbox>
              </td>
            </tr>
            <tr>
              <th scope="row">Any Unreleased</th>
              <td>
                <ValidationProvider v-if="!internalValue.unlimitedFreeDroppableGames" v-slot="{ errors }" rules="required|max_value:100|integer">
                  <input id="freeDroppableGames" v-model="internalValue.freeDroppableGames" name="Unrestricted Droppable Games" type="text" class="form-control input drop-number" />
                  <span class="text-danger">{{ errors[0] }}</span>
                </ValidationProvider>
              </td>
              <td>
                <b-form-checkbox v-model="internalValue.unlimitedFreeDroppableGames" class="unlimited-checkbox"></b-form-checkbox>
              </td>
            </tr>
          </tbody>
        </table>
        <div>
          <b-form-checkbox v-model="internalValue.dropOnlyDraftGames">
            <span class="checkbox-label">Only allow drafted to be dropped</span>
            <p>If this is checked, pickup games will not be droppable, no matter what the above settings are. Counter picks are never droppable.</p>
          </b-form-checkbox>
        </div>
        <div>
          <b-form-checkbox v-model="internalValue.grantSuperDrops">
            <span class="checkbox-label">Enable Super Drops</span>
            <p>
              Super drops allow players to drop a single game, with no conditions. For example, you can drop a game that already came out and scored poorly, or you can drop a counterpick that scored
              very well. While league managers can always manually grant a player a super drop, if you choose this option, players that are doing poorly in the league will be given one automatically.
              For more details,
              <a href="/faq#dropping-games" target="_blank">click here.</a>
            </p>
          </b-form-checkbox>
        </div>
        <div>
          <b-form-checkbox v-model="internalValue.counterPicksBlockDrops">
            <span class="checkbox-label">Counter Picks Block Drops</span>
            <p>If this is checked, counter picking a game will prevent the original player from dropping the game.</p>
          </b-form-checkbox>
        </div>
      </div>

      <hr />
      <h3>Eligibility Settings</h3>
      <div class="alert alert-info">
        These options let you choose what games are available in your league. These settings can be overriden on a game by game basis, and I recommend you lean towards being more restrictive, and
        allow specific exemptions if your entire league decides on one. The default options are the recommended settings.
        <br />
        <br />
        All games have a number of tags associated with them. If you place a tag in the "Banned" column, any game with that tag will not be selectable.
      </div>
      <leagueTagSelector v-model="internalValue.tags" :game-mode="gameMode"></leagueTagSelector>

      <hr />
      <h3>Special Game Slots</h3>
      <div class="alert alert-info">
        You can use this setting to have certain slots in every player's lineup require certain tags, overriding the rules chosen above.
        <br />
        You can read more in the
        <a href="/faq#drafting" target="_blank" class="text-secondary">FAQ.</a>
        , but for example, you can choose to ban 'Yearly Installments' above, but here, specify that one slot
        <em>must</em>
        be a yearly installment. Then, every player must have exactly one 'Yearly Installment'.
      </div>
      <specialGameSlotSelector v-model="internalValue.specialGameSlots"></specialGameSlotSelector>

      <hr />
      <h3 v-b-toggle.advanced-settings-collapse class="collapse-header">
        Advanced Settings
        <span class="collapse-opened"><font-awesome-icon icon="caret-down" /></span>
        <span class="collapse-closed"><font-awesome-icon icon="caret-left" /></span>
      </h3>
      <b-collapse id="advanced-settings-collapse">
        <div class="alert alert-info">We recommend you keep these settings to the default options.</div>
        <div v-if="year >= 2023">
          <b-form-checkbox v-model="bonusPointsNinety">
            <span class="checkbox-label">Double Points for Critic Scores above 90</span>
            <p>From 2018-2022, this option being on was the only way to play. Now, in 2023, we're giving the option to turn this off. If you uncheck this, a 95 will give 25 points, instead of 30.</p>
          </b-form-checkbox>
        </div>
        <div class="form-group">
          <label for="tiebreakSystem" class="control-label">Tiebreak System</label>
          <b-form-select v-model="internalValue.tiebreakSystem" :options="possibleLeagueOptions.tiebreakSystems"></b-form-select>
          <p>This setting handles how ties on bids are handled. We recommend you stick with 'Lowest Projected Points', as it keeps leagues more competitive.</p>
        </div>
        <div class="form-group">
          <label for="releaseSystem" class="control-label">Game Release Rule</label>
          <b-form-select v-model="internalValue.releaseSystem" :options="possibleLeagueOptions.releaseSystems"></b-form-select>
          <p>
            This setting handles whether or not a game must be officially released in order to get points. By default, they do. However, recently, more and more games have had "strange" release
            situations such as
            <em>Overwatch 2</em>
            and
            <em>Multiversus</em>
            . This option will allow a player to get points as soon as a game has an Open Critic score, regardless of it's release status. This also means that Early Access games
            <em>will</em>
            count for points while they are in early access.
          </p>
        </div>
        <div class="form-group">
          <label for="counterPickDeadline" class="control-label">Counter Pick Deadline</label>
          <b-form-datepicker v-model="internalValue.counterPickDeadline" class="form-control"></b-form-datepicker>
          <p>
            After this date, a game without a confirmed release date can no longer be counter picked. This is only relevant if your league has "pickup counter picks". The intention for this is to
            prevent players from waiting until the very end of the year to counter pick a game that is almost certain to not release this year.
          </p>
          <p>A 'confirmed release date' means a single date, like "December 12th, 2022". A release date of "2022" does not count.</p>
          <p>If you set this to December 31st, it will effectively have no relevance.</p>
        </div>
        <div class="form-group">
          <label for="mightReleaseDroppableDate" class="control-label">Might Release Droppable Date</label>
          <div class="clearable-date">
            <b-form-datepicker v-model="internalValue.mightReleaseDroppableDate" class="form-control"></b-form-datepicker>
            <b-button variant="warning" size="sm" @click="internalValue.mightReleaseDroppableDate = null">Clear</b-button>
          </div>

          <p>If you enable this option, then after this date, a game that is considered "might release" will be treated like a "will not release" game for dropping and counter-picking purposes.</p>
          <p>"Might Release" means a game with a release date like "TBA". If a game has a non-specific "2023" date, then it will still be counter as "Will Release" for 2023.</p>
          <p>In more technical terms, this is based on the "Maximum Release Date" that you can see on the Master Game page.</p>
        </div>
      </b-collapse>
    </div>
  </div>
</template>
<script>
import LeagueTagSelector from '@/components/leagueTagSelector';
import SpecialGameSlotSelector from '@/components/specialGameSlotSelector';

export default {
  components: {
    LeagueTagSelector,
    SpecialGameSlotSelector
  },
  props: {
    value: { type: Object, required: true },
    year: { type: Number, required: true },
    possibleLeagueOptions: { type: Object, required: true },
    editMode: { type: Boolean },
    currentNumberOfPlayers: { type: Number, default: null },
    freshSettings: { type: Boolean, required: true }
  },
  data() {
    return {
      intendedNumberOfPlayers: null,
      intendedNumberOfPlayersEverValid: false,
      gameMode: 'Standard',
      gameModeOptions: ['One Shot', 'Beginner', 'Standard', 'Advanced'],
      internalValue: null,
      bonusPointsNinety: true
    };
  },
  computed: {
    oneShotMode() {
      return this.gameMode === 'One Shot';
    }
  },
  watch: {
    intendedNumberOfPlayers: function () {
      if (this.intendedNumberOfPlayers >= 2 && this.intendedNumberOfPlayers <= 20) {
        this.intendedNumberOfPlayersEverValid = true;
      }
    },
    internalValue: function () {
      this.updateInternalValue();
    },
    bonusPointsNinety: function (newValue) {
      if (newValue) {
        this.internalValue.scoringSystem = 'Standard';
      } else {
        this.internalValue.scoringSystem = 'LinearPositive';
      }
    }
  },
  mounted() {
    if (this.currentNumberOfPlayers && this.freshSettings) {
      this.intendedNumberOfPlayers = this.currentNumberOfPlayers;
    }

    this.internalValue = _.cloneDeep(this.value);
  },
  methods: {
    updateInternalValue() {
      if (this.internalValue.scoringSystem === 'Standard') {
        this.bonusPointsNinety = true;
      } else if (this.internalValue.scoringSystem === 'LinearPositive') {
        this.bonusPointsNinety = false;
      }
      this.$emit('input', this.internalValue);
    },
    fullAutoUpdate() {
      this.autoUpdateOptions();
      this.autoUpdateSpecialSlotOptions();
    },
    autoUpdateOptions() {
      if (!this.freshSettings) {
        return;
      }

      this.internalValue.counterPickDeadline = `${this.year}-11-01`;

      if (!this.intendedNumberOfPlayers) {
        return;
      }

      if (this.intendedNumberOfPlayers >= 2 && this.intendedNumberOfPlayers <= 20) {
        this.intendedNumberOfPlayersEverValid = true;
      }

      let recommendedNumberOfGames = 72;
      let draftGameRatio = 1 / 2;
      if (this.oneShotMode) {
        recommendedNumberOfGames = 50;
        draftGameRatio = 1;
      } else if (this.gameMode === 'Beginner') {
        recommendedNumberOfGames = 42;
        draftGameRatio = 4 / 7;
      } else if (this.gameMode === 'Advanced') {
        recommendedNumberOfGames = 108;
        draftGameRatio = 4 / 9;
      }

      let averageSizeLeagueStandardGames = Math.floor(recommendedNumberOfGames / 6);
      let averageSizeLeagueCounterPicks = Math.floor(averageSizeLeagueStandardGames / 6);
      let averageSizeLeagueGamesToDraft = Math.floor(averageSizeLeagueStandardGames * draftGameRatio);
      let averageSizeLeagueCounterPicksToDraft = Math.floor(averageSizeLeagueCounterPicks * draftGameRatio);

      let thisSizeLeagueStandardGames = Math.floor(recommendedNumberOfGames / this.intendedNumberOfPlayers);
      let thisSizeLeagueCounterPicks = Math.floor(thisSizeLeagueStandardGames / 6);
      let thisSizeLeagueGamesToDraft = Math.floor(thisSizeLeagueStandardGames * draftGameRatio);
      let thisSizeLeagueCounterPicksToDraft = Math.floor(thisSizeLeagueCounterPicks * draftGameRatio);

      this.internalValue.standardGames = Math.floor((averageSizeLeagueStandardGames + thisSizeLeagueStandardGames) / 2);
      this.internalValue.counterPicks = Math.floor((averageSizeLeagueCounterPicks + thisSizeLeagueCounterPicks) / 2);
      this.internalValue.gamesToDraft = Math.floor((averageSizeLeagueGamesToDraft + thisSizeLeagueGamesToDraft) / 2);
      this.internalValue.counterPicksToDraft = Math.floor((averageSizeLeagueCounterPicksToDraft + thisSizeLeagueCounterPicksToDraft) / 2);

      if (this.internalValue.counterPicks === 0 || this.internalValue.counterPicksToDraft === 0) {
        this.internalValue.counterPicks = 1;
        this.internalValue.counterPicksToDraft = 1;
      }

      if (this.gameMode === 'Beginner') {
        this.internalValue.counterPicks = 0;
        this.internalValue.counterPicksToDraft = 0;
      }

      this.internalValue.minimumBidAmount = 0;
      this.internalValue.freeDroppableGames = 0;
      this.internalValue.willNotReleaseDroppableGames = 0;
      this.internalValue.unlimitedFreeDroppableGames = false;
      this.internalValue.unlimitedWillReleaseDroppableGames = false;
      if (!this.oneShotMode) {
        this.internalValue.willReleaseDroppableGames = 1;
        this.internalValue.unlimitedWillNotReleaseDroppableGames = true;
        this.internalValue.tradingSystem = 'NoTrades';
      } else {
        this.internalValue.willReleaseDroppableGames = 0;
        this.internalValue.unlimitedWillNotReleaseDroppableGames = false;
      }

      let alwaysBannedTags = ['Port', 'CurrentlyInEarlyAccess', 'ReleasedInternationally'];
      let standardBannedTags = ['YearlyInstallment', 'DirectorsCut', 'PartialRemake', 'Remaster', 'ExpansionPack'];

      let bannedTags = alwaysBannedTags;
      if (this.gameMode !== 'Beginner') {
        bannedTags = bannedTags.concat(standardBannedTags);
      }

      this.internalValue.tags = {
        required: [],
        banned: bannedTags
      };
    },
    autoUpdateSpecialSlotOptions() {
      if (!this.freshSettings) {
        return;
      }

      if (!this.internalValue.standardGames || this.internalValue.standardGames > 50) {
        return;
      }

      this.internalValue.specialGameSlots = [];
      if (this.gameMode === 'Beginner') {
        return;
      }

      let numberOfSpecialSlots = Math.floor(this.internalValue.standardGames / 2);
      if (numberOfSpecialSlots < 1) {
        return;
      }

      let includeYearlyInstallmentSlot = this.intendedNumberOfPlayers < 5;
      let includeExpansionPackSlot = numberOfSpecialSlots >= 2;
      let includeRemakeSlot = numberOfSpecialSlots >= 2;
      let numberNonNGFSlots = 0;
      if (includeYearlyInstallmentSlot) {
        numberNonNGFSlots++;
      }
      if (includeExpansionPackSlot) {
        numberNonNGFSlots++;
      }
      if (includeRemakeSlot) {
        numberNonNGFSlots++;
      }

      let numberNGFSlots = numberOfSpecialSlots - numberNonNGFSlots;
      if (numberNGFSlots < 0) {
        numberNGFSlots = 0;
      }

      let slotIndex = 0;
      for (slotIndex = 0; slotIndex < numberNGFSlots; slotIndex++) {
        this.internalValue.specialGameSlots.push({
          specialSlotPosition: slotIndex,
          requiredTags: ['NewGamingFranchise']
        });
      }

      if (includeYearlyInstallmentSlot) {
        this.internalValue.specialGameSlots.push({
          specialSlotPosition: slotIndex,
          requiredTags: ['YearlyInstallment']
        });
        slotIndex++;
      }

      if (includeExpansionPackSlot) {
        this.internalValue.specialGameSlots.push({
          specialSlotPosition: slotIndex,
          requiredTags: ['ExpansionPack']
        });
        slotIndex++;
      }

      if (includeRemakeSlot) {
        this.internalValue.specialGameSlots.push({
          specialSlotPosition: slotIndex,
          requiredTags: ['Remake', 'PartialRemake', 'DirectorsCut']
        });
        slotIndex++;
      }
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

.unlimited-checkbox {
  margin-bottom: 10px;
}

.drop-number {
  width: 100px;
}

.clearable-date {
  display: flex;
  gap: 5px;
}
</style>
