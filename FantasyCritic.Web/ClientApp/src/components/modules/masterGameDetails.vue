<template>
  <div v-if="masterGame">
    <ul>
      <li>
        <strong>Release Date: </strong>
        <span v-if="masterGame.releaseDate">{{formatDate(masterGame.releaseDate)}}</span>
        <span v-else>{{masterGame.estimatedReleaseDate}} (Estimated)</span>
      </li>
      <li v-if="masterGame.earlyAccessReleaseDate">
        <strong>Early Access Release Date: </strong>
        <span v-if="masterGame.earlyAccessReleaseDate">{{formatDate(masterGame.earlyAccessReleaseDate)}}</span>
      </li>
      <li v-if="masterGame.internationalReleaseDate">
        <strong>International Release Date: </strong>
        <span v-if="masterGame.internationalReleaseDate">{{formatDate(masterGame.internationalReleaseDate)}}</span>
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
      <li>Eligibility Level: {{ masterGame.eligibilitySettings.eligibilityLevel.name }}</li>
      <li>Yearly Installment: {{ masterGame.eligibilitySettings.yearlyInstallment | yesNo }}</li>
      <li>Early Access: {{ masterGame.eligibilitySettings.earlyAccess | yesNo }}</li>
      <li>Free to Play: {{ masterGame.eligibilitySettings.freeToPlay | yesNo }}</li>
      <li>Released Internationally: {{ masterGame.eligibilitySettings.releasedInternationally | yesNo }}</li>
      <li>Expansion Pack: {{ masterGame.eligibilitySettings.expansionPack | yesNo }}</li>
      <li>Unannounced: {{ masterGame.eligibilitySettings.unannouncedGame | yesNo }}</li>
    </ul>
    <h4>Tags</h4>
    <ul v-show="masterGame.tags && masterGame.tags.length > 0">
      <li v-for="tag in masterGame.tags">{{tag.readableName}}</li>
    </ul>
    <div v-show="masterGame.notes">
      <h3>Special Notes</h3>
      {{masterGame.notes}}
    </div>
  </div>
</template>

<script>
import moment from 'moment';

export default {
  props: ['masterGame'],
  methods: {
    formatDate(releaseDate) {
      return moment(releaseDate).format('MMMM Do, YYYY');
    },
    openCriticLink(game) {
      return 'https://opencritic.com/game/' + game.openCriticID + '/a';
    }
  }
};
</script>
