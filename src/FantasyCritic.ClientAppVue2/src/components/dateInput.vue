<template>
  <input type="date" class="form-control" :value="value" @input="update" />
</template>

<script>
export default {
  props: {
    value: { type: String, default: null }
  },
  methods: {
    update(event) {
      this.$emit('input', event.target.value || null);
    }
  }
};
</script>
