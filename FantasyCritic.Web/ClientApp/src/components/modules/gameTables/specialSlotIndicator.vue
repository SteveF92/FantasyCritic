<template>
  <div>
    <div v-if="specialSlot">
      <span v-for="(tag, index) in specialSlot.requiredTags">
        <masterGameTagBadge :tagName="specialSlot.requiredTags[index]"></masterGameTagBadge>
      </span>
    </div>
    <div v-else>

    </div>
  </div>
</template>
<script>
import MasterGameTagBadge from '@/components/modules/masterGameTagBadge';

export default {
  props: ['specialSlot'],
  components: {
    MasterGameTagBadge
  }
};
</script>
