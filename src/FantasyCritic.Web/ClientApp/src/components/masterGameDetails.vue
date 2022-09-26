<template>
  <div v-if="masterGame">
    <ul>
      <li>
        <span class="detail-label">Release Date:</span>
        <span v-if="masterGame.releaseDate">{{ formatDate(masterGame.releaseDate) }}</span>
        <span v-else>{{ masterGame.estimatedReleaseDate }} (Estimated)</span>
      </li>
      <li v-if="masterGame.minimumReleaseDate && !masterGame.releaseDate">
        <span class="detail-label">Minimum Release Date:</span>
        <span v-if="masterGame.minimumReleaseDate">{{ formatDate(masterGame.minimumReleaseDate) }}</span>
        <font-awesome-icon
          v-b-popover.hover.focus="'Minimum Release Date is our attempt at defining the \'earlist possible release date\' based on the above estimate from the makers of the game.'"
          color="white"
          icon="info-circle"
          class="date-info" />
      </li>
      <li v-if="masterGame.maximumReleaseDate && !masterGame.releaseDate">
        <span class="detail-label">Maximum Release Date:</span>
        <span v-if="masterGame.maximumReleaseDate">{{ formatDate(masterGame.maximumReleaseDate) }}</span>
        <font-awesome-icon
          v-b-popover.hover.focus="'Maximum Release Date is our attempt at defining the \'latest possible release date\' based on the above estimate from the makers of the game.'"
          color="white"
          icon="info-circle"
          class="date-info" />
      </li>
      <li v-if="masterGame.earlyAccessReleaseDate">
        <span class="detail-label">Early Access Release Date:</span>
        <span v-if="masterGame.earlyAccessReleaseDate">{{ formatDate(masterGame.earlyAccessReleaseDate) }}</span>
      </li>
      <li v-if="masterGame.internationalReleaseDate">
        <span class="detail-label">International Release Date:</span>
        <span v-if="masterGame.internationalReleaseDate">{{ formatDate(masterGame.internationalReleaseDate) }}</span>
      </li>
      <li v-if="masterGame.announcementDate">
        <span class="detail-label">Announcement Date:</span>
        <span v-if="masterGame.announcementDate">{{ formatDate(masterGame.announcementDate) }}</span>
      </li>
      <li v-if="masterGame.delayContention">
        <span class="detail-label">Delay In Contention:</span>
        There are very credible reports that this game has been delayed and therefore will not release this year. The game is still counted as a "will release" game for drop purposes, but it cannot be
        counter picked, just like a "will not release" game cannot be counter picked.
      </li>
      <li>
        <label v-if="masterGame.averagedScore">This is an episodic game. We have caluclated an average score.</label>
        <div>
          <span class="detail-label">Critic Score:</span>
          {{ masterGame.criticScore | score(2) }}
          <span v-if="masterGame.averagedScore">(Averaged Score)</span>
        </div>
      </li>
      <li>
        <a v-if="masterGame.openCriticID" :href="openCriticLink(masterGame)" target="_blank">
          OpenCritic Link
          <font-awesome-icon icon="external-link-alt" size="xs" />
        </a>
        <span v-else>Not linked to OpenCritic</span>
      </li>
      <li v-if="isFactChecker">Added by {{ masterGame.addedByUser.displayName }}</li>
    </ul>
    <div v-if="masterGame.tags && masterGame.tags.length > 0" class="long-tag-list">
      <h4>Tags</h4>
      <span v-for="tag in masterGame.tags" :key="tag">
        <masterGameTagBadge :tag-name="tag"></masterGameTagBadge>
      </span>
    </div>
    <div v-if="masterGame.notes">
      <h3>Special Notes</h3>
      {{ masterGame.notes }}
    </div>
  </div>
</template>

<script>
import moment from 'moment';
import MasterGameTagBadge from '@/components/masterGameTagBadge';

export default {
  components: {
    MasterGameTagBadge
  },
  props: {
    masterGame: { type: Object, required: true }
  },
  methods: {
    formatDate(releaseDate) {
      if (releaseDate === '9999-12-31') {
        return 'No Maximum Release Date';
      }
      return moment(releaseDate).format('MMMM Do, YYYY');
    },
    openCriticLink(game) {
      return 'https://opencritic.com/game/' + game.openCriticID + '/a';
    }
  }
};
</script>
<style scoped>
.detail-label {
  font-weight: bold;
  margin-right: 4px;
}

.date-info {
  margin-left: 5px;
}
</style>
