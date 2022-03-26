<template>
  <span>
    <span v-if="!gameSlot.dropped && gameSlot.specialSlot">
      <template v-if="gameSlot.specialSlot.requiredTags.length === 1">
        <masterGameTagBadge :tag-name="gameSlot.specialSlot.requiredTags[0]" short class="slot-badge"></masterGameTagBadge>
      </template>
      <template v-else>
        <span v-b-popover.hover.focus.top="getFlexText(gameSlot.specialSlot.requiredTags)" class="badge tag-badge slot-badge" :style="getMultiBadgeColor(gameSlot.specialSlot.requiredTags)">FLX</span>
      </template>
    </span>
    <span v-if="!gameSlot.dropped && !gameSlot.specialSlot && !gameSlot.counterPick">
      <span v-b-popover.hover.focus.top="regularText" class="badge tag-badge regular-slot-badge slot-badge">REG</span>
    </span>
    <span v-if="gameSlot.dropped">
      <span v-b-popover.hover.focus.top="droppedText" class="badge tag-badge dropped-badge slot-badge">DRP</span>
    </span>
    <span v-if="!gameSlot.specialSlot && gameSlot.counterPick">
      <span v-b-popover.hover.focus.top="counterPickText" class="badge tag-badge counter-pick-badge slot-badge">CPK</span>
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
    gameSlot: Object
  },
  computed: {
    regularText() {
      return {
        html: true,
        title: () => {
          return 'Regular Slot';
        },
        content: () => {
          return 'This slot has no special requirements, so it follows the normal eligibility rules for the league.';
        }
      };
    },
    counterPickText() {
      return {
        html: true,
        title: () => {
          return 'Counter Pick';
        },
        content: () => {
          return 'This slot is for counter picks, which are bets against a game. See the FAQ for more details.';
        }
      };
    },
    droppedText() {
      return {
        html: true,
        title: () => {
          return 'Dropped';
        },
        content: () => {
          return "This game is no longer on this publisher's lineup.";
        }
      };
    }
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
        'text-shadow': '1px 0 0 #000, 0 -1px 0 #000, 0 1px 0 #000, -1px 0 0 #000'
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

.slot-badge {
  margin-right: 8px;
}

.regular-slot-badge {
  background-color: #cccccc;
}

.counter-pick-badge {
  background-color: #aa1e1e;
}

.dropped-badge {
  background-color: black;
}
</style>
