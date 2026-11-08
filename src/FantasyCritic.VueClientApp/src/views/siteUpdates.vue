<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Site updates</h1>
    <hr />
    <div class="text-well">
      <p v-if="!siteAnnouncements.length" class="mb-0">No site updates have been posted yet.</p>
      <SiteAnnouncementsWidget v-else :announcements="siteAnnouncements" :show-heading="false" />
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';

import { generalClient, SiteAnnouncementViewModel } from '@/api/clients';
import SiteAnnouncementsWidget from '@/components/siteAnnouncementsWidget.vue';

export default defineComponent({
  components: {
    SiteAnnouncementsWidget
  },
  data() {
    return {
      siteAnnouncements: [] as SiteAnnouncementViewModel[]
    };
  },
  async created() {
    try {
      this.siteAnnouncements = await generalClient.siteAnnouncements();
    } catch {
      this.siteAnnouncements = [];
    }
  }
});
</script>
