<template>
  <div v-if="publisher && leagueYear">
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div class="publisher-info">
        <div v-if="publisher.publisherIcon && iconIsValid" class="publisher-icon">
          {{ publisher.publisherIcon }}
        </div>
        <div class="publisher-details">
          <div class="publisher-name">
            <h1>{{publisher.publisherName}}</h1>
          </div>

          <h4>Player Name: {{publisher.playerName}}</h4>

          <h4>
            <router-link :to="{ name: 'league', params: { leagueid: publisher.leagueID, year: publisher.year }}">League: {{publisher.leagueName}}</router-link>
          </h4>
          <ul>
            <li>Budget: {{publisher.budget | money}}</li>
            <li>Will Release Games Dropped: {{getDropStatus(publisher.willReleaseGamesDropped, publisher.willReleaseDroppableGames)}}</li>
            <li>Will Not Release Games Dropped: {{getDropStatus(publisher.willNotReleaseGamesDropped, publisher.willNotReleaseDroppableGames)}}</li>
            <li>"Any Unreleased" Games Dropped: {{getDropStatus(publisher.freeGamesDropped, publisher.freeDroppableGames)}}</li>
          </ul>
        </div>
      </div>

      <div v-if="!publisher.publicLeague && !(publisher.userIsInLeague || publisher.outstandingInvite)" class="alert alert-warning" role="info">
        You are viewing a private league.
      </div>


      <div v-if="leagueYear && publisher">
        <playerGameTable :publisher="publisher" :leagueYear="leagueYear" v-on:gamesMoved="fetchPublisher"></playerGameTable>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import PlayerGameTable from '@/components/modules/gameTables/playerGameTable';
import GlobalFunctions from '@/globalFunctions';

export default {
  data() {
    return {
      errorInfo: '',
      publisher: null,
      leagueYear: null
    };
  },
  components: {
    PlayerGameTable
  },
  props: ['publisherid'],
  computed: {
    moveMode() {
      return this.$store.getters.moveMode;
    },
    userIsPublisher() {
      return this.$store.getters.userInfo && this.publisher.userID === this.$store.getters.userInfo.userID;
    },
    iconIsValid() {
      return GlobalFunctions.publisherIconIsValid(this.publisher.publisherIcon);
    }
  },
  methods: {
    fetchPublisher() {
      axios
        .get('/api/League/GetPublisher/' + this.publisherid)
        .then(response => {
          this.publisher = response.data;
          this.fetchLeagueYear();
          this.$store.dispatch('initialize', this.publisher);
        })
        .catch(returnedError => (this.error = returnedError));
    },
    fetchLeagueYear() {
      axios
        .get('/api/League/GetLeagueYear?leagueID=' + this.publisher.leagueID + '&year=' + this.publisher.year)
        .then(response => {
          this.leagueYear = response.data;
        })
        .catch(returnedError => (this.error = returnedError));
    },
    getDropStatus(dropped, droppable) {
      if (!droppable) {
        return 'N/A';
      }
      if (droppable === -1) {
        return dropped + '/' + '\u221E';
      }
      return dropped + '/' + droppable;
    }
  },
  mounted() {
    this.fetchPublisher();
  },
  watch: {
    '$route'(to, from) {
      this.fetchPublisher();
    }
  }
};
</script>
<style scoped>
  .publisher-info {
    margin-top: 10px;
    display: flex;
  }

  .publisher-name {
    display: block;
    max-width: 100%;
    word-wrap: break-word;
  }

  .publisher-icon {
    font-size: 100px;
  }
</style>
