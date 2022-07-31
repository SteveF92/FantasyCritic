<template>
  <b-modal id="specialAuctionsModal" ref="specialAuctionsModalRef" size="lg" title="Manage Special Auctions" hide-footer @hidden="clearData">
    <div class="alert alert-info">
      Special auctions allow your league to process the bids for a given game before the normal bid processing time. The main reason to do this is in case of 'shadow drops', which is when a game is
      announced and then released very shortly after.
    </div>
    <div v-if="leagueYear.activeSpecialAuctions.length > 0">
      <label>Active Special Auctions</label>
      <b-table-lite :items="leagueYear.activeSpecialAuctions" :fields="activeSpecialAuctionFields" bordered responsive striped>
        <template #cell(masterGame)="data">
          <masterGamePopover :master-game="data.item.masterGameYear"></masterGamePopover>
        </template>
        <template #cell(creationTime)="data">
          {{ data.item.creationTime | dateTime }}
        </template>
        <template #cell(scheduledEndTime)="data">
          {{ data.item.scheduledEndTime | dateTime }}
          <b-badge v-if="data.item.isLocked" variant="danger">Ended</b-badge>
          <b-button v-else variant="danger" @click="cancelSpecialAuction(data.item.masterGameYear)">Cancel</b-button>
        </template>
      </b-table-lite>
    </div>

    <form method="post" class="form-horizontal" role="form" @submit.prevent="searchGame">
      <div class="form-group">
        <label for="searchGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input id="searchGameName" v-model="searchGameName" name="searchGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" :disabled="!searchGameName" @click="searchGame">Search Game</b-button>
          </span>
        </div>
        <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="specialAuctionMasterGame" :possible-games="possibleMasterGames" @input="newGameSelected"></possibleMasterGamesTable>
      </div>
    </form>
    <div v-if="specialAuctionMasterGame">
      <h3 for="bidMasterGame" class="selected-game text-black">Selected Game:</h3>
      <masterGameSummary :master-game="specialAuctionMasterGame"></masterGameSummary>
      <div v-if="specialAuctionMasterGame.releasingToday" class="alert alert-warning">
        This game is releasing today. It may already be released. You can make a special auction, but it's up to you to decide if there is already too much information out about this game. You should
        consider:
        <ul>
          <li>Is it playable right now?</li>
          <li>Are there already reviews?</li>
        </ul>

        It is ultimately up to you though.
      </div>
      <hr />
      <label>Auction End Time (In your local time zone)</label>
      <b-form-datepicker v-model="scheduledEndTime" :config="datePickerConfig" class="form-control"></b-form-datepicker>
      <b-button variant="success" class="create-button" @click="createSpecialAuction">Create Special Auction</b-button>
    </div>
    <br />
    <div v-if="errorInfo" class="alert alert-danger">
      <h3 class="alert-heading">Error!</h3>
      <p class="text-white">{{ errorInfo }}</p>
    </div>
  </b-modal>
</template>

<script>
import axios from 'axios';
import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable';
import LeagueMixin from '@/mixins/leagueMixin';
import MasterGameSummary from '@/components/masterGameSummary';
import MasterGamePopover from '@/components/masterGamePopover';

export default {
  components: {
    PossibleMasterGamesTable,
    MasterGameSummary,
    MasterGamePopover
  },
  mixins: [LeagueMixin],
  data() {
    return {
      searchGameName: '',
      specialAuctionMasterGame: null,
      scheduledEndTime: null,
      possibleMasterGames: [],
      errorInfo: '',
      activeSpecialAuctionFields: [
        { key: 'masterGame', label: 'Game', thClass: ['bg-primary'] },
        { key: 'creationTime', label: 'Creation Time', thClass: ['bg-primary'] },
        { key: 'scheduledEndTime', label: 'End Time', thClass: ['bg-primary'] }
      ],
      datePickerConfig: {
        enableTime: true
      }
    };
  },
  computed: {
    formIsValid() {
      return this.specialAuctionMasterGame;
    }
  },
  methods: {
    async createSpecialAuction() {
      const dateObject = new Date(this.scheduledEndTime);
      const utcDate = dateObject.toISOString();

      var model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        masterGameID: this.specialAuctionMasterGame.masterGameID,
        scheduledEndTime: utcDate
      };

      try {
        await axios.post('/api/leagueManager/CreateSpecialAuction', model);
        this.notifyAction('Special auction created for: ' + this.specialAuctionMasterGame.gameName);
        this.clearData();
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    async cancelSpecialAuction(masterGameYear) {
      var model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        masterGameID: masterGameYear.masterGameID
      };

      try {
        await axios.post('/api/leagueManager/CancelSpecialAuction', model);
        this.notifyAction('Cancelled special auction for: ' + masterGameYear.gameName);
        this.clearData();
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    async searchGame() {
      this.specialAuctionMasterGame = null;
      this.possibleMasterGames = [];

      const endpointQuery = '/api/league/PossibleMasterGames?gameName=' + this.searchGameName + '&year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID;
      try {
        const response = await axios.get(endpointQuery);
        this.possibleMasterGames = response.data;
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    clearData() {
      this.searchGameName = '';
      this.specialAuctionMasterGame = null;
      this.possibleMasterGames = [];
      this.errorInfo = '';
    },
    newGameSelected() {
      this.errorInfo = '';
    }
  }
};
</script>
<style scoped>
.select-cell {
  text-align: center;
}
.game-search-input {
  margin-bottom: 15px;
}

.create-button {
  margin-top: 20px;
  width: 100%;
}
</style>
