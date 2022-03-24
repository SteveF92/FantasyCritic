<template>
  <div v-if="publisher && leagueYear">
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div class="publisher-info">
        <div v-if="publisher.publisherIcon && iconIsValid" class="publisher-icon">
          {{ publisher.publisherIcon }}
        </div>
        <div class="publisher-details">
          <div class="publisher-name">
            <h1>{{ publisher.publisherName }}</h1>
          </div>

          <h4>Player Name: {{ publisher.playerName }}</h4>

          <h4>
            <router-link :to="{ name: 'league', params: { leagueid: publisher.leagueID, year: publisher.year } }">League: {{ publisher.leagueName }}</router-link>
          </h4>
          <ul>
            <li>Budget: {{ publisher.budget | money }}</li>
            <li>Will Release Games Dropped: {{ getDropStatus(publisher.willReleaseGamesDropped, publisher.willReleaseDroppableGames) }}</li>
            <li>Will Not Release Games Dropped: {{ getDropStatus(publisher.willNotReleaseGamesDropped, publisher.willNotReleaseDroppableGames) }}</li>
            <li>"Any Unreleased" Games Dropped: {{ getDropStatus(publisher.freeGamesDropped, publisher.freeDroppableGames) }}</li>
          </ul>
        </div>
      </div>

      <div v-if="!publisher.publicLeague && !(publisher.userIsInLeague || publisher.outstandingInvite)" class="alert alert-warning">You are viewing a private league.</div>

      <div v-show="moveGameError" class="alert alert-danger">
        {{ moveGameError }}
      </div>

      <div v-if="hasIneligibleGame" class="alert alert-warning">
        {{ referToPlayer('You have', 'This player has') }} games in ineligible slots. There are a few reasons this could happen.
        <ul>
          <li>A game's tags changed after a correction was made.</li>
          <li>{{ referToPlayer('You', 'They') }} picked up a game and there were no eligible slots it could fit in.</li>
          <li v-if="leagueYear.hasSpecialSlots">{{ referToPlayer('You', 'They') }} intentionally moved a game into a slot it's not eligble for. This used to be possible but no longer is.</li>
        </ul>

        The options going forward are:
        <ul>
          <li v-if="leagueYear.hasSpecialSlots">{{ referToPlayer('You', 'They') }} can reorganize {{ referToPlayer('your', 'their') }} games so everything is eligible.</li>
          <li>The league manager can override a game's tags if the league disagrees with the tags the site decided.</li>
          <li>{{ referToPlayer('You', 'They') }} can drop the game (depending on the league settings).</li>
          <li>The league could decide to give {{ referToPlayer('you', 'they') }} a "free drop" regardless of the league settings if they decide that this game should not be eligible.</li>
          <li>{{ referToPlayer('You', 'They') }} could trade the game to another player.</li>
        </ul>

        <template v-if="leagueYear.hasSpecialSlots">
          Generally speaking, players should always be putting their games into slots they are eligible for. Intentionally keeping games in ineligible slots in order to free up 'more valuable' slots
          is outside of the spirit of Fantasy Critic.
        </template>
      </div>

      <playerGameTable></playerGameTable>
    </div>
  </div>
</template>

<script>
import PlayerGameTable from '@/components/gameTables/playerGameTable';
import GlobalFunctions from '@/globalFunctions';
import PublisherMixin from '@/mixins/publisherMixin';

export default {
  components: {
    PlayerGameTable
  },
  mixins: [PublisherMixin],
  props: {
    publisherid: String
  },
  data() {
    return {
      errorInfo: ''
    };
  },
  computed: {
    iconIsValid() {
      return GlobalFunctions.publisherIconIsValid(this.publisher.publisherIcon);
    },
    moveGameError() {
      return this.$store.getters.moveGameError;
    },
    hasIneligibleGame() {
      return _.some(this.publisher.gameSlots, (x) => !x.gameMeetsSlotCriteria);
    }
  },
  watch: {
    async $route(to, from) {
      if (to.path !== from.path) {
        await this.initializePage();
      }
    }
  },
  mounted() {
    this.initializePage();
  },
  methods: {
    async initializePage() {
      await this.$store.dispatch('initializePublisherPage', this.publisherid);
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
    referToPlayer(publisherText, notPublisherText) {
      if (this.userIsPublisher) {
        return publisherText;
      }

      return notPublisherText;
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
