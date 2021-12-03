<template>
  <div>
    <div v-for="specialGameSlot in internalValue" class="special-game-slot">
      <h4>Special Slot {{specialGameSlot.specialSlotPosition}}</h4>
      <masterGameTagSelector v-model="specialGameSlot.requiredTags" @input="handleInput"></masterGameTagSelector>
    </div>
  </div>
</template>

<script>
  import draggable from 'vuedraggable';
  import MasterGameTagSelector from '@/components/modules/masterGameTagSelector';

  export default {
    props: ['value'],
    components: {
      draggable,
      MasterGameTagSelector
    },
    data() {
      return {
        internalValue: []
      }
    },
    methods: {
      handleInput(e) {
        let returnValue = [];
        this.internalValue.forEach(singleValue => {
          let newReturnItem = {
            specialSlotPosition: singleValue.specialSlotPosition,
            requiredTags: singleValue.requiredTags.map(v => v.name)
          };
          returnValue.push(newReturnItem);
        });

        this.$emit('input', returnValue);
      },
      getTags(tagNames) {
        let allTags = this.$store.getters.allTags;
        let matchingTags = [];
        tagNames.forEach(tagName => {
          let match = _.filter(allTags, { 'name': tagName });
          matchingTags.push(match[0]);
        });

        return matchingTags;
      },
      updateInternal() {
        this.internalValue = [];

        this.value.forEach(singleValue => {
          let newInternalItem = {
            specialSlotPosition: singleValue.specialSlotPosition,
            requiredTags: this.getTags(singleValue.requiredTags)
          };
          this.internalValue.push(newInternalItem);
        });
      }
    },
    computed: {
      tagOptions() {
        return this.$store.getters.allTags;
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

<style scoped>
  .special-game-slot {
    background: #404040;
    border-radius: 10px;
    padding: 10px;
    margin-bottom: 15px;
  }
</style>
