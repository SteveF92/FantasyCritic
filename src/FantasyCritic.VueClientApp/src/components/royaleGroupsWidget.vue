<template>
  <b-card header-class="header">
    <template #header>
      <h2>Royale Groups</h2>
    </template>

    <b-tabs pills class="royale-groups-tabs">
      <b-tab v-if="isAuth" title="My Groups" title-item-class="tab-header">
        <b-table v-if="myGroups && myGroups.length > 0" :items="myGroups" :fields="groupFields" thead-class="hidden_header" bordered striped responsive small class="royale-groups-table">
          <template #cell(groupName)="data">
            <router-link :to="groupQuarterLink(data.item.groupID)" class="group-link">{{ data.item.groupName }}</router-link>
            <div class="group-detail text-white">{{ data.item.memberCount }} members</div>
          </template>
        </b-table>
        <div v-else-if="myGroups" class="text-muted small">No groups yet.</div>
        <b-button v-b-modal="'createRoyaleGroupModal'" variant="primary" size="sm" class="mb-2">Create Group</b-button>
      </b-tab>

      <b-tab title="Featured" title-item-class="tab-header">
        <b-table
          v-if="rulesBasedGroups && rulesBasedGroups.length > 0"
          :items="rulesBasedGroups"
          :fields="groupFields"
          thead-class="hidden_header"
          bordered
          striped
          responsive
          small
          class="royale-groups-table">
          <template #cell(groupName)="data">
            <router-link :to="groupQuarterLink(data.item.groupID)" class="group-link">{{ data.item.groupName }}</router-link>
            <div class="group-detail text-muted">{{ data.item.memberCount }} members</div>
          </template>
        </b-table>
        <div v-else-if="rulesBasedGroups" class="text-muted small">No featured groups.</div>
      </b-tab>

      <b-tab title="Search" title-item-class="tab-header">
        <b-input-group class="mb-2">
          <b-form-input
            :model-value="groupSearchQuery"
            placeholder="Search groups by name..."
            aria-label="Search groups by name"
            @update:model-value="onSearchInput"
            @keyup.enter="emitSearchRequest"></b-form-input>
          <b-button variant="primary" @click="emitSearchRequest">Search</b-button>
        </b-input-group>

        <b-table
          v-if="groupSearchResults && groupSearchResults.length > 0"
          :items="groupSearchResults"
          :fields="groupFields"
          thead-class="hidden_header"
          bordered
          striped
          responsive
          small
          class="royale-groups-table">
          <template #cell(groupName)="data">
            <router-link :to="groupQuarterLink(data.item.groupID)" class="group-link">{{ data.item.groupName }}</router-link>
            <div class="group-detail text-muted">{{ data.item.memberCount }} members &middot; {{ data.item.groupType }}</div>
          </template>
        </b-table>
        <div v-if="groupSearchQuery && groupSearchQuery.trim().length >= 2 && groupSearchResults && groupSearchResults.length === 0" class="text-muted small">No groups found.</div>
      </b-tab>
    </b-tabs>
  </b-card>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import { mapState } from 'pinia';
import type { RouteLocationRaw } from 'vue-router';
import type { TableField } from 'bootstrap-vue-next';

import type { RoyaleGroupViewModel } from '@/api/clients';
import { useAuthStore } from '@/store/authStore';

export default defineComponent({
  setup() {
    const groupFields: TableField<RoyaleGroupViewModel>[] = [{ key: 'groupName', label: '' }];

    return { groupFields };
  },
  props: {
    myGroups: { type: Array as PropType<RoyaleGroupViewModel[] | null>, default: null },
    rulesBasedGroups: { type: Array as PropType<RoyaleGroupViewModel[] | null>, default: null },
    year: { type: Number, required: true },
    quarter: { type: Number, required: true },
    groupSearchQuery: { type: String, default: '' },
    groupSearchResults: { type: Array as PropType<RoyaleGroupViewModel[] | null>, default: null }
  },
  emits: {
    'search-query-change': (_value: string) => true,
    'search-request': () => true
  },
  computed: {
    ...mapState(useAuthStore, ['isAuth'])
  },
  methods: {
    groupQuarterLink(groupID: string): RouteLocationRaw {
      return { name: 'royaleGroupQuarter', params: { groupid: groupID, year: this.year, quarter: this.quarter } };
    },
    onSearchInput(value: string | number | null): void {
      this.$emit('search-query-change', String(value ?? ''));
    },
    emitSearchRequest(): void {
      this.$emit('search-request');
    }
  }
});
</script>

<style scoped>
:deep(.header) {
  margin-bottom: 0;
  padding-bottom: 0;
}

.header h2 {
  font-size: 25px;
  margin-bottom: 0;
  padding-bottom: 0;
}

.group-link {
  font-weight: bold;
}

.group-detail {
  font-size: 13px;
  margin-top: 2px;
}

.royale-groups-table {
  max-height: 420px;
}

div :deep(.hidden_header) {
  display: none;
}

div :deep(.tab-header) {
  margin-bottom: 5px;
}

div :deep(.tab-header .nav-link) {
  border-radius: 0px;
  font-weight: bolder;
  color: white;
}
</style>
