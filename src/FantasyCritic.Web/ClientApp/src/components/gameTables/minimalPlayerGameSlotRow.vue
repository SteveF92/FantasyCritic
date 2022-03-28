<template>
  <tr :class="{ 'table-warning': gameSlot && !gameSlot.gameMeetsSlotCriteria }" class="minimal-game-row">
    <td>
      <gameNameColumn :game-slot="gameSlot" :has-special-slots="hasSpecialSlots" :supported-year="supportedYear"></gameNameColumn>
    </td>
    <template v-if="game">
      <td class="score-column">{{ game.criticScore | score }}</td>
      <template v-if="showProjections">
        <td v-if="game.fantasyPoints || !game.willRelease" class="score-column">{{ game.fantasyPoints | score }}</td>
        <td v-else class="score-column projected-text">~{{ gameSlot.projectedFantasyPoints | score }}</td>
      </template>
      <template v-else>
        <td class="score-column">{{ game.fantasyPoints | score }}</td>
      </template>
    </template>
    <template v-else>
      <td class="score-column"></td>
      <td class="score-column">{{ emptySlotScore }}</td>
    </template>
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
