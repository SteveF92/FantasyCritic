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
        <toggle-button class="toggle" v-model="sortOrderMode" :sync="true" :labels="{ checked: 'Sort Mode', unchecked: 'Slot Mode' }" :css-colors="true" :font-size="13" :width="107" :height="28" />
      </template>
    </div>

    <b-table
      :items="tableRows"
      :fields="tableFields"
      bordered
      responsive
      striped
      :sort-by.sync="sortBy"
      :sort-desc.sync="sortDesc"
      primary-key="overallSlotNumber"
      tbody-tr-class="btable-player-game-row">
      <template #head(publisherGame.masterGame.projectedFantasyPoints)="data">
        Projected
        <br />
        points
        <font-awesome-icon color="black" size="lg" icon="info-circle" v-b-popover.hover.top="projectedPointsText" />
      </template>

      <template #head(publisherGame.timestamp)="data">
        <span v-show="!sortOrderMode || !includeRemovedInSorted">Acquired</span>
        <span v-show="sortOrderMode && includeRemovedInSorted">Acquired/Dropped</span>
      </template>

      <template #cell(publisherGame.gameName)="data">
        <gameNameColumn :gameSlot="data.item" :hasSpecialSlots="leagueYear.hasSpecialSlots" :supportedYear="leagueYear.supportedYear"></gameNameColumn>
      </template>

      <template #cell(publisherGame.masterGame.maximumReleaseDate)="data">
        <template v-if="data.item.publisherGame">
          {{ getReleaseDate(data.item.publisherGame) }}
        </template>
      </template>

      <template #cell(publisherGame.masterGame.criticScore)="data">
        <template v-if="data.item.publisherGame">
          {{ data.item.publisherGame.criticScore | score(2) }}
        </template>
      </template>

      <template #cell(publisherGame.masterGame.projectedFantasyPoints)="data">
        <template v-if="data.item.publisherGame && data.item.publisherGame.masterGame" class="projected-text">~{{ data.item.publisherGame.masterGame.projectedFantasyPoints | score(2) }}</template>
      </template>

      <template #cell(publisherGame.fantasyPoints)="data">
        <template v-if="data.item.publisherGame">
          {{ data.item.publisherGame.fantasyPoints | score(2) }}
        </template>
      </template>

      <template #cell(publisherGame.timestamp)="data">
        <template v-if="data.item.publisherGame">
          {{ getAcquiredDate(data.item.publisherGame) }}
          <template v-if="data.item.publisherGame.removedTimestamp">
            <br />
            {{ data.item.publisherGame.removedNote }} on {{ getRemovedDate(data.item.publisherGame) }}
          </template>
        </template>
      </template>

      <template slot="custom-foot">
        <b-tr>
          <b-td class="total-description">
            <span class="total-description-text">Total Fantasy Points</span>
          </b-td>
          <b-td></b-td>
          <b-td></b-td>
          <b-td class="average-critic-column">{{ publisher.averageCriticScore | score(2) }} (Average)</b-td>
          <b-td class="total-column projected-text bg-info">~{{ publisher.totalProjectedPoints | score(2) }}</b-td>
          <b-td class="total-column bg-success">{{ publisher.totalFantasyPoints | score(2) }}</b-td>
        </b-tr>
      </template>
    </b-table>
  </div>
</template>

<script>
import SlotTypeBadge from '@/components/modules/gameTables/slotTypeBadge';
import GlobalFunctions from '@/globalFunctions';
import MasterGamePopover from '@/components/modules/masterGamePopover';
import GameNameColumn from '@/components/modules/gameTables/gameNameColumn';
import { ToggleButton } from 'vue-js-toggle-button';

export default {
  data() {
    return {
      sortOrderMode: false,
      includeRemovedInSorted: false,
      sortBy: 'overallSlotNumber',
      sortDesc: false
    };
  },
  components: {
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
      if (!this.sortOrderMode) {
        return this.$store.getters.gameSlots;
      }
      let slotsWithGames = _.reject(this.publisher.gameSlots, ['publisherGame', null]);
      if (this.includeRemovedInSorted) {
        let fakeFormerSlots = [];

        let overallSlotNumber = _.maxBy(this.publisher.gameSlots, 'overallSlotNumber').overallSlotNumber;
        for (const formerGame of this.publisher.formerGames) {
          overallSlotNumber++;
          let fakeSlot = this.getFakeGameSlot(formerGame, overallSlotNumber);
          fakeFormerSlots.push(fakeSlot);
        }

        return slotsWithGames.concat(fakeFormerSlots);
      }

      return slotsWithGames;
    },
    tableRows() {
      let rows = this.tableItems;
      for (let i = 0; i < rows.length; ++i) {
        if (!rows[i].gameMeetsSlotCriteria) {
          rows[i]._rowVariant = 'warning';
        }
      }
      return rows;
    },
    tableFields() {
      return [
        { key: 'publisherGame.gameName', label: 'Game Name', sortable: this.sortOrderMode, thClass: ['bg-primary', 'btable-player-table-header'], class: ['full-table-game-name-column'] },
        { key: 'publisherGame.masterGame.maximumReleaseDate', label: 'Release Date', sortable: this.sortOrderMode, thClass: ['bg-primary', 'btable-player-table-header', 'release-date-column'] },
        { key: 'publisherGame.timestamp', sortable: this.sortOrderMode, thClass: ['bg-primary', 'btable-player-table-header', 'acquired-column'] },
        { key: 'publisherGame.masterGame.criticScore', label: 'Critic Score', sortable: this.sortOrderMode, thClass: ['bg-primary', 'btable-player-table-header'], class: ['numeric-column'] },
        { key: 'publisherGame.masterGame.projectedFantasyPoints', sortable: this.sortOrderMode, thClass: ['bg-primary', 'btable-player-table-header'], class: ['numeric-column', 'projected-text'] },
        { key: 'publisherGame.fantasyPoints', label: 'Fantasy Points', sortable: this.sortOrderMode, thClass: ['bg-primary', 'btable-player-table-header'], class: ['numeric-column'] }
      ];
    },
    hasFormerGames() {
      return this.publisher && this.publisher.formerGames.length > 0;
    },
    projectedPointsText() {
      return {
        html: true,
        title: () => {
          return 'Projected Points';
        },
        content: () => {
          return (
            'This is the amount of fantasy points that our algorithm believes this game will result in.' +
            ' If the game already has a critic score, then this was our final projection before the score came in.' +
            "<br/> The number at the bottom is this player's projected final score."
          );
        }
      };
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
      this.$store.dispatch('confirmPositions').then(() => {
        this.$emit('gamesMoved');
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
        specialSlot: null,
        dropped: true
      };
    }
  },
  watch: {
    sortOrderMode: function () {
      if (!this.sortOrderMode) {
        this.sortBy = 'overallSlotNumber';
        this.sortDesc = false;
      }
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
  background-color: #aa1e1e;
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

.btable-player-game-row td,
th.btable-player-table-header {
  padding: 0.3rem;
}

th.btable-player-table-header {
  text-align: center;
  font-weight: bold;
}

.full-table-game-name-column {
  min-width: 300px;
}

.release-date-column {
  min-width: 200px;
}

.acquired-column {
  min-width: 250px;
}

.numeric-column {
  text-align: center;
  font-weight: bold;
  width: 80px;
}
</style>
