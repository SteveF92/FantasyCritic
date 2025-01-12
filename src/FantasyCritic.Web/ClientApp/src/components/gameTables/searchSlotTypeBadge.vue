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
<script>
import _ from 'lodash';

import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

export default {
  components: {
    MasterGameTagBadge
  },
  props: {
    gameSlot: { type: Object, required: true },
    name: { type: String, default: null },
    selected: { type: Boolean, default: false }
  },
  methods: {
    getTag(tagName) {
      return this.$store.getters.allTags.find((x) => x.name === tagName);
    },
    getMultiBadgeColor(tagNames) {
      let colorString = '';
      let stripeWidth = 100.0 / tagNames.length;
      for (let i = 0; i < tagNames.length; i++) {
        let tag = this.getTag(tagNames[i]);
        let badgeColor = tag.badgeColor;

        colorString += `#${badgeColor} ${i * stripeWidth}%, #${badgeColor} ${(i + 1) * stripeWidth}%,`;
      }

      colorString = colorString.substring(0, colorString.length - 1);
      let backGroundString = `linear-gradient(135deg, ${colorString})`;

      return {
        backgroundImage: backGroundString,
        color: 'white',
        'text-shadow': '0 0 2px black, 0 0 2px black, 0 0 2px black, 0 0 2px black'
      };
    },
    getFlexText(requiredTags) {
      let tagElements = requiredTags.map((x) => `<li>${this.getTag(x).readableName}</li>`);
      let joinedTagElements = tagElements.join('');
      return {
        html: true,
        title: () => {
          return 'Flex Slot';
        },
        content: () => {
          return `<p>A game in this slot must have at least one of these tags:</p><ul>${joinedTagElements}</ul>`;
        }
      };
    }
  }
};
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
