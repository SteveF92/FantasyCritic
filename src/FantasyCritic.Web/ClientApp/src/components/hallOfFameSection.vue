<template>
  <div class="hall-of-fame-section">
    <h2>{{ hallOfFameList.name }}</h2>
    <div class="hall-of-fame-container">
      <HallOfFameGame v-for="game in hallOfFameList.games" :key="game.masterGame.masterGameID" :hallOfFameGame="game" :statTypes="hallOfFameList.statTypes"></HallOfFameGame>
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
.hall-of-fame-container {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 6px;
}
</style>
