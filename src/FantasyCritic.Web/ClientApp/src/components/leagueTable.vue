<template>
  <b-table :items="leagues" :fields="leagueFields" thead-class="hidden_header" bordered striped>
    <template #cell(leagueName)="data">
      <div class="row-flex">
        <div class="league-icon-area">
          <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: data.item.activeYear } }" class="league-icon">
            <font-awesome-icon v-if="leagueIcon !== 'user'" :icon="leagueIcon" size="2x" />
            <template v-else>
              <font-awesome-icon v-if="data.item.conferenceID" icon="globe" size="2x" transform="left-1" />
              <font-awesome-icon v-if="!data.item.conferenceID && data.item.customRulesLeague" icon="book" size="2x" />
              <font-awesome-icon v-if="!data.item.conferenceID && !data.item.customRulesLeague && data.item.oneShotMode" icon="1" size="2x" transform="right-3" />
              <font-awesome-icon v-if="!data.item.conferenceID && !data.item.customRulesLeague && !data.item.oneShotMode" icon="user" size="2x" />
            </template>
          </router-link>
        </div>
        <div>
          <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: data.item.activeYear } }" class="league-link">{{ data.item.leagueName }}</router-link>
          <div v-if="data.item.conferenceID" class="league-detail">
            Conference:
            <router-link :to="{ name: 'conference', params: { conferenceid: data.item.conferenceID, year: data.item.activeYear } }" class="conference-link">{{ data.item.conferenceName }}</router-link>
          </div>
          <div v-if="data.item.leagueManager" class="league-detail">
            Manager:
            <span v-if="data.item.leagueManager.userID === userInfo.userID" class="text-bold">
              {{ data.item.leagueManager.displayName }}
            </span>
            <span v-else>{{ data.item.leagueManager.displayName }}</span>
          </div>
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
      const model = {
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

.league-icon-area {
  display: flex;
  align-items: center;
  justify-content: center;
}

.league-icon {
  width: 40px;
  font-size: 20px;
  margin-right: 18px;
  margin-left: 5px;
}

.league-link {
  font-weight: bold;
  margin-bottom: 0;
  padding-bottom: 0;
}

.league-detail {
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
