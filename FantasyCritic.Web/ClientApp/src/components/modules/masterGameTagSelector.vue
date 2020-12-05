<template>
  <div>
    <multiselect v-model="value" tag-placeholder="Add this as new tag"
                 placeholder="Search or add a tag" label="readableName"
                 track-by="name" :options="tagOptions" :multiple="true">
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
      value: [
      ]
    }
  },
  computed: {
    tagOptions() {
      return this.$store.getters.allTags;
    },
  },
  methods: {
    updateValue: function (value) {
      this.$emit('input', value)
    }
  },
}
</script>

<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>
