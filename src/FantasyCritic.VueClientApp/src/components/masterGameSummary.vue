<template>
  <div v-if="masterGame" class="summary">
    <div v-if="!hideImage" class="game-image-area">
      <div v-if="ggLink && ggCoverArtLink" class="gg-image-area">
        <img v-if="ggCoverArtLink" :src="ggCoverArtLink" alt="Cover Image" class="game-image" />
        <a :href="ggLink" target="_blank">
          <strong>
            Image Provided by GG|
            <font-awesome-icon icon="external-link-alt" />
          </strong>
        </a>
      </div>
      <font-awesome-layers v-if="!masterGame.ggCoverArtFileName" class="fa-8x no-game-image">
        <font-awesome-icon :icon="['far', 'square']" />
        <font-awesome-layers-text transform="shrink-14" value="No image found" />
      </font-awesome-layers>
    </div>

    <div class="game-description">
      <h4 class="game-name">{{ masterGame.gameName }}</h4>

      <div v-if="masterGame.tags && masterGame.tags.length > 0" class="long-tag-list">
        <span class="detail-label">Tags:</span>
        <span v-for="tag in masterGame.tags" :key="tag">
          <masterGameTagBadge :tag-name="tag"></masterGameTagBadge>
        </span>
      </div>

      <div>
        <span class="detail-label">Release Date:</span>
        <span v-if="masterGame.releaseDate">{{ releaseDate }}</span>
        <span v-else>{{ masterGame.estimatedReleaseDate }} (Estimated)</span>
      </div>
      <template v-if="masterGameYear">
        <div>
          <span class="detail-label">Percent Published:</span>
          {{ formatPercent(masterGameYear.eligiblePercentStandardGame, 1) }}
        </div>
        <div>
          <span class="detail-label">Percent Counterpicked:</span>
          {{ formatPercent(masterGameYear.adjustedPercentCounterPick, 1) }}
        </div>
        <div>
          <span class="detail-label">Average Draft Position:</span>
          <span v-if="masterGameYear.averageDraftPosition">{{ formatScore(masterGameYear.averageDraftPosition, 1) }}</span>
          <span v-else>Undrafted</span>
        </div>
        <div>
          <span class="detail-label">Hype Factor:</span>
          <span v-if="masterGameYear.dateAdjustedHypeFactor">{{ formatScore(masterGameYear.dateAdjustedHypeFactor, 1) }}</span>
          <span v-else>Unhyped...</span>
        </div>
        <div v-if="masterGameYear.projectedFantasyPoints">
          <span v-if="!masterGameYear.criticScore" class="detail-label">Projected Points:</span>
          <span v-else class="detail-label">Pre-Release Projected Points:</span>
          <span>~{{ formatScore(masterGameYear.projectedFantasyPoints, 1) }}</span>
        </div>
      </template>
      <div v-if="masterGame.openCriticID">
        <a :href="openCriticLink" target="_blank">
          <strong>
            OpenCritic Link
            <font-awesome-icon icon="external-link-alt" />
          </strong>
        </a>
      </div>
      <div v-if="ggLink && (!masterGame.ggCoverArtFileName || hideImage)">
        <a :href="ggLink" target="_blank">
          <strong>
            GG| Link
            <font-awesome-icon icon="external-link-alt" />
          </strong>
        </a>
      </div>
      <div>
        <router-link class="text-primary" :to="{ name: 'mastergame', params: { mastergameid: masterGame.masterGameID } }"><strong>View full details</strong></router-link>
      </div>
      <div>
        <router-link class="text-primary" :to="{ name: 'masterGameChangeRequest', query: { mastergameid: masterGame.masterGameID } }"><strong>Suggest a correction</strong></router-link>
      </div>
      <div v-if="isFactChecker">
        <router-link class="text-primary" :to="{ name: 'masterGameEditor', params: { mastergameid: masterGame.masterGameID } }"><strong>Edit Master Game</strong></router-link>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import { mapState } from 'pinia';
import { DateTime } from 'luxon';

import { MasterGameViewModel, MasterGameYearViewModel } from '@/api/clients';
import { useAuthStore } from '@/store/authStore';
import { formatPercent, formatScore } from '@/utilities/formatters';
import { getGGCoverArtLinkForGame, getGGLinkForGame } from '@/utilities/ggLinks';
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

export default defineComponent({
  components: {
    MasterGameTagBadge
  },
  props: {
    masterGame: { type: Object as PropType<MasterGameViewModel | MasterGameYearViewModel>, required: true },
    hideImage: { type: Boolean }
  },
  computed: {
    ...mapState(useAuthStore, ['isFactChecker']),
    masterGameYear(): MasterGameYearViewModel | null {
      return 'year' in this.masterGame ? this.masterGame : null;
    },
    ggCoverArtLink(): string | null {
      return getGGCoverArtLinkForGame(this.masterGame, 165);
    },
    ggLink(): string | null {
      return getGGLinkForGame(this.masterGame);
    },
    releaseDate(): string | null {
      if (!this.masterGame.releaseDate) {
        return null;
      }
      return DateTime.fromISO(this.masterGame.releaseDate).toFormat('MMMM dd, yyyy');
    },
    openCriticLink(): string {
      return `https://opencritic.com/game/${this.masterGame.openCriticID}/${this.masterGame.openCriticSlug ?? 'b'}`;
    }
  },
  methods: {
    formatPercent,
    formatScore
  }
});
</script>
<style scoped>
.detail-label {
  font-weight: bold;
  margin-right: 4px;
}

.summary {
  display: flex;
  flex-direction: row;
  flex-wrap: wrap;
  justify-content: space-evenly;
  align-items: center;
  color: black;
  padding: 5px;
  gap: 10px;
  text-align: center;
}

.gg-image-area {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.game-image {
  display: block;
  margin: auto;
  border-radius: 5%;
}

.game-description {
  max-width: 250px;
}

.game-name {
  text-align: center;
}

@media only screen and (min-width: 500px) {
  .minimal-game-image {
    display: none;
  }
}

@media only screen and (max-width: 501px) {
  .full-game-image {
    display: none;
  }
}
</style>
