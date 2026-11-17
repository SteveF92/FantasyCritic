<template>
  <div>
    <div v-if="totalGamesToDraft > standardGames" class="alert alert-warning">
      Total games to draft across all drafts ({{ totalGamesToDraft }}) exceeds the total number of games ({{ standardGames }}). Please adjust the numbers below.
    </div>

    <div v-for="(draft, index) in modelValue" :key="index" class="draft-section">
      <div v-if="modelValue.length > 1" class="form-group">
        <label :for="`draftName-${index}`" class="control-label">Draft Name</label>
        <input :id="`draftName-${index}`" type="text" :value="draft.name" @input="updateDraftName(index, $event)" class="form-control input" />
      </div>

      <div class="form-group">
        <label :for="`scheduledDate-${index}`" class="control-label">
          Scheduled Date
          <span v-show="isScheduledDateOptional(index)">(Optional)</span>
        </label>
        <input :id="`scheduledDate-${index}`" type="date" :value="draft.scheduledDate" @input="updateDraftScheduledDate(index, $event)" class="form-control input" />
      </div>

      <div class="form-group">
        <label :for="`gamesToDraft-${index}`" class="control-label">Games to Draft</label>
        <ValidationField v-slot="{ errors }" :model-value="draft.gamesToDraft" :name="`gamesToDraft-${index}`" rules="required|min_value:1|max_value:50|integer">
          <input :id="`gamesToDraft-${index}`" type="number" :value="draft.gamesToDraft" @input="updateDraftGamesToDraft(index, $event)" class="form-control input" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationField>
      </div>

      <div class="form-group">
        <label :for="`counterPicksToDraft-${index}`" class="control-label">Counter Picks to Draft</label>
        <ValidationField v-slot="{ errors }" :model-value="draft.counterPicksToDraft" :name="`counterPicksToDraft-${index}`" rules="required|min_value:0|max_value:50|integer">
          <input :id="`counterPicksToDraft-${index}`" type="number" :value="draft.counterPicksToDraft" @input="updateDraftCounterPicksToDraft(index, $event)" class="form-control input" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationField>
      </div>

      <div v-if="gameMode === 'Multi Draft' && draft.counterPicksToDraft > 0" class="form-group">
        <b-form-checkbox :model-value="draft.counterPicksMustBeFromThisDraft" @update:model-value="(checked) => updateDraftCounterPicksMustBeFromThisDraft(index, checked === true)">
          <span class="checkbox-label">Require counter picks to be made on games taken during this draft.</span>
          <p>
            If checked, all of the counter picks included in this draft must be of the standard games taken in this draft. If you don't enable this, players will be able to counter pick a game that
            was drafted in a previous draft. It's recommended that you keep this enabled.
          </p>
        </b-form-checkbox>
      </div>

      <div v-if="canRemoveDraft(index)">
        <b-button size="sm" variant="outline-danger" @click="removeDraft(index)">Remove Draft {{ index + 1 }}</b-button>
      </div>
    </div>

    <div v-if="showAddButton">
      <b-button variant="outline-secondary" @click="addDraft">+ Add Another Draft</b-button>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';

import type { DraftSettingsRequest } from '@/api/clients';
import { getDefaultDraft, type GameMode } from '@/utilities/leagueCreationPresets';

export default defineComponent({
  name: 'DraftCreationSettings',
  props: {
    modelValue: { type: Array as PropType<DraftSettingsRequest[]>, required: true },
    standardGames: { type: Number, default: 0 },
    gameMode: { type: String as PropType<GameMode>, default: 'Standard' },
    editMode: { type: Boolean, default: false },
    bidsOnlyBeforeNextScheduledDraft: { type: Boolean, default: false }
  },
  emits: {
    'update:modelValue': (_value: DraftSettingsRequest[]) => true
  },
  computed: {
    computedModelValue: {
      get(): DraftSettingsRequest[] {
        return this.modelValue;
      },
      set(newValue: DraftSettingsRequest[]): void {
        this.$emit('update:modelValue', newValue);
      }
    },
    totalGamesToDraft(): number {
      return this.modelValue.reduce((sum, d) => sum + (d.gamesToDraft || 0), 0);
    },
    showAddButton(): boolean {
      return this.gameMode === 'Multi Draft' && !this.editMode;
    }
  },
  methods: {
    updateDraftName(indexToUpdate: number, event: Event): void {
      const name = (event.target as HTMLInputElement).value;
      this.computedModelValue = this.computedModelValue.map((draft, index) => (index === indexToUpdate ? { ...draft, name } : draft));
    },
    updateDraftScheduledDate(indexToUpdate: number, event: Event): void {
      const scheduledDate = (event.target as HTMLInputElement).value;
      this.computedModelValue = this.computedModelValue.map((draft, index) => (index === indexToUpdate ? { ...draft, scheduledDate } : draft));
    },
    updateDraftGamesToDraft(indexToUpdate: number, event: Event): void {
      const gamesToDraft = (event.target as HTMLInputElement).valueAsNumber;
      this.computedModelValue = this.computedModelValue.map((draft, index) => (index === indexToUpdate ? { ...draft, gamesToDraft } : draft));
    },
    updateDraftCounterPicksToDraft(indexToUpdate: number, event: Event): void {
      const counterPicksToDraft = (event.target as HTMLInputElement).valueAsNumber;
      this.computedModelValue = this.computedModelValue.map((draft, index) => (index === indexToUpdate ? { ...draft, counterPicksToDraft } : draft));
    },
    updateDraftCounterPicksMustBeFromThisDraft(indexToUpdate: number, counterPicksMustBeFromThisDraft: boolean): void {
      this.computedModelValue = this.computedModelValue.map((draft, index) => (index === indexToUpdate ? { ...draft, counterPicksMustBeFromThisDraft } : draft));
    },
    isScheduledDateOptional(index: number): boolean {
      if (this.gameMode !== 'Multi Draft' || !this.bidsOnlyBeforeNextScheduledDraft) {
        return true;
      }
      return index === 0;
    },
    canRemoveDraft(index: number): boolean {
      // Only drafts beyond the first can be removed, and only if at least 2 remain (minimum for multi-draft)
      return !this.editMode && index > 0 && this.computedModelValue.length > 2;
    },
    addDraft(): void {
      const allocatedSoFar = this.computedModelValue.reduce((s, d) => s + (d.gamesToDraft || 0), 0);
      const newDraft = getDefaultDraft(this.computedModelValue.length, this.standardGames, allocatedSoFar);
      this.computedModelValue = [...this.computedModelValue, newDraft];
    },
    removeDraft(indexToRemove: number): void {
      const newDraftArray: DraftSettingsRequest[] = [];
      for (const [index, draft] of this.computedModelValue.entries()) {
        if (index === indexToRemove) {
          continue;
        }

        newDraftArray.push(draft);
      }

      this.computedModelValue = newDraftArray;
    }
  }
});
</script>

<style scoped>
.draft-section + .draft-section {
  margin-top: 0.5rem;
  padding-top: 1.5rem;
  border-top: 3px solid rgba(255, 255, 255, 0.35);
}
</style>
