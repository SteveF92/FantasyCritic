<template>
  <div class="hall-of-fame-section">
    <h2>{{ hallOfFameList.name }}</h2>
    <div class="hall-of-fame-container">
      <HallOfFameGame v-for="game in hallOfFameList.games" :key="game.masterGame.masterGameID" :hallOfFameGame="game"></HallOfFameGame>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';

import type { HallOfFameGameListResponse } from '@/api/clients';
import HallOfFameGame from '@/components/hallOfFameGame.vue';

export default defineComponent({
  components: {
    HallOfFameGame
  },
  props: {
    hallOfFameList: { type: Object as PropType<HallOfFameGameListResponse>, required: true }
  }
});
</script>

<style scoped>
.hall-of-fame-container {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 6px;
}
</style>
