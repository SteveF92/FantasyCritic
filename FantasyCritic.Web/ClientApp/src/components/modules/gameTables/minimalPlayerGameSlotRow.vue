<template>
  <tr v-bind:class="{ 'table-warning': gameSlot && !gameSlot.gameMeetsSlotCriteria }" class="minimal-game-row">
    <template v-if="game">
      <td>
        <gameNameColumn :gameSlot="gameSlot" :hasSpecialSlots="hasSpecialSlots" :supportedYear="supportedYear"></gameNameColumn>
      </td>
      <td class="score-column">{{game.criticScore | score}}</td>
      <template v-if="advancedProjections">
        <td class="score-column" v-if="game.fantasyPoints || !game.willRelease">{{game.fantasyPoints | score}}</td>
        <td class="score-column" v-else><em>~{{gameSlot.advancedProjectedFantasyPoints | score}}</em></td>
      </template>
      <template v-else>
        <td class="score-column">{{game.fantasyPoints | score}}</td>
      </template>
    </template>
    <template v-else>
      <td>
        <span class="game-name-column">
          <span class="game-name-side">
            <slotTypeBadge v-if="hasSpecialSlots || gameSlot.counterPick" :gameSlot="gameSlot"></slotTypeBadge>
          </span>
          <span v-if="gameSlot.counterPick" class="game-status">
            Warning!
            <font-awesome-icon color="white" size="lg" icon="exclamation-triangle" v-b-popover.hover.top="emptyCounterpickText" />
          </span>
        </span>
      </td>
      <td class="score-column"></td> 
      <td class="score-column">{{emptySlotScore}}</td>
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
    game(){
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
    },
    emptyCounterpickText() {
      return {
        html: true,
        title: () => {
          return "Warning!";
        },
        content: () => {
          return 'If you do not fill this slot by the end of the year, it will count as -15 points. <br/> <br/>' +
            'See the FAQ for a full explanation.';
        }
      }
    },
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
    color: #B1B1B1;
    font-style: italic;
    margin-left: auto;
  }
</style>
