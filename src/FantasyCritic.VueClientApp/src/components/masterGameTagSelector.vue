<template>
  <multiselect
    v-if="tagOptions"
    v-model="internalValue"
    tag-placeholder="Add this as new tag"
    placeholder="Search or add a tag"
    label="readableName"
    track-by="name"
    :options="tagOptions"
    :multiple="true"
    @update:model-value="handleInput">
    <template #tag="{ option }">
      <masterGameTagBadge :tag-name="option.name"></masterGameTagBadge>
    </template>
  </multiselect>
</template>

<script>
import Multiselect from 'vue-multiselect';

import { useInterLeagueStore } from '@/store/interLeagueStore';
import { deepClone } from '@/utilities/deepClone';
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

export default {
  components: {
    Multiselect,
    MasterGameTagBadge
  },
  props: {
    modelValue: { type: Array, default: null },
    includeSystem: { type: Boolean }
  },
  emits: ['update:modelValue'],
  data() {
    return {
      internalValue: []
    };
  },
  computed: {
    tagOptions() {
      const interLeagueStore = useInterLeagueStore();
      if (this.includeSystem) {
        return interLeagueStore.allTags;
      }
      return interLeagueStore.allTags.filter((x) => !x.systemTagOnly);
    }
  },
  watch: {
    modelValue() {
      this.updateInternal();
    }
  },
  created() {
    this.updateInternal();
  },
  methods: {
    handleInput() {
      this.$emit('update:modelValue', this.internalValue);
    },
    updateInternal() {
      this.internalValue = deepClone(this.modelValue);
    }
  }
};
</script>

<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>
