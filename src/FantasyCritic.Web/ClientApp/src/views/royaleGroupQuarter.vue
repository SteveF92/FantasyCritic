<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="groupQuarter">
      <div class="publisher-header bg-secondary">
        <h1 class="publisher-name">{{ groupQuarter.groupName }}</h1>
        <h4>
          Year/Quarter:
          <router-link :to="{ name: 'criticsRoyale', params: { year: groupQuarter.year, quarter: groupQuarter.quarter } }">{{ groupQuarter.year }}-Q{{ groupQuarter.quarter }}</router-link>
        </h4>
      </div>

      <div class="nav-area">
        <router-link :to="{ name: 'royaleGroup', params: { groupid: groupid } }">Back to Group</router-link>
      </div>

      <div v-if="allQuarters && selectedYear" class="quarter-selection">
        <b-dropdown :text="selectedYear.toString()">
          <b-dropdown-item v-for="y in years" :key="y" :active="selectedYear === y" @click="selectYear(y)">
            {{ y }}
          </b-dropdown-item>
        </b-dropdown>
        <b-dropdown :text="`Q${quarter}`">
          <b-dropdown-item
            v-for="q in quartersInSelectedYear"
            :key="q.year + '-' + q.quarter"
            :active="q.year === year && q.quarter === quarter"
            :to="{ name: 'royaleGroupQuarter', params: { groupid, year: q.year, quarter: q.quarter } }">
            {{ q.year }}-Q{{ q.quarter }}
          </b-dropdown-item>
        </b-dropdown>
      </div>

      <h2>Standings</h2>
      <b-table v-if="groupQuarter.members.length > 0" striped bordered small responsive :items="groupQuarter.members" :fields="standingsFields">
        <template #cell(ranking)="data">
          <template v-if="data.item.ranking">{{ data.item.ranking }}</template>
          <template v-else>--</template>
        </template>
        <template #cell(displayName)="data">
          <router-link :to="{ name: 'royaleHistory', params: { userid: data.item.userID } }">{{ data.item.displayName }}</router-link>
        </template>
        <template #cell(publisherName)="data">
          <template v-if="data.item.hasPublisher">
            <router-link :to="{ name: 'royalePublisher', params: { publisherid: data.item.publisherID } }">
              {{ data.item.publisherName }}
            </router-link>
          </template>
          <template v-else>
            <span class="text-muted">No publisher yet</span>
          </template>
        </template>
        <template #cell(totalFantasyPoints)="data">
          <template v-if="data.item.hasPublisher">{{ data.item.totalFantasyPoints }}</template>
          <template v-else>--</template>
        </template>
      </b-table>
      <div v-else>
        <p>No members in this group.</p>
      </div>

      <template v-if="showGroupPointsChart">
        <div class="royale-chart-container">
          <LineChartGenerator
            chart-id="group-quarter-points-chart"
            dataset-id-key="label"
            :width="groupChartCanvasWidth"
            :height="groupChartCanvasHeight"
            :styles="groupChartCanvasStyles"
            :chart-options="groupChartOptions"
            :chart-data="groupChartData" />
        </div>
      </template>
    </div>
    <div v-else-if="notFound">
      <div class="alert alert-warning">Group or quarter not found.</div>
    </div>
    <div v-else class="spinner">
      <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import { DateTime } from 'luxon';
import { Line as LineChartGenerator } from 'vue-chartjs/legacy';

import { orderBy } from '@/globalFunctions';

const SERIES_COLORS = ['#d6993a', '#5cb8d4', '#9b7ed9', '#6bcf7a', '#e07a7a', '#e6d84a', '#4a9fe6', '#d46a9e'];

export default {
  components: { LineChartGenerator },
  props: {
    groupid: { type: String, required: true },
    year: { type: Number, required: true },
    quarter: { type: Number, required: true }
  },
  data() {
    return {
      groupQuarter: null,
      allQuarters: null,
      selectedYear: null,
      notFound: false,
      groupChartCanvasWidth: 1200,
      groupChartCanvasHeight: 600,
      groupChartCanvasStyles: {
        width: '100%',
        height: '100%',
        position: 'relative'
      },
      standingsFields: [
        { key: 'ranking', label: 'Rank', thClass: ['bg-primary', 'ranking-column'], tdClass: 'ranking-column' },
        { key: 'displayName', label: 'Player', thClass: 'bg-primary' },
        { key: 'publisherName', label: 'Publisher', thClass: 'bg-primary' },
        { key: 'totalFantasyPoints', label: 'Total Points', thClass: 'bg-primary' }
      ]
    };
  },
  computed: {
    years() {
      if (!this.allQuarters) return [];
      const yearsList = this.allQuarters.map((x) => x.year);
      return orderBy([...new Set(yearsList)], (x) => x);
    },
    quartersInSelectedYear() {
      if (!this.allQuarters) return [];
      return this.allQuarters.filter((q) => q.year === this.selectedYear).sort((a, b) => a.quarter - b.quarter);
    },
    chartMembers() {
      if (!this.groupQuarter?.members) {
        return [];
      }
      return this.groupQuarter.members.filter((m) => m.hasPublisher && m.publisherID && m.statistics?.length > 0);
    },
    combinedStatisticsLabels() {
      const set = new Set();
      for (const m of this.chartMembers) {
        for (const s of m.statistics) {
          set.add(s.date);
        }
      }
      return [...set].sort();
    },
    showGroupPointsChart() {
      return this.chartMembers.length > 0 && this.combinedStatisticsLabels.length > 0;
    },
    groupReleaseAnnotations() {
      if (!this.showGroupPointsChart) {
        return {};
      }
      const labelSet = new Set(this.combinedStatisticsLabels);
      const byGameAndDate = new Map();
      for (const member of this.chartMembers) {
        for (const pg of member.publisherGames || []) {
          if (!pg.masterGame) {
            continue;
          }
          const dateLabel = this.masterGameReleaseChartLabel(pg.masterGame);
          if (!dateLabel || !labelSet.has(dateLabel)) {
            continue;
          }
          const masterGameID = pg.masterGame.masterGameID;
          const dedupeKey = `${dateLabel}\0${masterGameID}`;
          let entry = byGameAndDate.get(dedupeKey);
          if (!entry) {
            entry = {
              dateLabel,
              masterGameID,
              gameName: pg.masterGame.gameName,
              publisherNames: []
            };
            byGameAndDate.set(dedupeKey, entry);
          }
          entry.publisherNames.push(member.publisherName);
        }
      }
      const lineColor = 'rgba(255, 255, 255, 0.42)';
      const labelBorder = 'rgba(214, 153, 58, 0.5)';
      const annotations = {};
      let annIndex = 0;
      for (const g of byGameAndDate.values()) {
        const content = this.formatReleaseAnnotationLabel(g.gameName, g.publisherNames);
        annotations[`release-${g.masterGameID}-${g.dateLabel}-${annIndex}`] = {
          type: 'line',
          scaleID: 'x',
          value: g.dateLabel,
          borderColor: lineColor,
          borderWidth: 2,
          borderDash: [4, 4],
          drawTime: 'beforeDatasetsDraw',
          z: -1,
          label: {
            display: false,
            content,
            drawTime: 'afterDatasetsDraw',
            backgroundColor: 'rgba(20, 20, 20, 0.95)',
            borderColor: labelBorder,
            borderWidth: 1,
            color: '#ffffff',
            font: { size: 12 },
            padding: 8,
            borderRadius: 4,
            position: 'start',
            yAdjust: -6
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
        annIndex += 1;
      }
      return annotations;
    },
    groupChartOptions() {
      return {
        responsive: true,
        maintainAspectRatio: false,
        layout: {
          padding: {
            top: 4,
            right: 8,
            bottom: 52,
            left: 4
          }
        },
        interaction: { mode: 'index', intersect: false },
        plugins: {
          legend: {
            labels: {
              color: 'rgba(255, 255, 255, 0.9)',
              font: { size: 13 },
              usePointStyle: true,
              padding: 16
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
            interaction: {
              mode: 'nearest',
              axis: 'x',
              intersect: false
            },
            annotations: this.groupReleaseAnnotations
          }
        },
        scales: {
          x: {
            ticks: {
              color: 'rgba(255, 255, 255, 0.75)',
              maxRotation: 45,
              minRotation: 0,
              padding: 6
            },
            grid: { color: 'rgba(255, 255, 255, 0.08)' }
          },
          y: {
            min: 0,
            ticks: { color: 'rgba(255, 255, 255, 0.75)' },
            grid: { color: 'rgba(255, 255, 255, 0.08)' }
          }
        }
      };
    },
    groupChartData() {
      if (!this.showGroupPointsChart) {
        return { labels: [], datasets: [] };
      }
      const labels = this.combinedStatisticsLabels;
      const datasets = this.chartMembers.map((member, i) => {
        const color = SERIES_COLORS[i % SERIES_COLORS.length];
        return {
          label: member.publisherName,
          borderColor: color,
          backgroundColor: 'transparent',
          borderWidth: 2,
          fill: false,
          stepped: true,
          pointRadius: 0,
          data: this.alignPublisherStatsToLabels(member, labels)
        };
      });
      return { labels, datasets };
    }
  },
  watch: {
    async $route() {
      await this.fetchData();
    }
  },
  async created() {
    await this.fetchData();
  },
  methods: {
    async fetchData() {
      try {
        const [quarterResponse, allQuartersResponse] = await Promise.all([
          axios.get(`/api/RoyaleGroup/GetRoyaleGroupQuarter/${this.groupid}/${this.year}/${this.quarter}`),
          axios.get('/api/Royale/RoyaleQuarters')
        ]);
        this.groupQuarter = quarterResponse.data;
        this.allQuarters = allQuartersResponse.data;
        this.selectedYear = this.year;
      } catch {
        this.notFound = true;
      }
    },
    selectYear(year) {
      this.selectedYear = year;
    },
    formatReleaseAnnotationLabel(gameName, publisherNames) {
      const unique = [...new Set(publisherNames)].sort((a, b) => a.localeCompare(b));
      const n = unique.length;
      if (n === 0) {
        return gameName;
      }
      if (n === 1) {
        return `${gameName} — ${unique[0]}`;
      }
      const listStr = unique.join(', ');
      const full = `${gameName} — ${listStr}`;
      const maxLen = 72;
      if (full.length <= maxLen && n <= 5) {
        return full;
      }
      return `${gameName} (${n} publishers)`;
    },
    masterGameReleaseChartLabel(game) {
      return game?.releaseDate;
    },
    alignPublisherStatsToLabels(member, labels) {
      const sorted = [...member.statistics]
        .map((s) => ({
          date: s.date,
          fantasyPoints: Number(s.fantasyPoints)
        }))
        .sort((a, b) => a.date.localeCompare(b.date));
      let j = 0;
      let last = null;
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

<style scoped>
.publisher-header {
  margin-top: 10px;
  border: 2px;
  border-color: #d6993a;
  border-style: solid;
  padding-left: 5px;
}

.publisher-name {
  display: block;
  max-width: 100%;
  word-wrap: break-word;
}

.nav-area {
  margin-bottom: 10px;
}

.quarter-selection {
  float: right;
  margin-bottom: 10px;
}

.spinner {
  display: flex;
  justify-content: space-around;
}

.group-chart-heading {
  margin-top: 1.5rem;
}

.royale-chart-container {
  height: 625px;
  max-width: 100%;
  padding: 16px 12px 20px;
  margin-top: 8px;
  margin-bottom: 12px;
  background: #252525;
  border: 1px solid rgba(214, 153, 58, 0.35);
  border-radius: 8px;
  box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.04);
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.royale-chart-container > div {
  flex: 1 1 auto;
  min-height: 0;
}
</style>

<style>
.ranking-column {
  width: 50px;
  text-align: right;
}
</style>
