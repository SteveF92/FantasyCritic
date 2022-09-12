<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div>
        <h1>Edit Master Game</h1>
        <b-button variant="info" :to="{ name: 'activeMasterGameChangeRequests' }">View master change game requests</b-button>
        <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
        <b-button variant="info" size="sm" @click="generateSQL(masterGame)">Generate SQL</b-button>
      </div>
      <hr />
      <div v-if="responseInfo" class="alert alert-success">Master Game edited successfully!</div>
      <div v-if="errorInfo" class="alert alert-danger">An error has occurred with your request: {{ errorInfo }}</div>
      <div v-if="generatedSQL">
        <h3>Generated SQL</h3>
        <div class="row">
          <div class="col-xl-8 col-lg-10 col-md-12 text-well">
            <div class="form-group">
              <label for="generated SQL" class="control-label">GeneratedSQL</label>
              <input id="generatedSQL" v-model="generatedSQL" name="generatedSQL" class="form-control input" />
            </div>
          </div>
        </div>
      </div>
      <div v-if="masterGame">
        <h2>{{ masterGame.gameName }}</h2>
        <router-link class="text-primary" :to="{ name: 'mastergame', params: { mastergameid: masterGame.masterGameID } }"><strong>View full details</strong></router-link>
        <div v-if="changeRequest" class="row">
          <div class="text-well">
            <h2>Request Note</h2>
            <p>{{ changeRequest.requestNote }}</p>
          </div>
        </div>
        <hr />

        <div class="row">
          <div class="col-lg-10 col-md-12 offset-lg-1 text-well">
            <ValidationObserver v-slot="{ invalid }">
              <form @submit.prevent="sendEditMasterGameRequest">
                <div class="form-group">
                  <label for="gameName" class="control-label">Game Name</label>
                  <ValidationProvider v-slot="{ errors }" rules="required">
                    <input id="gameName" v-model="masterGame.gameName" name="Game Name" type="text" class="form-control input" />
                    <span class="text-danger">{{ errors[0] }}</span>
                  </ValidationProvider>
                </div>

                <div class="form-group">
                  <label for="releaseDate" class="control-label">Release Date</label>
                  <b-form-datepicker v-model="masterGame.releaseDate" class="form-control"></b-form-datepicker>
                </div>

                <b-button variant="info" size="sm" @click="propagateDate">Propagate Date</b-button>
                <b-button variant="info" size="sm" @click="parseEstimatedReleaseDate">Parse Estimated</b-button>
                <b-button variant="warning" size="sm" @click="clearDates">Clear Dates</b-button>

                <div class="form-group">
                  <label for="estimatedReleaseDate" class="control-label">Estimated Release Date</label>
                  <input id="estimatedReleaseDate" v-model="masterGame.estimatedReleaseDate" name="estimatedReleaseDate" class="form-control input" />
                </div>
                <div class="form-group">
                  <label for="minimumReleaseDate" class="control-label">Minimum Release Date</label>
                  <b-form-datepicker v-model="masterGame.minimumReleaseDate" class="form-control"></b-form-datepicker>
                </div>
                <div class="form-group">
                  <label for="maximumReleaseDate" class="control-label">Maximum Release Date</label>
                  <b-form-datepicker v-model="masterGame.maximumReleaseDate" class="form-control"></b-form-datepicker>
                </div>
                <div class="form-group">
                  <label for="earlyAccessReleaseDate" class="control-label">Early Access Release Date</label>
                  <b-form-datepicker v-model="masterGame.earlyAccessReleaseDate" class="form-control"></b-form-datepicker>
                </div>
                <div class="form-group">
                  <label for="internationalReleaseDate" class="control-label">International Release Date</label>
                  <b-form-datepicker v-model="masterGame.internationalReleaseDate" class="form-control"></b-form-datepicker>
                </div>
                <div class="form-group">
                  <label for="announcementDate" class="control-label">Announcement Date</label>
                  <b-form-datepicker v-model="masterGame.announcementDate" class="form-control"></b-form-datepicker>
                </div>

                <div class="form-group">
                  <label for="openCriticID" class="control-label">Open Critic ID</label>
                  <input id="openCriticID" v-model="masterGame.openCriticID" name="openCriticID" class="form-control input" />
                </div>

                <div class="form-group">
                  <label for="ggToken" class="control-label">GG Token</label>
                  <input id="ggToken" v-model="masterGame.ggToken" name="ggToken" class="form-control input" />
                </div>

                <h3>Tags</h3>
                <masterGameTagSelector v-model="tags" include-system></masterGameTagSelector>

                <div class="form-group">
                  <label for="notes" class="control-label">Other Notes</label>
                  <input id="notes" v-model="masterGame.notes" name="notes" class="form-control input" />
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
                  <input id="boxartFileName" v-model="masterGame.boxartFileName" name="boxartFileName" class="form-control input" />
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
  components: {
    MasterGameTagSelector
  },
  props: {
    mastergameid: { type: String, required: true }
  },
  data() {
    return {
      errorInfo: null,
      masterGame: null,
      changeRequest: null,
      tags: [],
      responseInfo: null,
      generatedSQL: null
    };
  },
  watch: {
    $route() {
      this.fetchMasterGame();
    }
  },
  async mounted() {
    await this.fetchMasterGame();
    await this.fetchChangeRequest();
    this.populateTags();
  },
  methods: {
    async fetchMasterGame() {
      try {
        const response = await axios.get('/api/game/MasterGame/' + this.mastergameid);
        this.masterGame = response.data;
        if (this.masterGame.maximumReleaseDate === '9999-12-31') {
          this.masterGame.maximumReleaseDate = null;
        }
      } catch (error) {
        this.errorInfo = error;
      }
    },
    async fetchChangeRequest() {
      let changeRequestID = this.$route.query.changeRequestID;
      if (!changeRequestID) {
        return;
      }

      try {
        const response = await axios.get('/api/factChecker/GetMasterGameChangeRequest?changeRequestID=' + changeRequestID);
        this.changeRequest = response.data;
      } catch (error) {
        this.errorInfo = error;
      }
    },
    async parseEstimatedReleaseDate() {
      if (this.masterGame.releaseDate || !this.masterGame.estimatedReleaseDate) {
        return;
      }
      try {
        const response = await axios.get('/api/factChecker/ParseEstimatedDate?estimatedReleaseDate=' + this.masterGame.estimatedReleaseDate);
        this.masterGame.minimumReleaseDate = response.data.minimumReleaseDate;
        this.masterGame.maximumReleaseDate = response.data.maximumReleaseDate;
      } catch (error) {
        this.errorInfo = error;
      }
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
    async sendEditMasterGameRequest() {
      let tagNames = _.map(this.tags, 'name');

      let request = this.masterGame;
      request.tags = tagNames;

      try {
        const response = await axios.post('/api/factChecker/EditMasterGame', request);
        this.responseInfo = response.data;
        window.scroll({
          top: 0,
          left: 0,
          behavior: 'smooth'
        });
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    generateSQL() {
      this.generatedSQL = "select * from tbl_mastergame where MasterGameID = '" + this.masterGame.masterGameID + "';";
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
