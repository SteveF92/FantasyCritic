<template>
  <div>
    <div class="table-options">
      <b-button v-if="!sortOrderMode && leagueYear.hasSpecialSlots && userIsPublisher && !moveMode" variant="info" v-on:click="enterMoveMode">Move Games</b-button>
      <b-button v-if="!sortOrderMode && leagueYear.hasSpecialSlots && moveMode" variant="secondary" v-on:click="cancelMoveMode">Cancel Movement</b-button>
      <b-button v-if="!sortOrderMode && leagueYear.hasSpecialSlots && moveMode" variant="success" v-on:click="confirmPositions">Confirm Positions</b-button>
      <template v-if="!moveMode && isPlusUser">
        <b-form-checkbox v-show="sortOrderMode && hasFormerGames" v-model="includeRemovedInSorted">
          <span class="checkbox-label">Include Dropped Games</span>
        </b-form-checkbox>
        <toggle-button class="toggle" v-model="sortOrderMode" :sync="true" :labels="{checked: 'Sort Mode', unchecked: 'Slot Mode'}" :css-colors="true" :font-size="13" :width="107" :height="28" />
      </template>
    </div>

    <b-table :items="tableItems"
             :fields="tableFields"
             bordered responsive striped
             primary-key="overallSlotNumber">
      <template #head(publisherGame.masterGame.projectedFantasyPoints)="data">
        Projected points
        <font-awesome-icon color="black" size="lg" icon="info-circle" v-b-popover.hover.top="projectedPointsText" />
      </template>

      <template v-slot:cell(publisherGame.gameName)="data">
        <gameNameColumn :gameSlot="data.item" :hasSpecialSlots="leagueYear.hasSpecialSlots" :supportedYear="leagueYear.supportedYear"></gameNameColumn>
      </template>
      <template v-slot:cell(publisherGame.masterGame.maximumReleaseDate)="data">
        {{getReleaseDate(data.item.publisherGame)}}
      </template>
      <template v-slot:cell(publisherGame.masterGame.criticScore)="data">
        {{data.item.publisherGame.criticScore | score(2)}}
      </template>
      <template v-slot:cell(publisherGame.masterGame.projectedFantasyPoints)="data">
        <em>~{{data.item.publisherGame.masterGame.projectedFantasyPoints | score(2)}}</em>
      </template>
      <template v-slot:cell(publisherGame.fantasyPoints)="data">
        {{data.item.publisherGame.fantasyPoints | score(2)}}
      </template>
      <template v-slot:cell(publisherGame.timestamp)="data">
        {{getAcquiredDate(data.item.publisherGame)}}
      </template>
      <template v-slot:cell(publisherGame.removedTimestamp)="data">
        {{getRemovedDate(data.item.publisherGame)}}
      </template>

      <template slot="custom-foot">
        <b-tr>
          <b-td class="total-description">
            <span class="total-description-text">Total Fantasy Points</span>
          </b-td>
          <b-td></b-td>
          <b-td></b-td>
          <b-td class="average-critic-column">{{publisher.averageCriticScore | score(2)}} (Average)</b-td>
          <b-td class="total-column projected-footer bg-info">~{{publisher.totalProjectedPoints | score(2)}}</b-td>
          <b-td class="total-column bg-success">{{publisher.totalFantasyPoints | score(2)}}</b-td>
          <b-td v-show="includeRemovedInSorted"></b-td>
          <b-td v-show="includeRemovedInSorted"></b-td>
        </b-tr>
      </template>
    </b-table>
  </div>
</template>

<script>
  import PlayerGameTable from '@/components/modules/gameTables/playerGameTable';
  import SlotTypeBadge from '@/components/modules/gameTables/slotTypeBadge';
  import GlobalFunctions from '@/globalFunctions';
  import MasterGamePopover from '@/components/modules/masterGamePopover';
  import GameNameColumn from '@/components/modules/gameTables/gameNameColumn';
  import { ToggleButton } from 'vue-js-toggle-button';

  export default {
    data() {
      return {
        sortOrderMode: true,
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
    props: ['publisher', 'leagueYear'],
    computed: {
      moveMode() {
        return this.$store.getters.moveMode;
      },
      userIsPublisher() {
        return this.$store.getters.userInfo && this.publisher.userID === this.$store.getters.userInfo.userID;
      },
      isPlusUser() {
        return this.$store.getters.isPlusUser;
      },
      tableItems() {
        if (this.sortOrderMode) {
          return _.reject(this.publisher.gameSlots, ['publisherGame', null]);
        }
        if (this.includeRemovedInSorted) {
          let fakeFormerSlots = [];

          let overallSlotNumber = _maxBy(this.publisher.gameSlots, 'overallSlotNumber').overallSlotNumber;
          for (const formerGame of this.publisher.formerGames) {
            overallSlotNumber++;
            let fakeSlot = getFakeGameSlot(formerGame, overallSlotNumber);
            fakeFormerSlots.push(fakeSlot)
          }
          return this.publisher.gameSlots.concat(fakeFormerSlots);
        }

        return this.publisher.gameSlots;
      },
      tableFields() {
        let baseGameFields = [
          { key: 'publisherGame.gameName', label: 'Game Name', sortable: this.sortOrderMode, thClass: 'bg-primary' },
          { key: 'publisherGame.masterGame.maximumReleaseDate', label: 'Release Date', sortable: this.sortOrderMode, thClass: 'bg-primary' },
          { key: 'publisherGame.timestamp', label: 'Acquired', sortable: this.sortOrderMode, thClass: ['bg-primary'] },
          { key: 'publisherGame.masterGame.criticScore', label: 'Critic Score', sortable: this.sortOrderMode, thClass: ['bg-primary'], tdClass: ['score-column'] },
          { key: 'publisherGame.masterGame.projectedFantasyPoints', label: 'Projected Points', sortable: this.sortOrderMode, thClass: ['bg-primary'], tdClass: ['score-column'] },
          { key: 'publisherGame.fantasyPoints', label: 'Fantasy Points', sortable: this.sortOrderMode, thClass: ['bg-primary'], tdClass: ['score-column'] }
        ];
        let formerGameFields = [
          { key: 'publisherGame.removedTimestamp', label: 'Removed Timestamp', sortable: this.sortOrderMode, thClass: ['bg-primary'] },
          { key: 'publisherGame.removedNote', label: 'Outcome', thClass: ['bg-primary'] }
        ];

        if (this.sortOrderMode && this.includeRemovedInSorted) {
          return baseGameFields.concat(formerGameFields);
        }
        return baseGameFields;
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
      getFakeGameSlot(formerPublisherGame, overallSlotNumber) {
        //Return a fake slot object so the row draws properly
        return {
          counterPick: formerPublisherGame.counterPick,
          publisherGame: formerPublisherGame,
          eligibilityErrors: [],
          gameMeetsSlotCriteria: true,
          overallSlotNumber: overallSlotNumber,
          specialSlot: null
        };
      }
    }
  };
</script>
<style scoped>
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

  .projected-footer {
    font-style: italic;
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

  .projected-points-column {
    width: 110px;
  }
</style>
