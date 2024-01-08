<template>
  <b-modal id="assignPlayers" ref="assignPlayersRef" title="Assign Players" size="lg" @show="initialize" @hide="clearData">
    <div v-show="errorInfo" class="alert alert-danger">
      {{ errorInfo }}
    </div>
    <div class="alert alert-info">
      This form allows you assign each of your conference's players to a league. If you want a player to be in more than one league, you'll need to do that manually by inviting them to the league
      directly, using the normal (non-conference) invite system.
    </div>

    <draggable class="player-drag-list bg-secondary" :list="editableUnassignedPlayers" group="players">
      <div v-for="element in editableUnassignedPlayers" :key="element.userID" class="player-drag-item">
        <font-awesome-icon icon="bars" />
        {{ element.displayName }}
      </div>
      <template #header>
        <span class="player-header">Unassigned Players</span>
      </template>
    </draggable>

    <div class="player-flex-container">
      <div v-for="(value, leagueID) in editableLeagueAssignments" :key="leagueID" class="player-flex-drag">
        <draggable class="player-drag-list bg-secondary" :list="value" group="players">
          <div v-for="element in value" :key="element.userID" class="player-drag-item">
            <font-awesome-icon icon="bars" />
            {{ element.displayName }}
          </div>
          <template #header>
            <div class="assign-league-header">
              <span class="player-header">{{ leagueNames[leagueID] }}</span>

              <font-awesome-icon v-if="!leaguesLocked[leagueID]" icon="lock-open" size="lg" class="lock-icon" @click="lockLeague(leagueID)" />
              <font-awesome-icon v-if="leaguesLocked[leagueID]" icon="lock" size="lg" class="lock-icon" @click="unlockLeague(leagueID)" />
            </div>
          </template>
        </draggable>
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
import _ from 'lodash';
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
      leaguesLocked: null
    };
  },
  created() {
    this.initialize();
  },
  methods: {
    initialize() {
      let unassignedPlayers = [];
      for (const player of this.conference.players) {
        if (player.leaguesIn.length === 0) {
          unassignedPlayers.push(player);
        }
      }
      this.initialUnassignedPlayers = unassignedPlayers;

      let leagueAssignments = {};
      let leaguesLocked = {};
      let leagueNames = {};
      for (const league of this.conference.leaguesInConference) {
        leagueAssignments[league.leagueID] = [];
        leagueNames[league.leagueID] = league.leagueName;
        leaguesLocked[league.leagueID] = false;
      }

      for (const player of this.conference.players) {
        for (const leagueIn of player.leaguesIn) {
          leagueAssignments[leagueIn].push(player);
        }
      }

      this.leagueNames = leagueNames;
      this.initialLeagueAssignments = leagueAssignments;
      this.leaguesLocked = leaguesLocked;

      this.editableUnassignedPlayers = _.cloneDeep(this.initialUnassignedPlayers);
      this.editableLeagueAssignments = _.cloneDeep(this.initialLeagueAssignments);
    },
    lockLeague(leagueID) {
      this.leaguesLocked[leagueID] = true;
    },
    unlockLeague(leagueID) {
      this.leaguesLocked[leagueID] = false;
    },
    resetChanges() {
      this.editableUnassignedPlayers = _.cloneDeep(this.initialUnassignedPlayers);
      this.editableLeagueAssignments = _.cloneDeep(this.initialLeagueAssignments);
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
  display: block;
  padding: 10px 15px;
  margin-bottom: -1px;
  background-color: #5b6977 !important;
  border: 1px solid #ddd;
}

.assign-league-header {
  display: flex;
  justify-content: space-between;
}

.player-header {
  padding-left: 10px;
  font-size: 20px;
  font-weight: bold;
  color: #d6993a;
}

.lock-icon {
  padding-right: 10px;
  padding-top: 5px;
}
</style>
