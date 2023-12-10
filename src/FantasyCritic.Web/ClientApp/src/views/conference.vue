<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div v-if="hasError" class="alert alert-danger" role="alert">
        Something went wrong with this conference. Contact us on Twitter or Discord for support. Please include the conference ID in your message (Linking the URL will do).
      </div>
      <div v-if="errorInfo" class="alert alert-danger" role="alert">{{ errorInfo }}</div>
      <div v-if="conferenceYear">
        <div class="row">
          <div class="conference-header-row">
            <div class="conference-header-flex">
              <div class="conference-name">
                <h1>
                  {{ conference.conferenceName }}
                </h1>
              </div>

              <div class="selector-area">
                <b-form-select v-model="selectedYear" :options="conference.years" class="year-selector" @change="changeConferenceYear" />
              </div>
            </div>
          </div>
        </div>

        <hr />

        <div class="conference-manager-info">
          <h4>Conference Manager:</h4>
          <span class="conference-manager-info-item">{{ conference.conferenceManager.displayName }}</span>
        </div>

        <b-table :items="conferenceYear.leagueYears" :fields="leagueYearFields" bordered small responsive striped>
          <template #cell(leagueName)="data">
            <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: year } }" class="league-link">{{ data.item.leagueName }}</router-link>
          </template>
          <template #cell(leagueManager)="data">{{ data.item.leagueManager.displayName }}</template>
        </b-table>
      </div>
    </div>
  </div>
</template>
<script>
import { mapGetters } from 'vuex';

export default {
  props: {
    conferenceid: { type: String, required: true },
    year: { type: Number, required: true }
  },
  data() {
    return {
      selectedYear: null,
      errorInfo: null,
      leagueYearFields: [
        { key: 'leagueName', label: 'League', thClass: 'bg-primary' },
        { key: 'leagueManager', label: 'League Manager', thClass: 'bg-primary' }
      ]
    };
  },
  computed: {
    ...mapGetters(['conference', 'conferenceYear', 'hasError'])
  },
  watch: {
    async $route(to, from) {
      if (to.path !== from.path) {
        await this.initializePage();
      }
    }
  },
  async created() {
    await this.initializePage();
  },
  methods: {
    async initializePage() {
      this.selectedYear = this.year;
      const conferencePageParams = { conferenceID: this.conferenceid, year: this.year };
      await this.$store.dispatch('initializeConferencePage', conferencePageParams);
    },
    changeConferenceYear(newVal) {
      var parameters = {
        conferenceid: this.conferenceid,
        year: newVal
      };
      this.$router.push({ name: 'conference', params: parameters });
    }
  }
};
</script>
<style scoped>
.conference-manager-info {
  display: flex;
  flex-direction: row;
}
.conference-manager-info-item {
  padding-left: 5px;
  padding-top: 3px;
}

.conference-header-row {
  width: 100%;
}

.conference-header-flex {
  display: flex;
  justify-content: space-between;
  flex-wrap: wrap;
}

.selector-area {
  display: flex;
  align-items: flex-start;
}

.year-selector {
  width: 100px;
}

.conference-name {
  display: block;
  max-width: 100%;
  word-wrap: break-word;
}
</style>
