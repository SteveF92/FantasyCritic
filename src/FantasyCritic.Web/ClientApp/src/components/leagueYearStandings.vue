<template>
  <div>
    <div class="header">
      <h2>Standings</h2>
      <span>
        <label class="projections-label">Advanced Projections</label>
        <toggle-button
          class="toggle"
          :class="{ 'toggle-on': advancedProjections }"
          v-model="advancedProjections"
          :sync="true"
          :labels="{ checked: 'On', unchecked: 'Off' }"
          :css-colors="true"
          :font-size="13" />
      </span>
    </div>

    <b-table :sort-by.sync="sortBy" :sort-desc.sync="sortDesc" :items="standings" :fields="standingFields" bordered small responsive striped>
      <template #cell(userName)="data">
        <span v-if="data.item.user">{{ data.item.user.displayName }}</span>
        <font-awesome-icon v-if="data.item.previousYearWinner" icon="crown" class="previous-year-winner" v-b-popover.hover="'Reigning Champion'" />
        <span v-if="!data.item.user">{{ data.item.inviteName }}</span>
      </template>
      <template #cell(publisher)="data">
        <span v-if="data.item.publisher">
          <a :href="'#' + data.item.publisher.publisherID">{{ data.item.publisher.publisherName }}</a>
          <span class="publisher-badge badge badge-pill badge-primary badge-info" v-show="!leagueYear.playStatus.draftFinished && data.item.publisher.autoDraft">Auto Draft</span>
        </span>
        <span v-if="data.item.user && !data.item.publisher">&lt;Not Created&gt;</span>
        <span v-if="!data.item.user">
          &lt;Invite Sent&gt;
          <span v-if="league.isManager">
            <b-button variant="danger" size="sm" v-on:click="rescindInvite(data.item.inviteID, data.item.inviteName)">Rescind Invite</b-button>
          </span>
        </span>
      </template>
      <template #cell(simpleProjectedFantasyPoints)="data">{{ data.item.simpleProjectedFantasyPoints | score(2) }}</template>
      <template #cell(advancedProjectedFantasyPoints)="data">{{ data.item.advancedProjectedFantasyPoints | score(2) }}</template>
      <template #cell(totalFantasyPoints)="data">{{ data.item.totalFantasyPoints | score(2) }}</template>
      <template #cell(budget)="data">
        <span v-if="data.item.publisher">{{ data.item.publisher.budget | money }}</span>
      </template>
      <template #cell(totalFantasyPoints)="data">{{ data.item.totalFantasyPoints | score(2) }}</template>
      <template #cell(gamesReleased)="data">
        <span v-if="data.item.publisher">{{ data.item.publisher.gamesReleased }}</span>
      </template>
      <template #cell(gamesWillRelease)="data">
        <span v-if="data.item.publisher">{{ data.item.publisher.gamesWillRelease }}</span>
      </template>
    </b-table>
  </div>
</template>
<script>
import axios from 'axios';
import { ToggleButton } from 'vue-js-toggle-button';

export default {
  components: {
    ToggleButton
  },
  props: ['league', 'leagueYear'],
  data() {
    return {
      basicStandingFields: [
        { key: 'userName', label: 'User', thClass: 'bg-primary' },
        { key: 'publisher', label: 'Publisher', thClass: 'bg-primary' },
        { key: 'totalFantasyPoints', label: 'Points (Actual)', thClass: 'bg-primary', sortable: true },
        { key: 'gamesReleased', label: 'Released', thClass: 'bg-primary' },
        { key: 'gamesWillRelease', label: 'Expecting', thClass: 'bg-primary' },
        { key: 'budget', label: 'Budget', thClass: 'bg-primary' }
      ],
      projectionFields: [
        { key: 'simpleProjectedFantasyPoints', label: 'Points (Projected)', thClass: 'bg-primary', sortable: true },
        { key: 'advancedProjectedFantasyPoints', label: 'Points (Projected)', thClass: 'bg-primary', sortable: true }
      ],
      sortBy: 'totalFantasyPoints',
      sortDesc: true
    };
  },
  computed: {
    standingFields() {
      let copiedArray = this.basicStandingFields.slice(0);
      if (this.advancedProjections) {
        copiedArray.splice(2, 0, this.projectionFields[1]);
      } else {
        copiedArray.splice(2, 0, this.projectionFields[0]);
      }

      return copiedArray;
    },
    advancedProjections: {
      get() {
        return this.$store.getters.advancedProjections;
      },
      set(value) {
        if (value && this.sortBy === 'simpleProjectedFantasyPoints') {
          this.sortBy = 'advancedProjectedFantasyPoints';
        } else if (!value && this.sortBy === 'advancedProjectedFantasyPoints') {
          this.sortBy = 'simpleProjectedFantasyPoints';
        }
        this.$store.commit('setAdvancedProjections', value);
      }
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
          this.$emit('actionTaken', actionInfo);
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
