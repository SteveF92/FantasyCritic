<template>
  <tr v-bind:class="{ 'counter-pick-row': counterPick }">
    <td class="game-column"></td>
    <td class="score-column"></td>
    <td class="score-column"></td>
  </tr>
</template>
<script>
  export default {
    props: ['counterPick']
  };
</script>

<style scoped>
  tr {
    height: 40px;
  }

  .counter-pick-row td {
    background-color: #AA1E1E !important;
  }
</style>
