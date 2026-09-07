<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Advanced Game Stats</h1>

    <div class="advanced-stats-nav">
      <template v-for="(section, index) in sections">
        <router-link :key="section.hash" :to="{ hash: '#' + section.hash }" class="advanced-stats-nav-link text-primary" :class="{ 'active-section': activeSection === section.hash }">
          {{ section.label }}
        </router-link>
        <span v-if="index < sections.length - 1" :key="section.hash + '-sep'" class="advanced-stats-nav-sep">|</span>
      </template>
    </div>

    <div class="section-content">
      <longestTenuredGamesPanel v-if="activeSection === 'longestTenuredUnreleased'" :released="false"></longestTenuredGamesPanel>
      <longestTenuredGamesPanel v-else-if="activeSection === 'longestTenuredReleased'" :released="true"></longestTenuredGamesPanel>
      <mostDreamsDashedPanel v-else-if="activeSection === 'mostDreamsDashed'"></mostDreamsDashedPanel>
    </div>
  </div>
</template>

<script>
import LongestTenuredGamesPanel from '@/components/gameStats/longestTenuredGamesPanel.vue';
import MostDreamsDashedPanel from '@/components/gameStats/mostDreamsDashedPanel.vue';

const VALID_SECTIONS = ['longestTenuredUnreleased', 'longestTenuredReleased', 'mostDreamsDashed'];
const DEFAULT_SECTION = 'longestTenuredUnreleased';

export default {
  components: {
    LongestTenuredGamesPanel,
    MostDreamsDashedPanel
  },
  data() {
    return {
      sections: [
        { hash: 'longestTenuredUnreleased', label: 'Longest Tenured (Unreleased)' },
        { hash: 'longestTenuredReleased', label: 'Longest Tenured (Released)' },
        { hash: 'mostDreamsDashed', label: 'Most Dreams Dashed' }
      ]
    };
  },
  computed: {
    activeSection() {
      const hash = this.$route.hash ? this.$route.hash.replace('#', '') : '';
      return VALID_SECTIONS.includes(hash) ? hash : DEFAULT_SECTION;
    }
  }
};
</script>

<style scoped>
.advanced-stats-nav {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: center;
  gap: 0.35rem;
  margin-bottom: 1.5rem;
  font-size: 1.05rem;
}

.advanced-stats-nav-sep {
  color: #6c757d;
  user-select: none;
}

.advanced-stats-nav-link {
  text-decoration: none;
}

.advanced-stats-nav-link.active-section {
  font-weight: bold;
  text-decoration: underline;
}

.section-content {
  margin-top: 0.5rem;
}
</style>
