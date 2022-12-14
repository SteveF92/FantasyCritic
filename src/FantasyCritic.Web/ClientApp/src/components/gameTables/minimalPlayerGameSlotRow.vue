<template>
  <tr :class="{ 'table-warning': gameSlot && !gameSlot.gameMeetsSlotCriteria }" class="minimal-game-row">
    <td>
      <gameNameColumn :game-slot="gameSlot" :has-special-slots="hasSpecialSlots" :supported-year="supportedYear" :counter-pick-deadline="leagueYear.counterPickDeadline"></gameNameColumn>
    </td>
    <td class="score-column">
      <template v-if="game">
        {{ game.criticScore | score(1) }}
      </template>
    </td>

    <td class="score-column" :class="{ 'projected-text': showProjectedScore }">
      <template v-if="scoreColumnMode === 'RealScore'">{{ game.fantasyPoints | score(1) }}</template>
      <template v-if="scoreColumnMode === 'ProjectedPoints'">~{{ gameSlot.projectedFantasyPoints | score(1) }}</template>
      <template v-if="scoreColumnMode === 'Empty'">{{ emptySlotScore | score(1) }}</template>
    </td>
  </tr>
</template>
<script>
import LeagueMixin from '@/mixins/leagueMixin';
import GameNameColumn from '@/components/gameTables/gameNameColumn';

export default {
  components: {
    GameNameColumn
  },
  mixins: [LeagueMixin],
  props: {
    gameSlot: { type: Object, required: true }
  },
  computed: {
    game() {
      return this.gameSlot.publisherGame;
    },
    scoreColumnMode() {
      if (this.game && (this.game.fantasyPoints || !this.game.willRelease)) {
        return 'RealScore';
      }
      if (this.showProjections) {
        return 'ProjectedPoints';
      }

      return 'Empty';
    },
    showProjectedScore() {
      return this.showProjections && !(this.game && (this.game.fantasyPoints || !this.game.willRelease));
    },
    emptySlotScore() {
      if (this.gameSlot.counterPick && this.yearFinished) {
        return '-15';
      }

      return '';
    }
  }
};
</script>
<style scoped>
tr {
  height: 40px;
}

.minimal-game-row {
  height: 35px;
}

.minimal-game-row td {
  font-size: 10pt;
}

.game-name-column {
  display: inline-flex;
  justify-content: space-between;
  width: 100%;
}

.game-status {
  color: #b1b1b1;
  font-style: italic;
  margin-left: auto;
}
</style>
