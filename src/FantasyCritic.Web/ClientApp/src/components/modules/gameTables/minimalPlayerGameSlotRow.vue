<template>
  <tr v-bind:class="{ 'table-warning': gameSlot && !gameSlot.gameMeetsSlotCriteria }" class="minimal-game-row">
    <td>
      <gameNameColumn :gameSlot="gameSlot" :hasSpecialSlots="hasSpecialSlots" :supportedYear="supportedYear"></gameNameColumn>
    </td>
    <template v-if="game">
      <td class="score-column">{{ game.criticScore | score }}</td>
      <template v-if="advancedProjections">
        <td class="score-column" v-if="game.fantasyPoints || !game.willRelease">{{ game.fantasyPoints | score }}</td>
        <td class="score-column projected-text" v-else>~{{ gameSlot.advancedProjectedFantasyPoints | score }}</td>
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
import MasterGamePopover from '@/components/modules/masterGamePopover';
import SlotTypeBadge from '@/components/modules/gameTables/slotTypeBadge';
import GameNameColumn from '@/components/modules/gameTables/gameNameColumn';

export default {
  components: {
    MasterGamePopover,
    SlotTypeBadge,
    GameNameColumn
  },
  props: ['gameSlot', 'supportedYear', 'hasSpecialSlots'],
  computed: {
    game() {
      return this.gameSlot.publisherGame;
    },
    advancedProjections() {
      return this.$store.getters.advancedProjections;
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
