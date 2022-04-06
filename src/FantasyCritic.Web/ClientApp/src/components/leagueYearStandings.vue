<template>
  <div>
    <h2>Standings</h2>

    <b-table :sort-by.sync="sortBy" :sort-desc.sync="sortDesc" :items="standings" :fields="standingFields" bordered small responsive striped>
      <template #cell(userName)="data">
        <span v-if="data.item.user">{{ data.item.user.displayName }}</span>
        <font-awesome-icon v-if="data.item.previousYearWinner" v-b-popover.hover.focus="'Reigning Champion'" icon="crown" class="previous-year-winner" />
        <span v-if="!data.item.user">{{ data.item.inviteName }}</span>
      </template>
      <template #cell(publisher)="data">
        <span v-if="data.item.publisher">
          <router-link :to="{ hash: `#${data.item.publisher.publisherID}` }">{{ data.item.publisher.publisherName }}</router-link>
          <span v-show="!leagueYear.playStatus.draftFinished && data.item.publisher.autoDraft" class="publisher-badge badge badge-pill badge-primary badge-info">Auto Draft</span>
        </span>
        <span v-if="data.item.user && !data.item.publisher">&lt;Not Created&gt;</span>
        <span v-if="!data.item.user">
          &lt;Invite Sent&gt;
          <span v-if="league.isManager">
            <b-button variant="danger" size="sm" @click="rescindInvite(data.item.inviteID, data.item.inviteName)">Rescind Invite</b-button>
          </span>
        </span>
      </template>
      <template #cell(projectedFantasyPoints)="data">{{ data.item.projectedFantasyPoints | score(2) }}</template>
      <template #cell(totalFantasyPoints)="data">{{ data.item.totalFantasyPoints | score(2) }}</template>
      <template #cell(gamesReleased)="data">
        <span v-if="data.item.publisher">{{ data.item.publisher.gamesReleased }}</span>
      </template>
      <template #cell(gamesWillRelease)="data">
        <span v-if="data.item.publisher">{{ data.item.publisher.gamesWillRelease }}</span>
      </template>
      <template #cell(budget)="data">
        <span v-if="data.item.publisher">{{ data.item.publisher.budget | money }}</span>
      </template>
    </b-table>
  </div>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      standingFieldsInternal: [
        { key: 'userName', label: 'User', thClass: 'bg-primary' },
        { key: 'publisher', label: 'Publisher', thClass: 'bg-primary' },
        { key: 'projectedFantasyPoints', label: 'Points (Projected)', thClass: 'bg-primary', sortable: true },
        { key: 'totalFantasyPoints', label: 'Points (Actual)', thClass: 'bg-primary', sortable: true },
        { key: 'gamesReleased', label: 'Released', thClass: 'bg-primary' },
        { key: 'gamesWillRelease', label: 'Expecting', thClass: 'bg-primary' },
        { key: 'budget', label: 'Budget', thClass: 'bg-primary' }
      ],
      sortBy: 'totalFantasyPoints',
      sortDesc: true
    };
  },
  computed: {
    standingFields() {
      if (!this.oneShotMode) {
        return this.standingFieldsInternal;
      }

      return this.standingFieldsInternal.slice(0, -1);
    },
    standings() {
      let standings = this.leagueYear.players;
      if (!standings) {
        return [];
      }
      for (var i = 0; i < standings.length; ++i) {
        if (this.leagueYear.supportedYear.finished && this.topPublisher.publisherID === standings[i].publisher.publisherID) {
          standings[i]._rowVariant = 'success';
        }
      }
      return standings;
    },
    topPublisher() {
      if (this.leagueYear.publishers && this.leagueYear.publishers.length > 0) {
        return _.maxBy(this.leagueYear.publishers, 'totalFantasyPoints');
      }

      return null;
    }
  },
  methods: {
    rescindInvite(inviteID, inviteName) {
      var model = {
        inviteID: inviteID
      };
      axios
        .post('/api/leagueManager/RescindInvite', model)
        .then(() => {
          let actionInfo = {
            message: 'The invite to ' + inviteName + ' has been rescinded.',
            fetchLeague: true,
            fetchLeagueYear: true
          };
          this.notifyAction(actionInfo);
        })
        .catch(() => {});
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
