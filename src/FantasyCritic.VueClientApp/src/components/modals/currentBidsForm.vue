<template>
  <b-modal id="currentBidsForm" ref="currentBidsFormRef" size="lg" title="My Current Bids" no-footer @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <label v-show="settingPriority">Drag and drop to change order.</label>

    <b-form-checkbox v-model="settingPriority" class="priority-checkbox">
      <span class="checkbox-label">Change Priorities</span>
    </b-form-checkbox>

    <b-table-simple responsive bordered>
      <b-thead>
        <b-tr class="bg-primary">
          <b-th v-show="settingPriority"></b-th>
          <b-th class="game-column">Game</b-th>
          <b-th>Bid Amount</b-th>
          <b-th>Priority</b-th>
          <b-th>Conditional Drop</b-th>
          <b-th>Counter Pick Bid?</b-th>
          <b-th>Fail if no eligible slot?</b-th>
          <b-th v-show="!settingPriority">Edit</b-th>
          <b-th v-show="!settingPriority">Cancel</b-th>
        </b-tr>
      </b-thead>
      <VueDraggable v-if="settingPriority" v-model="desiredBidPriorities" tag="tbody" handle=".handle">
        <b-tr v-for="bid in desiredBidPriorities" :key="bid.bidID">
          <b-td class="handle"><font-awesome-icon icon="bars" size="lg" /></b-td>
          <b-td>{{ bid.masterGame.gameName }}</b-td>
          <b-td>{{ formatCurrency(bid.bidAmount, 0) }}</b-td>
          <b-td>{{ bid.priority }}</b-td>
          <b-td v-if="bid.conditionalDropPublisherGame">{{ bid.conditionalDropPublisherGame.gameName }}</b-td>
          <b-td v-else>None</b-td>
          <b-td>{{ formatYesNo(bid.counterPick) }}</b-td>
          <b-td>{{ formatYesNo(!bid.allowIneligibleSlot) }}</b-td>
        </b-tr>
      </VueDraggable>
      <b-tbody v-if="!settingPriority">
        <b-tr v-for="bid in currentBids" :key="bid.bidID">
          <b-td>{{ bid.masterGame.gameName }}</b-td>
          <b-td>{{ formatCurrency(bid.bidAmount, 0) }}</b-td>
          <b-td>{{ bid.priority }}</b-td>
          <b-td v-if="bid.conditionalDropPublisherGame">{{ bid.conditionalDropPublisherGame.gameName }}</b-td>
          <b-td v-else>None</b-td>
          <b-td>{{ formatYesNo(bid.counterPick) }}</b-td>
          <b-td>{{ formatYesNo(!bid.allowIneligibleSlot) }}</b-td>
          <b-td class="select-cell">
            <b-button variant="info" size="sm" @click="startEditingBid(bid)">Edit</b-button>
          </b-td>
          <b-td class="select-cell">
            <b-button variant="danger" size="sm" @click="confirmCancelBid(bid)">Cancel</b-button>
          </b-td>
        </b-tr>
      </b-tbody>
    </b-table-simple>
    <b-button v-if="settingPriority" variant="primary" class="full-width-button" @click="setBidPriorityOrder()">Set Priority Order</b-button>
    <div v-if="bidBeingEdited">
      <h3 for="bidBeingEdited" class="selected-game text-black">Edit Bid:</h3>
      <masterGameSummary :master-game="bidBeingEdited.masterGame"></masterGameSummary>
      <div class="form-group">
        <label for="bidAmount" class="control-label">Bid Amount (Remaining: {{ formatCurrency(requiredUserPublisher.budget, 0) }})</label>

        <ValidationField v-slot="{ errors }" :model-value="bidAmount" name="bidAmount" rules="required|integer">
          <input id="bidAmount" v-model="bidAmount" name="bidAmount" type="number" class="form-control input" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationField>
      </div>
      <div v-if="!bidBeingEdited.counterPick" class="form-group">
        <label for="conditionalDrop" class="control-label">Conditional Drop</label>
        <b-form-select v-model="conditionalDrop">
          <template #first>
            <option :value="null">None</option>
          </template>
          <option v-for="publisherGame in droppableGames" :key="publisherGame.publisherGameID" :value="publisherGame">
            {{ publisherGame.gameName }}
          </option>
        </b-form-select>
        <div v-if="!editHasBidSlotAcquisitionPath" class="alert alert-warning" role="alert">
          You have no open roster spots. Add a conditional drop to this bid or place a pending drop request before saving.
        </div>
      </div>
      <div v-if="requiredLeagueYear.settings.hasSpecialSlots" class="plain-check">
        <input v-model="allowIneligibleSlot" class="plain-check-input override-checkbox" type="checkbox" />
        <label class="plain-check-label">
          Allow bid to succeed even if there are no slots this game is eligible in.
          <font-awesome-icon v-b-popover.hover.focus="allowIneligibleText" icon="info-circle" />
        </label>
      </div>
      <b-button variant="primary" class="full-width-button" :disabled="isBusy || !editHasBidSlotAcquisitionPath" @click="editBid">Edit Bid</b-button>
      <div v-if="editBidResult && !editBidResult.success" class="alert alert-danger bid-error">
        <h3 class="alert-heading">Error!</h3>
        <ul>
          <li v-for="error in editBidResult.errors" :key="error">{{ error }}</li>
        </ul>
      </div>
    </div>
    <b-modal id="confirm-cancel-bid-modal" ref="confirmCancelBidModalRef" @ok="cancelBid">
      <!-- prettier-ignore -->
      <p>
        Are you sure you want to cancel your bid for
        <strong>{{ bidToCancel?.masterGame.gameName }}</strong>?
      </p>
    </b-modal>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';
import { VueDraggable } from 'vue-draggable-plus';

import {
  leagueClient,
  type BidPriorityOrderRequest,
  type PickupBidDeleteRequest,
  type PickupBidEditRequest,
  type PickupBidResultViewModel,
  type PickupBidViewModel,
  type PublisherGameViewModel
} from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import type { PopoverContent } from '@/types/vueTypes';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatCurrency, formatYesNo } from '@/utilities/formatters';
import MasterGameSummary from '@/components/masterGameSummary.vue';

interface CurrentBidsFormData {
  desiredBidPriorities: PickupBidViewModel[];
  settingPriority: boolean;
  errorInfo: string;
  bidBeingEdited: PickupBidViewModel | null;
  bidAmount: number | null;
  conditionalDrop: PublisherGameViewModel | null;
  editBidResult: PickupBidResultViewModel | null;
  isBusy: boolean;
  allowIneligibleSlot: boolean;
  bidToCancel: PickupBidViewModel | null;
}

export default defineComponent({
  setup() {
    const allowIneligibleText: PopoverContent = {
      title: 'What does this mean?',
      body:
        'If you do not check this box, if you win the bidding for this game but do not have a slot open that this game is eligible in, then the bid will fail. ' +
        'If you check this box, then the bid would succeed, but the game will land in an ineligible slot.'
    };

    return { allowIneligibleText };
  },
  components: {
    VueDraggable,
    MasterGameSummary
  },
  data(): CurrentBidsFormData {
    return {
      desiredBidPriorities: [],
      settingPriority: false,
      errorInfo: '',
      bidBeingEdited: null,
      bidAmount: null,
      conditionalDrop: null,
      editBidResult: null,
      isBusy: false,
      allowIneligibleSlot: false,
      bidToCancel: null
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear', 'requiredUserPublisher', 'currentBids', 'currentDrops']),
    droppableGames(): PublisherGameViewModel[] {
      return this.requiredUserPublisher.games.filter((x) => !x.counterPick);
    },
    editHasBidSlotAcquisitionPath(): boolean {
      if (!this.bidBeingEdited || this.bidBeingEdited.counterPick) {
        return true;
      }
      const standardGameSlots = this.requiredLeagueYear.settings.standardGames;
      const userStandardGames = this.requiredUserPublisher.games.filter((x) => !x.counterPick);
      if (userStandardGames.length < standardGameSlots) {
        return true;
      }
      if (this.conditionalDrop) {
        return true;
      }
      return this.currentDrops.length > 0;
    }
  },
  watch: {
    settingPriority() {
      this.desiredBidPriorities = this.currentBids;
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    formatCurrency,
    formatYesNo,
    async setBidPriorityOrder(): Promise<void> {
      const request: BidPriorityOrderRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        bidPriorities: this.desiredBidPriorities.map((x) => x.bidID)
      };

      this.isBusy = true;
      try {
        await leagueClient.setBidPriorities(request);
        this.notifyAction('Bid priority has been changed.');
        this.settingPriority = false;
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to change bid priorites.');
      } finally {
        this.isBusy = false;
      }
    },
    startEditingBid(bid: PickupBidViewModel): void {
      this.bidBeingEdited = bid;
      this.bidAmount = bid.bidAmount;
      this.conditionalDrop = bid.conditionalDropPublisherGame ?? null;
      this.allowIneligibleSlot = bid.allowIneligibleSlot;
    },
    async editBid(): Promise<void> {
      if (!this.bidBeingEdited) {
        throw new Error('Attempted to edit a bid with none selected.');
      }

      if (typeof this.bidAmount !== 'number') {
        throw new Error('No valid bid amount entered on an edit request.');
      }

      const request: PickupBidEditRequest = {
        bidID: this.bidBeingEdited.bidID,
        publisherID: this.requiredUserPublisher.publisherID,
        bidAmount: this.bidAmount,
        allowIneligibleSlot: this.allowIneligibleSlot
      };

      if (this.conditionalDrop) {
        request.conditionalDropPublisherGameID = this.conditionalDrop.publisherGameID;
      }

      this.isBusy = true;
      try {
        this.editBidResult = await leagueClient.editPickupBid(request);
        if (!this.editBidResult.success) {
          return;
        }
        await this.notifyAction('Bid for ' + this.bidBeingEdited.masterGame.gameName + ' for $' + this.bidAmount + ' was made.');
        this.clearData();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to change bid priorites.');
      } finally {
        this.isBusy = false;
      }
    },
    confirmCancelBid(bid: PickupBidViewModel) {
      this.bidToCancel = bid;
      (this.$refs.confirmCancelBidModalRef as InstanceType<typeof BModal> | undefined)?.show();
    },
    async cancelBid(): Promise<void> {
      if (!this.bidToCancel) {
        throw new Error('Attempted to cancel a bid with none selected.');
      }

      const request: PickupBidDeleteRequest = {
        bidID: this.bidToCancel.bidID,
        publisherID: this.requiredUserPublisher.publisherID
      };

      this.isBusy = true;
      try {
        await leagueClient.deletePickupBid(request);
        this.notifyAction('Bid for ' + this.bidToCancel.masterGame.gameName + ' for $' + this.bidToCancel.bidAmount + ' was canceled.');
        this.clearData();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to cancel bid.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.desiredBidPriorities = this.currentBids;
      this.bidBeingEdited = null;
      this.conditionalDrop = null;
      this.bidAmount = null;
      this.errorInfo = '';
      this.bidToCancel = null;
    }
  }
});
</script>

<style scoped>
.select-cell {
  text-align: center;
}
.priority-checkbox {
  float: right;
}
</style>
