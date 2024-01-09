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

        <div v-if="conferenceInviteCode && !conference.userIsInConference" class="alert alert-secondary">
          You have been invited to join this conference.
          <b-button v-if="isAuth" variant="primary" class="mx-2" @click="joinWithInviteLink()">Join Conference</b-button>
          <template v-else>
            <b-button variant="info" href="/Account/Login">
              <span>Log In</span>
              <font-awesome-icon class="topnav-button-icon" icon="sign-in-alt" />
            </b-button>
            <b-button variant="primary" href="/Account/Register">
              <span>Sign Up</span>
              <font-awesome-icon class="topnav-button-icon" icon="user-plus" />
            </b-button>
          </template>
        </div>

        <div v-if="conference.userIsInConference && !conferenceYear.openForDrafting && !conferenceYear.userIsInAtLeastOneLeague" class="alert alert-warning">
          You have joined the conference, but your conference manager has not yet assigned you to any of the leagues in the conference.
        </div>

        <template v-if="isConferenceManager && !conferenceYear.openForDrafting">
          <div class="alert alert-info">
            Now that your conference is created, here are the next steps:
            <ol>
              <li>Invite more players to your conference.</li>
              <li>Create additional leagues based off of the primary league. You will need to choose a league manager for each league.</li>
              <li>Assign all players in the conference to a league.</li>
              <li>Enable drafting.</li>
            </ol>
            All of these actions can be accessed in the "Manager Actions" dropdown below.
          </div>

          <div class="alert alert-info">
            <p v-if="numberOfPlayersInConferenceNotInAnyLeague > 1">
              Your conference currently contains {{ numberOfPlayersInConference }} players, {{ numberOfPlayersInConferenceNotInAnyLeague }} of which have not been assigned to a league.
            </p>
            <p v-if="numberOfPlayersInConferenceNotInAnyLeague === 1">
              Your conference currently contains {{ numberOfPlayersInConference }} players, {{ numberOfPlayersInConferenceNotInAnyLeague }} of which has not been assigned to a league.
            </p>
            <p v-else>Your conference currently contains {{ numberOfPlayersInConference }} players, all of which have been assigned to a league.</p>
            <p>
              It is recommended that you get your entire conference set up in terms of players before you enable drafting, but if you are ready now, click the button below to allow each league to
              begin drafting. Each league manager still choose when to actually start the draft, but they cannot do so until you enable drafting at the conference level.
            </p>
            <b-button variant="success" @click="editDraftStatusForConferenceYear(true)">Enable Drafting</b-button>
          </div>
        </template>

        <template v-if="conferenceYear.openForDrafting && numberOfLeaguesFinishedDrafting !== conferenceYear.leagueYears.length">
          <div v-if="isConferenceManager && conferenceYear.openForDrafting" class="alert alert-success">
            Your conference is ready to begin drafting! League managers can start drafts just like normal leagues.
          </div>

          <div class="alert alert-success">
            There are currently {{ numberOfLeaguesStartedDrafting }} leagues drafting, and {{ numberOfLeaguesNotStartedDrafting }} that still need to start their drafts.
          </div>
        </template>

        <div class="conference-manager-section">
          <div class="conference-manager-info">
            <h4>Conference Manager:</h4>
            <span class="conference-manager-info-item">{{ conference.conferenceManager.displayName }}</span>
          </div>
          <ConferenceManagerActions v-if="isConferenceManager"></ConferenceManagerActions>
        </div>

        <b-table :items="conferenceYear.leagueYears" :fields="leagueYearFields" bordered small responsive striped>
          <template #cell(leagueName)="data">
            <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: year } }" class="league-link">
              {{ data.item.leagueName }}
            </router-link>
            <font-awesome-icon v-if="data.item.isPrimaryLeague" icon="chess-king" v-b-popover.hover="'This is the primary league, from which all other leagues share settings.'" />
            <font-awesome-icon v-if="data.item.userIsInLeague" icon="user" v-b-popover.hover="'You are in this league.'" />
            <font-awesome-icon v-if="data.item.draftStarted && !data.item.draftFinished" icon="list-ol" v-b-popover.hover="'This league is currently drafting.'" />
          </template>
          <template #cell(leagueManager)="data">{{ data.item.leagueManager.displayName }}</template>
        </b-table>

        <conferenceYearStandings></conferenceYearStandings>
      </div>
    </div>
  </div>
</template>
<script>
import axios from 'axios';

import ConferenceMixin from '@/mixins/conferenceMixin.js';
import ConferenceManagerActions from '@/components/conferenceManagerActions.vue';
import ConferenceYearStandings from '@/components/conferenceYearStandings.vue';

export default {
  components: {
    ConferenceManagerActions,
    ConferenceYearStandings
  },
  mixins: [ConferenceMixin],
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
    numberOfPlayersInConference() {
      return this.conference.players.length;
    },
    numberOfPlayersInConferenceNotInAnyLeague() {
      return this.conference.players.filter((x) => x.leaguesIn.length === 0).length;
    },
    numberOfLeaguesNotStartedDrafting() {
      return this.conferenceYear.leagueYears.filter((x) => !x.draftStarted).length;
    },
    numberOfLeaguesStartedDrafting() {
      return this.conferenceYear.leagueYears.filter((x) => x.draftStarted & !x.draftFinished).length;
    },
    numberOfLeaguesFinishedDrafting() {
      return this.conferenceYear.leagueYears.filter((x) => x.draftFinished).length;
    }
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
      const inviteCode = this.$route.query.inviteCode;
      const conferencePageParams = { conferenceID: this.conferenceid, year: this.year, inviteCode };
      await this.$store.dispatch('initializeConferencePage', conferencePageParams);
    },
    changeConferenceYear(newVal) {
      const parameters = {
        conferenceid: this.conferenceid,
        year: newVal
      };
      this.$router.push({ name: 'conference', params: parameters });
    },
    async joinWithInviteLink() {
      const model = {
        conferenceID: this.conference.conferenceID,
        inviteCode: this.conferenceInviteCode
      };

      try {
        await axios.post('/api/conference/JoinWithInviteLink', model);
        await this.refreshConferenceYear();
      } catch (error) {
        this.errorInfo = 'Something went wrong joining the conference.';
      }
    },
    async editDraftStatusForConferenceYear(enable) {
      const model = {
        conferenceID: this.conference.conferenceID,
        year: this.conferenceYear.year,
        openForDrafting: enable
      };
      try {
        await axios.post('/api/conference/EditDraftStatusForConferenceYear', model);
        await this.notifyAction('Draft Status updated for conference.');
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    }
  }
};
</script>
<style scoped>
.conference-manager-section {
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  margin-bottom: 10px;
}

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
