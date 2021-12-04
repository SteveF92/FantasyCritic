<template>
  <span>
    <span v-if="gameSlot.specialSlot">
      <template v-if="gameSlot.specialSlot.requiredTags.length === 1">
        <span class="badge badge-pill tag-badge" v-bind:style="getBadgeColor(gameSlot.specialSlot.requiredTags[0]) ">
          {{getTag(gameSlot.specialSlot.requiredTags[0]).shortName}}
        </span>
      </template>
      <template v-else>
        <span class="badge badge-pill tag-badge" v-bind:style="getMultiBadgeColor(gameSlot.specialSlot.requiredTags)">
          Flex
        </span>
      </template>
    </span>
    <span v-if="!gameSlot.specialSlot && !gameSlot.counterPick">
      <span class="badge badge-pill tag-badge open-badge">
        Open
      </span>
    </span>
    <span v-if="!gameSlot.specialSlot && gameSlot.counterPick">
      <span class="badge badge-pill tag-badge counter-pick-badge">
        CP
      </span>
    </span>
  </span>
</template>
<script>
export default {
  props: ['gameSlot'],
  methods: {
    getTag(tagName) {
      let allTags = this.$store.getters.allTags;
      let singleTag = _.filter(allTags, { 'name': tagName });
      return singleTag[0];
    },
    getBadgeColor(tagName) {
      let fontColor = 'white';
      let tag = this.getTag(tagName);
      var rgb = parseInt(tag.badgeColor, 16);   // convert rrggbb to decimal
      var r = (rgb >> 16) & 0xff;  // extract red
      var g = (rgb >> 8) & 0xff;  // extract green
      var b = (rgb >> 0) & 0xff;  // extract blue

      var luma = 0.2126 * r + 0.7152 * g + 0.0722 * b; // per ITU-R BT.709

      if (luma > 165) {
        fontColor = 'black';
      }

      return {
        backgroundColor: '#' + tag.badgeColor,
        color: fontColor
      }
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
