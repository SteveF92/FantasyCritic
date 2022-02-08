<template>
  <div>
    <h3 v-if="mode === 'dryRunPage'">
      <router-link :to="{ name: 'league', params: { leagueid: leagueActionSet.leagueID, year: leagueActionSet.year }}">{{leagueActionSet.leagueName}} - {{leagueActionSet.year}}</router-link>
    </h3>
    <h3 v-if="mode === 'leagueHistory'">
      {{leagueActionSet.processTime}}
    </h3>
  </div>
</template>
<script>
export default {
  props: ['leagueActionSet', 'mode']
};
</script>
