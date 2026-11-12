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
        <b-table striped bordered small responsive v-model:sort-by="sortBy" :items="members" :fields="memberFields">
          <template #cell(displayName)="data">
            <router-link :to="{ name: 'royaleHistory', params: { userid: data.item.userID } }">{{ data.item.displayName }}</router-link>
          </template>
          <template #cell(averageRankWithinGroup)="data">
            <template v-if="data.item.averageRankWithinGroup != null">{{ formatAverageRank(data.item.averageRankWithinGroup) }}</template>
            <template v-else>--</template>
          </template>
          <template #cell(averageRankOverall)="data">
            <template v-if="data.item.averageRankOverall != null">{{ formatAverageRank(data.item.averageRankOverall) }}</template>
            <template v-else>--</template>
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

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import type { BTableSortBy, TableField } from 'bootstrap-vue-next';

import {
  royaleGroupClient,
  type JoinRoyaleGroupWithInviteLinkRequest,
  type RemoveRoyaleGroupMemberRequest,
  type RoyaleGroupMemberListItemViewModel,
  type RoyaleGroupViewModel,
  type RoyaleYearQuarterViewModel
} from '@/api/clients';
import { numericComparer } from '@/globalFunctions';
import { useAuthStore } from '@/store/authStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { useConfirm } from '@/utilities/confirm';
import RoyaleGroupInviteLinksModal from '@/components/modals/royaleGroupInviteLinksModal.vue';

interface RoyaleGroupData {
  group: RoyaleGroupViewModel | null;
  members: RoyaleGroupMemberListItemViewModel[] | null;
  activeQuarter: RoyaleYearQuarterViewModel | null;
  notFound: boolean;
  joinMessage: string | null;
  joinSuccess: boolean;
  sortBy: BTableSortBy[];
}

export default defineComponent({
  setup() {
    const displayNameField: TableField<RoyaleGroupMemberListItemViewModel> = { key: 'displayName', label: 'Player Name', thClass: 'bg-primary' };
    const quartersParticipatedField: TableField<RoyaleGroupMemberListItemViewModel> = {
      key: 'quartersParticipated',
      label: 'Quarters Played',
      thClass: 'bg-primary',
      tdClass: 'text-right',
      sortable: true
    };
    const averageRankField: TableField<RoyaleGroupMemberListItemViewModel> = {
      key: 'averageRankWithinGroup',
      label: 'Avg. Rank (Within Group)',
      thClass: 'bg-primary',
      tdClass: 'text-right',
      sortable: true,
      sortCompare: numericComparer
    };
    const averageRankOverallField: TableField<RoyaleGroupMemberListItemViewModel> = {
      key: 'averageRankOverall',
      label: 'Avg. Rank (Overall)',
      thClass: 'bg-primary',
      tdClass: 'text-right',
      sortable: true,
      sortCompare: numericComparer
    };
    const totalPointsField: TableField<RoyaleGroupMemberListItemViewModel> = {
      key: 'totalPoints',
      label: 'Total Points',
      thClass: 'bg-primary',
      tdClass: 'text-right',
      sortable: true,
      sortCompare: numericComparer
    };
    const actionField: TableField<RoyaleGroupMemberListItemViewModel> = { key: 'actions', label: 'Actions', thClass: 'bg-primary', thStyle: 'width: 1%' };

    return {
      confirm: useConfirm(),
      displayNameField,
      quartersParticipatedField,
      averageRankField,
      averageRankOverallField,
      totalPointsField,
      actionField
    };
  },
  components: { RoyaleGroupInviteLinksModal },
  props: {
    groupid: { type: String, required: true }
  },
  data(): RoyaleGroupData {
    return {
      group: null,
      members: null,
      activeQuarter: null,
      notFound: false,
      joinMessage: null,
      joinSuccess: false,
      sortBy: [{ key: 'averageRankWithinGroup', order: 'asc' }]
    };
  },
  computed: {
    ...mapState(useAuthStore, ['isAuth']),
    memberFields(): TableField<RoyaleGroupMemberListItemViewModel>[] {
      const statsFields = [this.displayNameField, this.quartersParticipatedField, this.averageRankField, this.averageRankOverallField, this.totalPointsField];
      if (this.isManager && this.showManagerActions) {
        return [...statsFields, this.actionField];
      }
      return statsFields;
    },
    isManager(): boolean {
      const authStore = useAuthStore();
      return this.isAuth && this.group !== null && !!this.group.managerUserID && authStore.userInfo?.userID === this.group.managerUserID;
    },
    showManagerActions(): boolean {
      return this.group?.groupType === 'Manual';
    },
    isMember(): boolean {
      if (!this.isAuth || !this.members) return false;
      const authStore = useAuthStore();
      return this.members.some((m) => m.userID === authStore.userInfo?.userID);
    },
    isMemberNotManager(): boolean {
      if (!this.isAuth || !this.group || !this.members) return false;
      const authStore = useAuthStore();
      const uid = authStore.userInfo?.userID;
      return uid !== this.group.managerUserID && this.members.some((m) => m.userID === uid);
    },
    pendingInviteCode(): string | null {
      const inviteCode = this.$route.query.inviteCode;
      if (typeof inviteCode !== 'string') {
        return null;
      }
      return inviteCode || null;
    },
    currentYear(): number {
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
    formatAverageRank(value: number): string {
      const n = Number(value);
      if (Number.isNaN(n)) return '--';
      return n.toFixed(2);
    },
    async fetchData(): Promise<void> {
      try {
        const response = await royaleGroupClient.getRoyaleGroupData(this.groupid);
        this.group = response.group;
        this.members = response.members;
        this.activeQuarter = response.activeQuarter;
      } catch {
        this.notFound = true;
      }
    },
    async removeMember(userID: string): Promise<void> {
      const confirmed = await this.confirm('Are you sure you want to remove this member?');
      if (!confirmed) return;
      const request: RemoveRoyaleGroupMemberRequest = { groupID: this.groupid, userID };
      await royaleGroupClient.removeMember(request);
      await this.fetchData();
    },
    async leaveGroup(): Promise<void> {
      const confirmed = await this.confirm('Are you sure you want to leave this group?');
      if (!confirmed) return;
      await royaleGroupClient.leaveGroup(this.groupid);
      this.$router.push({ name: 'criticsRoyale' });
    },
    async joinViaInviteLink(): Promise<void> {
      if (!this.pendingInviteCode) {
        return;
      }

      const request: JoinRoyaleGroupWithInviteLinkRequest = { inviteCode: this.pendingInviteCode };
      try {
        await royaleGroupClient.joinWithInviteLink(request);
        this.joinMessage = 'You have joined the group!';
        this.joinSuccess = true;
        this.$router.replace({ query: {} });
        await this.fetchData();
      } catch (error) {
        this.joinMessage = getApiErrorMessage(error, 'Failed to join group.');
        this.joinSuccess = false;
      }
    }
  }
});
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
