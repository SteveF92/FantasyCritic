<template>
  <b-table :items="leagues"
           :fields="leagueFields"
           thead-class="hidden_header"
           bordered
           striped>
    <template v-slot:cell(leagueName)="data">
      <div class="row-flex">
        <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: data.item.activeYear }}">
          <font-awesome-icon class="league-icon" :icon="leagueIcon" v-show="leagueIcon !== 'user'" />
          <font-awesome-icon class="league-icon" icon="user-cog" v-show="leagueIcon === 'user' && data.item.leagueManager.userID === userID" />
          <font-awesome-icon class="league-icon" icon="user" v-show="leagueIcon === 'user' && data.item.leagueManager.userID !== userID" />
        </router-link>
        <div>
          <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: data.item.activeYear }}" class="league-link">{{data.item.leagueName}}</router-link>
          <div class="manager" v-if="data.item.leagueManager">Manager: {{data.item.leagueManager.displayName}}</div>
        </div>
      </div>
    </template>
  </b-table>
</template>
<script>
  export default {
    props: ['leagues', 'leagueIcon', 'userID'],
    data() {
      return {
        leagueFields: [
          { key: 'leagueName' },
        ]
      }
    }
  }
</script>
<style scoped>
table >>> .hidden_header {
  display: none;
}

.row-flex {
  display: flex;
}

.league-icon{
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
</style>
