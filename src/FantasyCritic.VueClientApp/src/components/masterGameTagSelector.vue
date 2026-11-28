<template>
  <multiselect v-model="computedModelValue" :options="tagOptions" value-field="name" label-field="readableName" placeholder="Add Tags">
    <template #tag="{ option, remove }">
      <masterGameTagBadge :tag-name="option.name" :remove="remove"></masterGameTagBadge>
    </template>
  </multiselect>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';

import type { MasterGameTagViewModel } from '@/api/clients';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

import Multiselect from './multiselect.vue';

export default defineComponent({
  components: {
    Multiselect,
    MasterGameTagBadge
  },
  props: {
    modelValue: { type: Array as PropType<MasterGameTagViewModel[]>, required: true },
    includeSystem: { type: Boolean }
  },
  emits: {
    'update:modelValue': (_value: MasterGameTagViewModel[]) => true
  },
  computed: {
    tagOptions(): MasterGameTagViewModel[] {
      const interLeagueStore = useInterLeagueStore();
      if (this.includeSystem) {
        return interLeagueStore.allTags;
      }
      return interLeagueStore.allTags.filter((x) => !x.systemTagOnly);
    },
    computedModelValue: {
      get(): MasterGameTagViewModel[] {
        return this.modelValue;
      },
      set(newValue: MasterGameTagViewModel[]): void {
        this.$emit('update:modelValue', newValue);
      }
    }
  }
});
</script>
