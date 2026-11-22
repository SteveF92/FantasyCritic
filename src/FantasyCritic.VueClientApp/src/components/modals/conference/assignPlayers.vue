<template>
  <b-modal id="assignPlayers" ref="assignPlayersRef" title="Assign Players" size="lg" @show="initialize" @hide="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-info">
      <p class="text-white">
        This form allows you assign each of your conference's players to a league. If you want a player to be in more than one league, you'll need to do that manually by inviting them to the league
        directly, using the normal (non-conference) invite system.
      </p>
      <p class="text-white">
        If you are getting errors with this form, try using the direct league invite system, as that should handle more complex cases. We're still working out the bugs in the conference system.
      </p>
    </div>

    <div class="player-drag-list bg-secondary">
      <h4 class="player-header">Unassigned Players</h4>
      <VueDraggable v-model="unassignedPlayers" group="players">
        <div v-for="element in unassignedPlayers" :key="element.userID" class="player-drag-item">
          <font-awesome-icon icon="bars" />
          {{ element.displayName }}
        </div>
      </VueDraggable>
    </div>

    <div class="player-flex-container">
      <div v-for="league in leagues" :key="league.leagueID" class="player-flex-drag">
        <div class="player-drag-list bg-secondary">
          <h4 class="assign-league-header">
            <button
              v-if="!league.locked"
              type="button"
              class="font-awesome-button unlock-icon"
              :aria-label="`Lock ${league.leagueName}`"
              :disabled="isBusy"
              v-b-popover.hover.focus.top="'Lock League (Enables Drafting)'"
              @click="setLockStatus(league, true)">
              <font-awesome-icon icon="lock-open" size="lg" />
            </button>
            <button
              v-if="league.locked && !league.draftStarted"
              type="button"
              class="font-awesome-button lock-icon"
              :aria-label="`Unlock ${league.leagueName}`"
              :disabled="isBusy"
              v-b-popover.hover.focus.top="'Unlock league (Enables Player Re-Assignment)'"
              @click="setLockStatus(league, false)">
              <font-awesome-icon icon="lock" size="lg" />
            </button>
            <font-awesome-icon v-if="league.draftStarted" icon="lock" size="lg" class="lock-icon draft-lock-icon" v-b-popover.hover.focus.right="'League has already started drafting'" />
            <span class="league-name-header">{{ league.leagueName }}</span>
            <button
              type="button"
              class="font-awesome-button collapse-icon"
              :aria-label="league.collapsed ? `Expand ${league.leagueName}` : `Collapse ${league.leagueName}`"
              @click="league.collapsed = !league.collapsed">
              <font-awesome-icon :icon="league.collapsed ? 'plus-circle' : 'minus-circle'" size="lg" />
            </button>
          </h4>

          <template v-if="!league.collapsed">
            <VueDraggable v-if="!league.locked" v-model="league.players" group="players">
              <div v-for="element in league.players" :key="element.userID" class="player-drag-item">
                <font-awesome-icon icon="bars" />
                {{ element.displayName }}
              </div>
            </VueDraggable>
            <ol v-else>
              <li v-for="element in league.players" :key="element.userID">
                {{ element.displayName }}
              </li>
            </ol>
          </template>
        </div>
      </div>
    </div>
    <template #footer>
      <b-button variant="warning" class="reset-button" @click="resetChanges">Reset Changes</b-button>
      <input type="submit" class="btn btn-primary" value="Assign Players to Leagues" :disabled="isBusy" @click="submitAssignments" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';
import { VueDraggable } from 'vue-draggable-plus';

import { conferenceClient, type AssignLeaguePlayersRequest, type ConferenceYearPlayerViewModel, type SetConferenceLeagueLockStatusRequest } from '@/api/clients';
import { useConferenceStore } from '@/store/conferenceStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface LeagueAssignment {
  leagueID: string;
  leagueName: string;
  locked: boolean;
  draftStarted: boolean;
  collapsed: boolean;
  players: ConferenceYearPlayerViewModel[];
}

interface AssignPlayersData {
  unassignedPlayers: ConferenceYearPlayerViewModel[];
  leagues: LeagueAssignment[];
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  components: {
    VueDraggable
  },
  computed: {
    ...mapState(useConferenceStore, ['requiredConference', 'requiredConferenceYear'])
  },
  data(): AssignPlayersData {
    return {
      unassignedPlayers: [],
      leagues: [],
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    ...mapActions(useConferenceStore, ['notifyAction', 'refreshConferenceYear']),
    initialize(): void {
      this.leagues = this.requiredConferenceYear.leagueYears.map((leagueYear) => ({
        leagueID: leagueYear.leagueID,
        leagueName: leagueYear.leagueName,
        locked: leagueYear.conferenceLocked,
        draftStarted: leagueYear.draftStarted,
        collapsed: false,
        players: []
      }));
      this.resetChanges();
      this.errorInfo = '';
    },
    resetChanges(): void {
      // The lists are new arrays, so dragging never reorders the store's players.
      const playersForYear = this.requiredConferenceYear.playersForYear;
      this.unassignedPlayers = playersForYear.filter((player) => player.leaguesActiveIn.length === 0);
      for (const league of this.leagues) {
        league.players = playersForYear.filter((player) => player.leaguesActiveIn.includes(league.leagueID));
      }
    },
    async setLockStatus(league: LeagueAssignment, locked: boolean): Promise<void> {
      const request: SetConferenceLeagueLockStatusRequest = {
        conferenceID: this.requiredConference.conferenceID,
        year: this.requiredConferenceYear.year,
        leagueID: league.leagueID,
        locked
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await conferenceClient.setConferenceLeagueLockStatus(request);
        league.locked = locked;
        await this.refreshConferenceYear();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when changing the lock status.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.errorInfo = '';
      this.resetChanges();
    },
    async submitAssignments(): Promise<void> {
      const leagueAssignments: Record<string, string[]> = {};
      for (const league of this.leagues) {
        leagueAssignments[league.leagueID] = league.players.map((player) => player.userID);
      }

      const request: AssignLeaguePlayersRequest = {
        conferenceID: this.requiredConference.conferenceID,
        year: this.requiredConferenceYear.year,
        leagueAssignments
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await conferenceClient.assignLeaguePlayers(request);

        (this.$refs.assignPlayersRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('Player Assignments have been updated.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when assigning the players.');
      } finally {
        this.isBusy = false;
      }
    }
  }
});
</script>

<style>
.reset-button-flex {
  display: flex;
  justify-content: space-between;
  margin-bottom: 10px;
}

.player-flex-container {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
}
.player-flex-drag {
  flex: 1 1 calc(50% - 10px); /* 50% width for 2 items with some spacing */
  margin: 5px; /* Adjust margin as needed for spacing */
  box-sizing: border-box; /* Include padding and border in the width */
  max-width: 350px;
}

.player-drag-list {
  border-radius: 10px;
  padding: 5px;
}
.player-drag-item {
  margin: 10px;
  position: relative;
  display: bcollapse;
  padding: 10px 15px;
  margin-bottom: -1px;
  background-color: #5b6977 !important;
  border: 1px solid #ddd;
}

.assign-league-header {
  display: flex;
  justify-content: space-between;
}

.league-name-header {
  padding-left: 10px;
  font-size: 20px;
  font-weight: bold;
  color: #d6993a;
}

.unlock-icon {
  margin-left: 5px;
  margin-top: 5px;
}

.lock-icon {
  margin-left: 5px;
  margin-top: 5px;
  margin-right: 12px;
}

.draft-lock-icon {
  color: #888888;
}

.collapse-icon {
  margin-left: 5px;
  margin-right: 10px;
  margin-top: 5px;
}
</style>
