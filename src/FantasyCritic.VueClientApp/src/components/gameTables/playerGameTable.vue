<template>
  <b-table :items="tableRows" :fields="tableFields" bordered responsive striped v-model:sort-by="sortBy" primary-key="overallSlotNumber" tbody-tr-class="btable-player-game-row">
    <template #head(projectedFantasyPoints)>
      Projected
      <br />
      points
      <b-popover placement="top" title="Projected Points">
        <template #target>
          <font-awesome-icon color="black" size="lg" icon="info-circle" />
        </template>
        This is the amount of fantasy points that our algorithm believes this game will result in. If the game already has a critic score, then this was our final projection before the score came in.
        <br />
        The number at the bottom is this player's projected final score, which is not the sum of all the numbers above it. Instead, it is the sum of three things:
        <ul>
          <li>Any real points acquired from games that have already released.</li>
          <li>Expected points from games releasing soon (games that have a critic score but have not yet released).</li>
          <li>Projected points for any remaining games.</li>
        </ul>
      </b-popover>
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
        {{ formatScore(data.item.publisherGame.criticScore, 2) }}
      </template>
    </template>

    <template #cell(projectedFantasyPoints)="data">~{{ formatScore(data.item.projectedFantasyPoints, 2) }}</template>

    <template #cell(publisherGame.fantasyPoints)="data">
      <template v-if="data.item.publisherGame">
        {{ formatScore(data.item.publisherGame.fantasyPoints, 2) }}
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

    <template #custom-foot>
      <b-tr>
        <b-td class="total-description">
          <span class="total-description-text">Total Fantasy Points</span>
        </b-td>
        <b-td></b-td>
        <b-td></b-td>
        <b-td class="average-critic-column">{{ formatScore(publisher.averageCriticScore, 2) }} (Average)</b-td>
        <b-td class="total-column projected-text bg-info">~{{ formatScore(publisher.totalProjectedPoints, 2) }}</b-td>
        <b-td class="total-column bg-success">{{ formatScore(publisher.totalFantasyPoints, 2) }}</b-td>
      </b-tr>
    </template>
  </b-table>
</template>

<script>
import { mapState } from 'pinia';

import { formatPublisherGameAcquiredDate, formatPublisherGameReleaseDate, formatPublisherGameRemovedDate, numericComparer } from '@/globalFunctions';
import { usePublisherStore } from '@/store/publisherStore';
import { formatScore } from '@/utilities/formatters';
import GameNameColumn from '@/components/gameTables/gameNameColumn.vue';

export default {
  components: {
    GameNameColumn
  },
  data() {
    return {
      sortBy: [{ key: 'overallSlotNumber', order: 'asc' }]
    };
  },
  computed: {
    ...mapState(usePublisherStore, ['publisher', 'leagueYear', 'gameSlots', 'sortOrderMode', 'includeRemovedInSorted']),
    tableItems() {
      if (!this.sortOrderMode) {
        return this.gameSlots;
      }
      let slotsWithGames = this.publisher.gameSlots.filter((x) => x.publisherGame);
      if (this.includeRemovedInSorted) {
        let fakeFormerSlots = [];

        let overallSlotNumber = Math.max(this.publisher.gameSlots.map((x) => x.overallSlotNumber));
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
        if (rows[i].publisherGame && rows[i].showWarningRow && !rows[i].gameIsCancelled) {
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
        {
          key: 'publisherGame.masterGame.criticScore',
          label: 'Critic Score',
          sortable: this.sortOrderMode,
          thClass: ['bg-primary', 'btable-player-table-header'],
          class: ['numeric-column'],
          sortCompare: numericComparer
        },
        {
          key: 'projectedFantasyPoints',
          sortable: this.sortOrderMode,
          thClass: ['bg-primary', 'btable-player-table-header'],
          class: ['numeric-column', 'projected-text'],
          sortCompare: numericComparer
        },
        {
          key: 'publisherGame.fantasyPoints',
          label: 'Fantasy Points',
          sortable: this.sortOrderMode,
          thClass: ['bg-primary', 'btable-player-table-header'],
          class: ['numeric-column'],
          sortCompare: numericComparer
        }
      ];
    }
  },
  watch: {
    sortOrderMode: function () {
      if (!this.sortOrderMode) {
        this.sortBy = [{ key: 'overallSlotNumber', order: 'asc' }];
      }
    }
  },
  methods: {
    formatScore,
    getReleaseDate(publisherGame) {
      return formatPublisherGameReleaseDate(publisherGame);
    },
    getAcquiredDate(publisherGame) {
      return formatPublisherGameAcquiredDate(publisherGame, this.leagueYear.drafts);
    },
    getRemovedDate(publisherGame) {
      return formatPublisherGameRemovedDate(publisherGame);
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
