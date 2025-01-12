<template>
  <b-modal :id="modalID" :ref="modalRef" size="lg" title="Make a Bid" hide-footer @hidden="clearData" @show="getTopGames">
    <div v-if="errorInfo" class="alert alert-danger" role="alert">
      {{ errorInfo }}
    </div>
    <div v-if="!specialAuction" class="alert alert-info">
      You can use this form to place a bid on a game.
      <br />
      Bids are processed on Saturday Nights. See the FAQ for more info.
    </div>
    <div v-else class="alert alert-info">You are bidding on a game in a Special Auction.</div>

    <div v-if="publisherSlotsAreFilled" class="alert alert-warning">
      Warning! You have already filled all of your game slots. You can still make bids, but you must drop a game first. You have two options:
      <ul>
        <li>Use the conditional drop option on this form to drop a game only if this bid succeeds.</li>
        <li>Use the normal "drop a game" option to drop a game no matter what. The drop will process before any bids so it will work as long as it is a valid drop.</li>
      </ul>
    </div>

    <form v-if="!specialAuction" method="post" class="form-horizontal" role="form" @submit.prevent="searchGame">
      <label for="searchGameName" class="control-label">Game Name</label>
      <div class="input-group game-search-input">
        <input id="searchGameName" v-model="searchGameName" name="searchGameName" type="text" class="form-control input" />
        <span class="input-group-btn">
          <b-button variant="info" :disabled="!searchGameName" @click="searchGame">Search Game</b-button>
        </span>
      </div>

      <div v-if="!leagueYear.settings.hasSpecialSlots">
        <b-button variant="secondary" class="show-top-button" @click="getTopGames">Show Top Available Games</b-button>
        <b-button variant="secondary" class="show-top-button" @click="getQueuedGames">Show My Watchlist</b-button>
        <b-button variant="secondary" class="show-top-button" @click="getThisWeeksPublicBids" v-if="leagueYear.publicBiddingGames">Show This Week's Bids</b-button>
      </div>
      <div v-else>
        <b-button variant="secondary" class="show-top-button" @click="getQueuedGames">Show My Watchlist</b-button>
        <b-button variant="secondary" class="show-top-button" @click="getThisWeeksPublicBids" v-if="leagueYear.publicBiddingGames">Show This Week's Bids</b-button>
        <h5 class="text-black">Top Available by Slot</h5>
        <span class="search-tags">
          <searchSlotTypeBadge :game-slot="leagueYear.slotInfo.overallSlot" name="ALL" :selected="selectedSlotIndex === 0" @click.native="getTopGames"></searchSlotTypeBadge>
          <searchSlotTypeBadge
            :game-slot="leagueYear.slotInfo.regularSlot"
            name="REG"
            :selected="selectedSlotIndex === 1"
            @click.native="getGamesForSlot(leagueYear.slotInfo.regularSlot, 1)"></searchSlotTypeBadge>
          <searchSlotTypeBadge
            v-for="(specialSlot, index) in leagueYear.slotInfo.specialSlots"
            :key="specialSlot.overallSlotNumber"
            :game-slot="specialSlot"
            :selected="selectedSlotIndex === 2 + index"
            @click.native="getGamesForSlot(specialSlot, 2 + index)"></searchSlotTypeBadge>
        </span>
      </div>

      <div v-show="isBusy" class="spinner">
        <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#000000' }" />
      </div>

      <div class="search-results">
        <div v-if="!bidMasterGame">
          <h3 v-show="showingTopAvailable" class="text-black">Top Available Games</h3>
          <h3 v-show="showingQueuedGames" class="text-black">Watchlist</h3>
          <h3 v-show="showingPublicBids" class="text-black">This Week's Public Bids</h3>
          <h3 v-show="!showingTopAvailable && !showingQueuedGames && !showingPublicBids && possibleMasterGames && possibleMasterGames.length > 0" class="text-black">Search Results</h3>
          <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="bidMasterGame" :possible-games="possibleMasterGames" @input="newGameSelected"></possibleMasterGamesTable>
        </div>
      </div>

      <div v-show="searched && !bidMasterGame && possibleMasterGames.length === 0" class="alert alert-info">
        <div class="row">
          <span class="col-12 col-md-7">No games were found.</span>
        </div>
      </div>
    </form>

    <div v-if="bidMasterGame">
      <ValidationObserver>
        <h3 for="bidMasterGame" class="selected-game text-black">Selected Game:</h3>
        <masterGameSummary :master-game="bidMasterGame"></masterGameSummary>
        <hr />
        <div class="form-group">
          <label for="bidAmount" class="control-label">Bid Amount (Remaining: {{ userPublisher.budget | money }})</label>

          <ValidationProvider v-slot="{ errors }" rules="required|integer">
            <input id="bidAmount" v-model="bidAmount" name="bidAmount" type="number" class="form-control input" />
            <span class="text-danger">{{ errors[0] }}</span>
          </ValidationProvider>
        </div>
        <div class="form-group">
          <label for="conditionalDrop" class="control-label">
            Conditional Drop (Optional)
            <font-awesome-icon v-b-popover.hover.focus="'You can use this to drop a game only if your bid succeeds.'" icon="info-circle" />
          </label>
          <b-form-select v-model="conditionalDrop">
            <template #first>
              <option :value="null">None</option>
            </template>
            <option v-for="publisherGame in droppableGames" :key="publisherGame.publisherGameID" :value="publisherGame">
              {{ publisherGame.gameName }}
            </option>
          </b-form-select>
        </div>

        <b-alert
          :show="!bidMasterGame.willRelease && bidMasterGame.tags.filter((x) => x.includes('EarlyAccess').length > 0) && leagueYear.settings.releaseSystem === 'OnlyNeedsScore'"
          variant="warning">
          Warning! This game is not planned for full release this year, but your league does allow games that are in early access and get an OpenCritic score to count for points. The system will let
          you bid for it (provided there are no other reasons it could be banned), but do so at your own risk, as early access games don't often get reviewed.
        </b-alert>

        <div v-if="leagueYear.settings.hasSpecialSlots" class="form-check">
          <input v-model="allowIneligibleSlot" class="form-check-input override-checkbox" type="checkbox" />
          <label class="form-check-label">
            Allow bid to succeed even if there are no slots this game is eligible in.
            <font-awesome-icon v-b-popover.hover.focus="allowIneligibleText" icon="info-circle" />
          </label>
        </div>

        <b-button v-if="formIsValid" variant="primary" class="full-width-button" :disabled="requestIsBusy" @click="bidGame">Place Bid</b-button>
        <div v-if="bidResult && !bidResult.success" class="alert bid-error" :class="{ 'alert-danger': !bidResult.showAsWarning, 'alert-warning': bidResult.showAsWarning }">
          <h3 v-if="bidResult.showAsWarning" class="alert-heading">Warning!</h3>
          <h3 v-else class="alert-heading">Error!</h3>
          <ul>
            <li v-for="error in bidResult.errors" :key="error">{{ error }}</li>
          </ul>

          <div v-if="bidResult.noEligibleSpaceError">
            <div class="text-white">If you do not move, drop, or trade a game, this game will end up in an ineligible slot.</div>
            <div class="text-white">Check the box above if you are sure you want to make this bid.</div>
          </div>
        </div>
      </ValidationObserver>
    </div>
  </b-modal>
</template>

<script>
import axios from 'axios';
import _ from 'lodash';

import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable.vue';
import MasterGameSummary from '@/components/masterGameSummary.vue';
import SearchSlotTypeBadge from '@/components/gameTables/searchSlotTypeBadge.vue';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  components: {
    PossibleMasterGamesTable,
    MasterGameSummary,
    SearchSlotTypeBadge
  },
  mixins: [LeagueMixin],
  props: {
    specialAuction: { type: Object, required: false, default: null }
  },
  data() {
    return {
      searchGameName: '',
      bidMasterGame: null,
      bidAmount: 0,
      bidResult: null,
      conditionalDrop: null,
      possibleMasterGames: [],
      errorInfo: '',
      showingTopAvailable: false,
      showingQueuedGames: false,
      showingPublicBids: false,
      searched: false,
      isBusy: false,
      requestIsBusy: false,
      selectedSlotIndex: 0,
      allowIneligibleSlot: false
    };
  },
  computed: {
    formIsValid() {
      return !!this.bidMasterGame;
    },
    publisherSlotsAreFilled() {
      let userGames = this.userPublisher.games;
      let standardGameSlots = this.leagueYear.settings.standardGames;
      let userStandardGames = userGames.filter((x) => !x.counterPick);
      return userStandardGames.length >= standardGameSlots;
    },
    droppableGames() {
      return this.userPublisher.games.filter((x) => !x.counterPick);
    },
    modalID() {
      if (!this.specialAuction) {
        return 'bidGameForm';
      }
      return `bidGameForm-${this.specialAuction.masterGameYear.masterGameID}`;
    },
    modalRef() {
      if (!this.specialAuction) {
        return 'bidGameFormRef';
      }
      return `bidGameFormRef-${this.specialAuction.masterGameYear.masterGameID}`;
    },
    allowIneligibleText() {
      return {
        html: true,
        title: () => {
          return 'What does this mean?';
        },
        content: () => {
          return (
            'If you do not check this box, if you win the bidding for this game but do not have a slot open that this game is eligible in, then the bid will fail. ' +
            'If you check this box, then the bid would succeed, but the game will land in an ineligible slot.'
          );
        }
      };
    }
  },
  mounted() {
    if (this.specialAuction) {
      this.bidMasterGame = this.specialAuction.masterGameYear;
    }
  },
  methods: {
    searchGame() {
      this.clearDataExceptSearch();
      this.isBusy = true;
      axios
        .get('/api/league/PossibleMasterGames?gameName=' + this.searchGameName + '&year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
          this.searched = true;
        })
        .catch(() => {
          this.isBusy = false;
        });
    },
    getTopGames() {
      this.clearDataExceptSearch();
      this.selectedSlotIndex = 0;
      this.isBusy = true;
      axios
        .get('/api/league/TopAvailableGames?year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID + '&publisherid=' + this.userPublisher.publisherID)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
          this.showingTopAvailable = true;
        })
        .catch(() => {
          this.isBusy = false;
        });
    },
    getQueuedGames() {
      this.clearDataExceptSearch();
      this.isBusy = true;
      axios
        .get('/api/league/CurrentQueuedGameYears/' + this.userPublisher.publisherID)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
          this.showingQueuedGames = true;
        })
        .catch(() => {
          this.isBusy = false;
        });
    },
    getThisWeeksPublicBids() {
      this.clearDataExceptSearch();
      this.isBusy = true;
      axios
        .get('/api/league/ThisWeeksPublicBiddingGames?year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID + '&publisherid=' + this.userPublisher.publisherID)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
          this.showingPublicBids = true;
        })
        .catch(() => {
          this.isBusy = false;
        });
    },
    getGamesForSlot(slotInfo, slotIndex) {
      this.clearDataExceptSearch();
      this.selectedSlotIndex = slotIndex;
      this.isBusy = true;
      let slotJSON = JSON.stringify(slotInfo);
      let base64Slot = btoa(slotJSON);
      let urlEncodedSlot = encodeURI(base64Slot);
      axios
        .get('/api/league/TopAvailableGames?year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID + '&publisherid=' + this.userPublisher.publisherID + '&slotInfo=' + urlEncodedSlot)
        .then((response) => {
          this.possibleMasterGames = response.data;
          this.isBusy = false;
          this.showingTopAvailable = true;
        })
        .catch(() => {
          this.isBusy = false;
        });
    },
    async bidGame() {
      const request = {
        publisherID: this.userPublisher.publisherID,
        masterGameID: this.bidMasterGame.masterGameID,
        bidAmount: this.bidAmount,
        counterPick: false,
        allowIneligibleSlot: this.allowIneligibleSlot
      };

      if (this.conditionalDrop) {
        request.conditionalDropPublisherGameID = this.conditionalDrop.publisherGameID;
      }

      this.requestIsBusy = true;
      try {
        const response = await axios.post('/api/league/MakePickupBid', request);
        this.bidResult = response.data;
        this.requestIsBusy = false;
        if (!this.bidResult.success) {
          return;
        }

        this.$refs[this.modalRef].hide();
        this.notifyAction('Bid for ' + this.bidMasterGame.gameName + ' for $' + this.bidAmount + ' was made.');
        this.clearData();
      } catch (error) {
        this.errorInfo = error.response.data;
        this.requestIsBusy = false;
      }
    },
    clearDataExceptSearch() {
      if (!this.specialAuction) {
        this.bidMasterGame = null;
      }
      this.bidResult = null;
      this.bidAmount = 0;
      this.possibleMasterGames = [];
      this.searched = false;
      this.showingTopAvailable = false;
      this.showingQueuedGames = false;
      this.showingPublicBids = false;
      this.isBusy = false;
      this.requestIsBusy = false;
      this.conditionalDrop = null;
      this.selectedSlotIndex = null;
    },
    clearData() {
      this.clearDataExceptSearch();
      this.searchGameName = '';
    },
    newGameSelected() {
      this.bidResult = null;
    }
  }
};
</script>
<style scoped>
.game-search-input {
  margin-bottom: 15px;
}

.search-results {
  margin-top: 20px;
}

.spinner {
  margin-top: 20px;
  text-align: center;
}

.show-top-button {
  margin-bottom: 10px;
}
</style>
