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
        <b-button v-if="!sortOrderMode && leagueYear.hasSpecialSlots && userIsPublisher && !moveMode" variant="info" v-on:click="enterMoveMode">Move Games</b-button>
        <b-button v-if="!sortOrderMode && leagueYear.hasSpecialSlots && moveMode" variant="secondary" v-on:click="cancelMoveMode">Cancel Movement</b-button>
        <b-button v-if="!sortOrderMode && leagueYear.hasSpecialSlots && moveMode" variant="success" v-on:click="confirmPositions">Confirm Positions</b-button>
        <template v-if="isPlusUser">
          <b-form-checkbox v-show="sortOrderMode && hasFormerGames" v-model="includeRemovedInSorted">
            <span class="checkbox-label">Include Removed Games</span>
          </b-form-checkbox>
          <toggle-button class="toggle" v-model="sortOrderMode" :sync="true" :labels="{checked: 'Sort Mode', unchecked: 'Slot Mode'}" :css-colors="true" :font-size="13" :width="107" :height="28" />
        </template>
      </div>

      <div v-if="leagueYear && publisher">
        <div v-show="!sortOrderMode">
          <playerGameTable :publisher="publisher" :leagueYear="leagueYear"></playerGameTable>

          <div v-if="hasFormerGames && isPlusUser">
            <h3>Dropped/Removed Games</h3>
            <b-table :items="publisher.formerGames"
                     :fields="formerGameFields"
                     bordered responsive striped
                     primary-key="publisherGameID"
                     tbody-tr-class="btable-player-game-row">
              <template v-slot:cell(gameName)="data">
                <gameNameColumn :game="data.item"></gameNameColumn>
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
          <b-table :items="sortableGames"
                   :fields="sortableGameFields"
                   bordered responsive striped
                   primary-key="publisherGameID"
                   tbody-tr-class="btable-player-game-row">
            <template #head(masterGame.projectedFantasyPoints)="data">
              Projected points
              <font-awesome-icon color="black" size="lg" icon="info-circle" v-b-popover.hover.top="projectedPointsText" />
            </template>

            <template v-slot:cell(gameName)="data">
              <gameNameColumn :game="data.item" :gameSlot="getGameSlot(data.item)" :hasSpecialSlots="leagueYear.hasSpecialSlots" :yearFinished="leagueYear.supportedYear.yearFinished"></gameNameColumn>
            </template>
            <template v-slot:cell(masterGame.maximumReleaseDate)="data">
              {{getReleaseDate(data.item)}}
            </template>
            <template v-slot:cell(masterGame.criticScore)="data">
              {{data.item.criticScore | score(2)}}
            </template>
            <template v-slot:cell(masterGame.projectedFantasyPoints)="data">
              <em>~{{data.item.masterGame.projectedFantasyPoints | score(2)}}</em>
            </template>
            <template v-slot:cell(fantasyPoints)="data">
              {{data.item.fantasyPoints | score(2)}}
            </template>
            <template v-slot:cell(timestamp)="data">
              {{getAcquiredDate(data.item)}}
            </template>
            <template v-slot:cell(removedTimestamp)="data">
              {{getRemovedDate(data.item)}}
            </template>

            <template slot="custom-foot">
              <b-tr>
                <b-td class="total-description">
                  <span class="total-description-text">
                    Total Fantasy Points
                  </span>
                </b-td>
                <b-td></b-td>
                <b-td></b-td>
                <b-td v-show="includeRemovedInSorted"></b-td>
                <b-td v-show="includeRemovedInSorted"></b-td>
                <b-td class="average-critic-column">{{publisher.averageCriticScore | score(2)}} (Average)</b-td>
                <b-td class="total-column bg-info">{{publisher.totalProjectedPoints | score(2)}}</b-td>
                <b-td class="total-column bg-success">{{publisher.totalFantasyPoints | score(2)}}</b-td>
              </b-tr>
            </template>
          </b-table>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import Vue from 'vue';
import axios from 'axios';
import PlayerGameTable from '@/components/modules/gameTables/playerGameTable';
import SlotTypeBadge from '@/components/modules/gameTables/slotTypeBadge';
import GlobalFunctions from '@/globalFunctions';
import MasterGamePopover from '@/components/modules/masterGamePopover';
import GameNameColumn from '@/components/modules/gameTables/gameNameColumn';
import { ToggleButton } from 'vue-js-toggle-button';

export default {
  data() {
    return {
      errorInfo: '',
      publisher: null,
      leagueYear: null,
      sortableBaseGameFields: [
        { key: 'gameName', label: 'Game Name', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        { key: 'timestamp', label: 'Acquired', sortable: true, thClass: ['bg-primary'] },
        { key: 'masterGame.criticScore', label: 'Critic Score', sortable: true, thClass: ['bg-primary'], tdClass: ['score-column'] },
        { key: 'masterGame.projectedFantasyPoints', label: 'Projected Points', sortable: true, thClass: ['bg-primary'], tdClass: ['score-column'] },
        { key: 'fantasyPoints', label: 'Fantasy Points', sortable: true, thClass: ['bg-primary'], tdClass: ['score-column'] }
      ],
      sortableFormerGameFields: [
        { key: 'removedTimestamp', label: 'Removed Timestamp', sortable: true, thClass: ['bg-primary'] },
        { key: 'removedNote', label: 'Outcome', thClass: ['bg-primary'] }
      ],
      formerGameFields: [
        { key: 'gameName', label: 'Game Name', thClass: 'bg-primary' },
        { key: 'masterGame.maximumReleaseDate', label: 'Release Date', thClass: 'bg-primary' },
        { key: 'timestamp', label: 'Acquired', thClass: ['bg-primary'] },
        { key: 'masterGame.criticScore', label: 'Critic Score', thClass: ['bg-primary'], tdClass: ['score-column'] },
        { key: 'removedTimestamp', label: 'Removed Timestamp', thClass: ['bg-primary'] },
        { key: 'removedNote', label: 'Outcome', thClass: ['bg-primary'] }
      ],
      sortOrderMode: false,
      includeRemovedInSorted: false
    };
  },
  components: {
    PlayerGameTable,
    MasterGamePopover,
    ToggleButton,
    SlotTypeBadge,
    GameNameColumn
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
    },
    sortableGames() {
      if (this.includeRemovedInSorted) {
        return this.publisher.games.concat(this.publisher.formerGames);
      }

      return this.publisher.games;
    },
    sortableGameFields() {
      if (this.includeRemovedInSorted) {
        return this.sortableBaseGameFields.concat(this.sortableFormerGameFields);
      }

      return this.sortableBaseGameFields;
    },
    hasFormerGames() {
      return this.publisher && this.publisher.formerGames.length > 0;
    },
    projectedPointsText() {
      return {
        html: true,
        title: () => {
          return "Projected Points";
        },
        content: () => {
          return 'This is the amount of fantasy points that our algorithm believes this game will result in.' +
            ' If the game already has a critic score, then this was our final projection before the score came in.';
        }
      }
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
    },
    getGameSlot(publisherGame) {
      for (const slot of this.publisher.gameSlots) {
        if (!slot.publisherGame) {
          continue;
        }
        if (slot.publisherGame.publisherGameID === publisherGame.publisherGameID) {
          return slot;
        }
      }

      return null;
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

  .counter-pick-badge {
    background-color: #AA1E1E;
    color: white;
  }

  .master-game-popover {
    display: flex;
  }

  .total-description {
    vertical-align: middle;
  }

  .total-description-text {
    display: table-cell;
    vertical-align: middle;
    font-weight: bold;
    float: right;
  }

  .average-critic-column {
    text-align: center;
    vertical-align: middle;
    font-weight: bold;
  }

  .total-column {
    text-align: center;
    font-weight: bold;
    vertical-align: middle;
  }

  @media only screen and (max-width: 459px) {
    .average-critic-column {
      font-size: 14px;
    }

    .total-description-text {
      font-size: 14px;
    }

    .total-column {
      font-size: 20px;
    }
  }

  @media only screen and (min-width: 460px) {
    .average-critic-column {
      font-size: 20px;
    }

    .total-description-text {
      font-size: 20px;
    }

    .total-column {
      font-size: 25px;
    }
  }
</style>
<style>
  .btable-player-game-row {
    height: 40px;
  }

  .btable-player-game-row td {
    padding: 0.3rem;
  }
</style>
