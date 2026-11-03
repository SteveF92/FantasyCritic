<template>
  <div v-if="siteCounts" class="row counts-area bg-secondary">
    <div class="col-md-12 col-lg-10 offset-md-0 offset-lg-1">
      <div class="row">
        <div class="col-6 col-sm-3">
          <h3>{{ formatThousands(siteCounts.userCount) }}</h3>
          <h4>Users Joined</h4>
        </div>
        <div class="col-6 col-sm-3">
          <h3>{{ formatThousands(siteCounts.leagueCount) }}</h3>
          <h4>Leagues Created</h4>
        </div>
        <div class="col-6 col-sm-3">
          <h3>{{ formatThousands(siteCounts.masterGameCount) }}</h3>
          <h4>Unique Games</h4>
        </div>
        <div class="col-6 col-sm-3">
          <h3>{{ formatThousands(siteCounts.publisherGameCount) }}</h3>
          <h4>Games Drafted</h4>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { generalClient } from '@/api/clients';
import { formatThousands } from '@/utilities/formatters';

export default {
  data() {
    return {
      siteCounts: null
    };
  },
  async created() {
    await this.fetchSiteCounts();
  },
  methods: {
    formatThousands,
    async fetchSiteCounts() {
      this.siteCounts = await generalClient.siteCounts();
    }
  }
};
</script>

<style scoped>
.counts-area {
  text-align: center;
}
</style>
