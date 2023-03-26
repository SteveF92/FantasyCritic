<template>
  <b-modal id="editDraftOrderForm" ref="editDraftOrderFormRef" title="Set Draft Order" hide-footer @hidden="clearData">
    <div v-show="errorInfo" class="alert alert-danger">{{ errorInfo }}</div>
    <label>How do you want to set the draft order?</label>
    <div v-if="hasPreviousYear" class="order-options-list">
      <label>Recommended:</label>
      <b-button variant="success" size="sm" @click="setDraftOrder('InverseStandings')">Inverse of Last Year's Results</b-button>
      <label>Other Options:</label>
      <b-button variant="secondary" size="sm" @click="setDraftOrder('Random')">Randomly</b-button>
      <b-button variant="warning" size="sm" @click="showManualSettings = true">Manually</b-button>
    </div>
    <div v-else class="order-options-list">
      <label>Recommended:</label>
      <b-button variant="success" size="sm" @click="setDraftOrder('Random')">Randomly</b-button>
      <label>Other Options:</label>
      <b-button variant="warning" size="sm" @click="showManualSettings = true">Manually</b-button>
    </div>

    <div v-show="showManualSettings">
      <label>Drag and drop to change order.</label>
      <div class="fluid container draft-order-editor bg-secondary">
        <draggable v-model="desiredDraftOrder" class="list-group" element="ul" :options="dragOptions" handle=".handle" @start="isDragging = true" @end="isDragging = false">
          <transition-group type="transition" :name="'flip-list'">
            <li v-for="publisher in desiredDraftOrder" :key="publisher.draftPosition" class="draft-order-item">
              <font-awesome-icon icon="bars" class="handle" />
              <span class="badge">{{ publisher.draftPosition }}</span>
              {{ publisher.publisherName }} ({{ publisher.playerName }})
            </li>
          </transition-group>
        </draggable>
      </div>

      <b-button class="confirm-button" variant="primary" size="sm" @click="setDraftOrder('Manual')">Confirm Draft Order</b-button>
    </div>
  </b-modal>
</template>

<script>
import axios from 'axios';
import draggable from 'vuedraggable';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  components: {
    draggable
  },
  mixins: [LeagueMixin],
  data() {
    return {
      showManualSettings: false,
      desiredDraftOrder: [],
      isDragging: false,
      delayedDragging: false,
      errorInfo: null
    };
  },
  computed: {
    dragOptions() {
      return {
        animation: 0,
        group: 'description',
        disabled: false
      };
    },
    hasPreviousYear() {
      return this.league.years.includes(this.leagueYear.year - 1);
    }
  },
  watch: {
    isDragging(newValue) {
      if (newValue) {
        this.delayedDragging = true;
        return;
      }
      this.$nextTick(() => {
        this.delayedDragging = false;
      });
    }
  },
  mounted() {
    this.clearData();
  },
  methods: {
    async setDraftOrder(draftOrderType) {
      var model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
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

.flip-list-move {
  transition: transform 0.5s;
}

.no-move {
  transition: transform 0s;
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
