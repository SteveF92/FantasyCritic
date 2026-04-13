<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="joinMessage" class="alert" :class="joinSuccess ? 'alert-success' : 'alert-danger'">{{ joinMessage }}</div>

    <div v-if="group">
      <div v-if="pendingInviteCode && isAuth && !isMember" class="alert alert-info">
        You've been invited to join this group!
        <b-button variant="primary" class="ml-2" @click="joinViaInviteLink">Join Group</b-button>
      </div>

      <div class="group-header bg-secondary">
        <h1>{{ group.groupName }}</h1>
        <h4>
          <template v-if="group.managerDisplayName">Managed by: {{ group.managerDisplayName }}</template>
        </h4>
        <h5>{{ group.memberCount }} members</h5>
      </div>

      <div v-if="group.leagueID" class="league-link-area">
        <router-link :to="{ name: 'league', params: { leagueid: group.leagueID, year: currentYear } }">View Linked League</router-link>
      </div>

      <div v-if="group.conferenceID" class="league-link-area">
        <router-link :to="{ name: 'conference', params: { conferenceid: group.conferenceID, year: currentYear } }">View Linked Conference</router-link>
      </div>

      <div v-if="activeQuarter" class="view-quarter-area">
        <b-button variant="primary" :to="{ name: 'royaleGroupQuarter', params: { groupid: groupid, year: activeQuarter.year, quarter: activeQuarter.quarter } }">
          View Current Quarter Standings ({{ activeQuarter.year }}-Q{{ activeQuarter.quarter }})
        </b-button>
      </div>

      <hr />

      <div v-if="isManager && group.groupType === 'Manual'" class="manager-actions">
        <h3>Manager Actions</h3>
        <b-button variant="primary" @click="createInviteLink" :disabled="inviteLinks && inviteLinks.filter((l) => l.active).length >= 2">Create Invite Link</b-button>

        <div v-if="inviteLinks && inviteLinks.length > 0" class="invite-links-section">
          <h5>Invite Links</h5>
          <div v-for="link in inviteLinks" :key="link.inviteID" class="invite-link-row">
            <template v-if="link.active">
              <code>{{ inviteLinkUrl(link.inviteCode) }}</code>
              <b-button size="sm" variant="outline-secondary" v-clipboard:copy="inviteLinkUrl(link.inviteCode)" v-clipboard:success="linkCopied">Copy</b-button>
              <b-button size="sm" variant="outline-danger" @click="deactivateLink(link.inviteID)">Deactivate</b-button>
            </template>
            <template v-else>
              <span class="text-muted">
                <s>{{ inviteLinkUrl(link.inviteCode) }}</s>
                (Deactivated)
              </span>
            </template>
          </div>
        </div>
        <hr />
      </div>

      <h3>Members</h3>
      <div v-if="members && members.length > 0">
        <b-table striped bordered small responsive :items="members" :fields="memberFields">
          <template #cell(displayName)="data">
            <router-link :to="{ name: 'royaleHistory', params: { userid: data.item.userID } }">{{ data.item.displayName }}</router-link>
          </template>
          <template #cell(actions)="data">
            <b-button v-if="isManager && group.groupType === 'Manual' && data.item.userID !== group.managerUserID" size="sm" variant="outline-danger" @click="removeMember(data.item.userID)">
              Remove
            </b-button>
          </template>
        </b-table>
      </div>
      <div v-else>
        <p>No members yet.</p>
      </div>

      <div v-if="isMemberNotManager && group.groupType === 'Manual'" class="leave-area">
        <b-button variant="outline-danger" @click="leaveGroup">Leave Group</b-button>
      </div>
    </div>
    <div v-else-if="notFound">
      <div class="alert alert-warning">Group not found.</div>
    </div>
    <div v-else class="spinner">
      <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
    </div>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  props: {
    groupid: { type: String, required: true }
  },
  data() {
    return {
      group: null,
      members: null,
      inviteLinks: null,
      activeQuarter: null,
      notFound: false,
      joinMessage: null,
      joinSuccess: false,
      displayNameField: { key: 'displayName', label: 'Player Name', thClass: 'bg-primary' },
      actionField: { key: 'actions', label: '', thClass: 'bg-primary' }
    };
  },
  computed: {
    memberFields() {
      if (this.isManager) {
        return [this.displayNameField, this.actionField];
      }
      return [this.displayNameField];
    },
    isManager() {
      return this.isAuth && this.group && this.group.managerUserID && this.$store.getters.userInfo.userID === this.group.managerUserID;
    },
    isMember() {
      if (!this.isAuth || !this.members) return false;
      return this.members.some((m) => m.userID === this.$store.getters.userInfo.userID);
    },
    isMemberNotManager() {
      if (!this.isAuth || !this.group || !this.members) return false;
      const uid = this.$store.getters.userInfo.userID;
      return uid !== this.group.managerUserID && this.members.some((m) => m.userID === uid);
    },
    pendingInviteCode() {
      return this.$route.query.inviteCode || null;
    },
    currentYear() {
      return new Date().getFullYear();
    }
  },
  watch: {
    async $route() {
      await this.fetchData();
    }
  },
  async created() {
    await this.fetchData();
  },
  methods: {
    async fetchData() {
      try {
        const [groupResponse, membersResponse, quarterResponse] = await Promise.all([
          axios.get(`/api/RoyaleGroup/GetRoyaleGroup/${this.groupid}`),
          axios.get(`/api/RoyaleGroup/GetRoyaleGroupMembers/${this.groupid}`),
          axios.get('/api/Royale/ActiveRoyaleQuarter')
        ]);
        this.group = groupResponse.data;
        this.members = membersResponse.data;
        this.activeQuarter = quarterResponse.data;

        if (this.isManager && this.group.groupType === 'Manual') {
          await this.fetchInviteLinks();
        }
      } catch {
        this.notFound = true;
      }
    },
    async fetchInviteLinks() {
      try {
        const response = await axios.get(`/api/RoyaleGroup/GetGroupInviteLinks/${this.groupid}`);
        this.inviteLinks = response.data;
      } catch {
        this.inviteLinks = [];
      }
    },
    inviteLinkUrl(inviteCode) {
      return `${window.location.origin}/royaleGroup/${this.groupid}?inviteCode=${inviteCode}`;
    },
    linkCopied() {
      this.$bvToast.toast('Invite link copied to clipboard!', { variant: 'success', solid: true, noCloseButton: true });
    },
    async createInviteLink() {
      await axios.post(`/api/RoyaleGroup/CreateGroupInviteLink/${this.groupid}`);
      await this.fetchInviteLinks();
    },
    async deactivateLink(inviteID) {
      await axios.post(`/api/RoyaleGroup/DeactivateGroupInviteLink/${inviteID}`);
      await this.fetchInviteLinks();
    },
    async removeMember(userID) {
      const confirmed = await this.$bvModal.msgBoxConfirm('Are you sure you want to remove this member?');
      if (!confirmed) return;
      await axios.post('/api/RoyaleGroup/RemoveMember', { groupID: this.groupid, userID });
      await this.fetchData();
    },
    async leaveGroup() {
      const confirmed = await this.$bvModal.msgBoxConfirm('Are you sure you want to leave this group?');
      if (!confirmed) return;
      await axios.post(`/api/RoyaleGroup/LeaveGroup/${this.groupid}`);
      this.$router.push({ name: 'criticsRoyale' });
    },
    async joinViaInviteLink() {
      try {
        await axios.post('/api/RoyaleGroup/JoinWithInviteLink', { inviteCode: this.pendingInviteCode });
        this.joinMessage = 'You have joined the group!';
        this.joinSuccess = true;
        this.$router.replace({ query: {} });
        await this.fetchData();
      } catch (error) {
        this.joinMessage = error.response?.data || 'Failed to join group.';
        this.joinSuccess = false;
      }
    }
  }
};
</script>

<style scoped>
.group-header {
  padding: 15px;
  border-radius: 5px;
  margin-bottom: 15px;
}

.view-quarter-area {
  margin: 10px 0;
}

.league-link-area {
  margin: 5px 0;
}

.manager-actions {
  margin: 15px 0;
}

.invite-links-section {
  margin-top: 10px;
}

.invite-link-row {
  margin: 5px 0;
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.leave-area {
  margin-top: 20px;
}

.spinner {
  display: flex;
  justify-content: space-around;
}
</style>
