<template>
  <div>
    <a href="https://ggapp.io/" target="_blank">
      <strong>
        All Images Provided by GG|
        <font-awesome-icon icon="external-link-alt" />
      </strong>
    </a>
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
.cover-art-container {
  margin-top: 10px;
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  max-width: 1500px;
  gap: 10px;
}
</style>
