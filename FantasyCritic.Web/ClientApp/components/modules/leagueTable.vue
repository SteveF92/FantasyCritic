<template>
  <b-table :items="leagues"
           :fields="leagueFields"
           thead-class="hidden_header"
           bordered
           striped
           >
    <template slot="leagueName" slot-scope="data">
      <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: data.item.activeYear }}" class="league-link">{{data.item.leagueName}}</router-link>
      <div class="manager">Manager: {{data.item.leagueManager.displayName}}</div>
    </template>
  </b-table>
</template>
<script>
  export default {
    props: ['leagues'],
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
