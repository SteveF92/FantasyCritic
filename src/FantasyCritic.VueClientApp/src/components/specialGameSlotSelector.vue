<template>
  <div>
    <div class="alert alert-info">
      If you choose more than one tag for a single slot, then a game only needs to have at least ONE of those tags, not all of them. You can use this to say "this game must be a remake or a remaster",
      for example.
    </div>
    <div v-show="showNoTagsWarning" class="alert alert-warning">All of your special game slots should have at least one tag listed.</div>
    <div v-show="showPortDanger" class="alert alert-danger">
      Please, please, don't allow the tag 'Port'. These games very very rarely get new OpenCritic pages, so we usually end up assigning the points from the original game. You're free to allow the tag,
      but please be aware that this is an "unsupported" feature.
    </div>
    <div v-for="specialGameSlot in modelValue" :key="specialGameSlot.specialSlotPosition" class="special-game-slot">
      <div class="special-slot-header">
        <h4>Special Slot {{ specialGameSlot.specialSlotPosition + 1 }}</h4>
        <b-button variant="danger" size="sm" @click="removeSlot(specialGameSlot)">Remove</b-button>
      </div>
      <masterGameTagSelector :model-value="getMatchingTags(specialGameSlot.requiredTags)" @update:model-value="(tags) => updateTagsForSlot(specialGameSlot, tags)"></masterGameTagSelector>
    </div>
    <div class="add-slot-button-row">
      <b-button variant="primary" class="add-slot-button" @click="addSlot">Add Special Slot</b-button>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';

import type { MasterGameTagViewModel, SpecialGameSlotViewModel } from '@/api/clients';
import { getMatchingTags } from '@/utilities/masterGameTags';
import MasterGameTagSelector from '@/components/masterGameTagSelector.vue';

export default defineComponent({
  components: {
    MasterGameTagSelector
  },
  props: {
    modelValue: { type: Array as PropType<SpecialGameSlotViewModel[]>, required: true }
  },
  emits: {
    'update:modelValue': (_value: SpecialGameSlotViewModel[]) => true
  },
  computed: {
    computedModelValue: {
      get(): SpecialGameSlotViewModel[] {
        return this.modelValue;
      },
      set(newValue: SpecialGameSlotViewModel[]): void {
        this.$emit('update:modelValue', newValue);
      }
    },
    showNoTagsWarning(): boolean {
      return this.modelValue.some((x) => x.requiredTags.length === 0);
    },
    showPortDanger(): boolean {
      return this.modelValue.some((x) => x.requiredTags.some((y) => y === 'Port'));
    }
  },
  methods: {
    getMatchingTags,
    removeSlot(specialSlot: SpecialGameSlotViewModel): void {
      const newValue = this.computedModelValue
        .filter((x) => x.specialSlotPosition !== specialSlot.specialSlotPosition)
        .map((x, index) => ({ specialSlotPosition: index, requiredTags: x.requiredTags }));

      this.computedModelValue = newValue;
    },
    addSlot(): void {
      const newItem: SpecialGameSlotViewModel = {
        specialSlotPosition: this.computedModelValue.length,
        requiredTags: []
      };

      const newValue = [...this.computedModelValue, newItem];
      this.computedModelValue = newValue;
    },
    updateTagsForSlot(slotWhichWasUpdated: SpecialGameSlotViewModel, updatedTags: MasterGameTagViewModel[]): void {
      const newArray: SpecialGameSlotViewModel[] = [];
      for (let slotNumber = 0; slotNumber < this.computedModelValue.length; slotNumber++) {
        const existingSlot = this.computedModelValue[slotNumber];
        if (existingSlot.specialSlotPosition === slotWhichWasUpdated.specialSlotPosition) {
          newArray.push({ specialSlotPosition: slotNumber, requiredTags: updatedTags.map((x) => x.name) });
          continue;
        }
        newArray.push(existingSlot);
      }
      this.computedModelValue = newArray;
    }
  }
});
</script>

<style scoped>
.special-game-slot {
  background: #404040;
  border-radius: 10px;
  padding: 10px;
  margin-bottom: 15px;
}
.special-slot-header {
  width: 100%;
  display: flex;
  justify-content: space-between;
  margin-bottom: 5px;
}
.add-slot-button-row {
  display: flex;
  flex-direction: row-reverse;
}
</style>
