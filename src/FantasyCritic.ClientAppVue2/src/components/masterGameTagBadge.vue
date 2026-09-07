<template>
  <span>
    <span v-if="!short" :id="popoverID" class="badge tag-badge mg-badge-text" :style="badgeColor">
      {{ tag.readableName }}
    </span>
    <span v-if="short" :id="popoverID" class="badge tag-badge mg-badge-text" :style="badgeColor">
      {{ tag.shortName }}
    </span>
    <template v-if="!noPopover">
      <b-popover :target="popoverID" triggers="hover" placement="right" custom-class="master-game-tag-popover">
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
<script>
export default {
  props: {
    tagName: { type: String, required: true },
    short: { type: Boolean },
    noPopover: { type: Boolean }
  },
  computed: {
    tag() {
      return this.$store.getters.allTags.find((x) => x.name === this.tagName);
    },
    badgeColor() {
      return {
        backgroundColor: '#' + this.tag.badgeColor
      };
    },
    popoverID() {
      return `mg-badge-popover-${this._uid}`;
    }
  }
};
</script>
<style>
.popover-header {
  color: black;
}

.master-game-tag-popover {
  z-index: 300000;
}
</style>
