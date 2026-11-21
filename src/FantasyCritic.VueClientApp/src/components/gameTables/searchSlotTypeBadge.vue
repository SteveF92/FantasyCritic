<template>
  <span class="highlight-area" :class="{ highlight: selected }">
    <span v-if="name">
      <span class="badge tag-badge regular-slot-badge search-tag">
        {{ name }}
      </span>
    </span>
    <span v-if="!name">
      <template v-if="gameSlot.requiredTags.length === 1">
        <masterGameTagBadge :tag-name="gameSlot.requiredTags[0]" short no-popover class="search-tag"></masterGameTagBadge>
      </template>
      <template v-else>
        <span class="badge tag-badge search-tag" :style="getMultiBadgeColor(gameSlot.requiredTags)">FLX</span>
      </template>
    </span>
  </span>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';

import type { PublisherSingleSlotRequirementsViewModel } from '@/api/clients';
import { getMultiBadgeColor } from '@/utilities/masterGameTags';
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

export default defineComponent({
  setup() {
    return { getMultiBadgeColor };
  },
  components: {
    MasterGameTagBadge
  },
  props: {
    gameSlot: { type: Object as PropType<PublisherSingleSlotRequirementsViewModel>, required: true },
    name: { type: String as PropType<string | null>, default: null },
    selected: { type: Boolean, default: false }
  }
});
</script>

<style scoped>
.popover-header {
  color: black;
}

.regular-slot-badge {
  color: #ffffff;
  text-shadow:
    0 0 2px black,
    0 0 2px black,
    0 0 2px black,
    0 0 2px black;
  background-color: #cccccc;
}

.search-tag {
  cursor: pointer;
}

.search-tag:hover {
  opacity: 0.8;
}

.highlight-area {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 5px;
  border-radius: 10px;
}

.highlight {
  background-color: #000000;
}
</style>
