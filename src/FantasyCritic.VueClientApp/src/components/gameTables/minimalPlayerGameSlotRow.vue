<template>
  <tr :class="{ 'table-warning': gameSlot.showWarningRow }" class="minimal-game-row">
    <td>
      <gameNameColumn :league-year="requiredLeagueYear" :game-slot="gameSlot"></gameNameColumn>
    </td>
    <td class="score-column">
      <template v-if="scoreColumnMode === 'RealScore'">{{ formatScore(game?.criticScore, decimalsToShow) }}</template>
      <template v-if="scoreColumnMode === 'Empty'"></template>
    </td>

    <td class="score-column" :class="{ 'projected-text': showProjectedScore }">
      <template v-if="scoreColumnMode === 'RealScore'">{{ formatScore(game?.fantasyPoints, decimalsToShow) }}</template>
      <template v-if="scoreColumnMode === 'ProjectedPoints'">~{{ formatScore(gameSlot.projectedFantasyPoints, decimalsToShow) }}</template>
      <template v-if="scoreColumnMode === 'Empty'">{{ emptySlotScore }}</template>
    </td>
  </tr>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import { mapState } from 'pinia';

import type { PublisherGameViewModel, PublisherSlotViewModel } from '@/api/clients';
import { useAuthStore } from '@/store/authStore';
import { useLeagueStore } from '@/store/leagueStore';
import { formatScore } from '@/utilities/formatters';
import GameNameColumn from '@/components/gameTables/gameNameColumn.vue';

type ScoreColumnMode = 'RealScore' | 'ProjectedPoints' | 'Empty';

export default defineComponent({
  components: {
    GameNameColumn
  },
  props: {
    gameSlot: { type: Object as PropType<PublisherSlotViewModel>, required: true }
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear', 'showProjections', 'requiredSupportedYear']),
    ...mapState(useAuthStore, ['decimalsToShow']),
    game(): PublisherGameViewModel | undefined {
      return this.gameSlot.publisherGame;
    },
    scoreColumnMode(): ScoreColumnMode {
      if (this.game && (this.game.fantasyPoints || !this.game.willRelease || this.game.criticScore || this.game.released || this.requiredSupportedYear.finished)) {
        return 'RealScore';
      }
      if (this.showProjections) {
        return 'ProjectedPoints';
      }

      return 'Empty';
    },
    showProjectedScore(): boolean {
      return this.showProjections && !(this.game && (this.game.fantasyPoints || !this.game.willRelease));
    },
    emptySlotScore(): string {
      if (this.gameSlot.counterPick && this.requiredSupportedYear.finished) {
        return '-15';
      }

      return '';
    }
  },
  methods: {
    formatScore
  }
});
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

.score-column {
  width: 30px;
  text-align: center;
  font-weight: bold;
  height: 20px;
  line-height: 20px;
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
