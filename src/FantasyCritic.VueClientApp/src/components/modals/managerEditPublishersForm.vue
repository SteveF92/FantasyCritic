<template>
  <b-modal id="manager-edit-publishers-form" ref="managerEditPublishersFormRef" size="lg" title="Edit Publishers" no-footer @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-warning">Warning! This feature is intended to fix mistakes and other exceptional circumstances. In general, managers should not be editing Publisher details.</div>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="editPublisher" class="control-label">Publisher to Edit</label>
        <b-form-select id="editPublisher" v-model="editPublisher" @update:model-value="selectPublisher()">
          <option v-for="publisher in requiredLeagueYear.publishers" :key="publisher.publisherID" :value="publisher">
            {{ publisher.publisherName }}
          </option>
        </b-form-select>
      </div>
      <div v-if="editPublisher">
        <div class="form-group">
          <label for="newPublisherName" class="control-label">Publisher Name</label>
          <input id="newPublisherName" v-model="newPublisherName" name="newPublisherName" type="text" class="form-control input" />
        </div>
        <div class="form-group">
          <label for="newBudget" class="control-label">Budget</label>
          <input id="newBudget" v-model="newBudget" name="newBudget" type="number" class="form-control input" />
        </div>
        <div class="form-group">
          <label for="newUnrestrictedReleaseStatusGamesDropped" class="control-label">"Any Unreleased" Games Dropped</label>
          <input
            id="newUnrestrictedReleaseStatusGamesDropped"
            v-model="newUnrestrictedReleaseStatusGamesDropped"
            name="newUnrestrictedReleaseStatusGamesDropped"
            type="number"
            class="form-control input" />
        </div>
        <div class="form-group">
          <label for="newWillNotReleaseGamesDropped" class="control-label">Will not Release Games Dropped</label>
          <input id="newWillNotReleaseGamesDropped" v-model="newWillNotReleaseGamesDropped" name="newWillNotReleaseGamesDropped" type="number" class="form-control input" />
        </div>
        <div class="form-group">
          <label for="newWillReleaseGamesDropped" class="control-label">Will Release Games Dropped</label>
          <input id="newWillReleaseGamesDropped" v-model="newWillReleaseGamesDropped" name="newWillReleaseGamesDropped" type="number" class="form-control input" />
        </div>
        <div class="form-group">
          <label for="newSuperDropsAvailable" class="control-label">Super Drops Available</label>
          <input id="newSuperDropsAvailable" v-model="newSuperDropsAvailable" name="newSuperDropsAvailable" type="number" class="form-control input" />
        </div>

        <div>
          <input type="submit" class="btn btn-primary modal-submit-button" value="Edit Publisher" :disabled="isBusy" @click="makeEditRequest" />
        </div>
      </div>
    </div>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueManagerClient, type PublisherEditRequest, type PublisherViewModel } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface ManagerEditPublishersFormData {
  editPublisher: PublisherViewModel | null;
  newPublisherName: string;
  newBudget: number;
  newUnrestrictedReleaseStatusGamesDropped: number;
  newWillNotReleaseGamesDropped: number;
  newWillReleaseGamesDropped: number;
  newSuperDropsAvailable: number;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear'])
  },
  data(): ManagerEditPublishersFormData {
    return {
      editPublisher: null,
      newPublisherName: '',
      newBudget: 0,
      newUnrestrictedReleaseStatusGamesDropped: 0,
      newWillNotReleaseGamesDropped: 0,
      newWillReleaseGamesDropped: 0,
      newSuperDropsAvailable: 0,
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    selectPublisher(): void {
      const selectedPublisher = this.editPublisher;
      if (!selectedPublisher) {
        return;
      }

      this.newPublisherName = selectedPublisher.publisherName;
      this.newBudget = selectedPublisher.budget;
      this.newUnrestrictedReleaseStatusGamesDropped = selectedPublisher.unrestrictedReleaseStatusGamesDropped;
      this.newWillNotReleaseGamesDropped = selectedPublisher.willNotReleaseGamesDropped;
      this.newWillReleaseGamesDropped = selectedPublisher.willReleaseGamesDropped;
      this.newSuperDropsAvailable = selectedPublisher.superDropsAvailable;
      this.errorInfo = '';
    },
    async makeEditRequest(): Promise<void> {
      if (!this.editPublisher) {
        return;
      }

      const request: PublisherEditRequest = {
        publisherID: this.editPublisher.publisherID,
        publisherName: this.newPublisherName,
        budget: this.newBudget,
        unrestrictedReleaseStatusGamesDropped: this.newUnrestrictedReleaseStatusGamesDropped,
        willNotReleaseGamesDropped: this.newWillNotReleaseGamesDropped,
        willReleaseGamesDropped: this.newWillReleaseGamesDropped,
        superDropsAvailable: this.newSuperDropsAvailable
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.editPublisher(request);

        (this.$refs.managerEditPublishersFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('Publisher has been edited.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when editing the publisher.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.editPublisher = null;
      this.errorInfo = '';
    }
  }
});
</script>
