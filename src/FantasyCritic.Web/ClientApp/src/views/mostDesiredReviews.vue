<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Most Desired Reviews</h1>
    <p>These are the games that have the most interest in seeing more reviews written for them.</p>

    <b-table small bordered striped responsive :items="mostDesiredReviews" :fields="gameFields" :sort-by.sync="sortBy" :sort-desc.sync="sortDesc">
      <template #cell(masterGame.gameName)="data">
        <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
      </template>
      <template #cell(masterGame.maximumReleaseDate)="data">
        {{ data.item.masterGame.maximumReleaseDate | longDate }}
      </template>
      <template #cell(openCriticLink)="data">
        <a v-if="data.item.masterGame.openCriticID" :href="openCriticLink(data.item.masterGame)" target="_blank">
          <strong>
            OpenCritic
            <font-awesome-icon icon="external-link-alt" />
          </strong>
        </a>
        <span v-else>--</span>
      </template>
    </b-table>
  </div>
</template>

<script>
import axios from 'axios';
import MasterGamePopover from '@/components/masterGamePopover.vue';

export default {
  components: {
    MasterGamePopover
  },
  data() {
    return {
      mostDesiredReviews: null,
      sortBy: 'desireFactor',
      sortDesc: true,
      gameFields: [
        { key: 'masterGame.gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        { key: 'openCriticLink', label: 'Open Critic Link', thClass: ['bg-primary', 'md-screen-minimum', 'position-relative'], tdClass: 'md-screen-minimum' },
        { key: 'desireFactor', label: 'Desire Factor', sortable: true, thClass: 'bg-primary' }
      ]
    };
  },
  methods: {
    openCriticLink(game) {
      return `https://opencritic.com/game/${game.openCriticID}/${game.openCriticSlug ?? 'b'}`;
    }
  },
  async created() {
    const response = await axios.get('/api/game/GetMostDesiredReviews');
    this.mostDesiredReviews = response.data;
  }
};
</script>
