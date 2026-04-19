<template>
  <div v-if="masterGameYears && masterGameYears.length" class="master-game-year-stats">
    <div v-if="selectedMasterGameYear" class="text-well">
      <h2 class="stats-title-row">
        <span class="stats-for-label">Stats for</span>
        <b-dropdown v-if="selectedYear != null" id="master-game-stats-year" class="master-game-stats-year-dropdown" :text="selectedYear.toString()" aria-label="Year for stats">
          <b-dropdown-item v-for="year in selectableYears" :key="year" :active="selectedYear === year" @click="selectYear(year)">
            {{ year }}
          </b-dropdown-item>
        </b-dropdown>
      </h2>
      <ul>
        <li>Drafted or picked up in {{ selectedMasterGameYear.eligiblePercentStandardGame | percent(1) }} of leagues where it is eligible.</li>

        <li v-if="selectedMasterGameYear.averageDraftPosition">Average Draft Position: {{ selectedMasterGameYear.averageDraftPosition | score(1) }}</li>
        <li v-else>Average Draft Position: Undrafted</li>

        <li v-if="selectedMasterGameYear.dateAdjustedHypeFactor">Hype Factor: {{ selectedMasterGameYear.dateAdjustedHypeFactor | score(1) }}</li>
        <li v-else>Hype Factor: Unhyped...</li>

        <template v-if="selectedMasterGameYear.year >= 2022 && selectedMasterGameYear.peakHypeFactor > selectedMasterGameYear.dateAdjustedHypeFactor">
          <li v-if="selectedMasterGameYear.peakHypeFactor">
            Peak Hype Factor: {{ selectedMasterGameYear.peakHypeFactor | score(1) }}
            <font-awesome-icon v-b-popover.hover.top="peakHypeFactorText" color="white" size="lg" icon="info-circle" />
          </li>
          <li v-else>
            Peak Hype Factor: Unhyped...
            <font-awesome-icon v-b-popover.hover.top="peakHypeFactorText" color="white" size="lg" icon="info-circle" />
          </li>
        </template>

        <li v-if="selectedMasterGameYear.projectedFantasyPoints">Projected Points: ~{{ selectedMasterGameYear.projectedFantasyPoints | score(1) }}</li>

        <li>Counter Picked in {{ selectedMasterGameYear.adjustedPercentCounterPick | percent(1) }} of leagues where it is published.</li>
      </ul>
    </div>
  </div>
</template>

<script>
export default {
  props: {
    masterGameYears: { type: Array, required: true }
  },
  data() {
    return {
      selectedYear: null
    };
  },
  computed: {
    selectableYears() {
      if (!this.masterGameYears.length) {
        return [];
      }

      return [...this.masterGameYears].map((y) => y.year).sort((a, b) => b - a);
    },
    selectedMasterGameYear() {
      if (this.selectedYear == null) {
        return null;
      }

      return this.masterGameYears.find((y) => y.year === this.selectedYear) || null;
    },
    peakHypeFactorText() {
      return {
        html: true,
        title: () => {
          return 'Peak Hype Factor';
        },
        content: () => {
          return (
            "Sometimes a game's hype factor will go down over the course of the year, particularly if it gets delayed and many players drop it. " +
            "This number is the highest this game's hype factor ever was in the year."
          );
        }
      };
    }
  },
  created() {
    this.selectedYear = Math.max(...this.masterGameYears.map((y) => y.year));
  },
  methods: {
    selectYear(year) {
      this.selectedYear = year;
    }
  }
};
</script>

<style scoped>
.master-game-year-stats {
  margin-top: 20px;
}

.stats-title-row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.35rem 0.6rem;
  margin-bottom: 0.75rem;
  font-size: 1.5rem;
  font-weight: 500;
  line-height: 1.3;
}

.stats-for-label {
  flex: 0 0 auto;
}

.master-game-stats-year-dropdown {
  flex: 0 0 auto;
}

/* Match heading scale while keeping Bootstrap-Vue dropdown toggle (same pattern as criticsRoyale.vue). */
.stats-title-row ::v-deep .btn.dropdown-toggle {
  font-size: 1.5rem;
  font-weight: 500;
  line-height: 1.3;
  padding: 0.25rem 0.85rem;
}
</style>
