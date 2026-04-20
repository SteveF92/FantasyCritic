<template>
  <section v-if="announcements.length" class="site-announcements" aria-label="Site announcements">
    <div>
      <h2 v-if="showHeading" class="site-announcements-heading">
        Announcements
        <router-link :to="{ name: 'siteUpdates' }">(See All)</router-link>
      </h2>
    </div>
    <b-alert v-for="item in announcements" :key="item.id" show :variant="item.variant || 'secondary'" class="site-announcement-alert mb-2">
      <div class="site-announcement-title">{{ item.title }}</div>
      <div v-if="item.postedAt" class="site-announcement-date">{{ item.postedAt }}</div>
      <p class="site-announcement-body mb-1">{{ item.body }}</p>
      <b-link v-if="item.href && item.linkLabel" :href="item.href" class="site-announcement-link">{{ item.linkLabel }}</b-link>
    </b-alert>
  </section>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import type { SiteAnnouncement } from '@/models/SiteAnnouncement';

export default defineComponent({
  name: 'SiteAnnouncementsWidget',
  props: {
    announcements: {
      type: Array as PropType<SiteAnnouncement[]>,
      required: true
    },
    showHeading: {
      type: Boolean,
      default: true
    }
  }
});
</script>

<style scoped>
.site-announcements {
  margin-top: 0.75rem;
  margin-bottom: 0.35rem;
}

.site-announcements-heading {
  font-size: 1.1rem;
  font-weight: bold;
  margin-bottom: 0.5rem;
  text-align: center;
}

@media (min-width: 992px) {
  .site-announcements-heading {
    text-align: left;
  }
}

.site-announcement-title {
  font-weight: bold;
}

.site-announcement-date {
  font-size: 0.85rem;
  margin-bottom: 0.25rem;
}

.site-announcement-alert.alert-secondary .site-announcement-date {
  color: rgba(255, 255, 255, 0.58);
}

.site-announcement-alert.alert-info .site-announcement-date {
  color: rgba(255, 255, 255, 0.65);
}

.site-announcement-alert.alert-warning .site-announcement-date {
  color: rgba(0, 0, 0, 0.55);
}

.site-announcement-alert.alert-success .site-announcement-date {
  color: rgba(255, 255, 255, 0.65);
}

.site-announcement-alert.alert-danger .site-announcement-date {
  color: rgba(255, 255, 255, 0.65);
}

.site-announcement-body {
  margin-bottom: 0;
  white-space: pre-wrap;
}

.site-announcement-alert {
  text-align: left;
}

.see-all-site-updates {
  text-align: center;
}

@media (min-width: 992px) {
  .see-all-site-updates {
    text-align: left;
  }
}

.see-all-site-updates a {
  color: #d6993a;
  font-weight: 600;
  text-decoration: underline;
  text-underline-offset: 2px;
}

.see-all-site-updates a:hover,
.see-all-site-updates a:focus {
  color: #e8b366;
}

/* Links: brand orange on grey secondary; high-contrast on strong-colored alerts. */
.site-announcement-alert.alert-secondary .site-announcement-link {
  color: #d6993a;
  font-weight: 600;
  text-decoration: underline;
  text-underline-offset: 2px;
}

.site-announcement-alert.alert-secondary .site-announcement-link:hover,
.site-announcement-alert.alert-secondary .site-announcement-link:focus {
  color: #e8b366;
}

.site-announcement-alert.alert-info .site-announcement-link,
.site-announcement-alert.alert-primary .site-announcement-link {
  color: #fff;
  font-weight: 600;
  text-decoration: underline;
  text-underline-offset: 2px;
}

.site-announcement-alert.alert-info .site-announcement-link:hover,
.site-announcement-alert.alert-info .site-announcement-link:focus,
.site-announcement-alert.alert-primary .site-announcement-link:hover,
.site-announcement-alert.alert-primary .site-announcement-link:focus {
  color: #f5f5f5;
}

.site-announcement-alert.alert-warning .site-announcement-link {
  color: #1a1a1a;
  font-weight: 600;
  text-decoration: underline;
  text-underline-offset: 2px;
}

.site-announcement-alert.alert-warning .site-announcement-link:hover,
.site-announcement-alert.alert-warning .site-announcement-link:focus {
  color: #000;
}

.site-announcement-alert.alert-dark .site-announcement-link {
  color: #d6993a;
  font-weight: 600;
  text-decoration: underline;
  text-underline-offset: 2px;
}

.site-announcement-alert.alert-dark .site-announcement-link:hover,
.site-announcement-alert.alert-dark .site-announcement-link:focus {
  color: #e8b366;
}

.site-announcement-alert.alert-success .site-announcement-link {
  color: #0b2e13;
  font-weight: 600;
  text-decoration: underline;
  text-underline-offset: 2px;
}

.site-announcement-alert.alert-danger .site-announcement-link {
  color: #fff;
  font-weight: 600;
  text-decoration: underline;
  text-underline-offset: 2px;
}
</style>
