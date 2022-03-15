<template>
  <span>
    <span v-if="masterGame">
      <b-popover :target="`mg-popover-${_uid}`" triggers="click blur">
        <div class="mg-popover">
          <masterGameSummary :masterGame="masterGame"></masterGameSummary>
        </div>
      </b-popover>
      <a href="javascript:;" :class="{ 'text-white': currentlyIneligible }" :id="`mg-popover-${_uid}`">
        {{ masterGame.gameName }}
      </a>
    </span>
  </span>
</template>

<script>
import MasterGameSummary from '@/components/masterGameSummary';

export default {
  data() {
    return {
      error: ''
    };
  },
  components: {
    MasterGameSummary
  },
  props: ['masterGame', 'currentlyIneligible'],
  methods: {
    closePopover() {
      this.$refs.gamePopoverRef.doClose();
    }
  }
};
</script>
<style>
.popover {
  max-width: 500px;
}
</style>
