<template>
  <b-modal id="leagueActionsModal" ref="leagueActionsModalRef" title="League History" size="lg" hide-footer>
    <table class="table table-sm table-responsive-sm table-bordered table-striped">
      <thead>
        <tr class="bg-primary">
          <th scope="col">Publisher</th>
          <th scope="col">Time</th>
          <th scope="col">Action Type</th>
          <th scope="col">Description</th>
          <th scope="col">Manager Action?</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="leagueAction in leagueActions">
          <td>{{leagueAction.publisher.publisherName}}</td>
          <td>{{leagueAction.timestamp | date}}</td>
          <td>{{leagueAction.actionType}}</td>
          <td>{{leagueAction.description}}</td>
          <td>{{leagueAction.managerAction}}</td>
        </tr>
      </tbody>
    </table>
  </b-modal>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    export default {
        props: ['leagueActions']
    }
</script>
<style scoped>
</style>
