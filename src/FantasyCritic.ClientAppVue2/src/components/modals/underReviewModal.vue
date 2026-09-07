<template>
  <b-modal id="toggleUnderReview" ref="toggleUnderReviewRef" :title="modalTitle" @show="onShow">
    <div v-if="!leagueYear.underReview" class="alert alert-warning">
      <p class="text-white">
        <strong>Marking this league as Under Review</strong>
        will enable manager editing actions after the year has finished. Use this when you need to apply corrections (e.g., fixes to results, scoring, or roster issues) after the season is over.
      </p>
      <p class="mb-0 text-white">
        <strong>Time limit:</strong>
        This can only be enabled through
        <strong>January 31, {{ leagueYear.year + 1 }}</strong>
        . After that date, the league can no longer be placed Under Review.
      </p>
    </div>

    <div v-else class="alert alert-info">
      <strong>This league is currently Under Review.</strong>
      While Under Review, manager editing actions are enabled even though the year has finished. Turn this off when you’re done making corrections.
    </div>

    <div v-if="error" class="alert alert-danger mb-0">
      {{ error }}
    </div>

    <template #modal-footer>
      <input type="submit" class="btn" :class="leagueYear.underReview ? 'btn-secondary' : 'btn-warning'" :value="submitText" :disabled="isSubmitting" @click="toggleUnderReview" />
    </template>
  </b-modal>
</template>

<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      error: '',
      isSubmitting: false
    };
  },
  computed: {
    modalTitle() {
      return this.leagueYear.underReview ? 'Disable Under Review' : 'Mark Under Review';
    },
    submitText() {
      return this.leagueYear.underReview ? 'Disable Under Review' : 'Mark Under Review';
    }
  },
  methods: {
    onShow() {
      this.error = '';
      this.isSubmitting = false;
    },
    async toggleUnderReview() {
      const newValue = !this.leagueYear.underReview;

      const model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year, // adjust if your property name differs
        underReview: newValue
      };

      this.isSubmitting = true;
      this.error = '';

      try {
        await axios.post('/api/leagueManager/SetUnderReviewStatus', model);
        if (this.newValue) {
          this.notifyAction("League has been marked as 'under review'.");
        } else {
          this.notifyAction("League is no longer in 'under review' status.");
        }

        this.$refs.toggleUnderReviewRef.hide();
      } catch (error) {
        this.error = error && error.response && error.response.data ? error.response.data : 'Request failed.';
      } finally {
        this.isSubmitting = false;
      }
    }
  }
};
</script>
