<template>
  <b-modal id="edit-draft-order-form" ref="editDraftOrderFormRef" title="Set Draft Order" no-footer @hidden="clearData" @show="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <label>How do you want to set the draft order?</label>
    <div v-if="isFirstDraft && hasPreviousYear" class="order-options-list">
      <label>Recommended:</label>
      <b-button variant="success" size="sm" :disabled="isBusy" @click="setDraftOrder('InverseStandings')">Inverse of Last Year's Results</b-button>
      <label>Other Options:</label>
      <b-button variant="secondary" size="sm" :disabled="isBusy" @click="setDraftOrder('Random')">Randomly</b-button>
      <b-button variant="warning" size="sm" @click="showManualSettings = true">Manually</b-button>
    </div>
    <div v-else-if="isFirstDraft" class="order-options-list">
      <label>Recommended:</label>
      <b-button variant="success" size="sm" :disabled="isBusy" @click="setDraftOrder('Random')">Randomly</b-button>
      <label>Other Options:</label>
      <b-button variant="warning" size="sm" @click="showManualSettings = true">Manually</b-button>
    </div>
    <div v-else class="order-options-list">
      <label>Recommended:</label>
      <b-button variant="success" size="sm" :disabled="isBusy" @click="setDraftOrder('InverseProjectedPoints')">Inverse of Current Projected Points</b-button>
      <label>Other Options:</label>
      <b-button v-if="hasPreviousYear" variant="secondary" size="sm" :disabled="isBusy" @click="setDraftOrder('InverseStandings')">Inverse of Last Year's Results</b-button>
      <b-button variant="secondary" size="sm" :disabled="isBusy" @click="setDraftOrder('Random')">Randomly</b-button>
      <b-button variant="warning" size="sm" @click="showManualSettings = true">Manually</b-button>
    </div>

    <div v-show="showManualSettings">
      <label>Drag and drop to change order.</label>
      <div class="fluid container draft-order-editor bg-secondary">
        <VueDraggable v-model="desiredDraftOrder" tag="ul" class="list-group" :animation="150" handle=".handle">
          <li v-for="(publisher, index) in desiredDraftOrder" :key="publisher.publisherID" class="draft-order-item">
            <font-awesome-icon icon="bars" class="handle" />
            <span class="badge">{{ index + 1 }}</span>
            {{ publisher.publisherName }} ({{ publisher.playerName }})
          </li>
        </VueDraggable>
      </div>

      <b-button class="confirm-button" variant="primary" size="sm" :disabled="isBusy" @click="setDraftOrder('Manual')">Confirm Draft Order</b-button>
    </div>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';
import { VueDraggable } from 'vue-draggable-plus';

import { leagueManagerClient, type DraftOrderRequest, type PublisherViewModel } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

type DraftOrderType = 'Random' | 'Manual' | 'InverseStandings' | 'InverseProjectedPoints';

interface EditDraftOrderFormData {
  showManualSettings: boolean;
  desiredDraftOrder: PublisherViewModel[];
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  components: {
    VueDraggable
  },
  data(): EditDraftOrderFormData {
    return {
      showManualSettings: false,
      desiredDraftOrder: [],
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeague', 'requiredLeagueYear', 'pendingDraft']),
    hasPreviousYear(): boolean {
      return this.requiredLeague.years.includes(this.requiredLeagueYear.year - 1);
    },
    isFirstDraft(): boolean {
      return (this.pendingDraft?.draftNumber ?? 1) === 1;
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async setDraftOrder(draftOrderType: DraftOrderType): Promise<void> {
      if (!this.pendingDraft) {
        throw new Error("Can't set draft order if there is no pending draft.");
      }
      const request: DraftOrderRequest = {
        leagueID: this.requiredLeagueYear.leagueID,
        year: this.requiredLeagueYear.year,
        draftID: this.pendingDraft.draftID,
        draftOrderType: draftOrderType
      };
      if (draftOrderType === 'Manual') {
        request.manualPublisherDraftPositions = this.desiredDraftOrder.map((x) => x.publisherID);
      }

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.setDraftOrder(request);
        (this.$refs.editDraftOrderFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('Draft order has been changed.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when setting draft order.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.desiredDraftOrder = this.requiredLeagueYear.publishers;
      this.showManualSettings = false;
      this.errorInfo = '';
    }
  }
});
</script>

<style scoped>
.draft-order-editor {
  border-radius: 5px;
  padding-left: 20px;
  padding-right: 20px;
  padding-top: 5px;
  padding-bottom: 5px;
}

.list-group {
  min-height: 20px;
}

.draft-order-item {
  position: relative;
  display: block;
  padding: 10px 15px;
  margin-bottom: -1px;
  background-color: #5b6977 !important;
  border: 1px solid #ddd;
}

.handle {
  cursor: pointer;
}

.order-options-list {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.confirm-button {
  float: right;
  margin-top: 10px;
}
</style>
