<template>
  <div>
    <h2>Standings</h2>

    <b-table
      v-model:sort-by="sortBy"
      :items="requiredConferenceYear.standings"
      :fields="standingFields"
      :sort-compare="sortCompare"
      :tbody-tr-class="standingsRowClass"
      bordered
      small
      responsive
      striped>
      <template #cell(leagueName)="data">
        <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: data.item.year } }" class="league-link">
          {{ data.item.leagueName }}
        </router-link>
      </template>
      <template #cell(publisherName)="data">
        <router-link :to="{ name: 'publisher', params: { publisherid: data.item.publisherID } }">
          {{ data.item.publisherName }}
        </router-link>
      </template>

      <template #cell(projectedFantasyPoints)="data">
        {{ formatScore(data.item.projectedFantasyPoints, 2) }}
        <span v-if="playStarted" class="standings-position" :class="{ 'text-bold': isProjectedTopPublisher(data.item) }">- {{ ordinal_suffix_of(data.item.projectedRanking) }}</span>
      </template>
      <template #cell(totalFantasyPoints)="data">
        {{ formatScore(data.item.totalFantasyPoints, 2) }}
        <span v-if="playStarted" class="standings-position" :class="{ 'text-bold': isTopPublisher(data.item) }">- {{ ordinal_suffix_of(data.item.ranking) }}</span>
      </template>

      <template #cell(leagueRanking)="data">
        <span v-if="playStarted" class="standings-position">{{ ordinal_suffix_of(data.item.leagueRanking) }}</span>
      </template>
    </b-table>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import type { BTableSortBy, TableField, TableRowType, TableStrictClassValue } from 'bootstrap-vue-next';

import type { ConferenceYearStandingViewModel } from '@/api/clients';
import { maxBy, ordinal_suffix_of } from '@/globalFunctions';
import { useConferenceStore } from '@/store/conferenceStore';
import { formatScore } from '@/utilities/formatters';

interface ConferenceYearStandingsData {
  sortBy: BTableSortBy[];
}

export default defineComponent({
  setup() {
    const leadingFields: TableField<ConferenceYearStandingViewModel>[] = [
      { key: 'leagueName', label: 'League', thClass: 'bg-primary' },
      { key: 'displayName', label: 'User', thClass: 'bg-primary' },
      { key: 'publisherName', label: 'Publisher', thClass: 'bg-primary' }
    ];

    const midYearFields: TableField<ConferenceYearStandingViewModel>[] = [
      { key: 'totalFantasyPoints', label: 'Points (Actual)', thClass: 'bg-primary', sortable: true },
      { key: 'projectedFantasyPoints', label: 'Points (Projected)', thClass: 'bg-primary', sortable: true }
    ];

    const yearFinishedFields: TableField<ConferenceYearStandingViewModel>[] = [{ key: 'totalFantasyPoints', label: 'Points', thClass: 'bg-primary', sortable: true }];

    const leagueRankingField: TableField<ConferenceYearStandingViewModel> = { key: 'leagueRanking', label: 'League Rank', thClass: 'bg-primary', sortable: true };

    return { leadingFields, midYearFields, yearFinishedFields, leagueRankingField };
  },
  data(): ConferenceYearStandingsData {
    return {
      sortBy: [{ key: 'totalFantasyPoints', order: 'desc' }]
    };
  },
  computed: {
    ...mapState(useConferenceStore, ['requiredConferenceYear', 'playStarted']),
    standingFields(): TableField<ConferenceYearStandingViewModel>[] {
      const pointsFields = this.requiredConferenceYear.supportedYear.finished ? this.yearFinishedFields : this.midYearFields;
      return [...this.leadingFields, ...pointsFields, this.leagueRankingField];
    },
    standingsThatCanWin(): ConferenceYearStandingViewModel[] {
      // Publishers with no user stay in the standings, but can't win.
      return this.requiredConferenceYear.standings.filter((x) => x.hasUser);
    },
    topPublisher(): ConferenceYearStandingViewModel | null {
      return maxBy(this.standingsThatCanWin, (x) => x.totalFantasyPoints) ?? null;
    },
    projectedTopPublisher(): ConferenceYearStandingViewModel | null {
      return maxBy(this.standingsThatCanWin, (x) => x.projectedFantasyPoints) ?? null;
    }
  },
  methods: {
    formatScore,
    ordinal_suffix_of,
    standingsRowClass(item: ConferenceYearStandingViewModel | null, type: TableRowType): TableStrictClassValue {
      if (!item || type !== 'row') {
        return '';
      }

      if (this.requiredConferenceYear.supportedYear.finished && this.isTopPublisher(item)) {
        return 'table-success';
      }

      return '';
    },
    isTopPublisher(standing: ConferenceYearStandingViewModel): boolean {
      return !!this.topPublisher && this.topPublisher.publisherID === standing.publisherID;
    },
    isProjectedTopPublisher(standing: ConferenceYearStandingViewModel): boolean {
      return !!this.projectedTopPublisher && this.projectedTopPublisher.publisherID === standing.publisherID;
    },
    sortCompare(aRow: ConferenceYearStandingViewModel, bRow: ConferenceYearStandingViewModel, key: string): number {
      const sortValues = sortValuesByKey[key];
      if (!sortValues) {
        return 0;
      }

      const aValues = sortValues(aRow);
      const bValues = sortValues(bRow);
      for (let i = 0; i < aValues.length; i++) {
        if (aValues[i] !== bValues[i]) {
          return aValues[i] < bValues[i] ? -1 : 1;
        }
      }

      return 0;
    }
  }
});

// The values each sortable column sorts by, in order; the later ones break ties, such as everyone on 0 points before any games release.
const sortValuesByKey: Partial<Record<string, (standing: ConferenceYearStandingViewModel) => number[]>> = {
  totalFantasyPoints: (x) => [x.totalFantasyPoints, x.projectedFantasyPoints],
  projectedFantasyPoints: (x) => [x.projectedFantasyPoints, x.totalFantasyPoints],
  leagueRanking: (x) => [x.leagueRanking]
};
</script>

<style scoped>
div :deep(tr.table-success td) {
  font-weight: bolder;
}
</style>
