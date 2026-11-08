<template>
  <span>
    <span v-if="!short" :id="popoverID" class="badge tag-badge mg-badge-text" :style="badgeColor">
      {{ tag.readableName }}
    </span>
    <span v-if="short" :id="popoverID" class="badge tag-badge mg-badge-text" :style="badgeColor">
      {{ tag.shortName }}
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
import { defineComponent, useId } from 'vue';

import { MasterGameTagViewModel } from '@/api/clients';
import { useInterLeagueStore } from '@/store/interLeagueStore';

export default defineComponent({
  setup() {
    return { popoverID: `mg-popover-${useId()}` };
  },
  props: {
    tagName: { type: String, required: true },
    short: { type: Boolean },
    noPopover: { type: Boolean }
  },
  computed: {
    tag(): MasterGameTagViewModel {
      const interLeagueStore = useInterLeagueStore();
      return interLeagueStore.allTags.find((x) => x.name === this.tagName)!;
    },
    badgeColor(): { backgroundColor: string } {
      return {
        backgroundColor: '#' + this.tag.badgeColor
      };
    }
  }
});
</script>
<style>
.popover-header {
  color: black;
}

.master-game-tag-popover {
  z-index: 300000;
}
</style>
