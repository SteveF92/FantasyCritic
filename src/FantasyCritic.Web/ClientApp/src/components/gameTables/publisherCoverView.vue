<template>
  <div v-if="tagsLoaded" class="cover-art-container">
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
  mixins: [PublisherMixin],
  computed: {
    tagsLoaded() {
      return !!this.$store.getters.allTags;
    }
  }
};
</script>
<style scoped>
.cover-art-container {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  max-width: 1500px;
  gap: 10px;
}
.cover-art-child {
  width: 185px;
  height: 306px;
}
</style>
