<template>
  <input type="date" class="form-control" :value="modelValue" @input="update" />
</template>

<script>
export default {
  props: {
    modelValue: { type: String, default: null }
  },
  emits: ['update:modelValue'],
  methods: {
    update(event) {
      this.$emit('update:modelValue', event.target.value || null);
    }
  }
};
</script>
