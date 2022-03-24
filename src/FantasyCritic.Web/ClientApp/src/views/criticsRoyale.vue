<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div v-show="royaleYearQuarterOptions">
        <b-dropdown text="Quarters" class="quarter-select">
          <b-dropdown-item
            v-for="royaleYearQuarterOption in royaleYearQuarterOptions"
            v-bind:key="royaleYearQuarterOption.year + '-' + royaleYearQuarterOption.quarter"
            :active="royaleYearQuarterOption.year === year && royaleYearQuarterOption.quarter === quarter"
            :to="{ name: 'criticsRoyale', params: { year: royaleYearQuarterOption.year, quarter: royaleYearQuarterOption.quarter } }">
            {{ royaleYearQuarterOption.year }}-Q{{ royaleYearQuarterOption.quarter }}
          </b-dropdown-item>
        </b-dropdown>
      </div>
      <div class="critic-royale-header-area">
        <img class="critic-royale-header" src="@/assets/critic-royale-logo.svg" />
      </div>

      <div class="critic-royale-header-area-simple">
        <h1>Critics Royale</h1>
      </div>

      <div v-if="!userPublisherBusy">
        <div v-if="!userRoyalePublisher && isAuth" class="alert alert-info">
          Create your publisher to start playing!
          <b-button class="login-button" variant="primary" v-b-modal="'createRoyalePublisher'">Create Publisher</b-button>
          <createRoyalePublisherForm :royaleYearQuarter="royaleYearQuarter"></createRoyalePublisherForm>
        </div>
        <div v-if="!userRoyalePublisher && !isAuth" class="alert alert-success">
          Sign up or log in to start playing now!
          <b-button class="login-button" variant="info" :to="{ name: 'login' }">
            <span>Log In</span>
            <font-awesome-icon class="full-nav" icon="sign-in-alt" />
          </b-button>
          <b-button variant="primary" :to="{ name: 'register' }">
            <span>Sign Up</span>
            <font-awesome-icon class="full-nav" icon="user-plus" />
          </b-button>
        </div>

        <div class="leaderboard-header">
          <h2>Leaderboards {{ year }}-Q{{ quarter }}</h2>
          <b-button v-if="userRoyalePublisher" variant="info" :to="{ name: 'royalePublisher', params: { publisherid: userRoyalePublisher.publisherID } }">View My Publisher</b-button>
        </div>

        <div v-if="royaleStandings">
          <b-table striped bordered responsive small :items="royaleStandings" :fields="standingsFields" :per-page="perPage" :current-page="currentPage">
            <template #cell(ranking)="data">
              <template v-if="data.item.ranking">
                {{ data.item.ranking }}
              </template>
              <template v-else>--</template>
            </template>
            <template #cell(publisherName)="data">
              <router-link :to="{ name: 'royalePublisher', params: { publisherid: data.item.publisherID } }">
                {{ data.item.publisherName }}
              </router-link>
            </template>
            <template #cell(playerName)="data">
              {{ data.item.playerName }}
              <font-awesome-icon v-if="data.item.previousQuarterWinner" icon="crown" class="previous-quarter-winner" v-b-popover.hover="'Reigning Champion'" />
              <font-awesome-icon v-if="data.item.oneTimeWinner && !data.item.previousQuarterWinner" icon="crown" class="onetime-winner" v-b-popover.hover="'Previous Champion'" />
            </template>
          </b-table>
          <b-pagination class="pagination-dark" v-model="currentPage" :total-rows="rows" :per-page="perPage" aria-controls="my-table"></b-pagination>
        </div>
        <div v-else class="spinner">
          <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
        </div>
      </div>

      <h3>What is Critics Royale?</h3>
      <div class="text-well">
        <p>
          Critics Royale is a new way to play Fantasy Critic - no league required. Every quarter, you will create a publisher that will compete against the entire site. Instead of drafting, you're
          given a 'budget' which you will use to buy games that you believe will score well during that quarter. Your goal is to spend that money wisely and put together the best lineup of games that
          you can.
        </p>
        <p>
          For now, I'm calling this a beta because I will continue to expand upon this as time goes on. One of the advantages of running a new game every quarter instead of every year is that it will
          allow me to iterate and improve the game design faster than the main site.
        </p>
      </div>
      <h3>How's it work?</h3>
      <div class="text-well">
        <p>
          You'll be given a $100 "budget" with which to "buy" games to add them to your roster. Each game's price is set based upon how popular it is on the site. When the game releases, you get
          points using the same system as the regular Fantasy Critic leagues. You can also boost your points by setting an "advertising budget" for a game before it comes out. More on that below.
        </p>
        <p>If you lose confidence in a game, you can choose to "sell" it, and get back half the money you spent on it. You can't sell a game that has come out, or one that has reviews already.</p>
      </div>
      <h3>What's an "advertising budget"?</h3>
      <div class="text-well">
        <p>
          You can choose to assign some of your budget (the same one you use to buy games) to boost the score you get for a game. Every
          <strong>$1</strong>
          assigned to a game will increase it's points received by
          <strong>5%</strong>
          .
        </p>
        <p>
          For example, a game that recieves a critic score of
          <strong>80</strong>
          usually gets you
          <strong>10</strong>
          points. But, with an advertising budget of
          <strong>$5</strong>
          , it will be boosted by
          <strong>25%</strong>
          , giving you
          <strong>12.5</strong>
          points. You don't need to spend even dollars to get a bonus, every cent counts.
        </p>
        <p>
          You can assign up to
          <strong>$10</strong>
          in budget to any single game. If a game has already been released or already has review scores, you can't change it's advertising budget anymore.
        </p>
      </div>
      <h3>What games are elgible?</h3>
      <div class="text-well">
        <p>
          The eligibility rules are the same as the default rules in the standard leagues. That means anything with an eligibility level of "New Game", "Complete Remake", and "Remake". Anything higher
          than that ("Partial Remake" and up) is not eligible in Critics Royale. On top of that, yearly installments, games currently in early access, free to play games, games already released in
          other regions (other than the US), expansion packs/DLC are not allowed.
        </p>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import BasicMixin from '@/mixins/basicMixin';
import CreateRoyalePublisherForm from '@/components/modals/createRoyalePublisherForm';

export default {
  components: {
    CreateRoyalePublisherForm
  },
  mixins: [BasicMixin],
  props: ['year', 'quarter'],
  data() {
    return {
      perPage: 10,
      currentPage: 1,
      userRoyalePublisher: null,
      royaleYearQuarter: null,
      royaleYearQuarterOptions: null,
      royaleStandings: null,
      userPublisherBusy: true,
      standingsFields: [
        { key: 'ranking', label: 'Rank', thClass: ['bg-primary', 'ranking-column'], tdClass: 'ranking-column' },
        { key: 'publisherName', label: 'Publisher', thClass: 'bg-primary' },
        { key: 'playerName', label: 'Player Name', thClass: 'bg-primary' },
        { key: 'totalFantasyPoints', label: 'Total Points', thClass: 'bg-primary' }
      ]
    };
  },
  computed: {
    rows() {
      return this.royaleStandings.length;
    }
  },
  async mounted() {
    await this.initializePage();
  },
  watch: {
    async $route() {
      await this.initializePage();
    }
  },
  methods: {
    async initializePage() {
      await this.fetchRoyaleQuarters();
      if (!this.year || !this.quarter) {
        const mostRecentQuarter = this.royaleYearQuarterOptions[this.royaleYearQuarterOptions.length - 1];
        var parameters = {
          year: mostRecentQuarter.year.toString(),
          quarter: mostRecentQuarter.quarter.toString()
        };
        this.$router.replace({ params: parameters });
        return;
      }

      await Promise.all([this.fetchRoyaleYearQuarter(), this.fetchUserRoyalePublisher(), this.fetchRoyaleStandings()]);
    },
    fetchRoyaleQuarters() {
      return axios
        .get('/api/royale/RoyaleQuarters')
        .then((response) => {
          this.royaleYearQuarterOptions = response.data;
        })
        .catch(() => {});
    },
    fetchRoyaleYearQuarter() {
      this.royaleYearQuarter = null;
      return axios
        .get('/api/royale/RoyaleQuarter/' + this.year + '/' + this.quarter)
        .then((response) => {
          this.royaleYearQuarter = response.data;
        })
        .catch(() => {});
    },
    fetchRoyaleStandings() {
      this.royaleStandings = null;
      return axios
        .get('/api/royale/RoyaleStandings/' + this.year + '/' + this.quarter)
        .then((response) => {
          this.royaleStandings = response.data;
        })
        .catch(() => {});
    },
    fetchUserRoyalePublisher() {
      this.userPublisherBusy = true;
      this.userRoyalePublisher = null;
      return axios
        .get('/api/royale/GetUserRoyalePublisher/' + this.year + '/' + this.quarter)
        .then((response) => {
          this.userRoyalePublisher = response.data;
          this.userPublisherBusy = false;
        })
        .catch(() => {
          this.userPublisherBusy = false;
        });
    }
  }
};
</script>
<style scoped>
.leaderboard-header {
  display: flex;
  justify-content: space-between;
  flex-wrap: wrap;
  margin-bottom: 5px;
}

.spinner {
  display: flex;
  justify-content: space-around;
}

.critic-royale-header-area {
  margin-top: 10px;
  margin-bottom: 10px;
  display: flex;
  justify-content: center;
  background-color: #4a4a4a;
  margin-right: 25%;
  margin-left: 25%;
  border-radius: 5px;
}

.critic-royale-header {
  height: 300px;
}

@media only screen and (max-width: 1000px) {
  .critic-royale-header-area {
    display: none;
  }
}

@media only screen and (min-width: 1001px) {
  .critic-royale-header-area-simple {
    display: none;
  }
}

.login-button {
  margin-left: 15px;
}

.quarter-select {
  float: right;
}

.previous-quarter-winner {
  margin-left: 4px;
  color: #d6993a;
}

.onetime-winner {
  margin-left: 4px;
  color: white;
}
</style>
<style>
.ranking-column {
  width: 50px;
  text-align: right;
}
</style>
