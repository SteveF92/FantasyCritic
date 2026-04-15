<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="joinMessage" class="alert" :class="joinSuccess ? 'alert-success' : 'alert-danger'">{{ joinMessage }}</div>

    <div v-if="group">
      <div v-if="pendingInviteCode && isAuth && !isMember" class="alert alert-info">
        You've been invited to join this group!
        <b-button variant="primary" class="ml-2" @click="joinViaInviteLink">Join Group</b-button>
      </div>

      <div class="publisher-header bg-secondary">
        <h1 class="publisher-name">{{ group.groupName }}</h1>
        <h4 v-if="group.managerDisplayName">Managed by: {{ group.managerDisplayName }}</h4>
        <h4>{{ group.memberCount }} members</h4>
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
        <b-button variant="primary" v-b-modal.royaleGroupInviteLinksModal>Invite Links</b-button>
        <royale-group-invite-links-modal :groupid="groupid" />
        <hr />
      </div>

      <h3>Members</h3>
      <div v-if="members && members.length > 0">
        <b-table striped bordered small responsive :items="members" :fields="memberFields">
          <template #cell(displayName)="data">
            <router-link :to="{ name: 'royaleHistory', params: { userid: data.item.userID } }">{{ data.item.displayName }}</router-link>
          </template>
          <template #cell(actions)="data">
            <b-button v-if="data.item.userID !== group.managerUserID" size="sm" variant="danger" @click="removeMember(data.item.userID)" class="remove-member-button">Remove</b-button>
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
import RoyaleGroupInviteLinksModal from '@/components/modals/royaleGroupInviteLinksModal.vue';

export default {
  components: { RoyaleGroupInviteLinksModal },
  props: {
    groupid: { type: String, required: true }
  },
  data() {
    return {
      group: null,
      members: null,
      activeQuarter: null,
      notFound: false,
      joinMessage: null,
      joinSuccess: false,
      displayNameField: { key: 'displayName', label: 'Player Name', thClass: 'bg-primary' },
      actionField: { key: 'actions', label: 'Actions', thClass: 'bg-primary', thStyle: 'width: 1%' }
    };
  },
  computed: {
    memberFields() {
      if (this.isManager && this.showManagerActions) {
        return [this.displayNameField, this.actionField];
      }
      return [this.displayNameField];
    },
    isManager() {
      return this.isAuth && this.group && this.group.managerUserID && this.$store.getters.userInfo.userID === this.group.managerUserID;
    },
    showManagerActions() {
      return this.group.groupType === 'Manual';
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
      } catch {
        this.notFound = true;
      }
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
.publisher-header {
  margin-top: 10px;
  border: 2px;
  border-color: #d6993a;
  border-style: solid;
  padding-left: 5px;
}

.publisher-name {
  display: block;
  max-width: 100%;
  word-wrap: break-word;
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

.leave-area {
  margin-top: 20px;
}

.spinner {
  display: flex;
  justify-content: space-around;
}

.remove-member-button {
  width: 80px;
}
</style>
