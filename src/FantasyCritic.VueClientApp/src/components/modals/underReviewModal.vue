<template>
  <b-modal id="under-review-modal" ref="underReviewModalRef" :title="actionText" @show="onShow">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div v-if="!requiredLeagueYear.underReview" class="alert alert-warning">
      <p class="text-white">
        <strong>Marking this league as Under Review</strong>
        will enable manager editing actions after the year has finished. Use this when you need to apply corrections (e.g., fixes to results, scoring, or roster issues) after the season is over.
      </p>
      <p class="mb-0 text-white">
        <strong>Time limit:</strong>
        This can only be enabled through
        <strong>January 31, {{ requiredLeagueYear.year + 1 }}</strong>
        . After that date, the league can no longer be placed Under Review.
      </p>
    </div>

    <div v-else class="alert alert-info">
      <strong>This league is currently Under Review.</strong>
      While Under Review, manager editing actions are enabled even though the year has finished. Turn this off when you’re done making corrections.
    </div>

    <template #footer>
      <input type="submit" class="btn" :class="requiredLeagueYear.underReview ? 'btn-secondary' : 'btn-warning'" :value="actionText" :disabled="isBusy" @click="toggleUnderReview" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueManagerClient, type SetUnderReviewStatusRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface UnderReviewModalData {
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): UnderReviewModalData {
    return {
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear']),
    actionText(): string {
      return this.requiredLeagueYear.underReview ? 'Disable Under Review' : 'Mark Under Review';
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    onShow(): void {
      this.errorInfo = '';
    },
    async toggleUnderReview(): Promise<void> {
      const request: SetUnderReviewStatusRequest = {
        leagueID: this.requiredLeagueYear.league.leagueID,
        year: this.requiredLeagueYear.year,
        underReview: !this.requiredLeagueYear.underReview
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.setUnderReviewStatus(request);

        (this.$refs.underReviewModalRef as InstanceType<typeof BModal> | undefined)?.hide();
        if (request.underReview) {
          await this.notifyAction("League has been marked as 'under review'.");
        } else {
          await this.notifyAction("League is no longer in 'under review' status.");
        }
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Request failed.');
      } finally {
        this.isBusy = false;
      }
    }
  }
});
</script>
