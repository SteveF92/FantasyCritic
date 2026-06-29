<template>
  <div v-if="pendingDraftCalloutVisible">
    <!-- Main status banner: warning if not ready, success if ready -->
    <div :class="bannerClass" role="alert">
      <!-- Not ready -->
      <template v-if="!pendingDraft.readyToDraft">
        <h2 v-if="pendingDraftIsFirst">This year is not active yet!</h2>
        <h4 v-else>
          Your next draft —
          <strong>{{ pendingDraft.name }}</strong>
          — isn't ready to start yet.
        </h4>
        <ul>
          <li v-for="error in pendingDraft.startDraftErrors" :key="error">{{ error }}</li>
        </ul>
        <p v-if="pendingDraftIsFirst">Please note that once you start the draft, you can no longer add/remove players. Please make sure that everyone who wants to play this year joins beforehand.</p>
        <b-button v-if="mustSetDraftOrder" v-b-modal="'editDraftOrderForm'" variant="success">Set Draft Order</b-button>
        <b-button v-if="!pendingDraftIsFirst && isManager" variant="secondary" :to="manageDraftsRoute">Manage Drafts</b-button>
      </template>

      <!-- Ready to start -->
      <template v-if="pendingDraft.readyToDraft && !league.outstandingInvite">
        <template v-if="isManager">
          <p v-if="pendingDraftIsFirst">Things are all set to get started!</p>
          <p v-else>
            <strong>{{ pendingDraft.name }}</strong>
            is ready to go!
          </p>
          <p v-if="pendingDraftIsFirst">Please note that once you start the draft, you can no longer add/remove players. Please make sure that everyone who wants to play this year joins beforehand.</p>
          <b-button v-b-modal="'startDraft'" variant="primary" class="mx-2">Start Drafting!</b-button>
        </template>
        <template v-else>
          <span v-if="pendingDraftIsFirst">Things are all set! Your league manager can choose when to begin the draft.</span>
          <span v-else>
            <strong>{{ pendingDraft.name }}</strong>
            is ready to go! Your league manager can choose when to begin.
          </span>
        </template>
      </template>

      <!-- Imminent / scheduled line — shown inside the banner regardless of ready/not-ready -->
      <div v-if="showImminentLine" class="mt-2">
        <em v-if="pendingDraft.draftOrderSet && scheduledDateDisplay && !pendingDraftIsOverdue">Scheduled for {{ scheduledDateDisplay }} — draft order is set.</em>
        <em v-else-if="pendingDraft.draftOrderSet">Draft order is set.</em>
        <em v-else-if="scheduledDateDisplay && !pendingDraftIsOverdue">Scheduled for {{ scheduledDateDisplay }}.</em>
      </div>
    </div>

    <!-- Soft reminder: scheduled date has passed without starting -->
    <div v-if="pendingDraftIsOverdue" class="alert alert-secondary" role="alert">
      <span v-if="pendingDraftIsFirst">This draft was scheduled for {{ scheduledDateDisplay }} but hasn't started yet.</span>
      <span v-else>
        <strong>{{ pendingDraft.name }}</strong>
        was scheduled for {{ scheduledDateDisplay }} but hasn't started yet.
      </span>
      <span v-if="isManager">
        You can start it above or
        <router-link :to="manageDraftsRoute">reschedule it on the Manage Drafts page</router-link>
        whenever you're ready — there's no hard deadline.
      </span>
      <span v-else>Your league manager can start or reschedule whenever they're ready.</span>
    </div>

    <!-- Soft nudge: no scheduled date and not yet imminent -->
    <div v-if="!pendingDraft.scheduledDate && !pendingDraftIsImminent" class="alert alert-secondary">
      There is no scheduled date set for your draft —
      <router-link :to="manageDraftsRoute">set one on the Manage Drafts page</router-link>
      to help your players plan ahead.
    </div>
  </div>
</template>
<script>
import { DateTime } from 'luxon';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  computed: {
    bannerClass() {
      return this.pendingDraft?.readyToDraft ? 'alert alert-success' : 'alert alert-warning';
    },
    mustSetDraftOrder() {
      return (this.pendingDraft?.readyToSetDraftOrder ?? false) && (this.pendingDraft?.startDraftErrors ?? []).includes('You must set the draft order.');
    },
    showImminentLine() {
      if (!this.pendingDraftIsImminent) return false;
      return this.pendingDraft.draftOrderSet || (this.scheduledDateDisplay && !this.pendingDraftIsOverdue);
    },
    scheduledDateDisplay() {
      if (!this.pendingDraft?.scheduledDate) return null;
      return DateTime.fromISO(this.pendingDraft.scheduledDate).toFormat('MMMM d, yyyy');
    },
    manageDraftsRoute() {
      return { name: 'manageDrafts', params: { leagueid: this.league.leagueID, year: this.leagueYear.year } };
    }
  }
};
</script>
