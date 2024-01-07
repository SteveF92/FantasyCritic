<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Create a Conference</h1>
      <hr />
      <div v-show="errorInfo" class="alert alert-danger">
        <h2>Error!</h2>
        <p>{{ errorInfo }}</p>
      </div>
      <template v-if="possibleLeagueOptions">
        <div v-if="possibleLeagueOptions.openYears.length === 0" class="alert alert-warning">
          Unfortunately, conferences cannot be created right now, as the current year is closed and the next year is not open yet. Check Twitter for updates.
        </div>
        <template v-else>
          <div class="alert alert-info">Conferences are a new feature for 2024. They are intended for large groups that do not fit into one league. For more details, see the FAQ page.</div>
          <div class="text-well">
            <h2>Basic Settings</h2>
            <div class="form-group">
              <label for="conferenceName" class="control-label">Conference Name</label>
              <ValidationProvider v-slot="{ errors }" rules="required" name="Conference Name">
                <input id="conferenceName" v-model="conferenceName" name="conferenceName" type="text" class="form-control input" />
                <span class="text-danger">{{ errors[0] }}</span>
              </ValidationProvider>
            </div>

            <div class="alert alert-info">
              The first step in setting up a conference is to set up your "primary league". By default, you will be in this league as it's league manager. All other leagues that you create in this
              conference will have the same options as this league automatically set.
            </div>

            <div class="form-group">
              <label for="primaryLeagueName" class="control-label">Primary League Name</label>
              <ValidationProvider v-slot="{ errors }" rules="required" name="Primary League Name">
                <input id="primaryLeagueName" v-model="primaryLeagueName" name="primaryLeagueName" type="text" class="form-control input" />
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
              <leagueYearSettings v-model="leagueYearSettings" :year="initialYear" fresh-settings conference-mode></leagueYearSettings>
            </div>
          </div>

          <div v-if="leagueYearIsValid || leagueYearEverValid">
            <hr />
            <div class="text-well">
              <h2>Other Options</h2>
              <div>
                <b-form-checkbox v-model="customRulesConference">
                  <span class="checkbox-label">Custom Rules Conference</span>
                  <p>
                    If checked, all leagues in this conference won't affect the site's overall stats. Please use this if you are running a "highly custom" conference, such as a conference where you
                    are picking the
                    <em>worst</em>
                    games of the year. If you want to change a custom rules conference into a regular conference, you'll need to contact us.
                  </p>
                </b-form-checkbox>
              </div>
            </div>

            <div class="alert alert-info disclaimer">Reminder: All of these settings can always be changed later.</div>

            <div v-show="!leagueYearIsValid" class="alert alert-warning disclaimer">Can't create league. Some of your settings are invalid.</div>

            <div class="form-group">
              <b-button class="col-10 offset-1" variant="primary" :disabled="!leagueYearIsValid" @click="postRequest">Create Conference</b-button>
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
      conferenceName: '',
      primaryLeagueName: '',
      initialYear: '',
      leagueYearSettings: null,
      customRulesConference: false,
      leagueYearEverValid: false
    };
  },
  computed: {
    readyToSetupLeagueYear() {
      return !!this.conferenceName && !!this.primaryLeagueName && !!this.initialYear;
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
      let selectedConferenceOptions = {
        conferenceName: this.conferenceName,
        primaryLeagueName: this.primaryLeagueName,
        customRulesConference: this.customRulesConference,
        leagueYearSettings: this.leagueYearSettings
      };

      try {
        const response = await axios.post('/api/conference/createConference', selectedConferenceOptions);
        const newConferenceID = response.data;
        this.$router.push({ name: 'conference', params: { conferenceid: newConferenceID, year: this.initialYear } });
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
