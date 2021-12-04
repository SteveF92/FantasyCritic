<template>
  <span>
    <span v-if="!short" class="badge tag-badge" :id="'popover-target' + _uid" v-bind:style="badgeColor">
      {{tag.readableName}}
    </span>
    <span v-if="short" class="badge tag-badge" :id="'popover-target' + _uid" v-bind:style="badgeColor">
      {{tag.shortName}}
    </span>
    <b-popover :target="'popover-target' + _uid" triggers="hover" placement="right">
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
  props: ['tagName', 'short'],
  computed: {
    tag() {
        let allTags = this.$store.getters.allTags;
        let singleTag = _.filter(allTags, { 'name': this.tagName });
        return singleTag[0];
    },
    badgeColor() {
      return {
        backgroundColor: '#' + this.tag.badgeColor
      }
    }
  }
};
</script>
<style>
  .popover-header{
    color: black;
  }
</style>
