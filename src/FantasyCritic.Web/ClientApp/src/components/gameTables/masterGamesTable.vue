<template>
  <div>
    <b-row class="text-well table-controls">
      <b-col sm="12" md="6" class="my-1">
        <b-form-group label="Filter" label-for="filter-input" label-cols-sm="3" label-align-sm="right" label-size="sm" class="mb-0">
          <b-input-group size="sm">
            <b-form-input id="filter-input" v-model="filter" type="search" placeholder="Type to Search"></b-form-input>
          </b-input-group>
        </b-form-group>
      </b-col>

      <b-col sm="12" md="6" class="my-1">
        <b-form-group v-slot="{ ariaDescribedby }" label="Filter On" description="Leave all unchecked to filter on all data" label-cols-sm="3" label-align-sm="right" label-size="sm" class="mb-0">
          <b-form-checkbox-group v-model="filterOn" :aria-describedby="ariaDescribedby" class="mt-1">
            <b-form-checkbox value="gameName">Name</b-form-checkbox>
            <b-form-checkbox value="estimatedReleaseDate">Release Date</b-form-checkbox>
            <b-form-checkbox value="readableTags">Tags</b-form-checkbox>
          </b-form-checkbox-group>
        </b-form-group>
      </b-col>

      <b-col sm="12" md="6" class="my-1">
        <b-form-group v-slot="{ ariaDescribedby }" label="Sort" label-for="sort-by-select" label-cols-sm="3" label-align-sm="right" label-size="sm" class="mb-0">
          <b-input-group size="sm">
            <b-form-select id="sort-by-select" v-model="sortBy" :options="sortOptions" :aria-describedby="ariaDescribedby" class="w-75">
              <template #first>
                <option value="">-- none --</option>
              </template>
            </b-form-select>

            <b-form-select v-model="sortDesc" :disabled="!sortBy" :aria-describedby="ariaDescribedby" size="sm" class="w-25">
              <option :value="false">Asc</option>
              <option :value="true">Desc</option>
            </b-form-select>
          </b-input-group>
        </b-form-group>
      </b-col>

      <b-col sm="12" md="6" class="my-1">
        <b-form-group label="Per page" label-for="per-page-select" label-cols-sm="6" label-cols-md="4" label-cols-lg="3" label-align-sm="right" label-size="sm" class="mb-0">
          <b-form-select id="per-page-select" v-model="perPage" :options="pageOptions" size="sm"></b-form-select>
        </b-form-group>
      </b-col>

      <b-col sm="12" md="6" offset-md="6" lg="4" offset-lg="8" xl="3" offset-xl="9" class="my-1">
        <b-pagination v-model="currentPage" :total-rows="totalRows" :per-page="perPage" align="fill" size="sm" class="my-0 pagination-dark"></b-pagination>
      </b-col>
    </b-row>

    <b-table
      :sort-by.sync="sortBy"
      :sort-desc.sync="sortDesc"
      :items="gameRows"
      :fields="gameFields"
      :filter="filter"
      :filter-function="filterGames"
      bordered
      :small="tableIsSmall"
      responsive
      :per-page="perPage"
      :current-page="currentPage"
      striped
      primary-key="masterGameID"
      sticky-header="1000px"
      @filtered="onFiltered">
      <template #cell(gameName)="data">
        <masterGamePopover :master-game="data.item"></masterGamePopover>
      </template>
      <template #cell(maximumReleaseDate)="data">
        {{ getReleaseDate(data.item) }}
      </template>
      <template #cell(criticScore)="data">
        <a v-if="data.item.openCriticID && data.item.criticScore" :href="openCriticLink(data.item)" target="_blank">
          <strong>
            OpenCritic
            <font-awesome-icon icon="external-link-alt" />
          </strong>
        </a>
        <span v-else>--</span>
      </template>
      <template #cell(dateAdjustedHypeFactor)="data">
        <span v-if="showYearStats">{{ data.item.dateAdjustedHypeFactor | score(1) }}</span>
        <span v-if="!showYearStats">--</span>
      </template>
      <template #cell(projectedOrRealFantasyPoints)="data">
        <span v-if="data.item.fantasyPoints !== null">{{ data.item.fantasyPoints | score(1) }}</span>
        <span v-if="data.item.fantasyPoints === null && showYearStats" class="projected-text">~{{ data.item.projectedFantasyPoints | score(1) }}</span>
        <span v-if="data.item.fantasyPoints === null && !showYearStats" class="projected-text">--</span>
      </template>
      <template #cell(eligiblePercentStandardGame)="data">
        <span v-if="showYearStats">{{ data.item.eligiblePercentStandardGame | percent(1) }}</span>
        <span v-if="!showYearStats">--</span>
      </template>
      <template #cell(adjustedPercentCounterPick)="data">
        <template v-if="showYearStats">
          <span v-if="data.item.adjustedPercentCounterPick !== null">
            {{ data.item.adjustedPercentCounterPick | percent(1) }}
          </span>
          <span v-else>N/A</span>
        </template>
        <span v-if="!showYearStats">--</span>
      </template>
      <template #cell(addedTimestamp)="data">
        {{ data.item.addedTimestamp | date }}
      </template>
      <template #cell(tags)="data">
        <span v-for="tag in data.item.tags" :key="tag">
          <masterGameTagBadge :tag-name="tag" short></masterGameTagBadge>
        </span>
      </template>
    </b-table>
    <b-pagination v-model="currentPage" :total-rows="totalRows" :per-page="perPage" align="fill" size="sm" class="my-0 pagination-dark"></b-pagination>
  </div>
</template>
<script>
import { formatMasterGameReleaseDate } from '@/globalFunctions';
import MasterGamePopover from '@/components/masterGamePopover.vue';
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

export default {
  components: {
    MasterGamePopover,
    MasterGameTagBadge
  },
  props: {
    masterGames: { type: Array, required: true },
    showYearStats: { type: Boolean, required: true }
  },
  data() {
    return {
      gameFields: [
        { key: 'gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        { key: 'criticScore', label: 'Critic Score Link', thClass: ['bg-primary', 'md-screen-minimum', 'position-relative'], tdClass: 'md-screen-minimum' },
        { key: 'dateAdjustedHypeFactor', label: 'Hype Factor', sortable: true, thClass: 'bg-primary' },
        { key: 'projectedOrRealFantasyPoints', label: 'Points', sortable: true, thClass: 'bg-primary' },
        { key: 'eligiblePercentStandardGame', label: '% Picked', sortable: true, thClass: ['bg-primary', 'md-screen-minimum'], tdClass: 'md-screen-minimum' },
        { key: 'adjustedPercentCounterPick', label: '% Counter Picked', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
        { key: 'tags', label: 'Tags', thClass: ['bg-primary', 'lg-screen-minimum', 'position-relative'], tdClass: 'lg-screen-minimum' },
        { key: 'addedTimestamp', label: 'Date Added', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' }
      ],
      totalRows: 1,
      currentPage: 1,
      perPage: 50,
      pageOptions: [10, 50, 100, 1000],
      sortBy: 'dateAdjustedHypeFactor',
      sortDesc: true,
      filter: null,
      filterOn: []
    };
  },
  computed: {
    sortOptions() {
      // Create an options list from our fields
      return this.gameFields
        .filter((f) => f.sortable)
        .map((f) => {
          return { text: f.label, value: f.key };
        });
    },
    tableIsSmall() {
      if (window.innerWidth < 500) {
        return true;
      }

      return false;
    },
    gameRows() {
      let gameRows = this.masterGames;
      if (!gameRows) {
        return [];
      }
      for (let i = 0; i < gameRows.length; ++i) {
        if (gameRows[i].error) {
          gameRows[i]._rowVariant = 'danger';
        }
      }
      return gameRows;
    }
  },
  created() {
    this.totalRows = this.masterGames.length;
  },
  methods: {
    getReleaseDate(game) {
      return formatMasterGameReleaseDate(game);
    },
    openCriticLink(game) {
      return `https://opencritic.com/game/${game.openCriticID}/${game.openCriticSlug ?? 'b'}`;
    },
    onFiltered(filteredItems) {
      // Trigger pagination to update the number of buttons/pages due to filtering
      this.totalRows = filteredItems.length;
      this.currentPage = 1;
    },
    filterGames(item) {
      if (this.filterOn.length === 0 || this.filterOn.includes('gameName')) {
        let normalizedFilter = this.filter
          .normalize('NFD')
          .replace(/[\u0300-\u036f]/g, '')
          .toLowerCase();
        let normalizedGameName = item.gameName
          .normalize('NFD')
          .replace(/[\u0300-\u036f]/g, '')
          .toLowerCase();
        if (normalizedGameName.includes(normalizedFilter)) {
          return true;
        }
      }

      if (this.filterOn.length === 0 || this.filterOn.includes('estimatedReleaseDate')) {
        if (item.estimatedReleaseDate.toLowerCase().includes(this.filter.toLowerCase())) {
          return true;
        }
      }

      if (this.filterOn.length === 0 || this.filterOn.includes('readableTags')) {
        let allTags = item.readableTags.join().toLowerCase();
        if (allTags.includes(this.filter.toLowerCase())) {
          return true;
        }
      }

      return false;
    }
  }
};
</script>
<style scoped>
.table-controls {
  margin-bottom: 15px;
}
</style>
