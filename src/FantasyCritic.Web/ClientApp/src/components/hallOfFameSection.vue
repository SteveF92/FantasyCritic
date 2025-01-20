<template>
  <div class="hall-of-fame-section">
    <h3>{{ hallOfFameList.name }}</h3>
    <ul>
      <li v-for="game in hallOfFameList.games" :key="game.masterGame.masterGameID">
        <span>{{ game.masterGame.gameName }} - {{ hallOfFameList.statName }}: {{ formatStat(game.stat) }}</span>
      </li>
    </ul>
  </div>
</template>

<script>
import { roundNumber } from '@/globalFunctions';
export default {
  props: {
    hallOfFameList: { type: Object, required: true }
  },
  methods: {
    formatStat(stat) {
      if (this.hallOfFameList.statType === 'Score') {
        return roundNumber(stat, 2);
      }
      return stat;
    }
  }
};
</script>
