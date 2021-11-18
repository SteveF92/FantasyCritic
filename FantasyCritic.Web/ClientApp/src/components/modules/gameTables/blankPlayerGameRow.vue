<template>
  <tr v-bind:class="{ 'table-danger': counterPick }">
    <td></td>
    <td></td>
    <td></td>
    <td></td>
    <td></td>
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
</style>
