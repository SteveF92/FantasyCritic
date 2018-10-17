<template>
  <b-modal id="currentBidsForm" ref="currentBidsFormRef" title="Current Bids" hide-footer>
    <table class="table table-sm table-responsive-sm table-bordered">
      <thead>
        <tr class="table-secondary">
          <th scope="col" class="game-column">Game</th>
          <th scope="col" class="score-column">Bid Amount</th>
          <th scope="col" class="score-column">Priority</th>
          <th scope="col" class="score-column">Cancel</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="bid in currentBids" class="minimal-game-row">
          <td>{{bid.masterGame.gameName}}</td>
          <td>{{bid.bidAmount | money}}</td>
          <td>{{bid.priority}}</td>
          <td>
            <b-button variant="danger" size="sm">Cancel</b-button>
          </td>
        </tr>
      </tbody>
    </table>
  </b-modal>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    export default {
        props: ['currentBids'],
        methods: {

        }
    }
</script>
<style scoped>

</style>
