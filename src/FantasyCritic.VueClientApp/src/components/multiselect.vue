<script setup lang="ts" generic="T extends object">
import { computed } from 'vue';
import type { Size } from 'bootstrap-vue-next';

const { options, valueField, labelField } = defineProps<{
  options: T[];
  valueField: keyof T;
  labelField: keyof T;
  placeholder: string;
  size?: Size;
}>();

const model = defineModel<T[]>({ required: true });

const selectedKeys = computed({
  get(): string[] {
    return model.value.map((x) => String(x[valueField]));
  },
  set(newValue: string[]): void {
    model.value = options.filter((x) => newValue.includes(String(x[valueField])));
  }
});

const normalizedOptions = computed(() => {
  return options.map((x) => ({
    value: String(x[valueField]),
    text: String(x[labelField])
  }));
});

function remove(value: T) {
  model.value = model.value.filter((x) => x[valueField] !== value[valueField]);
}
</script>

<template>
  <div class="multiselect">
    <BAutocomplete v-model="selectedKeys" :options="normalizedOptions" :placeholder="placeholder" multiple tags open-on-focus open-on-click :size="size">
      <template v-if="$slots.tag" #tags>
        <template v-for="singleModel in model" :key="String(singleModel[valueField])">
          <slot name="tag" :option="singleModel" :remove="() => remove(singleModel)" />
        </template>
      </template>
    </BAutocomplete>
  </div>
</template>

<style scoped>
/* The toggle is part of the field, not one of the site's buttons from site.css. */
.multiselect :deep(button.b-autocomplete-trigger) {
  --bs-btn-bg: var(--bs-body-bg);
  --bs-btn-hover-bg: var(--bs-tertiary-bg);
  --bs-btn-active-bg: var(--bs-tertiary-bg);
  color: var(--bs-body-color);
  min-height: 0;
  border: var(--bs-border-width) solid var(--bs-border-color);
  border-left: none;
  border-radius: 0 var(--bs-border-radius) var(--bs-border-radius) 0;
}
</style>
