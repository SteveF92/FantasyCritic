<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Create a League</h1>
      <hr />
      <div v-show="errorInfo" class="alert alert-danger">
        <h2>Error!</h2>
        <p>{{ errorInfo }}</p>
      </div>
      <template v-if="possibleLeagueOptions">
        <div v-if="possibleLeagueOptions.openYears.length === 0" class="alert alert-warning">
          Unfortunately, leagues cannot be created right now, as the current year is closed and the next year is not open yet. Check Twitter for updates.
        </div>
        <template v-else>
          <div class="alert alert-info">
            If you already have a league and are looking to renew it for a new year, use the "Start New League" link on the sidebar of your league. That will keep your years linked together.
          </div>
          <div class="text-well">
            <h2>Basic Settings</h2>
            <div class="form-group">
              <label for="leagueName" class="control-label">League Name</label>
              <ValidationProvider v-slot="{ errors }" rules="required" name="League Name">
                <input id="leagueName" v-model="leagueName" name="leagueName" type="text" class="form-control input" />
                <span class="text-danger">{{ errors[0] }}</span>
              </ValidationProvider>
            </div>
            <hr />
            <div class="form-group">
              <label for="intialYear" class="control-label">Year to Play</label>
              <p>The best time to start a game is at the beginning of the year, the earlier the better. You are free to start playing as early as the December before the new year begins.</p>
              <select id="initialYear" v-model="initialYear" class="form-control">
                <option v-for="possibleYear in possibleLeagueOptions.openYears" :key="possibleYear" :value="possibleYear">{{ possibleYear }}</option>
              </select>
            </div>
          </div>

          <div v-if="readyToSetupLeagueYear">
            <hr />
            <div class="text-well">
              <leagueYearSettings v-model="leagueYearSettings" :year="initialYear" fresh-settings></leagueYearSettings>
            </div>
          </div>

          <div v-if="leagueYearIsValid || leagueYearEverValid">
            <hr />
            <div class="text-well">
              <h2>Other Options</h2>
              <div>
                <b-form-checkbox v-model="publicLeague">
                  <span class="checkbox-label">Public League</span>
                  <p>If checked, everyone will be able to see your league. Players still need to be invited to join. If unchecked, your league will only be viewable by its members.</p>
                </b-form-checkbox>
              </div>
              <div>
                <b-form-checkbox v-model="customRulesLeague">
                  <span class="checkbox-label">Custom Rules League</span>
                  <p>
                    If checked, this league won't affect the site's overall stats. Please use this if you are running a "highly custom" league, such as a league where you are picking the
                    <em>worst</em>
                    games of the year. If you want to change a custom rules league into a regular league, you'll need to contact us.
                  </p>
                </b-form-checkbox>
              </div>
              <div>
                <b-form-checkbox v-model="testLeague">
                  <span class="checkbox-label">Test League</span>
                  <p>If checked, this league won't affect the site's overall stats. Please check this if you are just testing out the site.</p>
                </b-form-checkbox>
              </div>
            </div>

            <b-alert variant="info">
              The difference between a "Custom Rules League" and a "Test League" is that a "Custom Rules League" will still be listed alongside your normal leagues and you will see upcoming games for
              that league on your home page. There is a dedicated home page tab for test leagues, and the games are not listed in your upcoming games on the home page. The "Custom Rules League" option
              is new as of 2023, and you
              <em>are</em>
              allowed to change your "Test League" into a "Custom Rules League", since neither affect the site's stats.
            </b-alert>

            <hr />
            <div class="alert alert-info disclaimer">Reminder: All of these settings can always be changed later.</div>

            <div v-show="!leagueYearIsValid" class="alert alert-warning disclaimer">Can't create league. Some of your settings are invalid.</div>

            <div class="form-group">
              <b-button class="col-10 offset-1" variant="primary" :disabled="!leagueYearIsValid" @click="postRequest">Create League</b-button>
            </div>
          </div>
        </template>
      </template>
    </div>
  </div>
</template>
<script>
import axios from 'axios';
import LeagueYearSettings from '@/components/leagueYearSettings.vue';

export default {
  components: {
    LeagueYearSettings
  },
  data() {
    return {
      errorInfo: '',
      leagueName: '',
      initialYear: '',
      leagueYearSettings: null,
      publicLeague: true,
      testLeague: false,
      customRulesLeague: false,
      leagueYearEverValid: false
    };
  },
  computed: {
    readyToSetupLeagueYear() {
      return !!this.leagueName && !!this.initialYear;
    },
    leagueYearIsValid() {
      let valid =
        this.leagueYearSettings &&
        this.leagueYearSettings.standardGames >= 1 &&
        this.leagueYearSettings.standardGames <= 50 &&
        this.leagueYearSettings.gamesToDraft >= 1 &&
        this.leagueYearSettings.gamesToDraft <= 50 &&
        this.leagueYearSettings.counterPicks >= 0 &&
        this.leagueYearSettings.counterPicks <= 20;

      let allValid = this.readyToSetupLeagueYear && valid;
      return allValid;
    }
  },
  watch: {
    leagueYearIsValid: function (newValue) {
      let allValid = this.readyToSetupLeagueYear && newValue;
      if (allValid) {
        this.leagueYearEverValid = true;
      }
    }
  },
  mounted() {
    this.leagueYearSettings = {
      standardGames: '',
      gamesToDraft: '',
      counterPicks: '',
      counterPicksToDraft: '',
      pickupSystem: 'SecretBidding',
      tiebreakSystem: 'LowestProjectedPoints',
      tradingSystem: 'Standard',
      draftSystem: 'Flexible',
      scoringSystem: 'Standard',
      releaseSystem: 'MustBeReleased',
      specialGameSlots: [],
      tags: { banned: [], allowed: [], required: [] }
    };
  },
  methods: {
    async postRequest() {
      this.leagueYearSettings.year = this.initialYear;
      let selectedLeagueOptions = {
        leagueName: this.leagueName,
        publicLeague: this.publicLeague,
        testLeague: this.testLeague,
        customRulesLeague: this.customRulesLeague,
        leagueYearSettings: this.leagueYearSettings
      };

      try {
        const response = await axios.post('/api/leagueManager/createLeague', selectedLeagueOptions);
        const newLeagueID = response.data;
        this.$router.push({ name: 'league', params: { leagueid: newLeagueID, year: this.initialYear } });
      } catch (error) {
        this.errorInfo = error.response.data;
        window.scroll({
          top: 0,
          left: 0,
          behavior: 'smooth'
        });
      }
    }
  }
};
</script>
<style scoped>
label {
  font-size: 18px;
}

.submit-button {
  text-align: right;
}
</style>
