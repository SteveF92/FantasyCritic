<!-- src/FantasyCritic.Web/ClientApp/src/components/DraftCreationSettings.vue -->
<template>
  <div>
    <div v-if="totalGamesToDraft > standardGames" class="alert alert-warning">
      Total games to draft across all drafts ({{ totalGamesToDraft }}) exceeds the total number of games ({{ standardGames }}). Please adjust the numbers below.
    </div>

    <div v-for="(draft, index) in internalDrafts" :key="index" class="draft-section">
      <div v-if="internalDrafts.length > 1" class="form-group">
        <label :for="`draftName-${index}`" class="control-label">Draft Name</label>
        <input :id="`draftName-${index}`" v-model="draft.name" type="text" class="form-control input" @input="emitUpdate" />
      </div>

      <div class="form-group">
        <label :for="`scheduledDate-${index}`" class="control-label">Scheduled Date (Optional)</label>
        <input :id="`scheduledDate-${index}`" v-model="draft.scheduledDate" type="date" class="form-control input" @input="emitUpdate" />
      </div>

      <div class="form-group">
        <label :for="`gamesToDraft-${index}`" class="control-label">Games to Draft</label>
        <ValidationProvider v-slot="{ errors }" rules="required|min_value:1|max_value:50|integer">
          <input :id="`gamesToDraft-${index}`" v-model.number="draft.gamesToDraft" type="text" class="form-control input" @input="emitUpdate" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationProvider>
      </div>

      <div class="form-group">
        <label :for="`counterPicksToDraft-${index}`" class="control-label">Counter Picks to Draft</label>
        <ValidationProvider v-slot="{ errors }" rules="required|min_value:0|max_value:50|integer">
          <input :id="`counterPicksToDraft-${index}`" v-model.number="draft.counterPicksToDraft" type="text" class="form-control input" @input="emitUpdate" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationProvider>
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

<script>
import { getDefaultDraft, getDefaultDraftName } from '@/utilities/leagueCreationPresets';

export default {
  name: 'DraftCreationSettings',
  props: {
    value: { type: Array, required: true },
    standardGames: { type: Number, default: 0 },
    gameMode: { type: String, default: 'Standard' },
    editMode: { type: Boolean, default: false }
  },
  data() {
    return {
      internalDrafts: []
    };
  },
  computed: {
    totalGamesToDraft() {
      return this.internalDrafts.reduce((sum, d) => sum + (d.gamesToDraft || 0), 0);
    },
    showAddButton() {
      return this.gameMode === 'Multi Draft' && !this.editMode;
    }
  },
  watch: {
    value: {
      immediate: true,
      handler(newVal) {
        const normalized = newVal.map((d, index) => ({
          ...d,
          name: d.name?.trim() ? d.name : getDefaultDraftName(index)
        }));
        const needsEmit = normalized.some((d, index) => d.name !== newVal[index]?.name);
        this.internalDrafts = normalized;
        if (needsEmit) {
          this.$nextTick(() => this.emitUpdate());
        }
      }
    }
  },
  methods: {
    emitUpdate() {
      this.$emit(
        'input',
        this.internalDrafts.map((d) => ({ ...d }))
      );
    },
    canRemoveDraft(index) {
      // Only drafts beyond the first can be removed, and only if at least 2 remain (minimum for multi-draft)
      return !this.editMode && index > 0 && this.internalDrafts.length > 2;
    },
    addDraft() {
      const allocatedSoFar = this.internalDrafts.reduce((s, d) => s + (d.gamesToDraft || 0), 0);
      const newDraft = getDefaultDraft(this.internalDrafts.length, this.standardGames, allocatedSoFar);
      this.internalDrafts.push(newDraft);
      this.emitUpdate();
    },
    removeDraft(index) {
      this.internalDrafts.splice(index, 1);
      this.emitUpdate();
    }
  }
};
</script>

<style scoped>
.draft-section + .draft-section {
  margin-top: 0.5rem;
  padding-top: 1.5rem;
  border-top: 3px solid rgba(255, 255, 255, 0.35);
}
</style>
