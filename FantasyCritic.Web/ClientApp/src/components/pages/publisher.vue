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

      <div v-if="!publisher.publicLeague && !(publisher.userIsInLeague || publisher.outstandingInvite)" class="alert alert-warning">
        You are viewing a private league.
      </div>

      <div v-show="moveGameError" class="alert alert-danger">
        {{moveGameError}}
      </div>

      <div v-if="hasIneligibleGame" class="alert alert-warning">
        {{referToPlayer(true)}} have games in ineligible slots. There are a few reasons this could happen.
        <ul>
          <li>A game's tags changed after a correction was made.</li>
          <li>{{referToPlayer(true)}} picked up a game and there were no eligible slots it could fit in.</li>
          <li v-if="leagueYear.hasSpecialSlots">{{referToPlayer(true)}} intentionally moved a game into a slot it's not eligble for. This used to be possible but no longer is.</li>
        </ul>

        The options going forward are:
        <ul>
          <li v-if="leagueYear.hasSpecialSlots">{{referToPlayer(true)}} can reorganize your games so everything is eligible.</li>
          <li>The league manager can override a game's tags if your league disagrees with the tags the site decided.</li>
          <li>{{referToPlayer(true)}} can drop the game (depending on your league settings).</li>
          <li>The league could decide to give {{referToPlayer()}} a "free drop" regardless of the league settings if they decide that this game should not be eligible.</li>
          <li>{{referToPlayer(true)}} could trade the game to another player.</li>
        </ul>

        <template v-if="leagueYear.hasSpecialSlots">
          One special note: if {{referToPlayer(false, true)}} intentionally holding a game that is already released in a slot it isn't eligible for, because the game has underperformed
          and {{referToPlayer(false)}} want to keep the slot the game is actually eligible for open, then {{referToPlayerWithVerb(false)}} playing outside of the spirit of the game.
          {{referToPlayer(true)}} should put {{referToPlayerPossessive(false)}} games in the correct slots.
        </template>
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
      return false;
      return this.$store.getters.userInfo && this.publisher.userID === this.$store.getters.userInfo.userID;
    },
    iconIsValid() {
      return GlobalFunctions.publisherIconIsValid(this.publisher.publisherIcon);
    },
    moveGameError() {
      return this.$store.getters.moveGameError;
    },
    hasIneligibleGame() {
      return _.some(this.publisher.gameSlots, x => !x.gameMeetsSlotCriteria);
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
    },
    referToPlayer(caps) {
      let name = 'this player';
      if (this.userIsPublisher) {
        name = 'you';
      }

      if (caps) {
        name = name.charAt(0).toUpperCase() + name.slice(1);
      }

      return name;
    },
    referToPlayerWithVerb(caps) {
      let name = 'they';
      if (this.userIsPublisher) {
        name = 'you';
      }

      if (caps) {
        name = name.charAt(0).toUpperCase() + name.slice(1);
      }

      return name + ' are';
    },
    referToPlayerPossessive(caps) {
      let name = 'their';
      if (this.userIsPublisher) {
        name = 'your';
      }

      if (caps) {
        name = name.charAt(0).toUpperCase() + name.slice(1);
      }
      return name;
    },
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
