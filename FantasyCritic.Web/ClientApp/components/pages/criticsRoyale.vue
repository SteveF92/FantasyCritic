<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Critics Royale</h1>

      <div v-if="!isBusy">
        <div v-if="!userRoyalePublisher" class="alert alert-info">
          Create your publisher to start playing!
          <b-button variant="primary" v-b-modal="'createRoyalePublisher'">Create Publisher</b-button>
          <createRoyalePublisherForm :royaleYearQuarter="royaleYearQuarter"></createRoyalePublisherForm>
        </div>

        <div class="leaderboard-header">
          <h2>Leaderboards {{year}}-Q{{quarter}}</h2>
          <b-button v-if="userRoyalePublisher" variant="info" :to="{ name: 'royalePublisher', params: { publisherid: userRoyalePublisher.publisherID }}">View My Publisher</b-button>
        </div>
        
        <b-table striped bordered small :items="royaleStandings" :fields="standingsFields">
          <template slot="publisherName" slot-scope="data">
            <router-link :to="{ name: 'royalePublisher', params: { publisherid: data.item.publisherID }}">
              {{ data.item.publisherName }}
            </router-link>
          </template>
        </b-table>
      </div>
    </div>
  </div>
</template>

<script>
  import Vue from 'vue';
  import axios from "axios";

  import CreateRoyalePublisherForm from "components/modules/modals/createRoyalePublisherForm";

  export default {
    props: ['year', 'quarter'],
    components: {
      CreateRoyalePublisherForm
    },
    data() {
      return {
        userRoyalePublisher: null,
        royaleYearQuarter: null,
        royaleStandings: null,
        isBusy: true,
        standingsFields: [
          { key: 'publisherName', label: 'Publisher', thClass:'bg-primary' },
          { key: 'playerName', label: 'Player Name', thClass: 'bg-primary' },
          { key: 'totalFantasyPoints', label: 'Total Points', thClass: 'bg-primary' }
        ],
      }
    },
    methods: {
      async fetchRoyaleYearQuarter() {
        axios
          .get('/api/royale/RoyaleQuarter/' + this.year + '/' + this.quarter)
          .then(response => {
            this.royaleYearQuarter = response.data;
          })
          .catch(response => {

          });
      },
      async fetchRoyaleStandings() {
        axios
          .get('/api/royale/RoyaleStandings/' + this.year + '/' + this.quarter)
          .then(response => {
            this.royaleStandings = response.data;
          })
          .catch(response => {

          });
      },
      async fetchUserRoyalePublisher() {
        axios
          .get('/api/royale/GetUserRoyalePublisher/' + this.year + '/' + this.quarter)
          .then(response => {
            this.userRoyalePublisher = response.data;
            this.isBusy = false;
          })
          .catch(response => {
            this.isBusy = false;
          });
      },
    },
    async mounted() {
      await Promise.all([this.fetchRoyaleYearQuarter(), this.fetchUserRoyalePublisher(), this.fetchRoyaleStandings()]);
    },
  }
</script>
<style scoped>
  .leaderboard-header{
    display: flex;
    justify-content: space-between;
  }
</style>
