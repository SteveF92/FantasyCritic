<template>
  <div>
    <h1>Master Game Request</h1>
    <div v-if="showSent" class="alert alert-success">Master Game request made.</div>
    <div class="row">
      <div class="col-xl-8 col-lg-10 col-md-12">
        <form v-on:submit.prevent="sendMasterGameRequestRequest">
          <div class="form-group">
            <label for="gameName" class="control-label">Game Name</label>
            <input v-model="gameName" id="gameName" name="gameName" class="form-control input" />
          </div>
          <div class="form-group">
            <label for="estimatedReleaseDate" class="control-label">Estimated Release Date</label>
            <input v-model="estimatedReleaseDate" id="estimatedReleaseDate" name="estimatedReleaseDate" class="form-control input" />
          </div>
          <div class="form-group">
            <label for="steamLink" class="control-label">Link to Steam Page</label>
            <input v-model="steamLink" id="steamLink" name="steamLink" class="form-control input" />
          </div>
          <div class="form-group">
            <label for="openCriticLink" class="control-label">Link to Open Critic Page</label>
            <input v-model="openCriticLink" id="openCriticLink" name="openCriticLink" class="form-control input" />
          </div>
          <div class="form-group">
            <label for="requestNote" class="control-label">Any other notes?</label>
            <input v-model="requestNote" id="requestNote" name="requestNote" class="form-control input" />
          </div>
          <div class="form-group">
            <div class="col-md-offset-2 col-md-4">
              <input type="submit" class="btn btn-primary" value="Submit" :disabled="!formIsValid" />
            </div>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
<script>
  import axios from 'axios';

  export default {
    data() {
      return {
        showSent: false,
        gameName: "",
        requestNote: "",
        steamLink: "",
        openCriticLink: "",
        estimatedReleaseDate: ""
      }
  },
  computed: {
    formIsValid() {
      return !Object.keys(this.veeFields).some(key => this.veeFields[key].invalid);
    }
  },
  methods: {
    sendMasterGameRequestRequest() {
      let request = {
        gameName: this.gameName,
        requestNote: this.requestNote,
        steamLink: this.steamLink,
        openCriticLink: this.openCriticLink,
        estimatedReleaseDate: this.estimatedReleaseDate
      };
      axios
        .post('/api/game/CreateMasterGameRequest', request)
        .then(response => {
            this.showSent = true;
        })
        .catch(response => {

        });
    }
  }
}
</script>
