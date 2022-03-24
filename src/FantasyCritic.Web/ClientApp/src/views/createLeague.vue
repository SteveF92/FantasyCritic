<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Create a league</h1>
      <hr />
      <div class="alert alert-danger" v-show="errorInfo">
        <h2>Error!</h2>
        <p>{{ errorInfo }}</p>
      </div>
      <template v-if="possibleLeagueOptions">
        <div class="alert alert-warning" v-if="possibleLeagueOptions.openYears.length === 0">
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
              <ValidationProvider rules="required" v-slot="{ errors }" name="League Name">
                <input v-model="leagueName" id="leagueName" name="leagueName" type="text" class="form-control input" />
                <span class="text-danger">{{ errors[0] }}</span>
              </ValidationProvider>
            </div>
            <hr />
            <div class="form-group">
              <label for="intialYear" class="control-label">Year to Play</label>
              <p>The best time to start a game is at the beginning of the year, the earlier the better. You are free to start playing as early as the December before the new year begins.</p>
              <select class="form-control" v-model="initialYear" id="initialYear">
                <option v-for="initialYear in possibleLeagueOptions.openYears" v-bind:value="initialYear" :key="initialYear">{{ initialYear }}</option>
              </select>
            </div>
          </div>

          <div v-if="readyToSetupLeagueYear">
            <hr />
            <div class="text-well">
              <leagueYearSettings v-model="leagueYearSettings" :year="initialYear" :possibleLeagueOptions="possibleLeagueOptions" :editMode="false" :freshSettings="true"></leagueYearSettings>
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
                <b-form-checkbox v-model="testLeague">
                  <span class="checkbox-label">Test League</span>
                  <p>If checked, this league won't affect the site's overall stats. Please check this if you are just testing out the site.</p>
                </b-form-checkbox>
              </div>
            </div>

            <hr />
            <div class="alert alert-info disclaimer">Reminder: All of these settings can always be changed later.</div>

            <div class="alert alert-warning disclaimer" v-show="!leagueYearIsValid">Can't create league. Some of your settings are invalid.</div>

            <div class="form-group">
              <b-button class="col-10 offset-1" variant="primary" v-on:click="postRequest" :disabled="!leagueYearIsValid">Create League</b-button>
            </div>
          </div>
        </template>
      </template>
    </div>
  </div>
</template>
<script>
import axios from 'axios';
import LeagueYearSettings from '@/components/leagueYearSettings';

export default {
  components: {
    LeagueYearSettings
  },
  data() {
    return {
      errorInfo: '',
      possibleLeagueOptions: null,
      leagueName: '',
      initialYear: '',
      leagueYearSettings: null,
      publicLeague: true,
      testLeague: false,
      leagueYearEverValid: false
    };
  },
  computed: {
    readyToSetupLeagueYear() {
      return this.leagueName && this.initialYear;
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
  mounted() {
    this.fetchLeagueOptions();
    this.leagueYearSettings = {
      standardGames: '',
      gamesToDraft: '',
      counterPicks: '',
      counterPicksToDraft: '',
      initialYear: '',
      pickupSystem: 'SecretBidding',
      tiebreakSystem: 'LowestProjectedPoints',
      tradingSystem: 'Standard',
      specialGameSlots: [],
      tags: { banned: [], allowed: [], required: [] }
    };
  },
  watch: {
    leagueYearIsValid: function (newValue) {
      let allValid = this.readyToSetupLeagueYear && newValue;
      if (allValid) {
        this.leagueYearEverValid = true;
      }
    }
  },
  methods: {
    fetchLeagueOptions() {
      axios
        .get('/api/League/LeagueOptions')
        .then((response) => {
          this.possibleLeagueOptions = response.data;
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    postRequest() {
      let selectedLeagueOptions = {
        leagueName: this.leagueName,
        initialYear: this.initialYear,
        standardGames: this.leagueYearSettings.standardGames,
        gamesToDraft: this.leagueYearSettings.gamesToDraft,
        counterPicks: this.leagueYearSettings.counterPicks,
        counterPicksToDraft: this.leagueYearSettings.counterPicksToDraft,
        freeDroppableGames: this.leagueYearSettings.freeDroppableGames,
        willNotReleaseDroppableGames: this.leagueYearSettings.willNotReleaseDroppableGames,
        willReleaseDroppableGames: this.leagueYearSettings.willReleaseDroppableGames,
        unlimitedFreeDroppableGames: this.leagueYearSettings.unlimitedFreeDroppableGames,
        unlimitedWillNotReleaseDroppableGames: this.leagueYearSettings.unlimitedWillNotReleaseDroppableGames,
        unlimitedWillReleaseDroppableGames: this.leagueYearSettings.unlimitedWillReleaseDroppableGames,
        dropOnlyDraftGames: this.leagueYearSettings.dropOnlyDraftGames,
        counterPicksBlockDrops: this.leagueYearSettings.counterPicksBlockDrops,
        minimumBidAmount: this.leagueYearSettings.minimumBidAmount,
        tags: this.leagueYearSettings.tags,
        specialGameSlots: this.leagueYearSettings.specialGameSlots,
        publicLeague: this.publicLeague,
        testLeague: this.testLeague,
        draftSystem: 'Flexible',
        pickupSystem: this.leagueYearSettings.pickupSystem,
        tiebreakSystem: this.leagueYearSettings.tiebreakSystem,
        tradingSystem: this.leagueYearSettings.tradingSystem,
        scoringSystem: 'Diminishing'
      };

      axios
        .post('/api/leagueManager/createLeague', selectedLeagueOptions)
        .then(() => {
          this.$router.push({ name: 'home' });
        })
        .catch((error) => {
          this.errorInfo = error.response.data;
          window.scroll({
            top: 0,
            left: 0,
            behavior: 'smooth'
          });
        });
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
