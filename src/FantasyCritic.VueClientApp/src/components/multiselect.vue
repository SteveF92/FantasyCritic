<template>
  <BAutocomplete v-model="internalModelValue" :options="flatOptions" :placeholder="placeHolder" multiple tags open-on-focus open-on-click size="sm">
    <template #tags="{ selected }">
      <slot name="tags" :selected="selected" />
    </template>
  </BAutocomplete>
</template>
<script lang="ts">
import { defineComponent, PropType } from 'vue';
import { BAutocomplete } from 'bootstrap-vue-next';

interface Option {
  value: string;
  text: string;
}

export default defineComponent({
  components: {
    BAutocomplete
  },
  props: {
    modelValue: { type: Object as PropType<Option[]>, required: true },
    options: { type: Array as PropType<Option[]>, required: true },
    placeHolder: { type: String, required: true }
  },
  emits: {
    'update:modelValue': (_value: Option[]) => true
  },
  computed: {
    internalModelValue: {
      get(): string[] {
        return this.modelValue.map((x) => x.value);
      },
      set(newValue: string[]): void {
        const matchingValues = this.options.filter((x) => newValue.includes(x.value));
        this.$emit('update:modelValue', matchingValues);
      }
    },
    flatOptions() {
      return this.options.map((x) => x.value);
    }
  }
});
</script>
