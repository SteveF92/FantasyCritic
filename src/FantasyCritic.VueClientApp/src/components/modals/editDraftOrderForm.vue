<template>
  <b-modal id="editDraftOrderForm" ref="editDraftOrderFormRef" title="Set Draft Order" no-footer @hidden="clearData">
    <div v-show="errorInfo" class="alert alert-danger">{{ errorInfo }}</div>
    <label>How do you want to set the draft order?</label>
    <div v-if="isFirstDraft && hasPreviousYear" class="order-options-list">
      <label>Recommended:</label>
      <b-button variant="success" size="sm" @click="setDraftOrder('InverseStandings')">Inverse of Last Year's Results</b-button>
      <label>Other Options:</label>
      <b-button variant="secondary" size="sm" @click="setDraftOrder('Random')">Randomly</b-button>
      <b-button variant="warning" size="sm" @click="showManualSettings = true">Manually</b-button>
    </div>
    <div v-else-if="isFirstDraft" class="order-options-list">
      <label>Recommended:</label>
      <b-button variant="success" size="sm" @click="setDraftOrder('Random')">Randomly</b-button>
      <label>Other Options:</label>
      <b-button variant="warning" size="sm" @click="showManualSettings = true">Manually</b-button>
    </div>
    <div v-else class="order-options-list">
      <label>Recommended:</label>
      <b-button variant="success" size="sm" @click="setDraftOrder('InverseProjectedPoints')">Inverse of Current Projected Points</b-button>
      <label>Other Options:</label>
      <b-button v-if="hasPreviousYear" variant="secondary" size="sm" @click="setDraftOrder('InverseStandings')">Inverse of Last Year's Results</b-button>
      <b-button variant="secondary" size="sm" @click="setDraftOrder('Random')">Randomly</b-button>
      <b-button variant="warning" size="sm" @click="showManualSettings = true">Manually</b-button>
    </div>

    <div v-show="showManualSettings">
      <label>Drag and drop to change order.</label>
      <div class="fluid container draft-order-editor bg-secondary">
        <VueDraggable v-model="desiredDraftOrder" tag="ul" class="list-group" :animation="150" handle=".handle">
          <li v-for="(publisher, index) in desiredDraftOrder" :key="publisher.draftPosition" class="draft-order-item">
            <font-awesome-icon icon="bars" class="handle" />
            <span class="badge">{{ index + 1 }}</span>
            {{ publisher.publisherName }} ({{ publisher.playerName }})
          </li>
        </VueDraggable>
      </div>

      <b-button class="confirm-button" variant="primary" size="sm" @click="setDraftOrder('Manual')">Confirm Draft Order</b-button>
    </div>
  </b-modal>
</template>

<script>
import { mapActions, mapState } from 'pinia';
import axios from 'axios';
import { VueDraggable } from 'vue-draggable-plus';

import { useLeagueStore } from '@/store/leagueStore';

export default {
  components: {
    VueDraggable
  },
  data() {
    return {
      showManualSettings: false,
      desiredDraftOrder: [],
      errorInfo: null
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['leagueYear', 'league', 'pendingDraft']),
    hasPreviousYear() {
      return this.league.years.includes(this.leagueYear.year - 1);
    },
    isFirstDraft() {
      return (this.pendingDraft?.draftNumber ?? 1) === 1;
    }
  },
  created() {
    this.clearData();
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async setDraftOrder(draftOrderType) {
      const model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        draftID: this.pendingDraft.draftID,
        draftOrderType: draftOrderType
      };
      if (draftOrderType === 'Manual') {
        const desiredDraftOrderIDs = this.desiredDraftOrder.map((v) => v.publisherID);
        model.manualPublisherDraftPositions = desiredDraftOrderIDs;
      }

      try {
        await axios.post('/api/leagueManager/SetDraftOrder', model);
        this.$refs.editDraftOrderFormRef.hide();
        this.notifyAction('Draft order has been changed.');
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    clearData() {
      this.desiredDraftOrder = this.leagueYear.publishers;
    }
  }
};
</script>
<style>
.draft-order-editor {
  border-radius: 5px;
  padding-left: 20px;
  padding-right: 20px;
  padding-top: 5px;
  padding-bottom: 5px;
}

.ghost {
  opacity: 0.5;
  background: #c8ebfb;
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

.draft-order-item i {
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
