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
    @input="handleInput">
    <template #tag="{ option }">
      <masterGameTagBadge :tag-name="option.name"></masterGameTagBadge>
    </template>
  </multiselect>
</template>

<script>
import Multiselect from 'vue-multiselect';
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

export default {
  components: {
    Multiselect,
    MasterGameTagBadge
  },
  props: {
    value: { type: Array, default: null },
    includeSystem: { type: Boolean }
  },
  data() {
    return {
      internalValue: []
    };
  },
  computed: {
    tagOptions() {
      if (this.includeSystem) {
        return this.$store.getters.allTags;
      }
      return this.$store.getters.allTags.filter((x) => !x.systemTagOnly);
    }
  },
  watch: {
    value() {
      this.updateInternal();
    }
  },
  created() {
    this.updateInternal();
  },
  methods: {
    handleInput() {
      this.$emit('input', this.internalValue);
    },
    updateInternal() {
      this.internalValue = structuredClone(this.value);
    }
  }
};
</script>

<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>
