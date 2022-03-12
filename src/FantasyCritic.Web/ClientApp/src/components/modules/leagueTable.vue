<template>
  <b-table :items="leagues" :fields="leagueFields" thead-class="hidden_header" bordered striped>
    <template #cell(leagueName)="data">
      <div class="row-flex">
        <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: data.item.activeYear } }">
          <font-awesome-icon class="league-icon" :icon="leagueIcon" v-show="leagueIcon !== 'user'" />
          <font-awesome-icon class="league-icon" icon="user-cog" v-show="leagueIcon === 'user' && data.item.leagueManager.userID === userID" />
          <font-awesome-icon class="league-icon" icon="user" v-show="leagueIcon === 'user' && data.item.leagueManager.userID !== userID" />
        </router-link>
        <div>
          <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: data.item.activeYear } }" class="league-link">{{ data.item.leagueName }}</router-link>
          <div class="manager" v-if="data.item.leagueManager">Manager: {{ data.item.leagueManager.displayName }}</div>
        </div>
        <div v-show="showArchive" class="archive-button-section">
          <font-awesome-icon class="archive-button fake-link" icon="archive" v-on:click="setArchive(data.item, true)" v-b-popover.hover.rightbottom="'Archive this league (only affects you)'" />
        </div>
        <div v-show="showUnArchive" class="archive-button-section">
          <font-awesome-icon class="archive-button fake-link" icon="thumbtack" v-on:click="setArchive(data.item, false)" v-b-popover.hover.rightbottom="'Un-Archive this league (only affects you)'" />
        </div>
      </div>
    </template>
  </b-table>
</template>
<script>
import axios from 'axios';

export default {
  props: ['leagues', 'leagueIcon', 'userID', 'showArchive', 'showUnArchive'],
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
        .then((response) => {})
        .catch((response) => {});
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
  width: 45px;
  height: 45px;
  margin-right: 10px;
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
