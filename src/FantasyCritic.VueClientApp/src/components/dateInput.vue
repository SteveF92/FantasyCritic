<template>
  <input type="date" class="form-control" :value="modelValue" @input="update" />
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';

export default defineComponent({
  props: {
    modelValue: { type: String as PropType<Temporal.PlainDate | string | null>, default: null }
  },
  emits: {
    'update:modelValue': (_value: string | null) => true
  },
  methods: {
    update(event: Event) {
      const input = event.target as HTMLInputElement;
      this.$emit('update:modelValue', input.value || null);
    }
  }
});
</script>
