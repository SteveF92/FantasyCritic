<template>
  <div>
    <div class="form-group">
      <label for="intendedNumberOfPlayers" class="control-label">{{ playerCountLabel }}</label>
      <ValidationField v-slot="{ errors }" :model-value="playerCount" name="Intended Number of Players" rules="required|min_value:2|max_value:20|integer">
        <input id="intendedNumberOfPlayers" v-model.number="playerCount" name="Intended Number of Players" type="number" class="form-control input" />
        <span class="text-danger">{{ errors[0] }}</span>
      </ValidationField>
      <p>You aren't locked into this number of people. This is just to recommend how many games to have per person.</p>
    </div>

    <div class="form-group">
      <label for="gameMode" class="control-label">Game Mode</label>
      <p>
        <strong>Standard</strong>
        — the full Fantasy Critic experience with one draft, then you bid for games throughout the rest of the year.
        <br />
        <strong>Multi Draft</strong>
        — run two or more drafts across the year. Bids between drafts are off by default.
        <br />
        <strong>One Shot</strong>
        — one draft, which is final. No drops, bids, or trades. Great for a low-commitment league.
      </p>
      <b-form-select id="gameMode" v-model="gameMode" :options="gameModeOptions"></b-form-select>
    </div>

    <div class="form-group">
      <label for="experienceLevel" class="control-label">Experience Level</label>
      <p>Controls the recommended number of games per player and a few other settings.</p>
      <b-form-select id="experienceLevel" v-model="experienceLevel" :options="experienceLevelOptions"></b-form-select>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';

import type { ExperienceLevel, GameMode, PresetInput } from '@/utilities/leagueCreationPresets';

function computeModelValue<K extends keyof PresetInput>(key: K) {
  return {
    get(this: { modelValue: PresetInput }): PresetInput[K] {
      return this.modelValue[key];
    },
    set(this: { updateModelValue(changes: Partial<PresetInput>): void }, value: PresetInput[K]): void {
      const changes: Partial<PresetInput> = {};
      changes[key] = value;
      this.updateModelValue(changes);
    }
  };
}

export default defineComponent({
  name: 'LeagueCreationPresets',
  setup() {
    const gameModeOptions: { value: GameMode; text: string }[] = [
      { value: 'Standard', text: 'Standard' },
      { value: 'Multi Draft', text: 'Multi Draft' },
      { value: 'One Shot', text: 'One Shot' }
    ];
    const experienceLevelOptions: { value: ExperienceLevel; text: string }[] = [
      { value: 'Beginner', text: 'Beginner' },
      { value: 'Standard', text: 'Standard' },
      { value: 'Advanced', text: 'Advanced' }
    ];

    return { gameModeOptions, experienceLevelOptions };
  },
  props: {
    modelValue: { type: Object as PropType<PresetInput>, required: true },
    playerCountLabel: {
      type: String,
      default: 'How many players do you think will be in this league?'
    }
  },
  emits: {
    'update:modelValue': (_value: PresetInput) => true
  },
  computed: {
    gameMode: computeModelValue('gameMode'),
    experienceLevel: computeModelValue('experienceLevel'),
    playerCount: computeModelValue('playerCount')
  },
  methods: {
    updateModelValue(changes: Partial<PresetInput>): void {
      this.$emit('update:modelValue', { ...this.modelValue, ...changes });
    }

    //I don't know if we need this anymore
    // emitIfReady(): void {
    //   if (!this.playerCount || this.playerCount < 2 || this.playerCount > 20) return;
    //   const result = computePreset(this.gameMode, this.experienceLevel, this.playerCount, this.year);
    //   this.$emit('presetApplied', { gameMode: this.gameMode, experienceLevel: this.experienceLevel, ...result });
    // }
  }
});
</script>
