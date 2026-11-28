<script setup lang="ts" generic="T extends object">
import { computed } from 'vue';
import type { Size } from 'bootstrap-vue-next';

type TPropertyName = keyof T & string;

const { options, keyField, labelField, placeholder } = defineProps<{
  options: readonly T[];
  keyField: TPropertyName;
  labelField: TPropertyName;
  placeholder?: string;
  size?: Size;
}>();

const model = defineModel<T[]>({ required: true });

interface TagSlotProps {
  option: T;
  remove: () => void;
}
defineSlots<{ tag?(props: TagSlotProps): unknown }>();

function readField(item: T, field: TPropertyName): string {
  return String(item[field]);
}
const keyOf = (item: T) => readField(item, keyField);
const labelOf = (item: T) => readField(item, labelField);

const optionMapByKey = computed(() => new Map([...options, ...model.value].map((x) => [keyOf(x), x])));
const selectedKeys = computed({
  get(): string[] {
    return model.value.map(keyOf);
  },
  set(newKeys: string[]): void {
    model.value = newKeys.map((key) => optionMapByKey.value.get(key)).filter((x) => !!x);
  }
});

const placeholderOnlyWhenEmpty = computed(() => (model.value.length === 0 ? placeholder : undefined));

const normalizedOptions = computed(() =>
  options.map((x) => ({
    value: keyOf(x),
    text: labelOf(x)
  }))
);

function remove(value: T) {
  model.value = model.value.filter((x) => keyOf(x) !== keyOf(value));
}
</script>

<template>
  <div class="multiselect">
    <BAutocomplete v-model="selectedKeys" :options="normalizedOptions" :placeholder="placeholderOnlyWhenEmpty" multiple tags open-on-focus open-on-click :size="size">
      <template v-if="$slots.tag" #tags>
        <template v-for="singleModel in model" :key="keyOf(singleModel)">
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

/* With many tags, Bootstrap's wrapping input group pushed the input and toggle onto a line of their own. */
.multiselect :deep(.input-group) {
  flex-wrap: nowrap;
}

/* The default chips use the site's dark secondary colour, where Bootstrap's dark × doesn't show. This is what btn-close-white does. */
.multiselect :deep(.b-form-tag-remove) {
  --bs-btn-close-filter: invert(1) grayscale(100%) brightness(200%);
}
</style>
