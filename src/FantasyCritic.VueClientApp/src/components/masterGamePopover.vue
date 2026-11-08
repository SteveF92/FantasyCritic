<template>
  <span v-if="masterGame">
    <a :id="popoverID" href="javascript:;" :class="{ 'text-white': showAsWarning }">
      {{ masterGame.gameName }}
    </a>
    <b-popover :target="popoverID" click placement="right" teleport-to="body" lazy class="master-game-popover">
      <div class="mg-popover">
        <masterGameSummary :master-game="masterGame"></masterGameSummary>
      </div>
    </b-popover>
  </span>
</template>

<script lang="ts">
import { defineComponent, useId, type PropType } from 'vue';

import { MasterGameYearViewModel } from '@/api/clients';
import MasterGameSummary from '@/components/masterGameSummary.vue';

export default defineComponent({
  setup() {
    return { popoverID: `mg-popover-${useId()}` };
  },
  components: {
    MasterGameSummary
  },
  props: {
    masterGame: { type: Object as PropType<MasterGameYearViewModel>, required: true },
    showAsWarning: { type: Boolean }
  }
});
</script>
<style scoped>
@media only screen and (max-width: 600px) {
  :global(.master-game-popover) {
    max-width: 300px;
  }
}

@media only screen and (min-width: 601px) {
  :global(.master-game-popover) {
    max-width: 500px;
  }
}
</style>
