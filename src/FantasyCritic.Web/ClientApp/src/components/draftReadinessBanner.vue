<template>
  <div v-if="pendingDraft">
    <!-- Main status banner: warning if not ready, success if ready -->
    <div :class="bannerClass" role="alert">
      <!-- Not ready -->
      <template v-if="!pendingDraft.readyToDraft">
        <h2 v-if="isFirstDraft">This year is not active yet!</h2>
        <h4 v-else>Your next draft — <strong>{{ pendingDraft.name }}</strong> — isn't ready to start yet.</h4>
        <ul>
          <li v-for="error in pendingDraft.startDraftErrors" :key="error">{{ error }}</li>
        </ul>
        <p v-if="isFirstDraft">Please note that once you start the draft, you can no longer add/remove players. Please make sure that everyone who wants to play this year joins beforehand.</p>
        <b-button v-if="mustSetDraftOrder" v-b-modal="'editDraftOrderForm'" variant="success">Set Draft Order</b-button>
        <router-link v-if="!isFirstDraft && isManager" :to="manageDraftsRoute" class="btn btn-secondary btn-sm ml-2">Manage Drafts</router-link>
      </template>

      <!-- Ready to start -->
      <template v-if="pendingDraft.readyToDraft && !league.outstandingInvite">
        <template v-if="isManager">
          <p v-if="isFirstDraft">Things are all set to get started!</p>
          <p v-else><strong>{{ pendingDraft.name }}</strong> is ready to go!</p>
          <p v-if="isFirstDraft">Please note that once you start the draft, you can no longer add/remove players. Please make sure that everyone who wants to play this year joins beforehand.</p>
          <b-button v-b-modal="'startDraft'" variant="primary" class="mx-2">Start Drafting!</b-button>
        </template>
        <template v-else>
          <span v-if="isFirstDraft">Things are all set! Your league manager can choose when to begin the draft.</span>
          <span v-else><strong>{{ pendingDraft.name }}</strong> is ready to go! Your league manager can choose when to begin.</span>
        </template>
      </template>

      <!-- Imminent / scheduled line — shown inside the banner regardless of ready/not-ready -->
      <div v-if="isImminent" class="mt-2">
        <em v-if="pendingDraft.draftOrderSet && scheduledDateDisplay">Scheduled for {{ scheduledDateDisplay }} — draft order is set.</em>
        <em v-else-if="pendingDraft.draftOrderSet">Draft order is set.</em>
        <em v-else-if="scheduledDateDisplay">Scheduled for {{ scheduledDateDisplay }}.</em>
      </div>
    </div>

    <!-- Soft nudge: no scheduled date and not yet imminent -->
    <div v-if="!pendingDraft.scheduledDate && !isImminent" class="alert alert-secondary">
      No scheduled date set —
      <router-link :to="manageDraftsRoute">set one on the Manage Drafts page</router-link>
      to help your players plan ahead.
    </div>
  </div>
</template>
<script>
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  computed: {
    bannerClass() {
      return this.pendingDraft?.readyToDraft ? 'alert alert-success' : 'alert alert-warning';
    },
    isFirstDraft() {
      return this.pendingDraft?.draftNumber === 1;
    },
    mustSetDraftOrder() {
      return (
        (this.pendingDraft?.readyToSetDraftOrder ?? false) &&
        (this.pendingDraft?.startDraftErrors ?? []).includes('You must set the draft order.')
      );
    },
    isImminent() {
      if (!this.pendingDraft) return false;
      if (this.pendingDraft.draftOrderSet) return true;
      if (!this.pendingDraft.scheduledDate) return false;
      // Parse YYYY-MM-DD without timezone conversion
      const parts = this.pendingDraft.scheduledDate.split('-');
      const scheduled = new Date(parseInt(parts[0]), parseInt(parts[1]) - 1, parseInt(parts[2]));
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      return (scheduled - today) / (1000 * 60 * 60 * 24) <= 7;
    },
    scheduledDateDisplay() {
      if (!this.pendingDraft?.scheduledDate) return null;
      const parts = this.pendingDraft.scheduledDate.split('-');
      const date = new Date(parseInt(parts[0]), parseInt(parts[1]) - 1, parseInt(parts[2]));
      return date.toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' });
    },
    manageDraftsRoute() {
      return { name: 'manageDrafts', params: { leagueid: this.league.leagueID, year: this.leagueYear.year } };
    }
  }
};
</script>
