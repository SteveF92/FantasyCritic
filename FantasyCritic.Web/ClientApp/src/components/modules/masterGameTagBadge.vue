<template>
  <div>
    <span class="badge badge-pill badge-primary" id="popover-target-1">{{value.readableName}}</span>
    <b-popover target="popover-target-1" triggers="hover" placement="top">
      <template #title>
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
  </div>
</template>
<script>

export default {
  props: ['value'],
  methods: {
    updateValue: function (value) {
      this.$emit('input', value)
    }
  }
};
</script>
