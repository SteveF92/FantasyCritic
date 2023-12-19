<template>
  <div>
    <b-dropdown id="conference-manager-actions" text="Manager Actions" class="m-md-2">
      <b-dropdown-item v-b-modal="'invitePlayers'">Invite Players</b-dropdown-item>
      <b-dropdown-item v-b-modal="'assignPlayers'">Assign Players to Leagues</b-dropdown-item>
      <b-dropdown-item v-b-modal="'addLeagueToConference'">Add League to Conference</b-dropdown-item>
      <b-dropdown-item disabled>Post New Manager Message</b-dropdown-item>
      <b-dropdown-divider></b-dropdown-divider>
      <b-dropdown-item v-b-modal="'editConferenceSettings'">Edit Conference Settings</b-dropdown-item>
      <b-dropdown-item v-b-modal="'promoteNewConferenceManager'">Promote New Conference Manager</b-dropdown-item>
      <b-dropdown-item v-b-modal="'reassignLeagueManager'">Reassign League Managers</b-dropdown-item>
    </b-dropdown>
    <div>
      <AddLeagueToConference></AddLeagueToConference>
      <AssignPlayers></AssignPlayers>
      <EditConferenceSettings></EditConferenceSettings>
      <InvitePlayers></InvitePlayers>
      <PromoteNewConferenceManager></PromoteNewConferenceManager>
      <ReassignLeagueManager></ReassignLeagueManager>
    </div>
  </div>
</template>
<script>
import AddLeagueToConference from '@/components/modals/conference/addLeagueToConference.vue';
import AssignPlayers from '@/components/modals/conference/assignPlayers.vue';
import EditConferenceSettings from '@/components/modals/conference/editConferenceSettings.vue';
import InvitePlayers from '@/components/modals/conference/invitePlayers.vue';
import PromoteNewConferenceManager from '@/components/modals/conference/promoteNewConferenceManager.vue';
import ReassignLeagueManager from '@/components/modals/conference/reassignLeagueManager.vue';

export default {
  components: {
    AddLeagueToConference,
    AssignPlayers,
    EditConferenceSettings,
    InvitePlayers,
    PromoteNewConferenceManager,
    ReassignLeagueManager
  }
};
</script>
