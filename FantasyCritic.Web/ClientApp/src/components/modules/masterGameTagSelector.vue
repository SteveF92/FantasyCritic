<template>
  <div>
    <multiselect v-model="internalValue" tag-placeholder="Add this as new tag"
                 placeholder="Search or add a tag" label="readableName"
                 track-by="name" :options="tagOptions" :multiple="true"
                 @input="handleInput">
      <template slot="tag" slot-scope="{ option }">
          <masterGameTagBadge :tag="option"></masterGameTagBadge>
      </template>             
    </multiselect>
  </div>
</template>

<script>
import Multiselect from 'vue-multiselect'
import MasterGameTagBadge from '@/components/modules/masterGameTagBadge';

export default {
  props: ['value'],
  components: {
    Multiselect,
    MasterGameTagBadge
  },
  data() {
    return {
      internalValue: [
      ]
    }
  },
  computed: {
    tagOptions() {
      return this.$store.getters.allTags;
    },
  },
  methods: {
    handleInput (e) {
      this.$emit('input', this.internalValue);
    }
  },
}
</script>

<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>
