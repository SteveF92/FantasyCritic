<template>
  <div class="cover-art-container">
    <div v-for="gameSlot in gameSlots" :key="gameSlot.overallSlotNumber">
      <publisherCoverViewSlot :game-slot="gameSlot"></publisherCoverViewSlot>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';

import { usePublisherStore } from '@/store/publisherStore';
import PublisherCoverViewSlot from '@/components/gameTables/publisherCoverViewSlot.vue';

export default defineComponent({
  components: {
    PublisherCoverViewSlot
  },
  computed: {
    ...mapState(usePublisherStore, ['gameSlots'])
  }
});
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
