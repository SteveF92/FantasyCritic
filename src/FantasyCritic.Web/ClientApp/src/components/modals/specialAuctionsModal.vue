<template>
  <b-modal id="specialAuctionsModal" ref="specialAuctionsModalRef" size="lg" title="Manage Special Auctions" hide-footer @hidden="clearData">
    <div class="alert alert-info">
      Special auctions allow your league to process the bids for a given game before the normal bid processing time. The main reason to do this is in case of 'shadow drops', which is when a game is
      announced and then released very shortly after.
    </div>
    <form method="post" class="form-horizontal" role="form" @submit.prevent="searchGame">
      <div class="form-group">
        <label for="specialAuctionGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input id="specialAuctionGameName" v-model="specialAuctionGameName" name="specialAuctionGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" @click="searchGame">Search Game</b-button>
          </span>
        </div>
        <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="specialAuctionMasterGame" :possible-games="possibleMasterGames" @input="newGameSelected"></possibleMasterGamesTable>
      </div>
    </form>
    <div v-if="specialAuctionMasterGame">
      <h3 for="bidMasterGame" class="selected-game text-black">Selected Game:</h3>
      <masterGameSummary :master-game="specialAuctionMasterGame"></masterGameSummary>
      <hr />
      <label>Auction End Time</label>
      <flat-pickr v-model="scheduledEndTime" :config="datePickerConfig" class="form-control"></flat-pickr>
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

export default {
  components: {
    PossibleMasterGamesTable,
    MasterGameSummary
  },
  mixins: [LeagueMixin],
  data() {
    return {
      specialAuctionGameName: '',
      specialAuctionMasterGame: null,
      scheduledEndTime: null,
      possibleMasterGames: [],
      errorInfo: ''
    };
  },
  computed: {
    formIsValid() {
      return this.specialAuctionMasterGame;
    },
    datePickerConfig() {
      return {
        enableTime: true
      };
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
        this.$refs.specialAuctionsModalRef.hide();
        this.notifyAction('Special auction created for: ' + this.specialAuctionMasterGame.gameName);
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    async searchGame() {
      this.specialAuctionMasterGame = null;
      this.possibleMasterGames = [];

      const endpointQuery = '/api/league/PossibleMasterGames?gameName=' + this.specialAuctionGameName + '&year=' + this.leagueYear.year + '&leagueid=' + this.leagueYear.leagueID;
      try {
        const response = await axios.get(endpointQuery);
        this.possibleMasterGames = response.data;
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    clearData() {
      this.specialAuctionGameName = '';
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
