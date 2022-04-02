<template>
  <div class="cover-art-container">
    <div v-for="gameSlot in gameSlots" :key="gameSlot.overallSlotNumber" class="cover-art-child">
      <publisherCoverViewSlot :game-slot="gameSlot"></publisherCoverViewSlot>
    </div>
  </div>
</template>

<script>
import PublisherMixin from '@/mixins/publisherMixin';
import PublisherCoverViewSlot from '@/components/gameTables/publisherCoverViewSlot';

export default {
  components: {
    PublisherCoverViewSlot
  },
  mixins: [PublisherMixin]
};
</script>
<style scoped>
.cover-art-container {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}
.cover-art-child {
  flex: 1 0 17%;
  background-color: blue;
  padding: 10px;
}
</style>
