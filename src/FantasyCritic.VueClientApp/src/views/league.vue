<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="forbidden">
      <div class="alert alert-danger" role="alert">You do not have permission to view this league.</div>
    </div>
    <div v-if="hasError" class="alert alert-danger" role="alert">
      Something went wrong with this league. Contact us on Discord or via email for support. Please include the league ID in your message (Linking the URL will do).
    </div>
    <div v-if="errorInfo" class="alert alert-danger" role="alert">{{ errorInfo }}</div>
    <div v-if="league && leagueYear">
      <div class="row">
        <div class="league-header-row">
          <div class="league-header-flex">
            <div class="league-name">
              <h1>
                {{ leagueYear.settings.leagueYearName || league.leagueName }}
                <font-awesome-layers v-if="oneShotMode" v-b-popover.hover.focus.right="oneShotText">
                  <font-awesome-icon icon="square" :style="{ color: '#d6993a' }" />
                  <font-awesome-icon icon="1" size="xs" :style="{ color: 'white' }" />
                </font-awesome-layers>
                <span @click="copyLeagueID">
                  <font-awesome-icon v-b-popover.hover.focus="'Copy League ID to Clipboard'" :icon="['far', 'copy']" size="xs" class="fake-link" />
                </span>
              </h1>
              <h4 v-if="leagueYear.settings.leagueYearName" class="league-year-name">{{ league.leagueName }}</h4>
              <div class="league-extra-links">
                <router-link v-if="league.years.length > 2" :to="{ name: 'leagueAllTimeStats', params: { leagueid: league.leagueID } }">All-Time League Stats</router-link>
                <router-link v-if="royaleGroupLink" :to="royaleGroupLink">View Royale Group</router-link>
                <a v-else-if="royaleGroupData && !royaleGroupData.hasRoyaleGroup && isManager" href="#" @click.prevent="enableRoyale">Enable Royale</a>
              </div>
            </div>

            <div class="selector-area">
              <div v-if="!league.userIsInLeague && isAuth">
                <b-button v-if="!league.userIsFollowingLeague" variant="primary" @click="followLeague">Follow</b-button>
                <b-button v-if="league.userIsFollowingLeague" variant="secondary" @click="unfollowLeague">Unfollow</b-button>
              </div>
              <b-form-select v-model="selectedYear" :options="league.years" class="year-selector" @update:model-value="changeLeagueYear" />
            </div>
          </div>
        </div>
      </div>
      <div v-if="league.conferenceID">
        Part of Conference:
        <router-link :to="{ name: 'conference', params: { conferenceid: league.conferenceID, year: year } }" class="conference-link">{{ league.conferenceName }}</router-link>
      </div>
      <hr />

      <div class="league-manager-info">
        <h4>League Manager:</h4>
        <span class="league-manager-info-item">{{ league.leagueManager.displayName }}</span>
      </div>

      <div>
        <label>Followers:</label>
        {{ league.numberOfFollowers }}
      </div>

      <b-alert v-if="mostRecentManagerMessage" :model-value="true" dismissible @close="dismissRecentManagerMessage">
        <h5>Manager's Message ({{ formatDateTime(mostRecentManagerMessage.timestamp) }})</h5>
        <div class="preserve-whitespace">{{ mostRecentManagerMessage.messageText }}</div>
      </b-alert>

      <div v-if="!league.publicLeague && !(league.userIsInLeague || league.outstandingInvite || inviteCode)" class="alert alert-warning" role="info">You are viewing a private league.</div>

      <draftCompleteModal ref="draftCompleteModalRef"></draftCompleteModal>
      <startDraftModal @draftStarted="startDraft"></startDraftModal>

      <div v-if="inviteCode && !league.userIsInLeague && !playStarted" class="alert alert-secondary">
        You have been invited to join this league.
        <b-button v-if="isAuth" variant="primary" class="mx-2" @click="joinWithInviteLink()">Join League</b-button>
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

      <div v-if="league.outstandingInvite" class="alert alert-info">
        You have been invited to join this league. Do you wish to join?
        <div class="row">
          <div class="btn-toolbar">
            <b-button variant="primary" class="mx-2" @click="acceptInvite">Join</b-button>
            <b-button variant="secondary" class="mx-2" @click="declineInvite">Decline</b-button>
          </div>
        </div>
      </div>

      <b-alert v-if="leagueYear.userIsActive && hasProposedTrade" :model-value="true">
        Someone has proposed a trade with you.
        <b-button v-b-modal="'active-trades-modal'" variant="success">View Trades</b-button>
      </b-alert>
      <b-alert v-if="(leagueYear.userIsActive || isManager) && hasActiveTrade" :model-value="true">
        There are active trades under consideration.
        <b-button v-b-modal="'active-trades-modal'" variant="success">View Trades</b-button>
      </b-alert>
      <specialAuctionInfo
        v-for="activeSpecialAuction in leagueYear.activeSpecialAuctions"
        :key="activeSpecialAuction.masterGameYear.masterGameID"
        :special-auction="activeSpecialAuction"></specialAuctionInfo>
      <div v-if="playStarted && leagueYear.supportedYear.finished">
        <div v-if="leagueYear.underReview" class="alert alert-danger" role="alert">
          This league has been set to "under review", which allows the league manager to make changes after the year is over.
        </div>

        <div v-if="topPublisher" class="alert alert-success" role="alert">
          This year is finished! The winner is
          <strong>{{ topPublisher.publisherName }}</strong>
          !
        </div>
      </div>
      <div v-if="!leagueYear.userIsActive && league.userIsInLeague && !isManager">
        <div class="alert alert-warning" role="alert">You are set to inactive for this year.</div>
      </div>

      <draftReadinessBanner></draftReadinessBanner>

      <div v-if="leagueYear.userIsActive && !userPublisher" class="alert alert-info">
        <p>You need to create your publisher for this year.</p>
        <b-button v-b-modal="'create-publisher'" variant="primary" class="mx-2">Create Publisher</b-button>
        <createPublisherForm></createPublisherForm>
      </div>

      <div v-if="leagueYear.userIsActive && !userPublisher && isManager" class="alert alert-info">
        Alternatively, if you want to manage this league without playing in it, you will need to set yourself as "inactive" by going to "Manager Active Players" in the Manage League menu.
      </div>

      <div v-if="isManager && !playStarted && !leagueYear.userIsActive" class="alert alert-info">You are currently set to manage this league without playing in it.</div>

      <draftStatusHeader></draftStatusHeader>

      <div class="row">
        <div class="col-xl-3 col-lg-4 col-md-12">
          <leagueActions></leagueActions>
        </div>
        <div class="col-xl-9 col-lg-8 col-md-12">
          <leagueYearStandings></leagueYearStandings>
          <div v-if="postDraftPlayable">
            <gameNews v-if="gameNews" :game-news="gameNews" mode="league" />
            <br />
            <template v-if="biddingAllowed">
              <bidCountdowns v-if="showPublicRevealCountdown" mode="NextPublic" @publicBidRevealTimeElapsed="refreshLeagueYear"></bidCountdowns>
              <bidCountdowns v-else mode="NextBid"></bidCountdowns>
              <div v-if="leagueYear.publicBiddingGames">
                <h2>This week's bids</h2>
                <activeBids />
              </div>
            </template>
          </div>
          <br />
          <leagueGameSummary></leagueGameSummary>
        </div>
      </div>
    </div>
    <audio id="draft-notification-sound" src="/sounds/draft-notification.mp3"></audio>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import { RouteLocationRaw } from 'vue-router';
import * as signalR from '@microsoft/signalr';

import { leagueClient, leagueManagerClient, royaleGroupClient } from '@/api/clients';
import type {
  AcceptInviteRequest,
  CreateLeagueTiedRoyaleGroupRequest,
  DeleteInviteRequest,
  DismissManagerMessageRequest,
  FollowLeagueRequest,
  JoinWithInviteLinkRequest,
  ManagerMessageViewModel,
  RoyaleGroupStatusViewModel,
  StartDraftRequest
} from '@/api/clients';
import { useAuthStore } from '@/store/authStore';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import { LeaguePageParams, useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatDateTime } from '@/utilities/formatters';
import { makeToast } from '@/utilities/toast';
import ActiveBids from '@/components/activeBids.vue';
import BidCountdowns from '@/components/bidCountdowns.vue';
import DraftReadinessBanner from '@/components/draftReadinessBanner.vue';
import DraftStatusHeader from '@/components/draftStatusHeader.vue';
import GameNews from '@/components/gameNews.vue';
import LeagueActions from '@/components/leagueActions.vue';
import LeagueGameSummary from '@/components/leagueGameSummary.vue';
import LeagueYearStandings from '@/components/leagueYearStandings.vue';
import CreatePublisherForm from '@/components/modals/createPublisherForm.vue';
import DraftCompleteModal from '@/components/modals/draftCompleteModal.vue';
import StartDraftModal from '@/components/modals/startDraftModal.vue';
import SpecialAuctionInfo from '@/components/specialAuctionInfo.vue';

interface LeagueData {
  selectedYear: number | null;
  errorInfo: string;
  royaleGroupData: RoyaleGroupStatusViewModel | null;
}

export default defineComponent({
  setup() {
    const oneShotText = "This is a 'one shot' league, meaning there are no bids or drops. The draft is final.";
    return { oneShotText };
  },
  components: {
    LeagueGameSummary,
    LeagueYearStandings,
    CreatePublisherForm,
    StartDraftModal,
    DraftCompleteModal,
    LeagueActions,
    GameNews,
    ActiveBids,
    BidCountdowns,
    SpecialAuctionInfo,
    DraftReadinessBanner,
    DraftStatusHeader
  },
  props: {
    leagueid: { type: String, required: true },
    year: { type: Number, required: true }
  },
  data(): LeagueData {
    return {
      selectedYear: null,
      errorInfo: '',
      royaleGroupData: null
    };
  },
  computed: {
    ...mapState(useAuthStore, ['isAuth']),
    ...mapState(useInterLeagueStore, ['activeRoyaleQuarter']),
    ...mapState(useLeagueStore, [
      'hasError',
      'forbidden',
      'inviteCode',
      'leagueYear',
      'requiredLeagueYear',
      'requiredLeague',
      'userPublisher',
      'isManager',
      'league',
      'pendingDraft',
      'activeDraft',
      'userIsNextInDraft',
      'topPublisher',
      'gameNews',
      'oneShotMode',
      'playStarted',
      'biddingAllowed',
      'postDraftPlayable'
    ]),
    mostRecentManagerMessage(): ManagerMessageViewModel | null {
      if (!this.leagueYear || !this.leagueYear.managerMessages || this.leagueYear.managerMessages.length === 0) {
        return null;
      }

      let mostRecentMessage = this.leagueYear.managerMessages[this.leagueYear.managerMessages.length - 1];
      if (mostRecentMessage.isDismissed) {
        return null;
      }

      return mostRecentMessage;
    },
    showPublicRevealCountdown(): boolean {
      const publicBiddingSystems = ['SemiPublicBidding', 'SemiPublicBiddingSecretCounterPicks', 'SemiPublicBiddingSemiPublicCounterPicks'];
      if (!this.leagueYear || !publicBiddingSystems.includes(this.leagueYear.settings.pickupSystem)) {
        return false;
      }

      const interLeagueStore = useInterLeagueStore();
      const revealIsNext = interLeagueStore.bidTimes.nextPublicBiddingTime < interLeagueStore.bidTimes.nextBidLockTime;
      return revealIsNext;
    },
    hasActiveTrade(): boolean {
      if (this.requiredLeagueYear.userIsActive) {
        return this.requiredLeagueYear.activeTrades.some((x) => x.counterPartyPublisherID !== this.userPublisher?.publisherID);
      } else if (this.isManager) {
        return this.requiredLeagueYear.activeTrades.length > 0;
      }
      return false;
    },
    hasProposedTrade(): boolean {
      if (this.requiredLeagueYear.userIsActive) {
        return this.requiredLeagueYear.activeTrades.some((x) => x.counterPartyPublisherID === this.userPublisher?.publisherID);
      } else if (this.isManager) {
        return this.requiredLeagueYear.activeTrades.length > 0;
      }
      return false;
    },
    mustSetDraftOrder(): boolean {
      return (this.pendingDraft?.readyToSetDraftOrder ?? false) && (this.pendingDraft?.startDraftErrors ?? []).includes('You must set the draft order.');
    },
    royaleGroupLink(): RouteLocationRaw | null {
      if (!this.royaleGroupData?.royaleGroup) return null;
      const group = this.royaleGroupData.royaleGroup;
      if (this.activeRoyaleQuarter) {
        return { name: 'royaleGroupQuarter', params: { groupid: group.groupID, year: this.activeRoyaleQuarter.year, quarter: this.activeRoyaleQuarter.quarter } };
      }
      return { name: 'royaleGroup', params: { groupid: group.groupID } };
    }
  },
  watch: {
    async $route(to, from) {
      if (to.path !== from.path) {
        await this.initializePage();
      }
    },
    userIsNextInDraft(val, oldVal) {
      if (val && val !== oldVal) {
        // play() rejects when the browser blocks autoplay; the sound is only a nicety.
        (document.getElementById('draft-notification-sound') as HTMLAudioElement | null)?.play().catch(() => {});
      }
    }
  },
  async created() {
    await this.initializePage();
  },
  methods: {
    formatDateTime,
    ...mapActions(useLeagueStore, ['refreshLeagueYear']),
    async initializePage(): Promise<void> {
      this.selectedYear = this.year;
      let inviteCode = undefined;
      if (typeof this.$route.query.inviteCode === 'string') {
        inviteCode = this.$route.query.inviteCode;
      }
      const leaguePageParams: LeaguePageParams = { leagueID: this.leagueid, year: this.year, inviteCode };
      const leagueStore = useLeagueStore();
      await leagueStore.initializeLeaguePage(leaguePageParams);
      await this.startHubConnection();
      this.fetchRoyaleGroupForLeague();
    },
    changeLeagueYear(newVal: number | number[] | null): void {
      if (typeof newVal !== 'number') {
        throw new Error('Invalid year chosen.');
      }
      this.$router.push({ name: 'league', params: { leagueid: this.leagueid, year: newVal } });
    },
    async acceptInvite(): Promise<void> {
      const request: AcceptInviteRequest = {
        leagueID: this.requiredLeagueYear.leagueID
      };
      try {
        await leagueClient.acceptInvite(request);
        await this.refreshLeagueYear();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to accept invite');
      }
    },
    async declineInvite(): Promise<void> {
      const invite = this.requiredLeague.outstandingInvite;
      if (!invite) {
        return;
      }
      const request: DeleteInviteRequest = {
        leagueID: invite.leagueID,
        inviteID: invite.inviteID
      };
      try {
        await leagueClient.declineInvite(request);
        this.$router.push({ name: 'home' });
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to decline invite');
      }
    },
    async joinWithInviteLink(): Promise<void> {
      if (!this.inviteCode) {
        return;
      }
      const request: JoinWithInviteLinkRequest = {
        leagueID: this.requiredLeagueYear.leagueID,
        inviteCode: this.inviteCode
      };
      try {
        await leagueClient.joinWithInviteLink(request);
        await this.refreshLeagueYear();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to join league.');
      }
    },
    async startDraft(): Promise<void> {
      const request: StartDraftRequest = {
        leagueID: this.requiredLeagueYear.leagueID,
        year: this.requiredLeagueYear.year
      };
      try {
        await leagueManagerClient.startDraft(request);
        await this.refreshLeagueYear();
        await this.startHubConnection();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to start draft.');
      }
    },
    async followLeague(): Promise<void> {
      const model: FollowLeagueRequest = {
        leagueID: this.requiredLeagueYear.leagueID
      };
      try {
        await leagueClient.followLeague(model);
        await this.refreshLeagueYear();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to follow league.');
      }
    },
    async unfollowLeague(): Promise<void> {
      const model: FollowLeagueRequest = {
        leagueID: this.requiredLeagueYear.leagueID
      };
      try {
        await leagueClient.unfollowLeague(model);
        await this.refreshLeagueYear();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to unfollow league.');
      }
    },
    async dismissRecentManagerMessage(): Promise<void> {
      if (!this.mostRecentManagerMessage) {
        return;
      }
      const model: DismissManagerMessageRequest = {
        messageID: this.mostRecentManagerMessage.messageID
      };

      try {
        await leagueClient.dismissManagerMessage(model);
        await this.refreshLeagueYear();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to dismiss manager message.');
      }
    },
    async startHubConnection(): Promise<void> {
      if (!this.leagueYear || !this.activeDraft) {
        return;
      }

      console.log('Connecting SignalR');
      let hubConnection = new signalR.HubConnectionBuilder().withUrl('/updatehub').withAutomaticReconnect().configureLogging(signalR.LogLevel.Error).build();

      await hubConnection.start().catch((err) => console.error(err.toString()));

      hubConnection.invoke('Subscribe', this.leagueid, this.year).catch((err) => console.error(err.toString()));

      hubConnection.on('RefreshLeagueYear', async () => {
        await this.refreshLeagueYear();
      });

      hubConnection.onreconnecting(() => {
        console.log('Reconnecting SignalR');
      });

      hubConnection.on('DraftFinished', () => {
        (this.$refs.draftCompleteModalRef as InstanceType<typeof DraftCompleteModal> | undefined)?.show();
      });

      hubConnection.onclose(async () => {
        await this.startHubConnection();
      });
    },
    async copyLeagueID(): Promise<void> {
      await navigator.clipboard.writeText(this.leagueid);
      makeToast('League ID copied to clipboard.');
    },
    async fetchRoyaleGroupForLeague(): Promise<void> {
      try {
        this.royaleGroupData = await royaleGroupClient.getRoyaleGroupForLeague(this.leagueid);
      } catch {
        this.royaleGroupData = null;
      }
    },
    async enableRoyale(): Promise<void> {
      try {
        const request: CreateLeagueTiedRoyaleGroupRequest = {
          groupName: this.requiredLeague.leagueName + ' Royale',
          leagueID: this.leagueid
        };
        const response = await royaleGroupClient.createLeagueTiedRoyaleGroup(request);
        this.$router.push({ name: 'royaleGroup', params: { groupid: response.groupID } });
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to enable Royale for this league.');
      }
    }
  }
});
</script>

<style scoped>
.league-manager-info {
  display: flex;
  flex-direction: row;
}
.league-manager-info-item {
  padding-left: 5px;
  padding-top: 3px;
}

.league-header-row {
  width: 100%;
}

.league-header-flex {
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

.league-name {
  display: block;
  max-width: 100%;
  word-wrap: break-word;
}

.league-year-name {
  margin-top: -5px;
  margin-bottom: 5px;
}

.league-extra-links {
  display: flex;
  gap: 15px;
  margin-top: 2px;
}
</style>
