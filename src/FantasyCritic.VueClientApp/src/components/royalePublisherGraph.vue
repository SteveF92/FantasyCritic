<template>
  <LineChartGenerator ref="lineChartRef" dataset-id-key="label" :options="chartOptions" :data="chartData" />
</template>

<script lang="ts">
import { defineComponent, markRaw, type PropType } from 'vue';
import type { Chart, ChartData, ChartOptions } from 'chart.js';
import type { AnnotationOptions } from 'chartjs-plugin-annotation';
import { Line as LineChartGenerator } from 'vue-chartjs';

import type { RoyalePublisherGraphSeries } from '@/types/vueTypes';

const SERIES_COLORS = ['#d6993a', '#5cb8d4', '#9b7ed9', '#6bcf7a', '#e07a7a', '#e6d84a', '#4a9fe6', '#d46a9e'];
const RELEASE_LINE_BORDER_COLOR = 'rgba(255, 255, 255, 0.42)';

type LineChartVueRef = { chart?: Chart<'line'> };

interface RoyalePublisherGraphData {
  legendDatasetVisible: boolean[];
}

export default defineComponent({
  components: { LineChartGenerator },
  props: {
    royalePublishers: { type: Array as PropType<RoyalePublisherGraphSeries[]>, required: true }
  },
  data(): RoyalePublisherGraphData {
    return {
      legendDatasetVisible: []
    };
  },
  computed: {
    isMultiPublisher(): boolean {
      return this.royalePublishers.length > 1;
    },
    chartLabels(): string[] {
      const set = new Set<string>();
      for (const m of this.royalePublishers) {
        for (const s of m.statistics) {
          set.add(s.date);
        }
      }
      return [...set].sort();
    },
    showChart(): boolean {
      return this.royalePublishers.length > 0 && this.chartLabels.length > 0;
    },
    shownPublishers(): RoyalePublisherGraphSeries[] {
      const pubs = this.royalePublishers;
      if (!pubs.length) {
        return [];
      }
      const mask = this.legendDatasetVisible;
      if (!mask.length || mask.length !== pubs.length) {
        return pubs;
      }
      return pubs.filter((_, i) => mask[i]);
    },
    releaseAnnotations(): Record<string, AnnotationOptions<'line'>> {
      if (!this.showChart) {
        return {};
      }
      const labels = this.chartLabels;
      const labelSet = new Set(labels);

      type GamesByDate = Record<string, Record<string, string[]>>;

      const byDate: GamesByDate = {};
      const shownPublishers = this.shownPublishers;

      for (const member of this.royalePublishers) {
        if (!shownPublishers.some((x) => x.publisherID === member.publisherID)) {
          continue;
        }

        if (!member.publisherID || !member.publisherName) {
          continue;
        }

        for (const pg of member.publisherGames) {
          const masterGame = pg.masterGame;
          if (!masterGame) continue;

          const dateLabel = masterGame.releaseDate;
          if (!dateLabel || !labelSet.has(dateLabel)) {
            continue;
          }

          const gameName = masterGame.gameName;

          byDate[dateLabel] = byDate[dateLabel] ?? {};
          byDate[dateLabel][gameName] = byDate[dateLabel][gameName] ?? [];

          byDate[dateLabel][gameName].push(member.publisherName);
        }
      }

      const labelBorder = 'rgba(214, 153, 58, 0.5)';
      const annotations: Record<string, AnnotationOptions<'line'>> = {};
      for (const [date, games] of Object.entries(byDate)) {
        const content = formatReleaseAnnotationLabel(date, games, this.isMultiPublisher);
        annotations[date] = {
          type: 'line',
          scaleID: 'x',
          value: date,
          borderColor: RELEASE_LINE_BORDER_COLOR,
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
    chartOptions(): ChartOptions<'line'> {
      const options: ChartOptions<'line'> = {
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
            onClick: (e, legendItem, legend) => {
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

      // markRaw keeps Vue from recursing into Chart.js's option objects.
      return markRaw(options);
    },
    chartData(): ChartData<'line', (number | null)[], string> {
      if (!this.showChart) {
        return markRaw({ labels: [], datasets: [] });
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
          data: alignPublisherStatsToLabels(member, labels)
        };
      });
      return markRaw({ labels, datasets });
    }
  },
  watch: {
    chartData() {
      this.$nextTick(() => this.syncLegendVisibilityFromChartIfReady());
    }
  },
  mounted() {
    this.$nextTick(() => this.syncLegendVisibilityFromChartIfReady());
  },
  methods: {
    syncLegendVisibilityFromChartIfReady(): void {
      const holder = this.$refs.lineChartRef as LineChartVueRef | undefined;
      const chart = holder?.chart;
      if (!chart?.data?.datasets?.length) {
        return;
      }
      this.syncLegendVisibilityFromChart(chart);
    },
    syncLegendVisibilityFromChart(chart: Chart): void {
      const n = chart.data.datasets.length;
      const next: boolean[] = [];
      for (let i = 0; i < n; i++) {
        next.push(chart.isDatasetVisible(i));
      }
      this.legendDatasetVisible = next;
    }
  }
});

function formatReleaseAnnotationLabel(date: string, games: Record<string, string[]>, isMultiPublisher: boolean): string[] {
  const lines: string[] = [];

  for (const [gameName, publisherNames] of Object.entries(games)) {
    if (!isMultiPublisher) {
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
}

function alignPublisherStatsToLabels(member: RoyalePublisherGraphSeries, labels: string[]): (number | null)[] {
  const sorted = [...member.statistics].sort((a, b) => a.date.localeCompare(b.date));
  let j = 0;
  let last: number | null = null;
  return labels.map((label) => {
    while (j < sorted.length && sorted[j].date <= label) {
      last = sorted[j].fantasyPoints;
      j++;
    }
    return last;
  });
}
</script>
