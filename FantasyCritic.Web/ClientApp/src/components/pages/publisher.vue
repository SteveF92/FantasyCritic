<template>
  <div v-if="publisher && leagueYear">
    <div class="col-md-10 offset-md-1 col-sm-12">

      <div class="publisher-info">
        <div v-if="publisher.publisherIcon && iconIsValid" class="publisher-icon">
          {{ publisher.publisherIcon }}
        </div>
        <div class="publisher-details">
          <div class="publisher-name">
            <h1>{{publisher.publisherName}}</h1>
          </div>

          <h4>Player Name: {{publisher.playerName}}</h4>

          <h4>
            <router-link :to="{ name: 'league', params: { leagueid: publisher.leagueID, year: publisher.year }}">League: {{publisher.leagueName}}</router-link>
          </h4>
          <ul>
            <li>Budget: {{publisher.budget | money}}</li>
            <li>Will Release Games Dropped: {{getDropStatus(publisher.willReleaseGamesDropped, publisher.willReleaseDroppableGames)}}</li>
            <li>Will Not Release Games Dropped: {{getDropStatus(publisher.willNotReleaseGamesDropped, publisher.willNotReleaseDroppableGames)}}</li>
            <li>"Any Unreleased" Games Dropped: {{getDropStatus(publisher.freeGamesDropped, publisher.freeDroppableGames)}}</li>
          </ul>
        </div>
      </div>

      <div v-if="!publisher.publicLeague && !(publisher.userIsInLeague || publisher.outstandingInvite)" class="alert alert-warning" role="info">
        You are viewing a private league.
      </div>

      <div class="table-options">
        <template v-if="isPlusUser">
          <label class="view-mode-label">View Mode</label>
          <toggle-button class="toggle" v-model="sortOrderMode" :sync="true" :labels="{checked: 'Sort Mode', unchecked: 'Slot Mode'}" :css-colors="true" :font-size="13" :width="107" :height="28" />
        </template>
        <b-button v-if="!sortOrderMode && leagueYear.hasSpecialSlots && userIsPublisher && !moveMode" variant="info" v-on:click="enterMoveMode">Move Games</b-button>
        <b-button v-if="!sortOrderMode && leagueYear.hasSpecialSlots && moveMode" variant="secondary" v-on:click="cancelMoveMode">Cancel Movement</b-button>
        <b-button v-if="!sortOrderMode && leagueYear.hasSpecialSlots && moveMode" variant="success" v-on:click="confirmPositions">Confirm Positions</b-button>
      </div>

      <div v-if="leagueYear && publisher">
        <div v-show="!sortOrderMode">
          <playerGameTable :publisher="publisher" :leagueYear="leagueYear"></playerGameTable>

          <div v-if="publisher.formerGames.length > 0">
            <h3>Dropped/Removed Games</h3>
            <b-table :items="publisher.formerGames"
                     :fields="formerGameFields"
                     bordered responsive striped
                     primary-key="publisherGameID">

              <template v-slot:cell(gameName)="data">
                <span class="master-game-popover">
                  <masterGamePopover v-if="data.item.linked" :masterGame="data.item.masterGame"></masterGamePopover>
                  <span v-if="!data.item.linked">{{data.item.gameName}}</span>
                </span>
              </template>
              <template v-slot:cell(masterGame.maximumReleaseDate)="data">
                {{getReleaseDate(data.item)}}
              </template>
              <template v-slot:cell(masterGame.criticScore)="data">
                {{data.item.criticScore | score}}
              </template>
              <template v-slot:cell(timestamp)="data">
                {{getAcquiredDate(data.item)}}
              </template>
              <template v-slot:cell(removedTimestamp)="data">
                {{getRemovedDate(data.item)}}
              </template>
            </b-table>
          </div>
        </div>
        <div v-show="sortOrderMode">
          Sort
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import Vue from 'vue';
import axios from 'axios';
import PlayerGameTable from '@/components/modules/gameTables/playerGameTable';
import GlobalFunctions from '@/globalFunctions';
import MasterGamePopover from '@/components/modules/masterGamePopover';
import { ToggleButton } from 'vue-js-toggle-button';

export default {
  data() {
    return {
      errorInfo: '',
      publisher: null,
      leagueYear: null,
      formerGameFields: [
        { key: 'gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame.maximumReleaseDate', label: 'Release Date', thClass: 'bg-primary' },
        { key: 'masterGame.criticScore', label: 'Critic Score', thClass: ['bg-primary'], tdClass: ['score-column'] },
        { key: 'timestamp', label: 'Acquired', sortable: true, thClass: ['bg-primary'] },
        { key: 'removedTimestamp', label: 'Removed Timestamp', sortable: true, thClass: ['bg-primary'] },
        { key: 'removedNote', label: 'Outcome', sortable: true, thClass: ['bg-primary'] }
      ],
      sortOrderMode: false
    };
  },
  components: {
    PlayerGameTable,
    MasterGamePopover,
    ToggleButton
  },
  props: ['publisherid'],
  computed: {
    moveMode() {
      return this.$store.getters.moveMode;
    },
    userIsPublisher() {
      return this.$store.getters.userInfo && this.publisher.userID === this.$store.getters.userInfo.userID;
    },
    iconIsValid() {
      return GlobalFunctions.publisherIconIsValid(this.publisher.publisherIcon);
    },
    isPlusUser() {
      return this.$store.getters.isPlusUser;
    }
  },
  methods: {
    fetchPublisher() {
      axios
        .get('/api/League/GetPublisher/' + this.publisherid)
        .then(response => {
          this.publisher = response.data;
          this.fetchLeagueYear();
          this.$store.dispatch('initialize', this.publisher);
        })
        .catch(returnedError => (this.error = returnedError));
    },
    fetchLeagueYear() {
      axios
        .get('/api/League/GetLeagueYear?leagueID=' + this.publisher.leagueID + '&year=' + this.publisher.year)
        .then(response => {
          this.leagueYear = response.data;
        })
        .catch(returnedError => (this.error = returnedError));
    },
    getDropStatus(dropped, droppable) {
      if (!droppable) {
        return 'N/A';
      }
      if (droppable === -1) {
        return dropped + '/' + '\u221E';
      }
      return dropped + '/' + droppable;
    },
    enterMoveMode() {
      this.$store.commit('enterMoveMode');
    },
    cancelMoveMode() {
      this.$store.commit('cancelMoveMode');
    },
    confirmPositions() {
      this.$store.dispatch("confirmPositions").then(() => {
        this.fetchPublisher();
      });
    },
    getReleaseDate(publisherGame) {
      return GlobalFunctions.formatPublisherGameReleaseDate(publisherGame);
    },
    getAcquiredDate(publisherGame) {
      return GlobalFunctions.formatPublisherGameAcquiredDate(publisherGame);
    },
    getRemovedDate(publisherGame) {
      return GlobalFunctions.formatPublisherGameRemovedDate(publisherGame);
    }
  },
  mounted() {
    this.fetchPublisher();
  },
  watch: {
    '$route'(to, from) {
      this.fetchPublisher();
    }
  }
};
</script>
<style scoped>
  .publisher-info {
    margin-top: 10px;
    display: flex;
  }

  .publisher-name {
    display: block;
    max-width: 100%;
    word-wrap: break-word;
  }

  .publisher-icon {
    font-size: 100px;
  }

  .table-options {
    display: flex;
    justify-content: flex-end;
    align-items: center;
    margin-bottom: 10px;
    gap: 5px;
  }

  .toggle {
    margin: 0;
  }

  .view-mode-label {
    margin: 0;
  }
</style>
