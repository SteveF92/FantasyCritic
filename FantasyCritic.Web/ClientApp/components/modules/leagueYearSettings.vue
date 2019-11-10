<template>
  <div class="form-group">
    <label for="intendedNumberOfPlayers" class="control-label">How many players do you think will be in this league?</label>
    <input v-model="intendedNumberOfPlayers" v-validate="'required|min_value:2|max_value:14'" id="intendedNumberOfPlayers" name="intendedNumberOfPlayers" type="text" class="form-control input" />
    <span class="text-danger">{{ errors.first('intendedNumberOfPlayers') }}</span>
    <p>You aren't locked into this number of people. This is just to recommend how many games to have per person.</p>
  </div>
</template>
<script>
  export default {
    props: ['year', 'value'],
    data() {
      return {
        leagueYearSettings: null,
        intendedNumberOfPlayers: ""
      }
    }
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
