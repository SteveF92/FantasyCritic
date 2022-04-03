<template>
  <div>
    <div class="cover-art-header">
      <a href="https://ggapp.io/" target="_blank">
        <strong>All Images Provided by GG|</strong>
      </a>
    </div>
    <div class="cover-art-container">
      <div v-for="gameSlot in gameSlots" :key="gameSlot.overallSlotNumber">
        <publisherCoverViewSlot :game-slot="gameSlot"></publisherCoverViewSlot>
      </div>
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
.cover-art-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
  padding-left: 10px;
}

.cover-art-container {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 6px;
  max-width: 1500px;
  padding: 10px;
}
</style>
