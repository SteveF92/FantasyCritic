<template>
  <b-table :items="leagues" :fields="leagueFields" thead-class="hidden_header" bordered striped>
    <template #cell(leagueName)="data">
      <div class="row-flex">
        <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: data.item.activeYear } }" class="league-icon">
          <font-awesome-icon v-if="leagueIcon !== 'user'" :icon="leagueIcon" size="2x" />
          <template v-else>
            <template v-if="data.item.customRulesLeague">
              <font-awesome-layers v-if="data.item.isManager">
                <font-awesome-icon icon="cog" transform="right-26 down-15" />
                <font-awesome-icon icon="book" size="2x" transform="left-1 down-2" />
              </font-awesome-layers>
              <font-awesome-icon v-else icon="book" size="2x" transform="right-4" />
            </template>
            <template v-else>
              <template v-if="data.item.oneShotMode">
                <font-awesome-layers v-if="data.item.isManager">
                  <font-awesome-icon icon="cog" transform="right-16 down-5" />
                  <font-awesome-icon icon="1" size="2x" transform="right-2 down-2" />
                </font-awesome-layers>
                <font-awesome-icon v-else icon="1" size="2x" transform="right-4" />
              </template>
              <template v-else>
                <template v-if="!data.item.oneShotMode">
                  <font-awesome-icon v-if="data.item.isManager" icon="user-cog" size="2x" />
                  <font-awesome-icon v-else icon="user" size="2x" />
                </template>
              </template>
            </template>
          </template>
        </router-link>
        <div>
          <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: data.item.activeYear } }" class="league-link">{{ data.item.leagueName }}</router-link>
          <div v-if="data.item.leagueManager" class="manager">Manager: {{ data.item.leagueManager.displayName }}</div>
        </div>
        <div v-show="showArchive" class="archive-button-section">
          <font-awesome-icon v-b-popover.hover.focus.rightbottom="'Archive this league (only affects you)'" class="archive-button fake-link" icon="archive" @click="setArchive(data.item, true)" />
        </div>
        <div v-show="showUnArchive" class="archive-button-section">
          <font-awesome-icon
            v-b-popover.hover.focus.rightbottom="'Un-Archive this league (only affects you)'"
            class="archive-button fake-link"
            icon="thumbtack"
            @click="setArchive(data.item, false)" />
        </div>
      </div>
    </template>
  </b-table>
</template>
<script>
import axios from 'axios';

export default {
  props: {
    leagues: { type: Array, required: true },
    leagueIcon: { type: String, required: true },
    showArchive: { type: Boolean },
    showUnArchive: { type: Boolean }
  },
  data() {
    return {
      leagueFields: [{ key: 'leagueName' }]
    };
  },
  methods: {
    setArchive(league, status) {
      league.archived = status;
      var model = {
        leagueID: league.leagueID,
        archive: status
      };
      axios
        .post('/api/league/SetArchiveStatus', model)
        .then(() => {})
        .catch(() => {});
    }
  }
};
</script>
<style scoped>
table >>> .hidden_header {
  display: none;
}

.row-flex {
  display: flex;
}

.league-icon {
  margin-top: 5px;
  margin-right: 17px;
  width: 40px;
  font-size: 20px;
}

.league-link {
  font-weight: bold;
  margin-bottom: 0;
  padding-bottom: 0;
}

.manager {
  margin-top: 0;
  padding-top: 0;
  font-size: 14px;
}

.archive-button-section {
  margin-left: auto;
  align-self: flex-end;
}

.archive-button {
  width: 35px;
  height: 35px;
  margin-bottom: 5px;
}

.archive-button:hover {
  color: grey;
}
</style>
