<template>
  <b-table :items="leagues"
           :fields="leagueFields"
           thead-class="hidden_header"
           bordered
           striped>
    <template slot="leagueName" slot-scope="data">
      <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: data.item.activeYear }}">{{data.item.leagueName}}</router-link>
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
</style>
