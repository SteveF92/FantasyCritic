<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div>
        <h1>Edit Master Game</h1>
        <b-button variant="info" :to="{ name: 'activeMasterGameChangeRequests' }">View master change game requests</b-button>
        <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
        <b-button variant="info" size="sm" v-on:click="generateSQL(masterGame)">Generate SQL</b-button>
      </div>
      <hr />
      <div v-if="responseInfo" class="alert alert-success">Master Game edited successfully!</div>
      <div v-if="generatedSQL">
        <h3>Generated SQL</h3>
        <div class="row">
          <div class="col-xl-8 col-lg-10 col-md-12 text-well">
            <div class="form-group">
              <label for="generated SQL" class="control-label">GeneratedSQL</label>
              <input v-model="generatedSQL" id="generatedSQL" name="generatedSQL" class="form-control input" />
            </div>
          </div>
        </div>
      </div>
      <div v-if="masterGame">
        <h2>{{ masterGame.gameName }}</h2>
        <router-link class="text-primary" :to="{ name: 'mastergame', params: { mastergameid: masterGame.masterGameID } }"><strong>View full details</strong></router-link>
        <div class="row" v-if="changeRequest">
          <div class="text-well">
            <h2>Request Note</h2>
            <p>{{ changeRequest.requestNote }}</p>
          </div>
        </div>
        <hr />

        <div class="row">
          <div class="col-lg-10 col-md-12 offset-lg-1 text-well">
            <ValidationObserver v-slot="{ invalid }">
              <form v-on:submit.prevent="sendEditMasterGameRequest">
                <div class="form-group">
                  <label for="gameName" class="control-label">Game Name</label>
                  <ValidationProvider rules="required" v-slot="{ errors }">
                    <input v-model="masterGame.gameName" id="gameName" name="Game Name" type="text" class="form-control input" />
                    <span class="text-danger">{{ errors[0] }}</span>
                  </ValidationProvider>
                </div>

                <div class="form-group">
                  <label for="releaseDate" class="control-label">Release Date</label>
                  <flat-pickr v-model="masterGame.releaseDate" class="form-control"></flat-pickr>
                </div>

                <b-button variant="info" size="sm" v-on:click="propagateDate">Propagate Date</b-button>
                <b-button variant="info" size="sm" v-on:click="parseEstimatedReleaseDate">Parse Estimated</b-button>
                <b-button variant="warning" size="sm" v-on:click="clearDates">Clear Dates</b-button>

                <div class="form-group">
                  <label for="estimatedReleaseDate" class="control-label">Estimated Release Date</label>
                  <input v-model="masterGame.estimatedReleaseDate" id="estimatedReleaseDate" name="estimatedReleaseDate" class="form-control input" />
                </div>
                <div class="form-group">
                  <label for="minimumReleaseDate" class="control-label">Minimum Release Date</label>
                  <flat-pickr v-model="masterGame.minimumReleaseDate" class="form-control"></flat-pickr>
                </div>
                <div class="form-group">
                  <label for="maximumReleaseDate" class="control-label">Maximum Release Date</label>
                  <flat-pickr v-model="masterGame.maximumReleaseDate" class="form-control"></flat-pickr>
                </div>
                <div class="form-group">
                  <label for="earlyAccessReleaseDate" class="control-label">Early Access Release Date</label>
                  <flat-pickr v-model="masterGame.earlyAccessReleaseDate" class="form-control"></flat-pickr>
                </div>
                <div class="form-group">
                  <label for="internationalReleaseDate" class="control-label">International Release Date</label>
                  <flat-pickr v-model="masterGame.internationalReleaseDate" class="form-control"></flat-pickr>
                </div>
                <div class="form-group">
                  <label for="announcementDate" class="control-label">Announcement Date</label>
                  <flat-pickr v-model="masterGame.announcementDate" class="form-control"></flat-pickr>
                </div>

                <div class="form-group">
                  <label for="openCriticID" class="control-label">Open Critic ID</label>
                  <input v-model="masterGame.openCriticID" id="openCriticID" name="openCriticID" class="form-control input" />
                </div>

                <div class="form-group">
                  <label for="ggToken" class="control-label">GG Token</label>
                  <input v-model="masterGame.ggToken" id="ggToken" name="ggToken" class="form-control input" />
                </div>

                <h3>Tags</h3>
                <masterGameTagSelector v-model="tags" :includeSystem="true"></masterGameTagSelector>

                <div class="form-group">
                  <label for="notes" class="control-label">Other Notes</label>
                  <input v-model="masterGame.notes" id="notes" name="notes" class="form-control input" />
                </div>

                <h3>Other</h3>
                <div class="form-group">
                  <b-form-checkbox v-model="masterGame.doNotRefreshDate">
                    <span class="checkbox-label">Do Not Refresh Date</span>
                  </b-form-checkbox>
                </div>
                <div class="form-group">
                  <b-form-checkbox v-model="masterGame.doNotRefreshAnything">
                    <span class="checkbox-label">Do Not Refresh Anything</span>
                  </b-form-checkbox>
                </div>
                <div class="form-group">
                  <b-form-checkbox v-model="masterGame.eligibilityChanged">
                    <span class="checkbox-label">Eligibility Changed</span>
                  </b-form-checkbox>
                </div>
                <div class="form-group">
                  <b-form-checkbox v-model="masterGame.delayContention">
                    <span class="checkbox-label">Delay in Contention</span>
                  </b-form-checkbox>
                </div>
                <div class="form-group">
                  <label for="notes" class="control-label">Box Art File Name</label>
                  <input v-model="masterGame.boxartFileName" id="boxartFileName" name="boxartFileName" class="form-control input" />
                </div>

                <div class="form-group">
                  <div class="col-md-offset-2 col-md-4">
                    <input type="submit" class="btn btn-primary" value="Submit" :disabled="invalid" />
                  </div>
                </div>
              </form>
            </ValidationObserver>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
import axios from 'axios';
import MasterGameTagSelector from '@/components/masterGameTagSelector';

export default {
  props: ['mastergameid'],
  data() {
    return {
      masterGame: null,
      changeRequest: null,
      tags: [],
      responseInfo: null,
      generatedSQL: null
    };
  },
  components: {
    MasterGameTagSelector
  },
  methods: {
    async fetchMasterGame() {
      await axios
        .get('/api/game/MasterGame/' + this.mastergameid)
        .then((response) => {
          this.masterGame = response.data;
          if (this.masterGame.maximumReleaseDate === '9999-12-31') {
            this.masterGame.maximumReleaseDate = null;
          }
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    async fetchChangeRequest() {
      let changeRequestID = this.$route.query.changeRequestID;
      if (!changeRequestID) {
        return;
      }
      await axios
        .get('/api/admin/GetMasterGameChangeRequest?changeRequestID=' + changeRequestID)
        .then((response) => {
          this.changeRequest = response.data;
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    async parseEstimatedReleaseDate() {
      await axios
        .get('/api/admin/ParseEstimatedDate?estimatedReleaseDate=' + this.masterGame.estimatedReleaseDate)
        .then((response) => {
          this.masterGame.minimumReleaseDate = response.data.minimumReleaseDate;
          this.masterGame.maximumReleaseDate = response.data.maximumReleaseDate;
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    propagateDate() {
      this.masterGame.maximumReleaseDate = this.masterGame.releaseDate;
      this.masterGame.minimumReleaseDate = this.masterGame.releaseDate;
      this.masterGame.estimatedReleaseDate = this.masterGame.releaseDate;
    },
    populateTags() {
      let allTags = this.$store.getters.allTags;
      let masterGameTagNames = this.masterGame.tags;
      let matchingTags = _.filter(allTags, (x) => masterGameTagNames.includes(x.name));
      this.tags = matchingTags;
    },
    clearDates() {
      this.masterGame.releaseDate = null;
      this.masterGame.minimumReleaseDate = null;
      this.masterGame.maximumReleaseDate = null;
      this.masterGame.estimatedReleaseDate = null;
      this.masterGame.internationalReleaseDate = null;
      this.masterGame.announcementDate = null;
      this.masterGame.earlyAccessReleaseDate = null;
    },
    sendEditMasterGameRequest() {
      let tagNames = _.map(this.tags, 'name');

      let request = this.masterGame;
      request.tags = tagNames;

      axios
        .post('/api/admin/EditMasterGame', request)
        .then((response) => {
          this.responseInfo = response.data;
          window.scroll({
            top: 0,
            left: 0,
            behavior: 'smooth'
          });
        })
        .catch((error) => {
          this.errorInfo = error.response;
        });
    },
    generateSQL() {
      this.generatedSQL = "select * from tbl_mastergame where MasterGameID = '" + this.masterGame.masterGameID + "';";
    }
  },
  async mounted() {
    await this.fetchMasterGame();
    await this.fetchChangeRequest();
    this.parseEstimatedReleaseDate();
    this.populateTags();
  },
  watch: {
    $route() {
      this.fetchMasterGame();
    }
  }
};
</script>
<style scoped>
.select-cell {
  text-align: center;
}

.eligibility-explanation {
  margin-bottom: 50px;
  max-width: 1300px;
}

.eligibility-section {
  margin-bottom: 30px;
}

.eligibility-description {
  margin-top: 25px;
}

.checkbox-label {
  padding-left: 25px;
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
