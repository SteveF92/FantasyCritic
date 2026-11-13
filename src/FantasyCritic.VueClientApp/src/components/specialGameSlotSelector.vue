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
    <div v-for="specialGameSlot in internalValue" :key="specialGameSlot.specialSlotPosition" class="special-game-slot">
      <div class="special-slot-header">
        <h4>Special Slot {{ specialGameSlot.specialSlotPosition + 1 }}</h4>
        <b-button variant="danger" size="sm" @click="removeSlot(specialGameSlot)">Remove</b-button>
      </div>
      <masterGameTagSelector v-model="specialGameSlot.requiredTags" @update:model-value="handleInput"></masterGameTagSelector>
    </div>
    <div class="add-slot-button-row">
      <b-button variant="primary" class="add-slot-button" @click="addSlot">Add Special Slot</b-button>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';

import type { MasterGameTagViewModel, SpecialGameSlotViewModel } from '@/api/clients';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import MasterGameTagSelector from '@/components/masterGameTagSelector.vue';

interface SpecialGameSlotWithTags {
  specialSlotPosition: number;
  requiredTags: MasterGameTagViewModel[];
}

interface SpecialGameSlotSelectorData {
  internalValue: SpecialGameSlotWithTags[];
}

export default defineComponent({
  components: {
    MasterGameTagSelector
  },
  props: {
    modelValue: { type: Array as PropType<SpecialGameSlotViewModel[]>, default: () => [] }
  },
  emits: {
    'update:modelValue': (_value: SpecialGameSlotViewModel[]) => true
  },
  data(): SpecialGameSlotSelectorData {
    return {
      internalValue: []
    };
  },
  computed: {
    showNoTagsWarning(): boolean {
      return this.internalValue.some((x) => x.requiredTags.length === 0);
    },
    showPortDanger(): boolean {
      return this.internalValue.some((x) => x.requiredTags.some((y) => y.name === 'Port'));
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
    handleInput(): void {
      let returnValue: SpecialGameSlotViewModel[] = [];
      this.internalValue.forEach((singleValue) => {
        let newReturnItem: SpecialGameSlotViewModel = {
          specialSlotPosition: singleValue.specialSlotPosition,
          requiredTags: singleValue.requiredTags.map((v) => v.name)
        };
        returnValue.push(newReturnItem);
      });

      this.$emit('update:modelValue', returnValue);
    },
    getTags(tagNames: string[]): MasterGameTagViewModel[] {
      const interLeagueStore = useInterLeagueStore();
      let allTags = interLeagueStore.allTags;
      let matchingTags: MasterGameTagViewModel[] = [];
      tagNames.forEach((tagName) => {
        const match = allTags.find((x) => x.name === tagName);
        matchingTags.push(match);
      });

      return matchingTags;
    },
    removeSlot(specialSlot: SpecialGameSlotWithTags): void {
      this.internalValue = this.internalValue.filter((x) => x.specialSlotPosition !== specialSlot.specialSlotPosition);

      let slotNumber = 0;
      this.internalValue.forEach((singleValue) => {
        singleValue.specialSlotPosition = slotNumber;
        slotNumber++;
      });
      this.handleInput();
    },
    addSlot(): void {
      let newItem: SpecialGameSlotWithTags = {
        specialSlotPosition: this.internalValue.length,
        requiredTags: []
      };
      this.internalValue.push(newItem);
      this.handleInput();
    },
    updateInternal(): void {
      this.internalValue = [];

      this.modelValue.forEach((singleValue) => {
        let newInternalItem: SpecialGameSlotWithTags = {
          specialSlotPosition: singleValue.specialSlotPosition,
          requiredTags: this.getTags(singleValue.requiredTags)
        };
        this.internalValue.push(newInternalItem);
      });
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
