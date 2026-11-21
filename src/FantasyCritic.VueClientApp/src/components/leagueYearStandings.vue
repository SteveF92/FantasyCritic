<template>
  <div>
    <h2>Standings</h2>

    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <b-table v-model:sort-by="sortBy" :items="requiredLeagueYear.players" :fields="standingFields" :sort-compare="sortCompare" :tbody-tr-class="standingsRowClass" bordered small responsive striped>
      <template #cell(userName)="data">
        <span v-if="data.item.user">{{ data.item.user.displayName }}</span>
        <font-awesome-icon v-if="data.item.previousYearWinner" v-b-popover.hover.focus="'Reigning Champion'" icon="crown" class="previous-year-winner" />
        <span v-if="data.item.inviteID">{{ data.item.inviteName }}</span>
        <span v-if="!data.item.user && data.item.publisher">&lt;{{ data.item.publisher.playerName }}&gt;</span>
      </template>
      <template #cell(publisher)="data">
        <span v-if="data.item.publisher">
          <router-link :to="{ hash: `#${data.item.publisher.publisherID}` }">{{ data.item.publisher.publisherName }}</router-link>
          <span v-show="hasPendingOrActiveDraft && data.item.publisher.autoDraftMode === 'All'" class="publisher-badge badge rounded-pill bg-info">Auto Draft</span>
          <span v-show="hasPendingOrActiveDraft && data.item.publisher.autoDraftMode === 'StandardGamesOnly'" class="publisher-badge badge rounded-pill bg-info">Auto Draft (No CPKs)</span>
        </span>
        <span v-if="data.item.user && !data.item.publisher">&lt;Not Created&gt;</span>
        <span v-if="data.item.inviteID && data.item.inviteName">
          &lt;Invite Sent&gt;
          <span v-if="requiredLeague.isManager">
            <b-button variant="danger" size="sm" @click="rescindInvite(data.item.inviteID, data.item.inviteName)">Rescind Invite</b-button>
          </span>
        </span>
      </template>
      <template #cell(projectedFantasyPoints)="data">
        <template v-if="data.item.publisher">
          {{ formatScore(data.item.projectedFantasyPoints, 2) }}
          <span v-if="playStarted" class="standings-position" :class="{ 'text-bold': isProjectedTopPublisher(data.item.publisher) }">- {{ ordinal_suffix_of(data.item.projectedRanking) }}</span>
        </template>
      </template>
      <template #cell(totalFantasyPoints)="data">
        <template v-if="data.item.publisher">
          {{ formatScore(data.item.totalFantasyPoints, 2) }}
          <span v-if="playStarted" class="standings-position" :class="{ 'text-bold': isTopPublisher(data.item.publisher) }">- {{ ordinal_suffix_of(data.item.ranking) }}</span>
        </template>
      </template>
      <template #cell(gamesReleased)="data">
        <span v-if="data.item.publisher">{{ data.item.publisher.gamesReleased }}</span>
      </template>
      <template #cell(gamesWillRelease)="data">
        <span v-if="data.item.publisher">{{ data.item.publisher.gamesWillRelease }}</span>
      </template>
      <template #cell(budget)="data">
        <span v-if="data.item.publisher">{{ formatCurrency(data.item.publisher.budget, 0) }}</span>
      </template>
    </b-table>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import { BTableSortBy, TableField, TableRowType, TableStrictClassValue } from 'bootstrap-vue-next';

import { leagueManagerClient, type DeleteInviteRequest, type PlayerPublisherViewModel, type PlayerWithPublisherViewModel, type PublisherViewModel } from '@/api/clients';
import { maxBy, ordinal_suffix_of } from '@/globalFunctions';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatCurrency, formatScore } from '@/utilities/formatters';

interface LeagueYearStandingsData {
  sortBy: BTableSortBy[];
  errorInfo: string;
}

export default defineComponent({
  setup() {
    const leadingFields: TableField<PlayerWithPublisherViewModel>[] = [
      { key: 'userName', label: 'User', thClass: 'bg-primary' },
      { key: 'publisher', label: 'Publisher', thClass: 'bg-primary' }
    ];

    const preDraftFields: TableField<PlayerWithPublisherViewModel>[] = [
      { key: 'draftPosition', label: 'Draft Position', thClass: 'bg-primary', sortable: true },
      { key: 'gamesWillRelease', label: 'Games Drafted', thClass: 'bg-primary', sortable: true }
    ];

    const nextDraftOrderField: TableField<PlayerWithPublisherViewModel> = { key: 'draftPosition', label: 'Next Draft Order', thClass: 'bg-primary', sortable: true };

    const midYearFields: TableField<PlayerWithPublisherViewModel>[] = [
      { key: 'totalFantasyPoints', label: 'Points (Actual)', thClass: 'bg-primary', sortable: true },
      { key: 'projectedFantasyPoints', label: 'Points (Projected)', thClass: 'bg-primary', sortable: true },
      { key: 'gamesReleased', label: 'Released', thClass: 'bg-primary' },
      { key: 'gamesWillRelease', label: 'Expecting', thClass: 'bg-primary' }
    ];

    const yearFinishedFields: TableField<PlayerWithPublisherViewModel>[] = [
      { key: 'totalFantasyPoints', label: 'Points', thClass: 'bg-primary', sortable: true },
      { key: 'gamesReleased', label: 'Released', thClass: 'bg-primary' }
    ];

    const budgetField: TableField<PlayerWithPublisherViewModel> = { key: 'budget', label: 'Budget', thClass: 'bg-primary' };

    return { leadingFields, preDraftFields, nextDraftOrderField, midYearFields, yearFinishedFields, budgetField };
  },
  data(): LeagueYearStandingsData {
    const leagueStore = useLeagueStore();
    const firstDraftFinished = leagueStore.firstDraftFinished;
    const defaultSort: BTableSortBy = firstDraftFinished ? { key: 'totalFantasyPoints', order: 'desc' } : { key: 'draftPosition', order: 'asc' };
    return {
      sortBy: [defaultSort],
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, [
      'requiredLeagueYear',
      'requiredLeague',
      'pendingDraft',
      'publishersThatCanWin',
      'topPublisher',
      'oneShotMode',
      'playStarted',
      'firstDraftFinished',
      'hasPendingOrActiveDraft'
    ]),
    standingFields(): TableField<PlayerWithPublisherViewModel>[] {
      if (!this.firstDraftFinished) {
        return [...this.leadingFields, ...this.preDraftFields];
      }

      const nextDraftOrder = this.pendingDraft?.draftOrderSet ? [this.nextDraftOrderField] : [];
      const budget = this.oneShotMode ? [] : [this.budgetField];

      if (this.requiredLeagueYear.supportedYear.finished) {
        return [...this.leadingFields, ...nextDraftOrder, ...this.yearFinishedFields, ...budget];
      }

      return [...this.leadingFields, ...nextDraftOrder, ...this.midYearFields, ...budget];
    },
    projectedTopPublisher(): PublisherViewModel | null {
      if (this.publishersThatCanWin.length > 0) {
        return maxBy(this.publishersThatCanWin, (x) => x.totalProjectedPoints) ?? null;
      }

      return null;
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    formatCurrency,
    formatScore,
    ordinal_suffix_of,
    standingsRowClass(item: PlayerWithPublisherViewModel | null, type: TableRowType): TableStrictClassValue {
      if (!item || !item.publisher || type !== 'row') {
        return '';
      }

      if (this.requiredLeagueYear.supportedYear.finished && this.topPublisher && this.topPublisher.publisherID === item.publisher.publisherID) {
        return 'table-success';
      }

      return '';
    },
    async rescindInvite(inviteID: string, inviteName: string): Promise<void> {
      this.errorInfo = '';
      const model: DeleteInviteRequest = {
        leagueID: this.requiredLeague.leagueID,
        inviteID: inviteID
      };

      try {
        await leagueManagerClient.rescindInvite(model);
        this.notifyAction('The invite to ' + inviteName + ' has been rescinded.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when rescinding the invite.');
      }
    },
    isTopPublisher(publisher: PlayerPublisherViewModel): boolean {
      return !!this.topPublisher && this.topPublisher.publisherID === publisher.publisherID;
    },
    isProjectedTopPublisher(publisher: PlayerPublisherViewModel): boolean {
      return !!this.projectedTopPublisher && this.projectedTopPublisher.publisherID === publisher.publisherID;
    },
    sortCompare(aRow: PlayerWithPublisherViewModel, bRow: PlayerWithPublisherViewModel, key: string): number {
      const sortValues = sortValuesByKey[key];
      if (!sortValues) {
        return 0;
      }

      if (!aRow.publisher || !bRow.publisher) {
        if (!aRow.publisher && !bRow.publisher) {
          return 0;
        }

        // Rows without a publisher (invites, players who haven't created one) stay at the bottom in both directions.
        // bootstrap-vue-next negates the result for a descending sort, so negate it first to cancel that out.
        const withoutPublisherLast = aRow.publisher ? -1 : 1;
        const descending = this.sortBy.find((x) => x.key === key)?.order === 'desc';
        return descending ? -withoutPublisherLast : withoutPublisherLast;
      }

      const aValues = sortValues(aRow.publisher);
      const bValues = sortValues(bRow.publisher);
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
const sortValuesByKey: Partial<Record<string, (publisher: PlayerPublisherViewModel) => number[]>> = {
  draftPosition: (x) => [x.draftPosition],
  gamesWillRelease: (x) => [x.gamesWillRelease, -x.draftPosition],
  totalFantasyPoints: (x) => [x.totalFantasyPoints, x.totalProjectedPoints],
  projectedFantasyPoints: (x) => [x.totalProjectedPoints, x.totalFantasyPoints]
};
</script>

<style scoped>
div :deep(tr.table-success td) {
  font-weight: bolder;
}

.previous-year-winner {
  margin-left: 4px;
  color: #d6993a;
}

.publisher-badge {
  margin-left: 5px;
}
</style>
