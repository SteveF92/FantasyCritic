<template>
  <div>
    <h2>Standings</h2>

    <b-table :sort-by.sync="sortBy" :sort-desc.sync="sortDesc" :items="standings" :fields="standingFields" :sort-compare="sortCompare" bordered small responsive striped>
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
        {{ data.item.projectedFantasyPoints | score(2) }}
        <span v-if="playStarted" class="standings-position" :class="{ 'text-bold': isProjectedTopPublisher(data.item.publisher) }">- {{ ordinal_suffix_of(data.item.projectedRanking) }}</span>
      </template>
      <template #cell(totalFantasyPoints)="data">
        {{ data.item.totalFantasyPoints | score(2) }}
        <span v-if="playStarted" class="standings-position" :class="{ 'text-bold': isTopPublisher(data.item.publisher) }">- {{ ordinal_suffix_of(data.item.ranking) }}</span>
      </template>
    </b-table>
  </div>
</template>
<script>
import ConferenceMixin from '@/mixins/conferenceMixin.js';
import { maxBy, ordinal_suffix_of } from '@/globalFunctions';

export default {
  mixins: [ConferenceMixin],
  data() {
    return {
      standingFieldsInternal: [
        { key: 'leagueName', label: 'League', thClass: 'bg-primary' },
        { key: 'displayName', label: 'User', thClass: 'bg-primary' },
        { key: 'publisherName', label: 'Publisher', thClass: 'bg-primary' },
        { key: 'totalFantasyPoints', label: 'Points (Actual)', thClass: 'bg-primary', sortable: true },
        { key: 'projectedFantasyPoints', label: 'Points (Projected)', thClass: 'bg-primary', sortable: true }
      ],
      sortBy: 'totalFantasyPoints',
      sortDesc: true
    };
  },
  computed: {
    standingFields() {
      if (!this.conferenceYear.supportedYear.finished) {
        return this.standingFieldsInternal;
      }

      let fieldsToUse = this.standingFieldsInternal;
      fieldsToUse = fieldsToUse.slice(0, 3).concat(fieldsToUse.slice(4));
      fieldsToUse[3].label = 'Points';

      return fieldsToUse;
    },
    standings() {
      let standings = this.conferenceYear.standings;
      if (!standings) {
        return [];
      }
      for (let i = 0; i < standings.length; ++i) {
        if (this.conferenceYear.supportedYear.finished && this.topPublisher && this.topPublisher.publisherID === standings[i].publisherID) {
          standings[i]._rowVariant = 'success';
        }
      }
      return standings;
    },
    topPublisher() {
      if (this.conferenceYear.standings && this.conferenceYear.standings.length > 0) {
        return maxBy(this.conferenceYear.standings, (x) => x.totalFantasyPoints);
      }

      return null;
    },
    projectedTopPublisher() {
      if (this.conferenceYear.standings && this.conferenceYear.standings.length > 0) {
        return maxBy(this.conferenceYear.standings, (x) => x.totalProjectedPoints);
      }

      return null;
    }
  },
  methods: {
    isTopPublisher(publisher) {
      if (!publisher) {
        return false;
      }
      return this.topPublisher && this.topPublisher.publisherID === publisher.publisherID;
    },
    isProjectedTopPublisher(publisher) {
      if (!publisher) {
        return false;
      }
      return this.projectedTopPublisher && this.projectedTopPublisher.publisherID === publisher.publisherID;
    },
    ordinal_suffix_of(num) {
      return ordinal_suffix_of(num);
    },
    sortCompare(aRow, bRow, key) {
      // Extend with other sortable columns as necessary.
      const secondarySorts = {
        totalFantasyPoints: 'projectedFantasyPoints',
        projectedFantasyPoints: 'totalFantasyPoints'
      };
      const a = aRow[key];
      const b = bRow[key];
      const primarySort = a < b ? -1 : a > b ? 1 : 0;
      // Return unless the primary Sort Key results in a Tie, i.e each row has 0 points.
      if (primarySort != 0) return primarySort;
      const secondarySortKey = secondarySorts[key];
      const secondaryA = aRow[secondarySortKey];
      const secondaryB = bRow[secondarySortKey];
      return secondaryA < secondaryB ? -1 : secondaryA > secondaryB ? 1 : 0;
    }
  }
};
</script>
<style scoped>
.header {
  display: flex;
  justify-content: space-between;
  flex-wrap: wrap;
}

.projections-label {
  margin-top: 14px;
}

.toggle {
  margin-top: 4px;
}

.publisher-name {
  display: inline-block;
  word-wrap: break-word;
  max-width: 300px;
}

@media only screen and (min-width: 1550px) {
  .publisher-name {
    max-width: 650px;
  }
}
@media only screen and (max-width: 1549px) {
  .publisher-name {
    max-width: 150px;
  }
}
@media only screen and (max-width: 768px) {
  .publisher-name {
    max-width: 200px;
  }
}
div >>> tr.table-success td {
  font-weight: bolder;
}

.previous-year-winner {
  margin-left: 4px;
  color: #d6993a;
}
</style>
