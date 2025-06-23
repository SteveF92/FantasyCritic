<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="forbidden">
      <div class="alert alert-danger" role="alert">You do not have permission to view this league.</div>
    </div>
    <div v-if="hasError" class="alert alert-danger" role="alert">
      Something went wrong with this league. Contact us on Discord or via email for support. Please include the league ID in your message (Linking the URL will do).
    </div>
    <div v-if="errorInfo" class="alert alert-danger" role="alert">{{ errorInfo }}</div>
    <div v-if="leagueYear">
      <div class="row">
        <div class="league-header-row">
          <div class="league-header-flex">
            <div class="league-name">
              <h1>
                {{ league.leagueName }}
                <font-awesome-layers v-if="oneShotMode" v-b-popover.hover.focus.rightbottom="oneShotText">
                  <font-awesome-icon icon="square" :style="{ color: '#d6993a' }" />
                  <font-awesome-icon icon="1" size="xs" :style="{ color: 'white' }" />
                </font-awesome-layers>
                <span v-clipboard:copy="leagueid" v-clipboard:success="leagueIDCopied">
                  <font-awesome-icon v-b-popover.hover.focus="'Copy League ID to Clipboard'" :icon="['far', 'copy']" size="xs" class="fake-link" />
                </span>
              </h1>
              <router-link v-if="league.years.length > 2" :to="{ name: 'leagueAllTimeStats', params: { leagueid: league.leagueID } }" class="all-time-stats-link">All-Time League Stats</router-link>
            </div>

            <div class="selector-area">
              <div v-if="!league.userIsInLeague && isAuth">
                <b-button v-if="!league.userIsFollowingLeague" variant="primary" @click="followLeague">Follow</b-button>
                <b-button v-if="league.userIsFollowingLeague" variant="secondary" @click="unfollowLeague">Unfollow</b-button>
              </div>
              <b-form-select v-model="selectedYear" :options="league.years" class="year-selector" @change="changeLeagueYear" />
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

      <b-alert v-if="mostRecentManagerMessage" show dismissible @dismissed="dismissRecentManagerMessage">
        <h5>Manager's Message ({{ mostRecentManagerMessage.timestamp | dateTime }})</h5>
        <div class="preserve-whitespace">{{ mostRecentManagerMessage.messageText }}</div>
      </b-alert>

      <div v-if="!league.publicLeague && !(league.userIsInLeague || league.outstandingInvite || inviteCode)" class="alert alert-warning" role="info">You are viewing a private league.</div>

      <b-modal id="draftFinishedModal" ref="draftFinishedModalRef" title="Draft Complete!" ok-only>
        <p v-if="!league.userIsInLeague || oneShotMode">The draft is complete!</p>
        <p v-else>The draft is complete! From here you can make bids for games that were not drafted, however, you may want to hold onto your available budget until later in the year!</p>
      </b-modal>

      <div v-if="inviteCode && !league.userIsInLeague && !leagueYear.playStatus.playStarted" class="alert alert-secondary">
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

      <b-alert v-if="leagueYear.userIsActive && hasProposedTrade" show>
        Someone has proposed a trade with you.
        <b-button v-b-modal="'activeTradesModal'" variant="success">View Trades</b-button>
      </b-alert>
      <b-alert v-if="(leagueYear.userIsActive || league.isManager) && hasActiveTrade" show>
        There are active trades under consideration.
        <b-button v-b-modal="'activeTradesModal'" variant="success">View Trades</b-button>
      </b-alert>
      <specialAuctionInfo v-for="activeSpecialAuction in leagueYear.activeSpecialAuctions" :key="activeSpecialAuction.masterGameID" :special-auction="activeSpecialAuction"></specialAuctionInfo>
      <div v-if="leagueYear.playStatus.playStarted && leagueYear.supportedYear.finished">
        <div class="alert alert-success" role="alert">
          This year is finished! The winner is
          <strong>{{ topPublisher.publisherName }}</strong>
          !
        </div>
      </div>
      <div v-if="!leagueYear.userIsActive && league.userIsInLeague && !league.isManager">
        <div class="alert alert-warning" role="alert">You are set to inactive for this year.</div>
      </div>

      <div v-if="(leagueYear.userIsActive || league.isManager) && !leagueYear.playStatus.readyToDraft" class="alert alert-warning">
        <h2>This year is not active yet!</h2>
        <ul>
          <li v-for="error in leagueYear.playStatus.startDraftErrors" :key="error">{{ error }}</li>
        </ul>
        <b-button v-if="mustSetDraftOrder" v-b-modal="'editDraftOrderForm'" variant="success">Set Draft Order</b-button>
      </div>

      <div v-if="leagueYear.userIsActive && !userPublisher" class="alert alert-info">
        <p>You need to create your publisher for this year.</p>
        <b-button v-b-modal="'createPublisher'" variant="primary" class="mx-2">Create Publisher</b-button>
        <createPublisherForm :league-year="leagueYear"></createPublisherForm>
      </div>

      <div v-if="leagueYear.userIsActive && !userPublisher && league.isManager" class="alert alert-info">
        Alternatively, if you want to manage this league without playing in it, you will need to set yourself as "inactive" by going to "Manager Active Players" in the Manage League menu.
      </div>

      <div v-if="league.isManager && !leagueYear.playStatus.playStarted && !leagueYear.userIsActive" class="alert alert-info">You are currently set to manage this league without playing in it.</div>

      <div v-if="!leagueYear.playStatus.playStarted && leagueYear.playStatus.readyToDraft && !league.outstandingInvite">
        <div class="alert alert-success">
          <span v-if="league.isManager">
            Things are all set to get started!
            <b-button v-b-modal="'startDraft'" variant="primary" class="mx-2">Start Drafting!</b-button>
          </span>
          <span v-if="!league.isManager">Things are all set to get started! Your league manager can choose when to begin the draft.</span>
        </div>
        <startDraftModal @draftStarted="startDraft"></startDraftModal>
      </div>

      <div v-if="leagueYear.playStatus.draftIsPaused">
        <div class="alert alert-danger">
          <div v-if="!league.isManager">The draft has been paused. Speak to your league manager for details.</div>
          <div v-else>
            The draft has been paused. You can undo games that have been drafted.
            <b-button v-b-modal="'setPauseModal'" variant="success">Resume Draft</b-button>
          </div>
        </div>
      </div>
      <div v-if="leagueYear.playStatus.draftIsActive && nextPublisherUp">
        <div v-if="!userIsNextInDraft">
          <div class="alert alert-info">
            <div v-show="!leagueYear.playStatus.draftingCounterPicks">The draft is currently in progress!</div>
            <div v-show="leagueYear.playStatus.draftingCounterPicks">It's time to draft Counter-Picks!</div>
            <div>
              Next to draft:
              <strong>{{ nextPublisherUp.publisherName }}</strong>
            </div>
            <div v-if="league.isManager">To select the next player's game for them, Select 'Select Next Game' under 'Draft Management' in the sidebar!</div>
          </div>
        </div>
        <div v-else>
          <div class="alert alert-success draft-header">
            <div>
              <div v-show="!leagueYear.playStatus.draftingCounterPicks">The draft is currently in progress!</div>
              <div v-show="leagueYear.playStatus.draftingCounterPicks">It's time to draft counter picks!</div>
              <div><strong>It is your turn to draft!</strong></div>
            </div>
            <div v-if="!leagueYear.playStatus.draftingCounterPicks">
              <b-button v-b-modal="'playerDraftGameForm'" variant="primary">Draft Game</b-button>
            </div>
            <div v-else>
              <b-button v-b-modal="'playerDraftCounterPickForm'" variant="primary">Draft Counter Pick</b-button>
            </div>
          </div>
        </div>
      </div>

      <div class="row">
        <div class="col-xl-3 col-lg-4 col-md-12">
          <leagueActions></leagueActions>
        </div>
        <div class="col-xl-9 col-lg-8 col-md-12">
          <leagueYearStandings></leagueYearStandings>
          <div v-if="leagueYear.playStatus.draftFinished && !leagueYear.supportedYear.finished">
            <gameNews :game-news="gameNews" mode="league" />
            <br />
            <div v-if="!oneShotMode">
              <bidCountdowns v-if="showPublicRevealCountdown" mode="NextPublic" @publicBidRevealTimeElapsed="revealPublicBids"></bidCountdowns>
              <bidCountdowns v-if="!showPublicRevealCountdown" mode="NextBid"></bidCountdowns>
            </div>
            <div v-if="leagueYear.publicBiddingGames">
              <h2>This week's bids</h2>
              <activeBids />
            </div>
          </div>
          <br />
          <leagueGameSummary></leagueGameSummary>
        </div>
      </div>
    </div>
    <audio id="draft-notification-sound" src="/sounds/draft-notification.mp3"></audio>
  </div>
</template>

<script>
import axios from 'axios';
import * as signalR from '@microsoft/signalr';

import LeagueGameSummary from '@/components/leagueGameSummary.vue';
import LeagueYearStandings from '@/components/leagueYearStandings.vue';
import LeagueActions from '@/components/leagueActions.vue';
import CreatePublisherForm from '@/components/modals/createPublisherForm.vue';
import StartDraftModal from '@/components/modals/startDraftModal.vue';
import GameNews from '@/components/gameNews.vue';
import ActiveBids from '@/components/activeBids.vue';
import BidCountdowns from '@/components/bidCountdowns.vue';
import LeagueMixin from '@/mixins/leagueMixin.js';
import SpecialAuctionInfo from '@/components/specialAuctionInfo.vue';

export default {
  components: {
    LeagueGameSummary,
    LeagueYearStandings,
    CreatePublisherForm,
    StartDraftModal,
    LeagueActions,
    GameNews,
    ActiveBids,
    BidCountdowns,
    SpecialAuctionInfo
  },
  mixins: [LeagueMixin],
  props: {
    leagueid: { type: String, required: true },
    year: { type: Number, required: true }
  },
  data() {
    return {
      selectedYear: null,
      errorInfo: null,
      oneShotText: "This is a 'one shot' league, meaning there are no bids or drops. The draft is final."
    };
  },
  computed: {
    mostRecentManagerMessage() {
      if (!this.leagueYear || !this.leagueYear.managerMessages || this.leagueYear.managerMessages.length === 0) {
        return null;
      }

      let mostRecentMessage = this.leagueYear.managerMessages[this.leagueYear.managerMessages.length - 1];
      if (mostRecentMessage.isDismissed) {
        return null;
      }

      return mostRecentMessage;
    },
    showPublicRevealCountdown() {
      if (!this.leagueYear || (this.leagueYear.settings.pickupSystem !== 'SemiPublicBidding' && this.leagueYear.settings.pickupSystem !== 'SemiPublicBiddingSecretCounterPicks')) {
        return false;
      }

      let revealIsNext = this.$store.getters.bidTimes.nextPublicBiddingTime < this.$store.getters.bidTimes.nextBidLockTime;
      return revealIsNext;
    },
    hasActiveTrade() {
      if (this.leagueYear.userIsActive) {
        return this.leagueYear.activeTrades.some((x) => x.counterPartyPublisherID !== this.userPublisher.publisherID);
      } else if (this.league.isManager) {
        return this.leagueYear.activeTrades.length > 0;
      }
      return false;
    },
    hasProposedTrade() {
      if (this.leagueYear.userIsActive) {
        return this.leagueYear.activeTrades.some((x) => x.counterPartyPublisherID === this.userPublisher.publisherID);
      } else if (this.league.isManager) {
        return this.leagueYear.activeTrades.length > 0;
      }
      return false;
    },
    mustSetDraftOrder() {
      return this.leagueYear.playStatus.readyToSetDraftOrder && this.leagueYear.playStatus.startDraftErrors.includes('You must set the draft order.');
    }
  },
  watch: {
    async $route(to, from) {
      if (to.path !== from.path) {
        await this.initializePage();
      }
    },
    userIsNextInDraft: function (val, oldVal) {
      if (val && val !== oldVal) {
        document.getElementById('draft-notification-sound').play();
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
      const leaguePageParams = { leagueID: this.leagueid, year: this.year, inviteCode };
      await this.$store.dispatch('initializeLeaguePage', leaguePageParams);
      await this.startHubConnection();
    },
    refreshLeagueYear() {
      return this.$store.dispatch('refreshLeagueYear');
    },
    async revealPublicBids() {
      await this.refreshLeagueYear();
      this.$store.dispatch('getBidTimes');
    },
    changeLeagueYear(newVal) {
      const parameters = {
        leagueid: this.leagueid,
        year: newVal
      };
      this.$router.push({ name: 'league', params: parameters });
    },
    acceptInvite() {
      const model = {
        leagueID: this.leagueYear.leagueID
      };
      axios
        .post('/api/league/AcceptInvite', model)
        .then(() => {
          this.refreshLeagueYear();
        })
        .catch(() => {});
    },
    declineInvite() {
      const model = {
        inviteID: this.league.outstandingInvite.inviteID
      };
      axios
        .post('/api/league/DeclineInvite', model)
        .then(() => {
          this.$router.push({ name: 'home' });
        })
        .catch(() => {});
    },
    joinWithInviteLink() {
      const model = {
        leagueID: this.leagueYear.leagueID,
        inviteCode: this.inviteCode
      };
      axios
        .post('/api/league/JoinWithInviteLink', model)
        .then(() => {
          this.refreshLeagueYear();
        })
        .catch(() => {
          this.errorInfo = 'Something went wrong joining the league';
        });
    },
    startDraft() {
      const model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year
      };
      axios
        .post('/api/leagueManager/startDraft', model)
        .then(async () => {
          await this.refreshLeagueYear();
          await this.startHubConnection();
        })
        .catch(() => {});
    },
    followLeague() {
      const model = {
        leagueID: this.leagueYear.leagueID
      };
      axios
        .post('/api/league/FollowLeague', model)
        .then(() => {
          this.refreshLeagueYear();
        })
        .catch(() => {});
    },
    unfollowLeague() {
      const model = {
        leagueID: this.leagueYear.leagueID
      };
      axios
        .post('/api/league/UnfollowLeague', model)
        .then(() => {
          this.refreshLeagueYear();
        })
        .catch(() => {});
    },
    async dismissRecentManagerMessage() {
      const model = {
        messageID: this.mostRecentManagerMessage.messageID
      };
      await axios.post('/api/league/DismissManagerMessage', model);
      this.refreshLeagueYear();
    },
    async startHubConnection() {
      if (!this.leagueYear || !this.leagueYear.playStatus.playStarted || this.leagueYear.draftFinished) {
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
        this.$refs.draftFinishedModalRef.show();
      });

      hubConnection.onclose(async () => {
        await this.startHubConnection();
      });
    },
    leagueIDCopied() {
      this.makeToast('League ID copied to clipboard.');
    }
  }
};
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

.draft-header {
  display: flex;
  gap: 20px;
}

.all-time-stats-link {
  margin-left: 15px;
}
</style>
