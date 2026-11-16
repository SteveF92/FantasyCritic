<template>
  <multiselect v-model="computedModelValue" tag-placeholder="Add this as new tag" placeholder="Search or add a tag" label="readableName" track-by="name" :options="tagOptions" :multiple="true">
    <template #tag="{ option }">
      <masterGameTagBadge :tag-name="option.name"></masterGameTagBadge>
    </template>
  </multiselect>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import Multiselect from 'vue-multiselect';

import type { MasterGameTagViewModel } from '@/api/clients';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

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

<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>
