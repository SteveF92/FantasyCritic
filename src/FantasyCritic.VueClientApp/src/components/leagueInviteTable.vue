<template>
  <b-table :items="invites" :fields="inviteFields" thead-class="hidden_header" bordered striped>
    <template #cell(leagueName)="data">
      <div class="row-flex">
        <div class="league-icon-area">
          <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: data.item.activeYear } }" class="league-icon">
            <font-awesome-icon icon="envelope" size="2x" />
          </router-link>
        </div>
        <div>
          <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: data.item.activeYear } }" class="league-link">
            {{ data.item.leagueName }}
          </router-link>
          <div class="league-detail">Manager: {{ data.item.leagueManager.displayName }}</div>
        </div>
      </div>
    </template>
  </b-table>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import type { TableField } from 'bootstrap-vue-next';

import type { CompleteLeagueInviteViewModel } from '@/api/clients';

export default defineComponent({
  setup() {
    const inviteFields: TableField<CompleteLeagueInviteViewModel>[] = [{ key: 'leagueName' }];
    return { inviteFields };
  },
  props: {
    invites: { type: Array as PropType<CompleteLeagueInviteViewModel[]>, required: true }
  }
});
</script>

<style scoped>
table :deep(.hidden_header) {
  display: none;
}

.row-flex {
  display: flex;
}

.league-icon-area {
  display: flex;
  align-items: center;
  justify-content: center;
}

.league-icon {
  width: 40px;
  font-size: 20px;
  margin-right: 21px;
  margin-left: 2px;
}

.league-link {
  font-weight: bold;
  margin-bottom: 0;
  padding-bottom: 0;
}

.league-detail {
  margin-top: 0;
  padding-top: 0;
  font-size: 14px;
}
</style>
