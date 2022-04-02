<template>
  <div>
    <div v-for="gameSlot in gameSlots" :key="gameSlot.overallSlotNumber">
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
