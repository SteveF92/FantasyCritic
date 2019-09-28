<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Critics Royale (Beta)</h1>

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

      <h3>What is Critics Royale?</h3>
      <div class="text-well">
        <p>
          Critics Royale is a new way to play Fantasy Critic - no league required. Every quarter, you will create a publisher that will compete against the entire site. Instead of drafting, you're given a 'budget'
          which you will use to buy games that you believe will score well during that quarter. Your goal is to spend that money wisely and put together the best lineup of games that you can.
        </p>
        <p>For now, I'm calling this a beta because I will continue to expand upon this as time goes on. One of the advantages of running a new game every quarter instead of every year
        is that it will allow me to iterate and improve the game design faster than the main site.</p>
      </div>
      <h3>What's an "advertising budget"?</h3>
      <div class="text-well">
        <p>
          You can choose to assign some of your budget (the same one you use to buy games) to boost the score you get for a game.
          Every <strong>$1</strong> assigned to a game will increase it's points received by <strong>5%</strong>.
        </p>
        <p>
          For example, a game that recieves a critic score of <strong>80</strong> usually gets you <strong>10</strong> points.
          But, with an advertising budget of <strong>$5</strong>, it will be boosted by <strong>25%</strong>, giving you <strong>12.5</strong> points.
        </p>
        <p>
          You can assign up to <strong>$10</strong> in budget to any single game.
        </p>
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
