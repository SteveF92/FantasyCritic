<template>
  <b-modal :id="modalId" size="lg" title="Make a Bid" no-footer @hidden="clearData" @show="initialize">
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

      <b-button v-if="!requiredLeagueYear.settings.hasSpecialSlots" variant="secondary" class="show-top-button" @click="getTopGames">Show Top Available Games</b-button>
      <b-button variant="secondary" class="show-top-button" @click="getQueuedGames">Show My Watchlist</b-button>
      <b-button variant="secondary" class="show-top-button" @click="getThisWeeksPublicBids" v-if="requiredLeagueYear.publicBiddingGames">Show This Week's Bids</b-button>

      <div v-if="requiredLeagueYear.settings.hasSpecialSlots">
        <h5 class="text-black">Top Available by Slot</h5>
        <span class="search-tags">
          <searchSlotTypeBadge :game-slot="requiredLeagueYear.slotInfo.overallSlot" name="ALL" :selected="searchMode === 'topAvailable' && !selectedSlotKey" @click="getTopGames"></searchSlotTypeBadge>
          <searchSlotTypeBadge
            :game-slot="requiredLeagueYear.slotInfo.regularSlot"
            name="REG"
            :selected="selectedSlotKey === 'regular'"
            @click="getGamesForSlot(requiredLeagueYear.slotInfo.regularSlot, 'regular')"></searchSlotTypeBadge>
          <searchSlotTypeBadge
            v-for="(specialSlot, index) in requiredLeagueYear.slotInfo.specialSlots"
            :key="index"
            :game-slot="specialSlot"
            :selected="selectedSlotKey === `special-${index}`"
            @click="getGamesForSlot(specialSlot, `special-${index}`)"></searchSlotTypeBadge>
        </span>
      </div>

      <div v-show="isBusy" class="spinner">
        <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#000000' }" />
      </div>

      <div v-if="!bidMasterGame && searchMode && possibleMasterGames.length > 0" class="search-results">
        <h3 class="text-black">{{ searchHeading }}</h3>
        <possibleMasterGamesTable v-model="bidMasterGame" :possible-games="possibleMasterGames" @update:model-value="newGameSelected" bid-mode></possibleMasterGamesTable>
      </div>

      <div v-show="searchMode && !bidMasterGame && possibleMasterGames.length === 0" class="alert alert-info">
        <div class="row">
          <span class="col-12 col-md-7">No games were found.</span>
        </div>
      </div>
    </form>

    <div v-if="bidMasterGame">
      <h3 class="selected-game text-black">Selected Game:</h3>
      <masterGameSummary :master-game="bidMasterGame"></masterGameSummary>
      <hr />
      <div class="form-group">
        <label for="bidAmount" class="control-label">Bid Amount (Remaining: {{ formatCurrency(requiredUserPublisher.budget) }})</label>

        <ValidationField v-slot="{ errors }" :model-value="bidAmount" name="bidAmount" label="Bid Amount" rules="required|integer">
          <input id="bidAmount" v-model="bidAmount" name="bidAmount" type="number" class="form-control input" aria-describedby="bidAmount-error" />
          <span id="bidAmount-error" class="text-danger">{{ errors[0] }}</span>
        </ValidationField>
      </div>
      <div class="form-group">
        <label for="bidConditionalDrop" class="control-label">
          Conditional Drop (Optional)
          <font-awesome-icon v-b-popover.hover.focus="'You can use this to drop a game only if your bid succeeds.'" icon="info-circle" />
        </label>
        <b-form-select id="bidConditionalDrop" v-model="conditionalDrop">
          <template #first>
            <option :value="null">None</option>
          </template>
          <option v-for="publisherGame in droppableGames" :key="publisherGame.publisherGameID" :value="publisherGame">
            {{ publisherGame.gameName }}
          </option>
        </b-form-select>
      </div>

      <b-alert :model-value="showEarlyAccessWarning" variant="warning">
        Warning! This game is not planned for full release this year, but your league does allow games that are in early access and get an OpenCritic score to count for points. The system will let you
        bid for it (provided there are no other reasons it could be banned), but do so at your own risk, as early access games don't often get reviewed.
      </b-alert>

      <div v-if="requiredLeagueYear.settings.hasSpecialSlots" class="plain-check">
        <input id="bidAllowIneligibleSlot" v-model="allowIneligibleSlot" class="plain-check-input override-checkbox" type="checkbox" />
        <label for="bidAllowIneligibleSlot" class="plain-check-label">
          Allow bid to succeed even if there are no slots this game is eligible in.
          <font-awesome-icon v-b-popover.hover.focus="allowIneligibleText" icon="info-circle" />
        </label>
      </div>

      <b-button variant="primary" class="full-width-button" :disabled="!canPlaceBid" @click="bidGame">Place Bid</b-button>
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

type SearchMode = 'search' | 'topAvailable' | 'watchlist' | 'publicBids' | null;

interface BidGameFormData {
  searchGameName: string;
  bidMasterGame: MasterGameYearViewModel | null;
  bidAmount: number;
  bidResult: PickupBidResultViewModel | null;
  conditionalDrop: PublisherGameViewModel | null;
  possibleMasterGames: PossibleMasterGameYearViewModel[];
  searchMode: SearchMode;
  errorInfo: string;
  isBusy: boolean;
  bidRequestIsBusy: boolean;
  selectedSlotKey: string | null;
  allowIneligibleSlot: boolean;
  latestSearchID: number;
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
    specialAuction: { type: Object as PropType<SpecialAuctionViewModel | null>, required: false, default: null },
    modalId: { type: String, required: true }
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
      searchMode: null,
      isBusy: false,
      bidRequestIsBusy: false,
      selectedSlotKey: null,
      allowIneligibleSlot: false,
      latestSearchID: 0
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear', 'requiredUserPublisher', 'currentDrops']),
    searchHeading(): string {
      switch (this.searchMode) {
        case 'search':
          return 'Search Results';
        case 'topAvailable':
          return 'Top Available Games';
        case 'watchlist':
          return 'Watchlist';
        case 'publicBids':
          return "This Week's Public Bids";
        default:
          return '';
      }
    },
    showEarlyAccessWarning(): boolean {
      return (
        !!this.bidMasterGame && !this.bidMasterGame.willRelease && this.bidMasterGame.tags.some((x) => x.includes('EarlyAccess')) && this.requiredLeagueYear.settings.releaseSystem === 'OnlyNeedsScore'
      );
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
      return !!this.bidMasterGame && this.hasBidSlotAcquisitionPath && !this.bidRequestIsBusy;
    },
    publisherSlotsAreFilled(): boolean {
      const userGames = this.requiredUserPublisher.games;
      const standardGameSlots = this.requiredLeagueYear.settings.standardGames;
      const userStandardGames = userGames.filter((x) => !x.counterPick);
      return userStandardGames.length >= standardGameSlots;
    },
    droppableGames(): PublisherGameViewModel[] {
      return this.requiredUserPublisher.games.filter((x) => !x.counterPick);
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    formatCurrency,
    async initialize(): Promise<void> {
      if (this.specialAuction) {
        return;
      }
      await this.getTopGames();
    },
    async searchGame(): Promise<void> {
      await this.fetchGames('search', null, () => leagueClient.possibleMasterGames(this.searchGameName, this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID));
    },
    async getTopGames(): Promise<void> {
      await this.fetchGames('topAvailable', null, () =>
        leagueClient.topAvailableGames(this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID, this.requiredUserPublisher.publisherID, null)
      );
    },
    async getQueuedGames(): Promise<void> {
      await this.fetchGames('watchlist', null, () => leagueClient.currentQueuedGameYears(this.requiredUserPublisher.publisherID, null));
    },
    async getThisWeeksPublicBids(): Promise<void> {
      await this.fetchGames('publicBids', null, () =>
        leagueClient.thisWeeksPublicBiddingGames(this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID, this.requiredUserPublisher.publisherID)
      );
    },
    async getGamesForSlot(slotInfo: PublisherSingleSlotRequirementsViewModel, slotKey: string): Promise<void> {
      const slotJSON = JSON.stringify(slotInfo);
      const base64Slot = btoa(slotJSON);
      await this.fetchGames('topAvailable', slotKey, () =>
        leagueClient.topAvailableGames(this.requiredLeagueYear.year, this.requiredLeagueYear.league.leagueID, this.requiredUserPublisher.publisherID, base64Slot)
      );
    },
    async fetchGames(searchMode: SearchMode, slotKeyToSelect: string | null, searchAction: () => Promise<PossibleMasterGameYearViewModel[]>): Promise<void> {
      this.clearDataExceptSearch();
      this.selectedSlotKey = slotKeyToSelect;
      this.isBusy = true;
      const searchID = ++this.latestSearchID;

      try {
        const games = await searchAction();
        if (searchID !== this.latestSearchID) {
          return;
        }

        this.possibleMasterGames = games;
        this.searchMode = searchMode;
      } catch (error) {
        const message = getApiErrorMessage(error, 'Failed to search for games.');
        if (searchID !== this.latestSearchID) {
          return;
        }

        this.errorInfo = message;
      } finally {
        if (searchID === this.latestSearchID) {
          this.isBusy = false;
        }
      }
    },
    async bidGame(): Promise<void> {
      if (!this.bidMasterGame) {
        throw new Error('No game is selected to bid on.');
      }

      const request: PickupBidRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        masterGameID: this.bidMasterGame.masterGameID,
        bidAmount: this.bidAmount,
        counterPick: false,
        allowIneligibleSlot: this.allowIneligibleSlot,
        conditionalDropPublisherGameID: this.conditionalDrop?.publisherGameID
      };

      this.bidRequestIsBusy = true;
      try {
        this.bidResult = await leagueClient.makePickupBid(request);
        if (!this.bidResult.success) {
          return;
        }

        await this.notifyAction('Bid for ' + this.bidMasterGame.gameName + ' for $' + this.bidAmount + ' was made.');
        this.clearDataAfterBid();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to place bid.');
      } finally {
        this.bidRequestIsBusy = false;
      }
    },
    clearDataAfterBid(): void {
      if (!this.specialAuction) {
        this.bidMasterGame = null;
      }
      this.bidResult = null;
      this.bidAmount = 0;
      this.conditionalDrop = null;
    },
    clearDataExceptSearch(): void {
      if (!this.specialAuction) {
        this.bidMasterGame = null;
      }
      this.bidResult = null;
      this.bidAmount = 0;
      this.possibleMasterGames = [];
      this.searchMode = null;
      this.isBusy = false;
      this.bidRequestIsBusy = false;
      this.conditionalDrop = null;
      this.selectedSlotKey = null;
      this.errorInfo = '';
    },
    clearData(): void {
      this.clearDataExceptSearch();
      this.searchGameName = '';
      this.latestSearchID++;
      this.isBusy = false;
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
