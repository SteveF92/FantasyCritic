<template>
  <b-modal :id="modalID" :ref="modalRef" size="lg" title="Make a Bid" no-footer @hidden="clearData" @show="getTopGames">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div v-if="!specialAuction" class="alert alert-info">
      You can use this form to place a bid on a game.
      <br />
      Bids are processed on Saturday Nights. See the FAQ for more info.
    </div>
    <div v-else class="alert alert-info">You are bidding on a game in a Special Auction.</div>

    <div v-if="publisherSlotsAreFilled" class="alert alert-warning">
      Warning! You have already filled all of your game slots. To place a bid you must have a conditional drop on this bid or a pending drop request. You have two options:
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

      <div v-if="!requiredLeagueYear.settings.hasSpecialSlots">
        <b-button variant="secondary" class="show-top-button" @click="getTopGames">Show Top Available Games</b-button>
        <b-button variant="secondary" class="show-top-button" @click="getQueuedGames">Show My Watchlist</b-button>
        <b-button variant="secondary" class="show-top-button" @click="getThisWeeksPublicBids" v-if="requiredLeagueYear.publicBiddingGames">Show This Week's Bids</b-button>
      </div>
      <div v-else>
        <b-button variant="secondary" class="show-top-button" @click="getQueuedGames">Show My Watchlist</b-button>
        <b-button variant="secondary" class="show-top-button" @click="getThisWeeksPublicBids" v-if="requiredLeagueYear.publicBiddingGames">Show This Week's Bids</b-button>
        <h5 class="text-black">Top Available by Slot</h5>
        <span class="search-tags">
          <searchSlotTypeBadge :game-slot="requiredLeagueYear.slotInfo.overallSlot" name="ALL" :selected="selectedSlotIndex === 0" @click="getTopGames"></searchSlotTypeBadge>
          <searchSlotTypeBadge
            :game-slot="requiredLeagueYear.slotInfo.regularSlot"
            name="REG"
            :selected="selectedSlotIndex === 1"
            @click="getGamesForSlot(requiredLeagueYear.slotInfo.regularSlot, 1)"></searchSlotTypeBadge>
          <searchSlotTypeBadge
            v-for="(specialSlot, index) in requiredLeagueYear.slotInfo.specialSlots"
            :key="specialSlot.overallSlotNumber"
            :game-slot="specialSlot"
            :selected="selectedSlotIndex === 2 + index"
            @click="getGamesForSlot(specialSlot, 2 + index)"></searchSlotTypeBadge>
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
          <possibleMasterGamesTable
            v-if="possibleMasterGames.length > 0"
            v-model="bidMasterGame"
            :possible-games="possibleMasterGames"
            @update:model-value="newGameSelected"
            bid-mode></possibleMasterGamesTable>
        </div>
      </div>

      <div v-show="searched && !bidMasterGame && possibleMasterGames.length === 0" class="alert alert-info">
        <div class="row">
          <span class="col-12 col-md-7">No games were found.</span>
        </div>
      </div>
    </form>

    <div v-if="bidMasterGame">
      <h3 for="bidMasterGame" class="selected-game text-black">Selected Game:</h3>
      <masterGameSummary :master-game="bidMasterGame"></masterGameSummary>
      <hr />
      <div class="form-group">
        <label for="bidAmount" class="control-label">Bid Amount (Remaining: {{ formatCurrency(requiredUserPublisher.budget) }})</label>

        <ValidationField v-slot="{ errors }" :model-value="bidAmount" name="bidAmount" rules="required|integer">
          <input id="bidAmount" v-model="bidAmount" name="bidAmount" type="number" class="form-control input" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationField>
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
        :model-value="!bidMasterGame.willRelease && bidMasterGame.tags.filter((x) => x.includes('EarlyAccess')).length > 0 && requiredLeagueYear.settings.releaseSystem === 'OnlyNeedsScore'"
        variant="warning">
        Warning! This game is not planned for full release this year, but your league does allow games that are in early access and get an OpenCritic score to count for points. The system will let you
        bid for it (provided there are no other reasons it could be banned), but do so at your own risk, as early access games don't often get reviewed.
      </b-alert>

      <div v-if="requiredLeagueYear.settings.hasSpecialSlots" class="plain-check">
        <input v-model="allowIneligibleSlot" class="plain-check-input override-checkbox" type="checkbox" />
        <label class="plain-check-label">
          Allow bid to succeed even if there are no slots this game is eligible in.
          <font-awesome-icon v-b-popover.hover.focus="allowIneligibleText" icon="info-circle" />
        </label>
      </div>

      <b-button v-if="formIsValid" variant="primary" class="full-width-button" :disabled="!canPlaceBid" @click="bidGame">Place Bid</b-button>
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
    </div>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import { mapActions, mapState } from 'pinia';

import {
  leagueClient,
  type MasterGameYearViewModel,
  type PickupBidRequest,
  type PickupBidResultViewModel,
  type PossibleMasterGameYearViewModel,
  type PublisherGameViewModel,
  type PublisherSingleSlotRequirementsViewModel,
  type SpecialAuctionViewModel
} from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { PopoverContent } from '@/types/vueTypes';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatCurrency } from '@/utilities/formatters';
import SearchSlotTypeBadge from '@/components/gameTables/searchSlotTypeBadge.vue';
import MasterGameSummary from '@/components/masterGameSummary.vue';
import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable.vue';

interface BidGameFormData {
  searchGameName: string;
  bidMasterGame: MasterGameYearViewModel | null;
  bidAmount: number;
  bidResult: PickupBidResultViewModel | null;
  conditionalDrop: PublisherGameViewModel | null;
  possibleMasterGames: PossibleMasterGameYearViewModel[];
  errorInfo: string;
  showingTopAvailable: boolean;
  showingQueuedGames: boolean;
  showingPublicBids: boolean;
  searched: boolean;
  isBusy: boolean;
  requestIsBusy: boolean;
  selectedSlotIndex: number | null;
  allowIneligibleSlot: boolean;
}

export default defineComponent({
  setup() {
    const allowIneligibleText: PopoverContent = {
      title: 'What does this mean?',
      body:
        'If you do not check this box, if you win the bidding for this game but do not have a slot open that this game is eligible in, then the bid will fail. ' +
        'If you check this box, then the bid would succeed, but the game will land in an ineligible slot.'
    };

    return { allowIneligibleText };
  },
  components: {
    PossibleMasterGamesTable,
    MasterGameSummary,
    SearchSlotTypeBadge
  },
  props: {
    specialAuction: { type: Object as PropType<SpecialAuctionViewModel>, required: false, default: null }
  },
  data(): BidGameFormData {
    let defaultMasterGame: MasterGameYearViewModel | null = null;
    if (this.specialAuction) {
      defaultMasterGame = this.specialAuction.masterGameYear;
    }

    return {
      searchGameName: '',
      bidMasterGame: defaultMasterGame,
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
      selectedSlotIndex: null,
      allowIneligibleSlot: false
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear', 'requiredUserPublisher', 'currentDrops']),
    formIsValid(): boolean {
      return !!this.bidMasterGame;
    },
    hasBidSlotAcquisitionPath(): boolean {
      if (!this.publisherSlotsAreFilled) {
        return true;
      }
      if (this.conditionalDrop) {
        return true;
      }
      return this.currentDrops.length > 0;
    },
    canPlaceBid(): boolean {
      return this.formIsValid && this.hasBidSlotAcquisitionPath && !this.requestIsBusy;
    },
    publisherSlotsAreFilled(): boolean {
      let userGames = this.requiredUserPublisher.games;
      let standardGameSlots = this.requiredLeagueYear.settings.standardGames;
      let userStandardGames = userGames.filter((x) => !x.counterPick);
      return userStandardGames.length >= standardGameSlots;
    },
    droppableGames(): PublisherGameViewModel[] {
      return this.requiredUserPublisher.games.filter((x) => !x.counterPick);
    },
    modalID(): string {
      if (!this.specialAuction) {
        return 'bidGameForm';
      }
      return `bidGameForm-${this.specialAuction.masterGameYear.masterGameID}`;
    },
    modalRef(): string {
      if (!this.specialAuction) {
        return 'bidGameFormRef';
      }
      return `bidGameFormRef-${this.specialAuction.masterGameYear.masterGameID}`;
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    formatCurrency,
    async searchGame(): Promise<void> {
      this.clearDataExceptSearch();
      this.isBusy = true;

      try {
        this.possibleMasterGames = await leagueClient.possibleMasterGames(this.searchGameName, this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID);
        this.searched = true;
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to search for games.');
      } finally {
        this.isBusy = false;
      }
    },
    async getTopGames(): Promise<void> {
      this.clearDataExceptSearch();
      this.selectedSlotIndex = null;
      this.isBusy = true;

      try {
        this.possibleMasterGames = await leagueClient.topAvailableGames(this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID, this.requiredUserPublisher.publisherID, null);
        this.showingTopAvailable = true;
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to search for games.');
      } finally {
        this.isBusy = false;
      }
    },
    async getQueuedGames(): Promise<void> {
      this.clearDataExceptSearch();
      this.isBusy = true;

      try {
        this.possibleMasterGames = await leagueClient.currentQueuedGameYears(this.requiredUserPublisher.publisherID, null);
        this.showingQueuedGames = true;
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to search for games.');
      } finally {
        this.isBusy = false;
      }
    },
    async getThisWeeksPublicBids(): Promise<void> {
      this.clearDataExceptSearch();
      this.isBusy = true;

      try {
        this.possibleMasterGames = await leagueClient.thisWeeksPublicBiddingGames(this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID, this.requiredUserPublisher.publisherID);
        this.showingPublicBids = true;
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to search for games.');
      } finally {
        this.isBusy = false;
      }
    },
    async getGamesForSlot(slotInfo: PublisherSingleSlotRequirementsViewModel, slotIndex: number): Promise<void> {
      this.clearDataExceptSearch();
      this.selectedSlotIndex = slotIndex;
      this.isBusy = true;
      const slotJSON = JSON.stringify(slotInfo);
      const base64Slot = btoa(slotJSON);
      const urlEncodedSlot = encodeURI(base64Slot);

      try {
        this.possibleMasterGames = await leagueClient.topAvailableGames(this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID, this.requiredUserPublisher.publisherID, urlEncodedSlot);
        this.showingTopAvailable = true;
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to search for games.');
      } finally {
        this.isBusy = false;
      }
    },
    async bidGame() {
      if (!this.bidMasterGame) {
        throw new Error('No game is selected to bid on.');
      }

      const request: PickupBidRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        masterGameID: this.bidMasterGame.masterGameID,
        bidAmount: this.bidAmount,
        counterPick: false,
        allowIneligibleSlot: this.allowIneligibleSlot,
        conditionalDropPublisherGameID: this.conditionalDrop ? this.conditionalDrop.publisherGameID : undefined
      };

      this.requestIsBusy = true;
      try {
        this.bidResult = await leagueClient.makePickupBid(request);
        if (!this.bidResult.success) {
          return;
        }
        this.notifyAction('Bid for ' + this.bidMasterGame.gameName + ' for $' + this.bidAmount + ' was made.');
        this.clearDataAfterBid();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to search for games.');
      } finally {
        this.requestIsBusy = false;
      }
    },
    clearDataAfterBid(): void {
      if (!this.specialAuction) {
        this.bidMasterGame = null;
      }
      this.bidResult = null;
      this.bidAmount = 0;
      this.isBusy = false;
      this.requestIsBusy = false;
      this.conditionalDrop = null;
    },
    clearDataExceptSearch(): void {
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
    clearData(): void {
      this.clearDataExceptSearch();
      this.searchGameName = '';
    },
    newGameSelected(): void {
      this.bidResult = null;
    }
  }
});
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
