<template>
  <b-modal id="assignPlayers" ref="assignPlayersRef" title="Assign Players" size="lg" @show="initialize" @hide="clearData">
    <div v-show="errorInfo" class="alert alert-danger">
      {{ errorInfo }}
    </div>
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
      <draggable :list="editableUnassignedPlayers" group="players">
        <div v-for="element in editableUnassignedPlayers" :key="element.userID" class="player-drag-item">
          <font-awesome-icon icon="bars" />
          {{ element.displayName }}
        </div>
      </draggable>
    </div>

    <div class="player-flex-container">
      <div v-for="(value, leagueID) in editableLeagueAssignments" :key="leagueID" class="player-flex-drag">
        <div class="player-drag-list bg-secondary">
          <h4 class="assign-league-header">
            <font-awesome-icon
              v-if="!leaguesLocked[leagueID]"
              icon="lock-open"
              size="lg"
              class="unlock-icon"
              @click="setLockStatus(leagueID, true)"
              v-b-popover.hover.focus.top="'Lock League (Enables Drafting)'" />
            <font-awesome-icon
              v-if="leaguesLocked[leagueID] && !leaguesDraftStarted[leagueID]"
              icon="lock"
              size="lg"
              class="lock-icon"
              @click="setLockStatus(leagueID, false)"
              v-b-popover.hover.focus.top="'Unlock league (Enables Player Re-Assignment)'" />
            <font-awesome-icon
              v-if="leaguesDraftStarted[leagueID]"
              icon="lock"
              size="lg"
              class="lock-icon draft-lock-icon"
              v-b-popover.hover.focus.rightbottom="'League has already started drafting'" />
            <span class="league-name-header">{{ leagueNames[leagueID] }}</span>
            <font-awesome-icon v-if="!leaguesCollapsed[leagueID]" icon="minus-circle" size="lg" class="collapse-icon" @click="collapseLeague(leagueID)" />
            <font-awesome-icon v-if="leaguesCollapsed[leagueID]" icon="plus-circle" size="lg" class="collapse-icon" @click="uncollapseLeague(leagueID)" />
          </h4>

          <template v-if="!leaguesCollapsed[leagueID]">
            <draggable v-if="!leaguesLocked[leagueID]" :list="value" group="players">
              <div v-for="element in value" :key="element.userID" class="player-drag-item">
                <font-awesome-icon icon="bars" />
                {{ element.displayName }}
              </div>
              <template #header></template>
            </draggable>
            <ol v-if="leaguesLocked[leagueID]">
              <li v-for="element in value" :key="element.userID">
                {{ element.displayName }}
              </li>
            </ol>
          </template>
        </div>
      </div>
    </div>
    <template #modal-footer>
      <b-button variant="warning" class="reset-button" @click="resetChanges">Reset Changes</b-button>
      <input type="submit" class="btn btn-primary" value="Assign Players to Leagues" @click="submitAssignments" />
    </template>
  </b-modal>
</template>
<script>
import axios from 'axios';
import draggable from 'vuedraggable';

import ConferenceMixin from '@/mixins/conferenceMixin.js';

export default {
  components: {
    draggable
  },
  mixins: [ConferenceMixin],
  data() {
    return {
      errorInfo: null,
      leagueNames: null,
      initialUnassignedPlayers: null,
      initialLeagueAssignments: null,
      editableUnassignedPlayers: null,
      editableLeagueAssignments: null,
      leaguesCollapsed: null,
      leaguesLocked: null,
      leaguesDraftsStarted: null
    };
  },
  created() {
    this.initialize();
  },
  methods: {
    initialize() {
      let unassignedPlayers = [];
      for (const player of this.conferenceYear.playersForYear) {
        if (player.leaguesActiveIn.length === 0) {
          unassignedPlayers.push(player);
        }
      }
      this.initialUnassignedPlayers = unassignedPlayers;

      let leagueAssignments = {};
      let leaguesCollapsed = {};
      let leagueNames = {};
      let leaguesLocked = {};
      let leaguesDraftStarted = {};
      for (const league of this.conference.leaguesInConference) {
        leagueAssignments[league.leagueID] = [];
        leagueNames[league.leagueID] = league.leagueName;
        leaguesCollapsed[league.leagueID] = false;
        leaguesLocked[league.leagueID] = league.conferenceLocked;
        leaguesDraftStarted[league.leagueID] = league.draftStarted;
      }

      for (const leagueYear of this.conferenceYear.leagueYears) {
        leaguesLocked[leagueYear.leagueID] = leagueYear.conferenceLocked;
        leaguesDraftStarted[leagueYear.leagueID] = leagueYear.draftStarted;
      }

      for (const player of this.conferenceYear.playersForYear) {
        for (const leagueIn of player.leaguesActiveIn) {
          leagueAssignments[leagueIn].push(player);
        }
      }

      this.leagueNames = leagueNames;
      this.initialLeagueAssignments = leagueAssignments;
      this.leaguesCollapsed = leaguesCollapsed;
      this.leaguesLocked = leaguesLocked;
      this.leaguesDraftStarted = leaguesDraftStarted;

      this.editableUnassignedPlayers = structuredClone(this.initialUnassignedPlayers);
      this.editableLeagueAssignments = structuredClone(this.initialLeagueAssignments);
    },
    collapseLeague(leagueID) {
      this.leaguesCollapsed[leagueID] = true;
    },
    uncollapseLeague(leagueID) {
      this.leaguesCollapsed[leagueID] = false;
    },
    async setLockStatus(leagueID, locked) {
      const model = {
        conferenceID: this.conference.conferenceID,
        year: this.conferenceYear.year,
        leagueID: leagueID,
        locked: locked
      };
      try {
        await axios.post('/api/conference/SetConferenceLeagueLockStatus', model);
        this.leaguesLocked[leagueID] = locked;
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    resetChanges() {
      this.editableUnassignedPlayers = structuredClone(this.initialUnassignedPlayers);
      this.editableLeagueAssignments = structuredClone(this.initialLeagueAssignments);
    },
    clearData() {
      this.errorInfo = null;
      this.resetChanges();
    },
    async submitAssignments() {
      let finalLeagueAssignments = {};
      Object.entries(this.editableLeagueAssignments).forEach(([key, value]) => {
        finalLeagueAssignments[key] = value.map((item) => item.userID);
      });

      const model = {
        conferenceID: this.conference.conferenceID,
        year: this.conferenceYear.year,
        leagueAssignments: finalLeagueAssignments
      };

      try {
        await axios.post('/api/conference/AssignLeaguePlayers', model);
        this.$refs.assignPlayersRef.hide();
        await this.notifyAction('Player Assignments have been updated.');
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    }
  }
};
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
