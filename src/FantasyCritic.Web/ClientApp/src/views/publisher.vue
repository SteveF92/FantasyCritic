<template>
  <div v-if="publisher && leagueYear" class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="isPlusUser" class="cover-art-mode-options">
      <b-button v-if="userIsPublisher" v-show="coverArtMode" variant="secondary" size="sm" @click="prepareSnapshot">
        <font-awesome-icon icon="share-alt" size="lg" class="share-button" />
        <span>Share</span>
      </b-button>
      <label>Cover Art Mode</label>
      <toggle-button v-model="editableCoverArtMode" class="toggle" :sync="true" :labels="{ checked: 'On', unchecked: 'Off' }" :css-colors="true" :font-size="13" :width="60" :height="28" />
    </div>
    <div v-show="!coverArtMode">
      <div class="publisher-header">
        <div class="publisher-details">
          <div class="publisher-name-and-icon">
            <div v-if="publisher.publisherIcon && iconIsValid" class="publisher-icon">
              {{ publisher.publisherIcon }}
            </div>
            <div class="publisher-name-and-slogan">
              <div class="publisher-name">
                <h1>{{ publisher.publisherName }}</h1>
              </div>
              <h2 v-if="publisher.publisherSlogan" class="publisher-slogan">"{{ publisher.publisherSlogan }}"</h2>
            </div>
          </div>
          <h4>Player Name: {{ publisher.playerName }}</h4>
          <h4>
            <router-link :to="{ name: 'league', params: { leagueid: publisher.leagueID, year: publisher.year } }">League: {{ publisher.leagueName }}</router-link>
          </h4>
          <ul>
            <li>Budget: {{ publisher.budget | money(0) }}</li>
            <li>Will Release Games Dropped: {{ getDropStatus(publisher.willReleaseGamesDropped, publisher.willReleaseDroppableGames) }}</li>
            <li>Will Not Release Games Dropped: {{ getDropStatus(publisher.willNotReleaseGamesDropped, publisher.willNotReleaseDroppableGames) }}</li>
            <li>"Any Unreleased" Games Dropped: {{ getDropStatus(publisher.freeGamesDropped, publisher.freeDroppableGames) }}</li>
            <li>Super Drops Available: {{ publisher.superDropsAvailable }}</li>
          </ul>
        </div>
      </div>

      <div v-if="!leagueYear.league.publicLeague && !(publisher.userIsInLeague || publisher.outstandingInvite)" class="alert alert-warning">You are viewing a private league.</div>

      <div v-show="moveGameError" class="alert alert-danger">
        {{ moveGameError }}
      </div>

      <div v-if="hasIneligibleGame && !allowMoveIntoIneligible" class="alert alert-warning">
        {{ referToPlayer('You have', 'This player has') }} games in ineligible slots. There are a few reasons this could happen.
        <ul>
          <li>A game's tags changed after a correction was made.</li>
          <li>{{ referToPlayer('You', 'They') }} picked up a game and there were no eligible slots it could fit in.</li>
          <li v-if="leagueYear.settings.hasSpecialSlots">{{ referToPlayer('You', 'They') }} intentionally moved a game into a slot it's not eligble for. This used to be possible but no longer is.</li>
        </ul>

        The options going forward are:
        <ul>
          <li v-if="leagueYear.settings.hasSpecialSlots">{{ referToPlayer('You', 'They') }} can reorganize {{ referToPlayer('your', 'their') }} games so everything is eligible.</li>
          <li>The league manager can override a game's tags if the league disagrees with the tags the site decided.</li>
          <li>{{ referToPlayer('You', 'They') }} can drop the game (depending on the league settings).</li>
          <li>The league could decide to give {{ referToPlayer('you', 'them') }} a "free drop" regardless of the league settings if they decide that this game should not be eligible.</li>
          <li>{{ referToPlayer('You', 'They') }} could trade the game to another player.</li>
        </ul>

        <template v-if="leagueYear.settings.hasSpecialSlots">
          Generally speaking, players should always be putting their games into slots they are eligible for. Intentionally keeping games in ineligible slots in order to free up 'more valuable' slots
          is outside of the spirit of Fantasy Critic.
        </template>
      </div>

      <div v-show="!coverArtMode" class="table-options">
        <b-button v-if="!sortOrderMode && leagueYear.settings.hasSpecialSlots && userIsPublisher && !moveMode" variant="info" @click="enterMoveMode">Move Games</b-button>
        <b-button v-if="!sortOrderMode && leagueYear.settings.hasSpecialSlots && moveMode" variant="secondary" @click="cancelMoveMode">Cancel Movement</b-button>
        <b-button v-if="!sortOrderMode && leagueYear.settings.hasSpecialSlots && moveMode" variant="success" @click="confirmPositions">Confirm Positions</b-button>
        <template v-if="!moveMode && isPlusUser">
          <b-form-checkbox v-show="sortOrderMode && hasFormerGames" v-model="editableIncludeRemovedInSorted">
            <span class="checkbox-label">Include Dropped Games</span>
          </b-form-checkbox>
          <toggle-button
            v-model="editableSortOrderMode"
            class="toggle"
            :sync="true"
            :labels="{ checked: 'Sort Mode', unchecked: 'Slot Mode' }"
            :css-colors="true"
            :font-size="13"
            :width="107"
            :height="28" />
        </template>
      </div>

      <playerGameTable></playerGameTable>
    </div>

    <div v-show="coverArtMode" class="cover-art-container">
      <div id="full-cover-art-view" :class="{ 'full-cover-art-view-render-mode': renderingSnapshot }">
        <div class="cover-art-view-publisher-info">
          <div class="publisher-name-and-icon">
            <div v-if="publisher.publisherIcon && iconIsValid" class="publisher-icon">
              {{ publisher.publisherIcon }}
            </div>
            <div class="publisher-name">
              <h1>{{ publisher.publisherName }}</h1>
            </div>
          </div>
          <div>
            <h4>Player Name: {{ publisher.playerName }}</h4>
            <h4>League: {{ publisher.leagueName }}</h4>
          </div>
        </div>
        <publisherCoverView></publisherCoverView>
        <div class="logo-container">
          <span class="gg-note">
            Images provided by
            <a href="https://ggapp.io/" target="_blank">
              <img src="@/assets/gg-logo.png" />
            </a>
          </span>
          <div v-show="renderingSnapshot">
            <img src="@/assets/cover-art-mode-watermark.png" />
          </div>
        </div>
      </div>
    </div>

    <div v-if="previousPublisher && nextPublisher" class="row">
      <div class="col-md-12 col-lg-5">
        <b-button class="publisher-nav" variant="primary" :to="{ name: 'publisher', params: { publisherid: previousPublisher.publisherID } }">
          Previous Publisher - {{ previousPublisher.publisherName }}
        </b-button>
      </div>
      <div class="col-md-12 col-lg-5 offset-lg-2">
        <b-button class="publisher-nav" variant="primary" :to="{ name: 'publisher', params: { publisherid: nextPublisher.publisherID } }">Next Publisher - {{ nextPublisher.publisherName }}</b-button>
      </div>
    </div>
  </div>
</template>

<script>
import html2canvas from 'html2canvas';
import { ToggleButton } from 'vue-js-toggle-button';

import PlayerGameTable from '@/components/gameTables/playerGameTable.vue';
import PublisherCoverView from '@/components/gameTables/publisherCoverView.vue';
import { publisherIconIsValid } from '@/globalFunctions';
import PublisherMixin from '@/mixins/publisherMixin.js';

export default {
  components: {
    PlayerGameTable,
    ToggleButton,
    PublisherCoverView
  },
  mixins: [PublisherMixin],
  props: {
    publisherid: { type: String, required: true }
  },
  data() {
    return {
      errorInfo: '',
      renderingSnapshot: false
    };
  },
  computed: {
    iconIsValid() {
      return publisherIconIsValid(this.publisher.publisherIcon);
    },
    moveGameError() {
      return this.$store.getters.moveGameError;
    },
    hasIneligibleGame() {
      return this.publisher.gameSlots.some((x) => !x.gameMeetsSlotCriteria);
    },
    allowMoveIntoIneligible() {
      return this.leagueYear.settings.allowMoveIntoIneligible;
    },
    editableSortOrderMode: {
      get() {
        return this.sortOrderMode;
      },
      set(value) {
        this.$store.commit('setSortOrderMode', value);
      }
    },
    editableCoverArtMode: {
      get() {
        return this.coverArtMode;
      },
      set(value) {
        this.$store.commit('setCoverArtMode', value);
      }
    },
    editableIncludeRemovedInSorted: {
      get() {
        return this.includeRemovedInSorted;
      },
      set(value) {
        this.$store.commit('setIncludeRemovedInSorted', value);
      }
    },
    nextPublisher() {
      let nextPublisher = this.leagueYear.publishers.filter((x) => x.draftPosition === this.publisher.draftPosition + 1)[0];
      if (!nextPublisher) {
        nextPublisher = this.leagueYear.publishers[0];
      }
      return nextPublisher;
    },
    previousPublisher() {
      let previousPublisher = this.leagueYear.publishers.filter((x) => x.draftPosition === this.publisher.draftPosition - 1)[0];
      if (!previousPublisher) {
        previousPublisher = this.leagueYear.publishers[this.leagueYear.publishers.length - 1];
      }
      return previousPublisher;
    }
  },
  watch: {
    async $route(to, from) {
      if (to.path !== from.path) {
        await this.initializePage();
      }
    }
  },
  async created() {
    await this.initializePage();
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
    },
    enterMoveMode() {
      this.$store.commit('enterMoveMode');
    },
    cancelMoveMode() {
      this.$store.commit('cancelMoveMode');
    },
    confirmPositions() {
      this.$store.dispatch('confirmPositions').then(() => {
        this.$emit('gamesMoved');
      });
    },
    prepareSnapshot() {
      this.renderingSnapshot = true;
      const viewport = document.querySelector('meta[name=viewport]');
      viewport.setAttribute('content', 'width=1920');
      setTimeout(this.sharePublisher, 1);
    },
    sharePublisher() {
      let elementID = '#full-cover-art-view';
      const options = { allowTaint: false, useCORS: true, scale: 2 };
      html2canvas(document.querySelector(elementID), options).then(async (canvas) => {
        const dataUrl = canvas.toDataURL('png');
        const blob = await (await fetch(dataUrl)).blob();
        const filesArray = [
          new File([blob], 'myPublisher.png', {
            type: blob.type,
            lastModified: new Date().getTime()
          })
        ];
        const shareData = {
          files: filesArray,
          title: 'My Fantasy Critic Publisher'
        };
        navigator.share(shareData);
      });
      this.renderingSnapshot = false;
      const viewport = document.querySelector('meta[name=viewport]');
      viewport.setAttribute('content', 'width=device-width');
    }
  }
};
</script>
<style scoped>
.publisher-header {
  margin-bottom: 10px;
  display: flex;
}

.publisher-details {
  background: #222222;
  border-radius: 5px;
  padding: 10px;
}

.publisher-name-and-icon {
  display: flex;
  align-items: center;
}

.publisher-name-and-slogan {
  display: flex;
  flex-direction: column;
}

.publisher-name {
  display: block;
  max-width: 100%;
  word-wrap: break-word;
}

.publisher-icon {
  font-size: 100px;
}

.publisher-view-options {
  display: flex;
  justify-content: space-between;
  height: 50px;
}

.cover-art-mode-options {
  margin-top: 10px;
  margin-bottom: 10px;
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 5px;
}

.cover-art-mode-options label {
  margin: 0;
}

.table-options {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  margin-bottom: 10px;
  gap: 5px;
}

.toggle {
  margin: 0;
}

.view-mode-label {
  margin: 0;
}

.cover-art-container {
  display: flex;
  justify-content: center;
}

#full-cover-art-view {
  background: #222222;
  border-radius: 5px;
  padding: 10px;
}

.full-cover-art-view-render-mode {
  border-radius: 0 !important;
}

.gg-note {
  font-size: 20px;
  font-weight: bolder;
}

.cover-art-view-publisher-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
}

.logo-container {
  margin-top: 10px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.publisher-nav {
  margin-top: 5px;
  width: 100%;
}
</style>
