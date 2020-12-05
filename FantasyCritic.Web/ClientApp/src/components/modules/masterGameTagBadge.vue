<template>
  <span>
    <span class="badge badge-pill tag-badge" :id="'popover-target' + tag.name" v-bind:style="badgeColor">
      {{tag.readableName}}
    </span>
    <b-popover :target="'popover-target' + tag.name" triggers="hover" placement="top">
      <template #title class="popover-title">
        {{tag.readableName}}
      </template>
      {{tag.description}}
      <div v-if="tag.examples && tag.examples.length > 0">
        <h6>Examples</h6>
        <ul>
          <li v-for="example in tag.examples">{{example}}</li>
        </ul>
      </div>
    </b-popover>
  </span>
</template>
<script>

export default {
  props: ['tagName'],
  computed: {
    tag() {
        let allTags = this.$store.getters.allTags;
        let singleTag = _.filter(allTags, { 'name': this.tagName });
        return singleTag[0];
    },
    badgeColor() {
      let fontColor = 'white';

      var rgb = parseInt(this.tag.badgeColor, 16);   // convert rrggbb to decimal
      var r = (rgb >> 16) & 0xff;  // extract red
      var g = (rgb >>  8) & 0xff;  // extract green
      var b = (rgb >>  0) & 0xff;  // extract blue

      var luma = 0.2126 * r + 0.7152 * g + 0.0722 * b; // per ITU-R BT.709

      if (luma > 200) {
          fontColor = 'black';
      }

      return {
        backgroundColor: '#' + this.tag.badgeColor,
        color: fontColor
      }
    }
  }
};
</script>
<style>
  .popover-header{
    color: black;
  }
  .tag-badge{
    font-size: 15px;
    margin: 3px;
  }
</style>
