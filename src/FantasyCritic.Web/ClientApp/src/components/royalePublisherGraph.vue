<template>
  <LineChartGenerator ref="lineChartRef" :chart-id="chartId" dataset-id-key="label" :height="chartHeight" :styles="canvasStyles" :chart-options="chartOptions" :chart-data="chartData" />
</template>
<script lang="ts">
import { Line as LineChartGenerator } from 'vue-chartjs/legacy';
import { RoyalePublisher } from '@/models/royale/RoyalePublisher.ts';

const SERIES_COLORS = ['#d6993a', '#5cb8d4', '#9b7ed9', '#6bcf7a', '#e07a7a', '#e6d84a', '#4a9fe6', '#d46a9e'];

/** Chart instance exposed by vue-chartjs (Vue 2) on the line component. */
type ChartjsLegendHost = {
  data: { datasets: unknown[] };
  isDatasetVisible: (index: number) => boolean;
  hide: (index: number) => void;
  show: (index: number) => void;
  update: () => void;
};

type LineChartVueRef = { $data?: { _chart?: ChartjsLegendHost } };

export default {
  components: { LineChartGenerator },
  props: {
    chartId: { type: String, required: true },
    royalePublishers: { type: Array, required: true },
    chartHeight: { type: Number, required: true }
  },
  data() {
    return {
      canvasStyles: {},
      releaseLineBorderColor: 'rgba(255, 255, 255, 0.42)',
      legendDatasetVisible: [] as boolean[]
    };
  },
  computed: {
    isMultiPublisher(): boolean {
      return this.royalePublishers.length > 1;
    },
    chartLabels(): string[] {
      const set = new Set<string>();
      for (const m of this.royalePublishers) {
        for (const s of m.statistics || []) {
          set.add(s.date);
        }
      }
      return [...set].sort();
    },
    showChart() {
      return this.royalePublishers.length > 0 && this.chartLabels.length > 0;
    },
    shownPublishers(): RoyalePublisher[] {
      const pubs: RoyalePublisher[] = this.royalePublishers;
      if (!pubs.length) {
        return [];
      }
      const mask = this.legendDatasetVisible;
      if (!mask.length || mask.length !== pubs.length) {
        return pubs;
      }
      return pubs.filter((_, i) => mask[i]);
    },
    releaseAnnotations() {
      if (!this.showChart) {
        return {};
      }
      const labels = this.chartLabels;
      const labelSet = new Set(labels);

      type GamesByDate = Record<string, Record<string, string[]>>;

      const byDate: GamesByDate = {};
      const shownPublishers: RoyalePublisher[] = this.shownPublishers;
      const royalePublishers: RoyalePublisher[] = this.royalePublishers;

      for (const member of royalePublishers) {
        if (!shownPublishers.some((x) => x.publisherID === member.publisherID)) {
          continue;
        }

        for (const pg of member.publisherGames ?? []) {
          const masterGame = pg.masterGame;
          if (!masterGame) continue;

          const dateLabel = masterGame.releaseDate;
          if (!dateLabel || !labelSet.has(dateLabel)) {
            continue;
          }

          const gameName = masterGame.gameName;

          this.$set(byDate, dateLabel, byDate[dateLabel] ?? {});
          this.$set(byDate[dateLabel], gameName, byDate[dateLabel][gameName] ?? []);

          byDate[dateLabel][gameName].push(member.publisherName);
        }
      }

      const labelBorder = 'rgba(214, 153, 58, 0.5)';
      const annotations = {};
      const borderColor = this.releaseLineBorderColor;
      for (const [date, games] of Object.entries(byDate)) {
        const content = this.formatReleaseAnnotationLabel(date, games);
        annotations[date] = {
          scaleID: 'x',
          value: date,
          borderColor,
          borderWidth: 2,
          borderDash: [4, 4],
          label: {
            content,
            borderColor: labelBorder,
            borderWidth: 1,
            position: 'start'
          },
          enter({ element }) {
            if (element.label) {
              element.label.options.display = true;
            }
            return true;
          },
          leave({ element }) {
            if (element.label) {
              element.label.options.display = false;
            }
            return true;
          }
        };
      }
      return annotations;
    },
    chartOptions() {
      return {
        maintainAspectRatio: false,
        layout: {
          padding: {
            bottom: 45
          }
        },
        interaction: { mode: 'index', intersect: false },
        plugins: {
          legend: {
            labels: {
              color: 'rgba(255, 255, 255, 0.9)',
              usePointStyle: true
            },
            onClick: (e: MouseEvent, legendItem: { datasetIndex?: number }, legend: { chart: ChartjsLegendHost }) => {
              const index = legendItem.datasetIndex;
              if (typeof index !== 'number') {
                return;
              }
              const ci = legend.chart;
              if (ci.isDatasetVisible(index)) {
                ci.hide(index);
              } else {
                ci.show(index);
              }
              ci.update();
              this.syncLegendVisibilityFromChart(ci);
            }
          },
          tooltip: {
            backgroundColor: 'rgba(20, 20, 20, 0.95)',
            titleColor: '#fff',
            bodyColor: '#fff',
            borderColor: 'rgba(214, 153, 58, 0.5)',
            borderWidth: 1,
            filter: (item) => item.parsed.y !== null && !Number.isNaN(item.parsed.y)
          },
          annotation: {
            annotations: this.releaseAnnotations
          }
        },
        scales: {
          x: {
            ticks: {
              color: 'rgba(255, 255, 255, 0.75)'
            }
          },
          y: {
            ticks: { color: 'rgba(255, 255, 255, 0.75)' },
            grid: { color: 'rgba(255, 255, 255, 0.08)' }
          }
        }
      };
    },
    chartData() {
      if (!this.showChart) {
        return { labels: [], datasets: [] };
      }
      const labels = this.chartLabels;
      const datasets = this.royalePublishers.map((member, i) => {
        const color = SERIES_COLORS[i % SERIES_COLORS.length];
        return {
          label: member.publisherName,
          borderColor: color,
          borderWidth: 2,
          stepped: true,
          pointRadius: 0,
          data: this.alignPublisherStatsToLabels(member, labels)
        };
      });
      return { labels, datasets };
    }
  },
  watch: {
    chartData: {
      handler() {
        this.$nextTick(() => this.syncLegendVisibilityFromChartIfReady());
      },
      deep: true
    }
  },
  mounted() {
    this.$nextTick(() => this.syncLegendVisibilityFromChartIfReady());
  },
  methods: {
    syncLegendVisibilityFromChartIfReady() {
      const holder = this.$refs.lineChartRef as LineChartVueRef | undefined;
      const chart = holder?.$data?._chart;
      if (!chart?.data?.datasets?.length) {
        return;
      }
      this.syncLegendVisibilityFromChart(chart);
    },
    syncLegendVisibilityFromChart(chart: ChartjsLegendHost) {
      const n = chart.data.datasets.length;
      const next: boolean[] = [];
      for (let i = 0; i < n; i++) {
        next.push(chart.isDatasetVisible(i));
      }
      this.legendDatasetVisible = next;
    },
    formatReleaseAnnotationLabel(date: string, games: Record<string, string[]>): string[] {
      const lines: string[] = [];

      for (const [gameName, publisherNames] of Object.entries(games)) {
        if (!this.isMultiPublisher) {
          lines.push(gameName);
          continue;
        }

        const unique = [...new Set(publisherNames)].sort((a, b) => a.localeCompare(b));
        const n = unique.length;

        if (n === 0) {
          lines.push(gameName);
          continue;
        }

        if (n === 1) {
          lines.push(`${gameName} — ${unique[0]}`);
          continue;
        }

        const listStr = unique.join(', ');
        const full = `${gameName} — ${listStr}`;
        const maxLen = 72;

        if (full.length <= maxLen && n <= 5) {
          lines.push(full);
        } else {
          lines.push(`${gameName} (${n} publishers)`);
        }
      }

      return [date, ...lines];
    },
    alignPublisherStatsToLabels(member: RoyalePublisher, labels: string[]): number[] {
      const sorted = [...(member.statistics || [])]
        .map((s) => ({
          date: s.date,
          fantasyPoints: Number(s.fantasyPoints)
        }))
        .sort((a, b) => a.date.localeCompare(b.date));
      let j = 0;
      let last: number = null;
      return labels.map((label) => {
        while (j < sorted.length && sorted[j].date <= label) {
          last = sorted[j].fantasyPoints;
          j++;
        }
        return last;
      });
    }
  }
};
</script>
