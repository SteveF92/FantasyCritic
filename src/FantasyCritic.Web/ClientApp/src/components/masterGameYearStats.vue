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

      <div v-if="statisticsLoadError" class="alert alert-warning small mt-3" role="alert">{{ statisticsLoadError }}</div>

      <div v-if="chartHasData" class="master-game-year-stats-chart">
        <h3 class="chart-heading">Statistics over time</h3>
        <div class="master-game-year-stats-chart-container">
          <LineChartGenerator
            :key="statisticsChartKey"
            chart-id="master-game-year-statistics-chart"
            dataset-id-key="label"
            :height="chartHeight"
            :styles="chartCanvasStyles"
            :chart-options="chartOptions"
            :chart-data="chartData" />
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import { Line as LineChartGenerator } from 'vue-chartjs/legacy';

const SERIES_COLORS = ['#d6993a', '#5cb8d4', '#9b7ed9', '#6bcf7a', '#e07a7a', '#e6d84a', '#4a9fe6', '#d46a9e'];

/** Every numeric series returned on each statistics row; trim or split axes later as needed. */
const STATISTICS_CHART_SERIES = [
  { key: 'eligiblePercentStandardGame', label: '% Published', scaleZeroToOneAsPercent: true },
  { key: 'adjustedPercentCounterPick', label: '% Counter Picked', scaleZeroToOneAsPercent: true },
  { key: 'dateAdjustedHypeFactor', label: 'Hype Factor' },
  { key: 'projectedFantasyPoints', label: 'Projected Points' },
  { key: 'royaleCost', label: 'Royale cost' }
];

function coerceChartNumber(value) {
  if (value === null || value === undefined) {
    return null;
  }
  const n = Number(value);
  return Number.isFinite(n) ? n : null;
}

function valueForChart(series, raw) {
  const v = coerceChartNumber(raw);
  if (v === null) {
    return null;
  }
  return series.scaleZeroToOneAsPercent ? v * 100 : v;
}

export default {
  components: { LineChartGenerator },
  props: {
    masterGameId: { type: String, required: true },
    masterGameYears: { type: Array, required: true }
  },
  data() {
    return {
      selectedYear: null,
      masterGameYearWithStatistics: null,
      statisticsLoadError: null,
      chartHeight: 520,
      chartCanvasStyles: {}
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
    },
    statisticsRowsSorted() {
      const stats = this.masterGameYearWithStatistics?.statistics;
      if (!stats?.length) {
        return [];
      }
      return [...stats].sort((a, b) => String(a.date).localeCompare(String(b.date)));
    },
    chartHasData() {
      return this.statisticsRowsSorted.length > 0;
    },
    statisticsChartKey() {
      return `${this.masterGameId}-${this.selectedYear}`;
    },
    chartLabels() {
      return this.statisticsRowsSorted.map((row) => row.date);
    },
    chartData() {
      if (!this.chartHasData) {
        return { labels: [], datasets: [] };
      }
      const labels = this.chartLabels;
      const rows = this.statisticsRowsSorted;
      const datasets = STATISTICS_CHART_SERIES.map((series, i) => {
        const color = SERIES_COLORS[i % SERIES_COLORS.length];
        return {
          label: series.label,
          borderColor: color,
          backgroundColor: color,
          borderWidth: 2,
          pointRadius: 0,
          tension: 0,
          percentageScale: Boolean(series.scaleZeroToOneAsPercent),
          data: rows.map((row) => valueForChart(series, row[series.key]))
        };
      });
      return { labels, datasets };
    },
    chartOptions() {
      return {
        maintainAspectRatio: false,
        layout: {
          padding: {
            bottom: 36
          }
        },
        interaction: { mode: 'index', intersect: false },
        plugins: {
          legend: {
            labels: {
              color: 'rgba(255, 255, 255, 0.9)',
              usePointStyle: true
            }
          },
          tooltip: {
            backgroundColor: 'rgba(20, 20, 20, 0.95)',
            titleColor: '#fff',
            bodyColor: '#fff',
            borderColor: 'rgba(214, 153, 58, 0.5)',
            borderWidth: 1,
            filter: (item) => item.parsed.y !== null && !Number.isNaN(item.parsed.y),
            callbacks: {
              label(context) {
                const label = context.dataset.label || '';
                const y = context.parsed.y;
                if (y === null || Number.isNaN(y)) {
                  return label;
                }
                if (context.dataset.percentageScale) {
                  return `${label}: ${y.toFixed(1)}%`;
                }
                return `${label}: ${y}`;
              }
            }
          }
        },
        scales: {
          x: {
            ticks: {
              color: 'rgba(255, 255, 255, 0.75)',
              maxRotation: 45,
              minRotation: 0
            },
            grid: { color: 'rgba(255, 255, 255, 0.08)' }
          },
          y: {
            ticks: { color: 'rgba(255, 255, 255, 0.75)' },
            grid: { color: 'rgba(255, 255, 255, 0.08)' }
          }
        }
      };
    }
  },
  watch: {
    selectedYear(year) {
      if (year == null) {
        this.masterGameYearWithStatistics = null;
        return;
      }

      this.fetchMasterGameYearWithStatistics(year);
    }
  },
  created() {
    this.selectedYear = Math.max(...this.masterGameYears.map((y) => y.year));
  },
  methods: {
    selectYear(year) {
      this.selectedYear = year;
    },
    async fetchMasterGameYearWithStatistics(year) {
      this.statisticsLoadError = null;
      try {
        const response = await axios.get(`/api/game/MasterGameYearWithStatistics/${this.masterGameId}/${year}`);
        this.masterGameYearWithStatistics = response.data;
      } catch (error) {
        const body = error.response?.data;
        let message = error.message ?? 'Failed to load year statistics.';
        if (typeof body === 'string') {
          message = body;
        } else if (body && typeof body === 'object') {
          message = body.message || body.title || JSON.stringify(body);
        }
        this.statisticsLoadError = message;
        this.masterGameYearWithStatistics = null;
      }
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

.chart-heading {
  margin-top: 1.25rem;
  margin-bottom: 0.75rem;
  font-size: 1.25rem;
  font-weight: 500;
}

.master-game-year-stats-chart-container {
  position: relative;
  height: 520px;
}
</style>
