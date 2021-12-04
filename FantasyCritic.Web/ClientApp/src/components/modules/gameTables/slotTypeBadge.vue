<template>
  <span>
    <span v-if="gameSlot.specialSlot">
      <template v-if="gameSlot.specialSlot.requiredTags.length === 1">
        <masterGameTagBadge :tagName="gameSlot.specialSlot.requiredTags[0]" short="true"></masterGameTagBadge>
      </template>
      <template v-else>
        <span class="badge badge-pill tag-badge" v-bind:style="getMultiBadgeColor(gameSlot.specialSlot.requiredTags)"
              v-b-popover.hover="getFlexText(gameSlot.specialSlot.requiredTags)">
          Flex
        </span>
      </template>
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

  .tag-badge {
    font-size: 13px;
    margin: 3px;
  }

  .open-badge {
    color: #fff;
    text-shadow: 1px 0 0 #000, 0 -1px 0 #000, 0 1px 0 #000, -1px 0 0 #000;
    background-image: linear-gradient(
      135deg, 
      orange 0%,
      orange 25%,
      skyblue 25%,
      skyblue 50%,
      green 50%,
      green 75%,
      firebrick 75%,
      firebrick 100%);
  }

  .counter-pick-badge{
    background-color: #f55442;
    color: white;
  }
</style>
