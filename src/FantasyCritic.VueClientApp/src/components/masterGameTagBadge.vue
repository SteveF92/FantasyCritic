<template>
  <span>
    <span :id="popoverID" class="badge tag-badge mg-badge-text" :style="badgeColor">
      {{ short ? tag.shortName : tag.readableName }}
      <button v-if="remove" type="button" class="tag-remove" :aria-label="`Remove ${tag.readableName}`" @click="remove">&times;</button>
    </span>
    <template v-if="!noPopover">
      <b-popover :target="popoverID" placement="right" class="master-game-tag-popover">
        <template #title>
          {{ tag.readableName }}
        </template>
        {{ tag.description }}
        <div v-if="tag.examples && tag.examples.length > 0">
          <h6>Examples</h6>
          <ul>
            <li v-for="example in tag.examples" :key="example">{{ example }}</li>
          </ul>
        </div>
      </b-popover>
    </template>
  </span>
</template>

<script lang="ts">
import { defineComponent, useId, type PropType } from 'vue';

import { MasterGameTagViewModel } from '@/api/clients';
import { useInterLeagueStore } from '@/store/interLeagueStore';

export default defineComponent({
  setup() {
    return { popoverID: `mg-popover-${useId()}` };
  },
  props: {
    tagName: { type: String, required: true },
    short: { type: Boolean },
    noPopover: { type: Boolean },
    remove: { type: Function as PropType<() => void> }
  },
  computed: {
    tag(): MasterGameTagViewModel {
      const interLeagueStore = useInterLeagueStore();
      const tag = interLeagueStore.allTags.find((x) => x.name === this.tagName);
      if (!tag) {
        throw new Error(`Unknown master game tag: ${this.tagName}`);
      }
      return tag;
    },
    badgeColor(): { backgroundColor: string } {
      return {
        backgroundColor: '#' + this.tag.badgeColor
      };
    }
  }
});
</script>
<style scoped>
/* b-popover teleports the popover out of this component, so it doesn't get the scope. The z-index keeps a
   tag's popover above the master game popover the tag is shown in. */
:global(.master-game-tag-popover) {
  z-index: 300000;
}

.tag-remove {
  margin-left: 0.25em;
  padding: 0;
  border: none;
  background: none;
  color: inherit;
  font: inherit;
  line-height: 1;
}
</style>
