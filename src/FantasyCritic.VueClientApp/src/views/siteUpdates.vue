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
import axios from 'axios';

import type { SiteAnnouncement } from '@/models/SiteAnnouncement';
import SiteAnnouncementsWidget from '@/components/siteAnnouncementsWidget.vue';

export default defineComponent({
  components: {
    SiteAnnouncementsWidget
  },
  data() {
    return {
      siteAnnouncements: [] as SiteAnnouncement[]
    };
  },
  async created() {
    try {
      const { data } = await axios.get<SiteAnnouncement[]>('/api/general/siteannouncements');
      this.siteAnnouncements = data;
    } catch {
      this.siteAnnouncements = [];
    }
  }
});
</script>
