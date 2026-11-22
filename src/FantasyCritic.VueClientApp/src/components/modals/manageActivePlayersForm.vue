<template>
  <b-modal id="manage-active-players-form" ref="manageActivePlayersFormRef" title="Manage Active Players" @show="setData">
    <h4 class="text-black">Active Players for {{ requiredLeagueYear.year }}</h4>
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <table class="table table-bordered table-striped table-sm">
      <thead>
        <tr class="bg-primary">
          <th scope="col">User</th>
          <th scope="col">Active?</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="player in playerActiveStatus" :key="player.userID">
          <td>{{ player.displayName }}</td>
          <td>
            <input v-model="player.active" type="checkbox" :aria-label="`${player.displayName} is active`" />
          </td>
        </tr>
      </tbody>
    </table>
    <template #footer>
      <input type="submit" class="btn btn-primary" value="Set Active Players" :disabled="isBusy" @click="confirmActivePlayers" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueManagerClient, type LeaguePlayerActiveRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface PlayerActiveStatus {
  userID: string;
  displayName: string;
  active: boolean;
}

interface ManageActivePlayersFormData {
  playerActiveStatus: PlayerActiveStatus[];
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear'])
  },
  data(): ManageActivePlayersFormData {
    return {
      playerActiveStatus: [],
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    setData(): void {
      const activeUserIDs = this.requiredLeagueYear.players.flatMap((x) => (x.user ? [x.user.userID] : []));
      this.playerActiveStatus = (this.requiredLeagueYear.league.players ?? []).map((player) => ({
        userID: player.userID,
        displayName: player.displayName,
        active: activeUserIDs.includes(player.userID)
      }));
      this.errorInfo = '';
    },
    async confirmActivePlayers(): Promise<void> {
      const activeStatus: Record<string, boolean> = {};
      for (const player of this.playerActiveStatus) {
        activeStatus[player.userID] = player.active;
      }

      const request: LeaguePlayerActiveRequest = {
        leagueID: this.requiredLeagueYear.league.leagueID,
        year: this.requiredLeagueYear.year,
        activeStatus
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.setPlayerActiveStatus(request);

        (this.$refs.manageActivePlayersFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('Active players were changed.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when setting the active players.');
      } finally {
        this.isBusy = false;
      }
    }
  }
});
</script>

<style scoped>
.text-black {
  color: black !important;
}
</style>
