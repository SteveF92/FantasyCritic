<template>
  <div>
    <div class="form-group">
      <label for="intendedNumberOfPlayers" class="control-label">{{ playerCountLabel }}</label>
      <ValidationField v-slot="{ errors }" :model-value="playerCount" name="Intended Number of Players" rules="required|min_value:2|max_value:20|integer">
        <input id="intendedNumberOfPlayers" v-model.number="playerCount" name="Intended Number of Players" type="text" class="form-control input" @input="emitIfReady" />
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
      <b-form-select id="gameMode" v-model="gameMode" :options="gameModeOptions" @update:model-value="emitIfReady"></b-form-select>
    </div>

    <div class="form-group">
      <label for="experienceLevel" class="control-label">Experience Level</label>
      <p>Controls the recommended number of games per player and a few other settings.</p>
      <b-form-select id="experienceLevel" v-model="experienceLevel" :options="experienceLevelOptions" @update:model-value="emitIfReady"></b-form-select>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';

import type { AppliedPreset } from '@/types/vueTypes';
import { computePreset, type ExperienceLevel, type GameMode } from '@/utilities/leagueCreationPresets';

interface LeagueCreationPresetsData {
  playerCount: number | null;
  gameMode: GameMode;
  experienceLevel: ExperienceLevel;
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
    year: { type: Number, required: true },
    playerCountLabel: {
      type: String,
      default: 'How many players do you think will be in this league?'
    }
  },
  emits: {
    presetApplied: (_preset: AppliedPreset) => true
  },
  data(): LeagueCreationPresetsData {
    return {
      playerCount: null,
      gameMode: 'Standard',
      experienceLevel: 'Standard'
    };
  },
  methods: {
    emitIfReady(): void {
      if (!this.playerCount || this.playerCount < 2 || this.playerCount > 20) return;
      const result = computePreset(this.gameMode, this.experienceLevel, this.playerCount, this.year);
      this.$emit('presetApplied', { gameMode: this.gameMode, experienceLevel: this.experienceLevel, ...result });
    }
  }
});
</script>
