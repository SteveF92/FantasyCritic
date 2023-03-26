<template>
  <div>
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
      <template #head(projectedFantasyPoints)>
        Projected
        <br />
        points
        <font-awesome-icon v-b-popover.hover.focus.top="projectedPointsText" color="black" size="lg" icon="info-circle" />
      </template>

      <template #head(publisherGame.timestamp)>
        <span v-show="!sortOrderMode || !includeRemovedInSorted">Acquired</span>
        <span v-show="sortOrderMode && includeRemovedInSorted">Acquired/Dropped</span>
      </template>

      <template #cell(publisherGame.gameName)="data">
        <gameNameColumn
          :game-slot="data.item"
          :has-special-slots="leagueYear.settings.hasSpecialSlots"
          :supported-year="leagueYear.supportedYear"
          :counter-pick-deadline="leagueYear.settings.counterPickDeadline"></gameNameColumn>
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

      <template #cell(projectedFantasyPoints)="data">~{{ data.item.projectedFantasyPoints | score(2) }}</template>

      <template #cell(publisherGame.fantasyPoints)="data">
        <template v-if="data.item.publisherGame">
          {{ data.item.publisherGame.fantasyPoints | score(2) }}
        </template>
        <template v-if="!data.item.publisherGame && data.item.counterPick && leagueYear.supportedYear.finished">-15</template>
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
import GlobalFunctions from '@/globalFunctions';
import GameNameColumn from '@/components/gameTables/gameNameColumn.vue';
import PublisherMixin from '@/mixins/publisherMixin.js';
import _ from 'lodash';

export default {
  components: {
    GameNameColumn
  },
  mixins: [PublisherMixin],
  data() {
    return {
      sortBy: 'overallSlotNumber',
      sortDesc: false
    };
  },
  computed: {
    tableItems() {
      if (!this.sortOrderMode) {
        return this.gameSlots;
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
        if (rows[i].publisherGame && !rows[i].gameMeetsSlotCriteria) {
          rows[i]._rowVariant = 'warning';
        } else {
          rows[i]._rowVariant = null;
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
        { key: 'projectedFantasyPoints', sortable: this.sortOrderMode, thClass: ['bg-primary', 'btable-player-table-header'], class: ['numeric-column', 'projected-text'] },
        { key: 'publisherGame.fantasyPoints', label: 'Fantasy Points', sortable: this.sortOrderMode, thClass: ['bg-primary', 'btable-player-table-header'], class: ['numeric-column'] }
      ];
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
            "<br/> The number at the bottom is this player's projected final score, which is not the sum of all the numbers above it." +
            ' Instead, it is the sum of three things: ' +
            '<ul>' +
            '<li>Any real points acquired from games that have already released.</li>' +
            '<li>Expected points from games releasing soon (games that have a critic score but have not yet released).</li>' +
            '<li>Projected points for any remaining games.</li>' +
            '</ul>'
          );
        }
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
  },
  methods: {
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
  }
};
</script>
<style scoped>
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
