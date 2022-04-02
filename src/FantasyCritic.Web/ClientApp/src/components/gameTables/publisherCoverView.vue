<template>
  <div>
    <ul>
      <li v-for="gameSlot in gameSlots" :key="gameSlot.overallSlotNumber">
        <span v-if="gameSlot.publisherGame">
          {{ gameSlot.publisherGame.gameName }}
        </span>
        <span v-else>Empty</span>
      </li>
    </ul>
  </div>
</template>

<script>
import PublisherMixin from '@/mixins/publisherMixin';

export default {
  mixins: [PublisherMixin]
};
</script>
