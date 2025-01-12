<template>
  <div>
    <h2>Standings</h2>

    <b-table :sort-by.sync="sortBy" :sort-desc.sync="sortDesc" :items="standings" :fields="standingFields" :sort-compare="sortCompare" bordered small responsive striped>
      <template #cell(userName)="data">
        <span v-if="data.item.user">{{ data.item.user.displayName }}</span>
        <font-awesome-icon v-if="data.item.previousYearWinner" v-b-popover.hover.focus="'Reigning Champion'" icon="crown" class="previous-year-winner" />
        <span v-if="!data.item.user">{{ data.item.inviteName }}</span>
      </template>
      <template #cell(publisher)="data">
        <span v-if="data.item.publisher">
          <router-link :to="{ hash: `#${data.item.publisher.publisherID}` }">{{ data.item.publisher.publisherName }}</router-link>
          <span v-show="!leagueYear.playStatus.draftFinished && data.item.publisher.autoDraftMode === 'All'" class="publisher-badge badge badge-pill badge-primary badge-info">Auto Draft</span>
          <span v-show="!leagueYear.playStatus.draftFinished && data.item.publisher.autoDraftMode === 'StandardGamesOnly'" class="publisher-badge badge badge-pill badge-primary badge-info">
            Auto Draft (No CPKs)
          </span>
        </span>
        <span v-if="data.item.user && !data.item.publisher">&lt;Not Created&gt;</span>
        <span v-if="!data.item.user">
          &lt;Invite Sent&gt;
          <span v-if="league.isManager">
            <b-button variant="danger" size="sm" @click="rescindInvite(data.item.inviteID, data.item.inviteName)">Rescind Invite</b-button>
          </span>
        </span>
      </template>
      <template #cell(projectedFantasyPoints)="data">
        {{ data.item.projectedFantasyPoints | score(2) }}
        <span v-if="playStarted" class="standings-position" :class="{ 'text-bold': isProjectedTopPublisher(data.item.publisher) }">- {{ ordinal_suffix_of(data.item.projectedRanking) }}</span>
      </template>
      <template #cell(totalFantasyPoints)="data">
        {{ data.item.totalFantasyPoints | score(2) }}
        <span v-if="playStarted" class="standings-position" :class="{ 'text-bold': isTopPublisher(data.item.publisher) }">- {{ ordinal_suffix_of(data.item.ranking) }}</span>
      </template>
      <template #cell(gamesReleased)="data">
        <span v-if="data.item.publisher">{{ data.item.publisher.gamesReleased }}</span>
      </template>
      <template #cell(gamesWillRelease)="data">
        <span v-if="data.item.publisher">{{ data.item.publisher.gamesWillRelease }}</span>
      </template>
      <template #cell(budget)="data">
        <span v-if="data.item.publisher">{{ data.item.publisher.budget | money(0) }}</span>
      </template>
    </b-table>
  </div>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';
import GlobalFunctions from '@/globalFunctions';
import _ from 'lodash';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      draftNotFinishedStandingsFields: [
        { key: 'userName', label: 'User', thClass: 'bg-primary' },
        { key: 'publisher', label: 'Publisher', thClass: 'bg-primary' },
        { key: 'draftPosition', label: 'Draft Position', thClass: 'bg-primary', sortable: true },
        { key: 'gamesWillRelease', label: 'Games Drafted', thClass: 'bg-primary', sortable: true }
      ],
      midYearStandingsFields: [
        { key: 'userName', label: 'User', thClass: 'bg-primary' },
        { key: 'publisher', label: 'Publisher', thClass: 'bg-primary' },
        { key: 'totalFantasyPoints', label: 'Points (Actual)', thClass: 'bg-primary', sortable: true },
        { key: 'projectedFantasyPoints', label: 'Points (Projected)', thClass: 'bg-primary', sortable: true },
        { key: 'gamesReleased', label: 'Released', thClass: 'bg-primary' },
        { key: 'gamesWillRelease', label: 'Expecting', thClass: 'bg-primary' },
        { key: 'budget', label: 'Budget', thClass: 'bg-primary' }
      ],
      yearFinishedStandingsFields: [
        { key: 'userName', label: 'User', thClass: 'bg-primary' },
        { key: 'publisher', label: 'Publisher', thClass: 'bg-primary' },
        { key: 'totalFantasyPoints', label: 'Points', thClass: 'bg-primary', sortable: true },
        { key: 'gamesReleased', label: 'Released', thClass: 'bg-primary' },
        { key: 'budget', label: 'Budget', thClass: 'bg-primary' }
      ],
      sortBy: 'totalFantasyPoints',
      sortDesc: true
    };
  },
  created() {
    if (!this.leagueYear.playStatus.draftFinished) {
      this.sortBy = 'draftPosition';
      this.sortDesc = false;
    }
  },
  computed: {
    standingFields() {
      if (!this.leagueYear.playStatus.draftFinished) {
        return this.draftNotFinishedStandingsFields;
      }

      if (!this.leagueYear.supportedYear.finished) {
        if (this.oneShotMode) {
          return this.midYearStandingsFields.slice(0, -1);
        }
        return this.midYearStandingsFields;
      }

      if (this.oneShotMode) {
        return this.yearFinishedStandingsFields.slice(0, -1);
      }

      return this.yearFinishedStandingsFields;
    },
    standings() {
      let standings = this.leagueYear.players;
      if (!standings) {
        return [];
      }
      for (let i = 0; i < standings.length; ++i) {
        if (!standings[i].publisher) {
          continue;
        }
        if (this.leagueYear.supportedYear.finished && this.topPublisher && this.topPublisher.publisherID === standings[i].publisher.publisherID) {
          standings[i]._rowVariant = 'success';
        }
      }
      return standings;
    },
    topPublisher() {
      if (this.leagueYear.publishers && this.leagueYear.publishers.length > 0) {
        return GlobalFunctions.maxBy(this.leagueYear.publishers, (x) => x.totalFantasyPoints);
      }

      return null;
    },
    projectedTopPublisher() {
      if (this.leagueYear.publishers && this.leagueYear.publishers.length > 0) {
        return GlobalFunctions.maxBy(this.leagueYear.publishers, (x) => x.totalProjectedPoints);
      }

      return null;
    }
  },
  methods: {
    rescindInvite(inviteID, inviteName) {
      const model = {
        leagueID: this.leagueYear.league.leagueID,
        inviteID: inviteID
      };
      axios
        .post('/api/leagueManager/RescindInvite', model)
        .then(() => {
          this.notifyAction('The invite to ' + inviteName + ' has been rescinded.');
        })
        .catch(() => {});
    },
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
      return GlobalFunctions.ordinal_suffix_of(num);
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

.publisher-badge {
  margin-left: 5px;
}
</style>
