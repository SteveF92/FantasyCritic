<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="hasError" class="alert alert-danger" role="alert">
      Something went wrong with this conference. Contact us on Discord or via email for support. Please include the conference ID in your message (Linking the URL will do).
    </div>

    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div v-if="conferenceYear && conference">
      <div v-if="isConferenceManager && !isPlusUser" class="alert alert-danger" role="alert">
        Your Fantasy Critic Plus membership has expired. In order to continue acting as conference manager, you must resubscribe to Fantasy Critic Plus. The only conference action you can take while
        not a Fantasy Critic Plus member is to pass the conference manager role to another player in your conference.
      </div>
      <div class="row">
        <div class="conference-header-row">
          <div class="conference-header-flex">
            <div class="conference-name">
              <h1>
                {{ conference.conferenceName }}
                <span @click="copyConferenceID">
                  <font-awesome-icon v-b-popover.hover.focus="'Copy Conference ID to Clipboard'" :icon="['far', 'copy']" size="xs" class="fake-link" />
                </span>
              </h1>
              <div class="conference-extra-links">
                <router-link v-if="conferenceYear.managerMessages.length > 0" :to="{ name: 'conferenceHistory', params: { conferenceid: conference.conferenceID, year: conferenceYear.year } }">
                  Conference History
                </router-link>
                <router-link v-if="royaleGroupLink" :to="royaleGroupLink">View Royale Group</router-link>
                <a v-else-if="royaleGroupData && !royaleGroupData.hasRoyaleGroup && isConferenceManager" href="#" @click.prevent="enableRoyale">Enable Royale</a>
              </div>
            </div>

            <div class="selector-area">
              <b-form-select v-model="selectedYear" :options="conference.years" class="year-selector" @update:model-value="changeConferenceYear" />
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

      <div v-if="conference.userIsInConference && !conferenceYear.userIsInAtLeastOneLeague" class="alert alert-warning">
        You have joined the conference, but your conference manager has not yet assigned you to any of the leagues in the conference.
      </div>

      <template v-if="isConferenceManager && numberOfLeaguesFinishedDrafting === 0">
        <div class="alert alert-info">
          Now that your conference is created, here are the next steps:
          <ol>
            <li>Invite more players to your conference.</li>
            <li>Create additional leagues based off of the primary league. You will need to choose a league manager for each league.</li>
            <li>Assign all players to leagues.</li>
            <li>Once a given league has all of the players that you want, you can "lock" the league, which allows those players to start their draft.</li>
          </ol>
          All of these actions can be accessed in the "Manager Actions" dropdown below.
          <br />
          <br />
          Alternatively, you can also invite players using the standard league-level invite links. That will allow players to join that specific league inside the conference. You will still need to
          "lock" the league via the "Assign Players to League" Conference Manager action in order to enable drafting for that league.
        </div>

        <div class="alert alert-info">
          <template v-if="numberOfPlayersInConferenceNotInAnyLeague > 1">
            Your conference currently contains {{ numberOfPlayersActiveForYear }} active players, {{ numberOfPlayersInConferenceNotInAnyLeague }} of which have not been assigned to a league.
          </template>
          <template v-if="numberOfPlayersInConferenceNotInAnyLeague === 1">
            Your conference currently contains {{ numberOfPlayersActiveForYear }} active players, {{ numberOfPlayersInConferenceNotInAnyLeague }} of which has not been assigned to a league.
          </template>
          <template v-if="numberOfPlayersInConferenceNotInAnyLeague === 0">
            Your conference currently contains {{ numberOfPlayersActiveForYear }} active players, all of which have been assigned to a league.
          </template>
        </div>
      </template>

      <template v-if="numberOfLeaguesFinishedDrafting > 0 && numberOfLeaguesFinishedDrafting !== conferenceYear.leagueYears.length">
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

      <b-alert v-if="mostRecentManagerMessage" :model-value="true" dismissible @close="dismissRecentManagerMessage">
        <h5>Manager's Message ({{ formatDateTime(mostRecentManagerMessage.timestamp) }})</h5>
        <div class="preserve-whitespace">{{ mostRecentManagerMessage.messageText }}</div>
      </b-alert>

      <b-table :items="sortedLeagueYears" :fields="leagueYearFields" bordered small responsive striped>
        <template #cell(leagueName)="data">
          <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: year } }" class="league-link">
            {{ data.item.leagueName }}
          </router-link>
          {{ ' ' }}
          <font-awesome-icon v-if="data.item.isPrimaryLeague" icon="chess-king" v-b-popover.hover="'This is the primary league, from which all other leagues share settings.'" />
          <font-awesome-icon v-if="data.item.userIsInLeague" icon="user" v-b-popover.hover="'You are in this league.'" />
          <font-awesome-icon
            v-if="data.item.activeDraftNumber !== null && data.item.activeDraftNumber !== undefined"
            icon="list-ol"
            v-b-popover.hover="data.item.activeDraftNumber > 1 ? `This league is currently drafting (Draft ${data.item.activeDraftNumber}).` : 'This league is currently drafting.'" />
        </template>
        <template #cell(leagueManager)="data">{{ data.item.leagueManager.displayName }}</template>
      </b-table>

      <conferenceYearStandings></conferenceYearStandings>
    </div>
  </div>
</template>
<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { RouteLocationRaw } from 'vue-router';
import type { TableField } from 'bootstrap-vue-next';

import { conferenceClient, royaleGroupClient } from '@/api/clients';
import type {
  ConferenceLeagueYearViewModel,
  CreateConferenceTiedRoyaleGroupRequest,
  DismissConferenceManagerMessageRequest,
  JoinConferenceWithInviteLinkRequest,
  ManagerMessageViewModel,
  RoyaleGroupStatusViewModel
} from '@/api/clients';
import { useAuthStore } from '@/store/authStore';
import { useConferenceStore, type ConferencePageParams } from '@/store/conferenceStore';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatDateTime } from '@/utilities/formatters';
import { makeToast } from '@/utilities/toast';
import ConferenceManagerActions from '@/components/conferenceManagerActions.vue';
import ConferenceYearStandings from '@/components/conferenceYearStandings.vue';

interface ConferenceData {
  selectedYear: number | null;
  errorInfo: string;
  royaleGroupData: RoyaleGroupStatusViewModel | null;
}

export default defineComponent({
  components: {
    ConferenceManagerActions,
    ConferenceYearStandings
  },
  setup() {
    const leagueYearFields: TableField<ConferenceLeagueYearViewModel>[] = [
      { key: 'leagueName', label: 'League', thClass: 'bg-primary' },
      { key: 'leagueManager', label: 'League Manager', thClass: 'bg-primary' }
    ];
    return { leagueYearFields };
  },
  props: {
    conferenceid: { type: String, required: true },
    year: { type: Number, required: true }
  },
  data(): ConferenceData {
    return {
      selectedYear: null,
      errorInfo: '',
      royaleGroupData: null
    };
  },
  computed: {
    ...mapState(useAuthStore, ['isPlusUser', 'isAuth']),
    ...mapState(useInterLeagueStore, ['activeRoyaleQuarter']),
    ...mapState(useConferenceStore, ['conference', 'conferenceYear', 'requiredConferenceYear', 'hasError', 'conferenceInviteCode', 'isConferenceManager']),
    numberOfPlayersActiveForYear(): number {
      return this.requiredConferenceYear.playersForYear.length;
    },
    numberOfPlayersInConferenceNotInAnyLeague(): number {
      return this.requiredConferenceYear.playersForYear.filter((x) => x.leaguesActiveIn.length === 0).length;
    },
    numberOfLeaguesNotStartedDrafting(): number {
      return this.requiredConferenceYear.leagueYears.filter((x) => !x.draftStarted).length;
    },
    numberOfLeaguesStartedDrafting(): number {
      return this.requiredConferenceYear.leagueYears.filter((x) => x.draftStarted && !x.draftFinished).length;
    },
    numberOfLeaguesFinishedDrafting(): number {
      return this.requiredConferenceYear.leagueYears.filter((x) => x.draftFinished).length;
    },
    sortedLeagueYears(): ConferenceLeagueYearViewModel[] {
      if (!this.conferenceYear) {
        return [];
      }

      return [...this.conferenceYear.leagueYears].sort((a, b) => {
        if (a.isPrimaryLeague) {
          return -1;
        }
        if (b.isPrimaryLeague) {
          return 1;
        }

        return a.leagueName.localeCompare(b.leagueName);
      });
    },
    royaleGroupLink(): RouteLocationRaw | null {
      if (!this.royaleGroupData?.royaleGroup) return null;
      const group = this.royaleGroupData.royaleGroup;
      if (this.activeRoyaleQuarter) {
        return { name: 'royaleGroupQuarter', params: { groupid: group.groupID, year: this.activeRoyaleQuarter.year, quarter: this.activeRoyaleQuarter.quarter } };
      }
      return { name: 'royaleGroup', params: { groupid: group.groupID } };
    },
    mostRecentManagerMessage(): ManagerMessageViewModel | null {
      if (!this.conferenceYear || this.conferenceYear.managerMessages.length === 0) {
        return null;
      }

      const mostRecentMessage = this.conferenceYear.managerMessages[this.conferenceYear.managerMessages.length - 1];
      if (mostRecentMessage.isDismissed) {
        return null;
      }

      return mostRecentMessage;
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
    ...mapActions(useConferenceStore, ['refreshConferenceYear']),
    formatDateTime,
    async initializePage(): Promise<void> {
      this.selectedYear = this.year;
      this.errorInfo = '';
      this.royaleGroupData = null;
      let inviteCode = undefined;
      if (typeof this.$route.query.inviteCode === 'string') {
        inviteCode = this.$route.query.inviteCode;
      }
      const conferencePageParams: ConferencePageParams = { conferenceID: this.conferenceid, year: this.year, inviteCode };
      const conferenceStore = useConferenceStore();
      await conferenceStore.initializeConferencePage(conferencePageParams);
      this.fetchRoyaleGroupForConference();
    },
    changeConferenceYear(newVal: number | number[] | null): void {
      if (typeof newVal !== 'number') {
        throw new Error('Invalid year chosen.');
      }
      this.$router.push({ name: 'conference', params: { conferenceid: this.conferenceid, year: newVal } });
    },
    async joinWithInviteLink(): Promise<void> {
      if (!this.conferenceInviteCode) {
        return;
      }
      const request: JoinConferenceWithInviteLinkRequest = {
        conferenceID: this.conferenceid,
        inviteCode: this.conferenceInviteCode
      };

      this.errorInfo = '';
      try {
        await conferenceClient.joinWithInviteLink(request);
        await this.refreshConferenceYear();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong joining the conference.');
      }
    },
    async copyConferenceID(): Promise<void> {
      await navigator.clipboard.writeText(this.conferenceid);
      makeToast('Conference ID copied to clipboard.');
    },
    async dismissRecentManagerMessage(): Promise<void> {
      if (!this.mostRecentManagerMessage) {
        return;
      }
      const request: DismissConferenceManagerMessageRequest = {
        messageID: this.mostRecentManagerMessage.messageID
      };

      this.errorInfo = '';
      try {
        await conferenceClient.dismissManagerMessage(request);
        await this.refreshConferenceYear();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to dismiss manager message.');
      }
    },
    async fetchRoyaleGroupForConference(): Promise<void> {
      try {
        this.royaleGroupData = await royaleGroupClient.getRoyaleGroupForConference(this.conferenceid);
      } catch {
        this.royaleGroupData = null;
      }
    },
    async enableRoyale(): Promise<void> {
      if (!this.conference) {
        return;
      }
      const request: CreateConferenceTiedRoyaleGroupRequest = {
        groupName: this.conference.conferenceName + ' Royale',
        conferenceID: this.conferenceid
      };

      this.errorInfo = '';
      try {
        const response = await royaleGroupClient.createConferenceTiedRoyaleGroup(request);
        this.$router.push({ name: 'royaleGroup', params: { groupid: response.groupID } });
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to enable Royale for this conference.');
      }
    }
  }
});
</script>
<style scoped>
.conference-manager-section {
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
}

.conference-manager-info {
  display: flex;
  flex-direction: row;
}

.conference-manager-info-item {
  padding-left: 5px;
  padding-top: 3px;
}

.conference-extra-links {
  display: flex;
  gap: 15px;
  margin-top: 2px;
  margin-bottom: 10px;
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
