<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Site updates</h1>
    <hr />
    <div class="text-well">
      <p v-if="!sortedAnnouncements.length" class="mb-0">No site updates have been posted yet.</p>
      <SiteAnnouncementsWidget
        v-else
        :announcements="sortedAnnouncements"
        :show-heading="false"
        :show-see-all-site-updates-link="false" />
    </div>
  </div>
</template>

<script>
import SiteAnnouncementsWidget from '@/components/siteAnnouncementsWidget.vue';
import { SITE_ANNOUNCEMENTS } from '@/data/siteAnnouncements';
import { sortSiteAnnouncementsNewestFirst } from '@/data/siteAnnouncementSort';

export default {
  components: {
    SiteAnnouncementsWidget
  },
  computed: {
    sortedAnnouncements() {
      return sortSiteAnnouncementsNewestFirst(SITE_ANNOUNCEMENTS);
    }
  }
};
</script>
