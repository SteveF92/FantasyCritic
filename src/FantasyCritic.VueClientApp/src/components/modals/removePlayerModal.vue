<template>
  <b-modal id="remove-player-modal" ref="removePlayerModalRef" title="Remove a Player From the League" @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>

    <div class="alert alert-info">
      This option will let you remove a player from the league, provided they have no publishers, either in the current year, or in previous years.
      <ul>
        <li>If you are starting a new year, and there is a player who doesn't wish to return, use "Manage Active Players".</li>
        <li>If you are in the middle of a year and need to remove a player, use "Disconnect a Player".</li>
      </ul>

      This particular feature is most useful for situations such as a player not showing up to the draft. If they never played at all, they can be removed here.
    </div>

    <div class="form-group">
      <label for="playerToRemove" class="control-label">Player</label>
      <b-form-select id="playerToRemove" v-model="playerToRemove">
        <option v-for="player in removablePlayers" :key="player.userID" :value="player">{{ player.displayName }}</option>
      </b-form-select>
    </div>

    <template #footer>
      <input type="submit" class="btn btn-danger" value="Remove Player" :disabled="!playerToRemove || isBusy" @click="removePlayer" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueManagerClient, type PlayerViewModel, type RemovePlayerFromLeagueRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface RemovePlayerModalData {
  playerToRemove: PlayerViewModel | null;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): RemovePlayerModalData {
    return {
      playerToRemove: null,
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeague']),
    removablePlayers(): PlayerViewModel[] {
      return (this.requiredLeague.players ?? []).filter((x) => x.userID !== this.requiredLeague.leagueManager.userID);
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async removePlayer(): Promise<void> {
      const player = this.playerToRemove;
      if (!player) {
        return;
      }

      const request: RemovePlayerFromLeagueRequest = {
        leagueID: this.requiredLeague.leagueID,
        userID: player.userID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.removePlayerFromLeague(request);

        (this.$refs.removePlayerModalRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction(`${player.displayName} has been removed from the league.`);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when removing the player.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.playerToRemove = null;
      this.errorInfo = '';
    }
  }
});
</script>
