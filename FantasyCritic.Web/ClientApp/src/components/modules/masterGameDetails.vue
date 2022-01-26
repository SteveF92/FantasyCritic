<template>
  <div v-if="masterGame">
    <ul>
      <li>
        <strong>Release Date: </strong>
        <span v-if="masterGame.releaseDate">{{formatDate(masterGame.releaseDate)}}</span>
        <span v-else>{{masterGame.estimatedReleaseDate}} (Estimated)</span>
      </li>
      <li v-if="masterGame.minimumReleaseDate && !masterGame.releaseDate">
        <strong>Minimum Release Date: </strong>
        <span v-if="masterGame.minimumReleaseDate">{{formatDate(masterGame.minimumReleaseDate)}}</span>
        <font-awesome-icon color="white" icon="info-circle" class="date-info"
                           v-b-popover.hover="'Minimum Release Date is our attempt at defining the \'earlist possible release date\' based on the above estimate from the makers of the game.'" />
      </li>
      <li v-if="masterGame.maximumReleaseDate  && !masterGame.releaseDate">
        <strong>Maximum Release Date: </strong>
        <span v-if="masterGame.maximumReleaseDate">{{formatDate(masterGame.maximumReleaseDate)}}</span>
        <font-awesome-icon color="white" icon="info-circle" class="date-info"
                           v-b-popover.hover="'Maximum Release Date is our attempt at defining the \'latest possible release date\' based on the above estimate from the makers of the game.'" />
      </li>
      <li v-if="masterGame.earlyAccessReleaseDate">
        <strong>Early Access Release Date: </strong>
        <span v-if="masterGame.earlyAccessReleaseDate">{{formatDate(masterGame.earlyAccessReleaseDate)}}</span>
      </li>
      <li v-if="masterGame.internationalReleaseDate">
        <strong>International Release Date: </strong>
        <span v-if="masterGame.internationalReleaseDate">{{formatDate(masterGame.internationalReleaseDate)}}</span>
      </li>
      <li v-if="masterGame.announcementDate">
        <strong>Announcement Date: </strong>
        <span v-if="masterGame.announcementDate">{{formatDate(masterGame.announcementDate)}}</span>
      </li>
      <li>
        <label v-if="masterGame.averagedScore">This is an episodic game. We have caluclated an average score.</label>
        <div>
          <strong>Critic Score: </strong>
          {{masterGame.criticScore | score(2)}}
          <span v-if="masterGame.averagedScore">(Averaged Score)</span>
        </div>
      </li>
      <li>
        <a v-if="masterGame.openCriticID" :href="openCriticLink(masterGame)" target="_blank">OpenCritic Link <font-awesome-icon icon="external-link-alt" size="xs" /></a>
        <span v-else>Not linked to OpenCritic</span>
      </li>
    </ul>
    <div v-if="masterGame.tags && masterGame.tags.length > 0" class="long-tag-list">
      <h4>Tags</h4>
      <span v-for="(tag, index) in masterGame.tags">
        <masterGameTagBadge :tagName="masterGame.tags[index]"></masterGameTagBadge>
      </span>
    </div>
    <div v-show="masterGame.notes">
      <h3>Special Notes</h3>
      {{masterGame.notes}}
    </div>
  </div>
</template>

<script>
import moment from 'moment';
import MasterGameTagBadge from '@/components/modules/masterGameTagBadge';

export default {
  props: ['masterGame'],
  components: {
    MasterGameTagBadge
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
<style>
  .date-info {
    margin-left: 5px;
  }
</style>
