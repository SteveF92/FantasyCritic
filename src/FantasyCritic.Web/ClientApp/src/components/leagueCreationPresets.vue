<template>
  <div>
    <div class="form-group">
      <label for="intendedNumberOfPlayers" class="control-label">{{ playerCountLabel }}</label>
      <ValidationProvider v-slot="{ errors }" rules="required|min_value:2|max_value:20|integer">
        <input id="intendedNumberOfPlayers" v-model.number="playerCount" name="Intended Number of Players" type="text" class="form-control input" @input="emitIfReady" />
        <span class="text-danger">{{ errors[0] }}</span>
      </ValidationProvider>
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
      <b-form-select id="gameMode" v-model="gameMode" :options="gameModeOptions" @change="emitIfReady"></b-form-select>
    </div>

    <div class="form-group">
      <label for="experienceLevel" class="control-label">Experience Level</label>
      <p>Controls the recommended number of games per player and a few other settings.</p>
      <b-form-select id="experienceLevel" v-model="experienceLevel" :options="experienceLevelOptions" @change="emitIfReady"></b-form-select>
    </div>
  </div>
</template>

<script>
import { computePreset } from '@/utilities/leagueCreationPresets';

export default {
  name: 'LeagueCreationPresets',
  props: {
    year: { type: Number, required: true },
    playerCountLabel: {
      type: String,
      default: 'How many players do you think will be in this league?'
    }
  },
  data() {
    return {
      playerCount: null,
      gameMode: 'Standard',
      experienceLevel: 'Standard',
      gameModeOptions: [
        { value: 'Standard', text: 'Standard' },
        { value: 'Multi Draft', text: 'Multi Draft' },
        { value: 'One Shot', text: 'One Shot' }
      ],
      experienceLevelOptions: [
        { value: 'Beginner', text: 'Beginner' },
        { value: 'Standard', text: 'Standard' },
        { value: 'Advanced', text: 'Advanced' }
      ]
    };
  },
  methods: {
    emitIfReady() {
      if (!this.playerCount || this.playerCount < 2 || this.playerCount > 20) return;
      const result = computePreset(this.gameMode, this.experienceLevel, this.playerCount, this.year);
      this.$emit('preset-applied', { gameMode: this.gameMode, experienceLevel: this.experienceLevel, ...result });
    }
  }
};
</script>
