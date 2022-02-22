<template>
  <span>
    <span v-if="!gameSlot.dropped && gameSlot.specialSlot">
      <template v-if="gameSlot.specialSlot.requiredTags.length === 1">
        <masterGameTagBadge :tagName="gameSlot.specialSlot.requiredTags[0]" short="true" class="slot-badge"></masterGameTagBadge>
      </template>
      <template v-else>
        <span class="badge tag-badge slot-badge" v-bind:style="getMultiBadgeColor(gameSlot.specialSlot.requiredTags)"
              v-b-popover.hover.top="getFlexText(gameSlot.specialSlot.requiredTags)">
          FLX
        </span>
      </template>
    </span>
    <span v-if="!gameSlot.dropped && !gameSlot.specialSlot && !gameSlot.counterPick">
      <span class="badge tag-badge regular-slot-badge slot-badge" v-b-popover.hover.top="regularText">
        REG
      </span>
    </span>
    <span v-if="!gameSlot.dropped && !gameSlot.specialSlot && gameSlot.counterPick">
      <span class="badge tag-badge counter-pick-badge slot-badge" v-b-popover.hover.top="counterPickText">
        CP
      </span>
    </span>
    <span v-if="gameSlot.dropped">
      <span class="badge tag-badge dropped-badge slot-badge" v-b-popover.hover.top="droppedText">
        DRP
      </span>
    </span>
  </span>
</template>
<script>
import MasterGameTagBadge from '@/components/modules/masterGameTagBadge';

export default {
  props: ['gameSlot'],
  components: {
    MasterGameTagBadge
  },
  computed: {
    regularText() {
      return {
        html: true,
        title: () => {
          return "Regular Slot";
        },
        content: () => {
          return 'This slot has no special requirements, so it follows the normal eligibility rules for the league.';
        }
      }
    },
    counterPickText() {
      return {
        html: true,
        title: () => {
          return "Counter Pick";
        },
        content: () => {
          return 'This slot is for counter picks, which are bets against a game. See the FAQ for more details.';
        }
      }
    },
    droppedText() {
      return {
        html: true,
        title: () => {
          return "Dropped";
        },
        content: () => {
          return "This game is no longer on this publisher's lineup.";
        }
      }
    }
  },
  methods: {
    getTag(tagName) {
      let allTags = this.$store.getters.allTags;
      let singleTag = _.filter(allTags, { 'name': tagName });
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
      }
    },
    getFlexText(requiredTags) {
      let tagElements = requiredTags.map(x => `<li>${this.getTag(x).readableName}</li>`);
      let joinedTagElements = tagElements.join('');
      return {
        html: true,
        title: () => {
          return "Flex Slot";
        },
        content: () => {
          return `<p>A game in this slot must have at least one of these tags:</p><ul>${joinedTagElements}</ul>`;
        }
      }
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
    background-color: #AA1E1E;
  }

  .dropped-badge {
    background-color: black;
  }
</style>
