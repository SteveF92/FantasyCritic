<template>
  <tr v-bind:class="{ 'table-warning': gameSlot && !gameSlot.gameMeetsSlotCriteria, 'minimal-game-row': minimal }">
    <template v-if="game">
      <td>
        <gameNameColumn :game="game" :gameSlot="gameSlot" :hasSpecialSlots="hasSpecialSlots" :supportedYear="supportedYear"></gameNameColumn>
      </td>
      <template v-if="!minimal">
        <td>{{releaseDate}}</td>
        <td>{{acquireDate}}</td>
        <td class="score-column">{{game.criticScore | score(2)}}</td>
        <td class="score-column"><em>~{{game.masterGame.projectedFantasyPoints | score(2)}}</em></td>
        <td class="score-column">{{game.fantasyPoints | score(2)}}</td>
      </template>
      <template v-else>
        <td class="score-column">{{game.criticScore | score}}</td>
        <template v-if="advancedProjections">
          <td class="score-column" v-if="game.fantasyPoints || !game.willRelease">{{game.fantasyPoints | score}}</td>
          <td class="score-column" v-else><em>~{{gameSlot.advancedProjectedFantasyPoints | score}}</em></td>
        </template>
        <template v-else>
          <td class="score-column">{{game.fantasyPoints | score}}</td>
        </template>
      </template>
    </template>
    <template v-else>
      <td>
        <span class="game-name-column">
          <span class="game-name-side">
            <b-button variant="success" class="move-button" v-show="holdingGame && !gameSlot.counterPick" v-on:click="placeGame">Here</b-button>
            <slotTypeBadge v-if="hasSpecialSlots || gameSlot.counterPick" :gameSlot="gameSlot"></slotTypeBadge>
          </span>
          <span v-if="gameSlot.counterPick" class="game-status">
            Warning!
            <font-awesome-icon color="white" size="lg" icon="exclamation-triangle" v-b-popover.hover.top="emptyCounterpickText" />
          </span>
        </span>
      </td>
      <template v-if="!minimal">
        <td></td>
        <td></td>
        <td></td>
      </template>
      <td class="score-column"></td> 
      <td class="score-column">{{emptySlotScore}}</td>
    </template>
  </tr>
</template>
<script>
import MasterGamePopover from '@/components/modules/masterGamePopover';
import SlotTypeBadge from '@/components/modules/gameTables/slotTypeBadge';
import GlobalFunctions from '@/globalFunctions';
import GameNameColumn from '@/components/modules/gameTables/gameNameColumn';

export default {
  components: {
    MasterGamePopover,
    SlotTypeBadge,
    GameNameColumn
  },
  props: ['minimal', 'gameSlot', 'supportedYear', 'hasSpecialSlots'],
  computed: {
    game(){
      return this.gameSlot.publisherGame;
    },
    advancedProjections() {
      return this.$store.getters.advancedProjections;
    },
    releaseDate() {
      return GlobalFunctions.formatPublisherGameReleaseDate(this.game);
    },
    acquireDate() {
      return GlobalFunctions.formatPublisherGameAcquiredDate(this.game);
    },
    moveMode() {
      return this.$store.getters.moveMode;
    },
    holdingGame() {
      return this.$store.getters.holdingGame;
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
  },
  methods:{
    holdGame() {
      this.$store.commit('holdGame', this.gameSlot);
    },
    placeGame() {
      this.$store.dispatch('moveGame', this.gameSlot);
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
    color: #B1B1B1;
    font-style: italic;
    margin-left: auto;
  }
</style>
