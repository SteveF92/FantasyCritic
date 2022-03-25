<template>
  <span>
    <span v-if="masterGame">
      <a href="javascript:;" :class="{ 'text-white': currentlyIneligible }" :id="popoverID">
        {{ masterGame.gameName }}
      </a>
      <b-popover :target="popoverID" triggers="click blur" custom-class="master-game-popover">
        <div class="mg-popover">
          <masterGameSummary :masterGame="masterGame"></masterGameSummary>
        </div>
      </b-popover>
    </span>
  </span>
</template>

<script>
import MasterGameSummary from '@/components/masterGameSummary';

export default {
  components: {
    MasterGameSummary
  },
  props: {
    masterGame: Object,
    currentlyIneligible: Boolean
  },
  computed: {
    popoverID() {
      return `mg-popover-${this._uid}`;
    }
  }
};
</script>
<style scoped>
@media only screen and (max-width: 600px) {
  .master-game-popover {
    max-width: 300px;
  }
}

@media only screen and (min-width: 601px) {
  .master-game-popover {
    max-width: 500px;
  }
}
</style>
