<template>
  <span>
    <span class="badge badge-pill" :id="'popover-target' + value.name" v-bind:style="badgeColor">
      {{value.readableName}}
    </span>
    <b-popover :target="'popover-target' + value.name" triggers="hover" placement="top">
      <template #title class="popover-title">
        {{value.readableName}}
      </template>
      {{value.description}}
      <div v-if="value.examples && value.examples.length > 0">
        <h6>Examples</h6>
        <ul>
          <li v-for="example in value.examples">{{example}}</li>
        </ul>
      </div>
    </b-popover>
  </span>
</template>
<script>

export default {
  props: ['value'],
  methods: {
    updateValue: function (value) {
      this.$emit('input', value)
    }
  },
  computed: {
    badgeColor() {
      return {
        backgroundColor: '#' + this.value.badgeColor
      };
    }
  }
};
</script>
<style>
  .popover-header{
    color: black;
  }
</style>
