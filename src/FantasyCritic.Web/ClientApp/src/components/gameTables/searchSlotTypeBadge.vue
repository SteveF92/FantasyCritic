<template>
  <span>
    <span v-if="gameSlot.counterPick">
      <span class="badge tag-badge counter-pick-badge flex-badge search-tag">CPK</span>
    </span>
    <span v-if="!gameSlot.counterPick && name">
      <span class="badge tag-badge regular-slot-badge flex-badge search-tag">
        {{ name }}
      </span>
    </span>
    <span v-if="!gameSlot.counterPick && !name">
      <template v-if="gameSlot.requiredTags.length === 1">
        <masterGameTagBadge :tag-name="gameSlot.requiredTags[0]" short no-popover class="flex-real-badge search-tag"></masterGameTagBadge>
      </template>
      <template v-else>
        <span class="badge tag-badge flex-badge search-tag" :style="getMultiBadgeColor(gameSlot.requiredTags)">FLX</span>
      </template>
    </span>
  </span>
</template>
<script>
import MasterGameTagBadge from '@/components/masterGameTagBadge';

export default {
  components: {
    MasterGameTagBadge
  },
  props: {
    gameSlot: { type: Object, required: true },
    name: { type: String, default: null }
  },
  methods: {
    getTag(tagName) {
      let allTags = this.$store.getters.allTags;
      let singleTag = _.filter(allTags, { name: tagName });
      return singleTag[0];
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

.flex-real-badge {
  margin-right: 5px;
}

.flex-badge {
  margin-right: 8px;
}

.regular-slot-badge {
  color: #ffffff;
  text-shadow: 0 0 2px black, 0 0 2px black, 0 0 2px black, 0 0 2px black;
  background-color: #cccccc;
}

.counter-pick-badge {
  background-color: #aa1e1e;
  color: white;
}

.search-tag {
  cursor: pointer;
}

.search-tag:hover {
  opacity: 0.8;
}
</style>
