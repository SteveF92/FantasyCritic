<template>
  <div>
    <multiselect v-if="tagOptions" v-model="internalValue" tag-placeholder="Add this as new tag"
                 placeholder="Search or add a tag" label="readableName"
                 track-by="name" :options="tagOptions" :multiple="true"
                 @input="handleInput">
      <template slot="tag" slot-scope="{ option }">
          <masterGameTagBadge :tagName="option.name"></masterGameTagBadge>
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
      internalValue: []
    }
  },
  computed: {
    tagOptions() {
      return this.$store.getters.allTags;
    },
  },
  methods: {
    handleInput(e) {
      this.$emit('input', this.internalValue);
    },
    updateInternal() {
      this.internalValue = _.cloneDeep(this.value);
    }
  },
  mounted() {
    this.updateInternal();
  },
  watch: {
    value() {
      this.updateInternal();
    }
  }
}
</script>

<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>
