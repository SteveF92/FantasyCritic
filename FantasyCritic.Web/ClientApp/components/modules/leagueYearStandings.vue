<template>
  <div>
    <h3>Standings</h3>
    <table class="table table-bordered table-striped table-sm">
      <thead>
        <tr class="table-primary">
          <th>Publisher</th>
          <th>Score</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="standing in standings">
          <td>
            <router-link class="text-primary" :to="{ name: 'publisher', params: { publisherid: standing.publisherID }}">{{ standing.publisherName }}</router-link>
          </td>
          <td>{{standing.totalFantasyScore | score(2)}}</td>
        </tr>
      </tbody>
    </table>
</div>
</template>
<script>
  import Vue from "vue";
  export default {
    props: ['standings']
  }
</script>
