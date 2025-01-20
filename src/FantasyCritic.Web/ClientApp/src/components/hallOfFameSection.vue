<template>
  <div class="hall-of-fame-section">
    <h3>{{ hallOfFameList.name }}</h3>
    <div class="cover-art-container">
      <HallOfFameGame
        v-for="game in hallOfFameList.games"
        :key="game.masterGame.masterGameID"
        :hallOfFameGame="game"
        :statName="hallOfFameList.statName"
        :statType="hallOfFameList.statType"></HallOfFameGame>
    </div>
  </div>
</template>

<script>
import HallOfFameGame from '@/components/hallOfFameGame.vue';
export default {
  components: {
    HallOfFameGame
  },
  props: {
    hallOfFameList: { type: Object, required: true }
  }
};
</script>
<style scoped>
.cover-art-container {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 6px;
  max-width: 1500px;
}
</style>
