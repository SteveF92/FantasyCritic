<template>
  <span class="badge badge-pill badge-primary">{{value.readableName}}</span>
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
