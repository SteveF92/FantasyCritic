<template>
  <div>
    <b-row class="text-well table-controls">
      <b-col sm="12" md="6" class="my-1">
        <b-form-group label="Filter"
                      label-for="filter-input"
                      label-cols-sm="3"
                      label-align-sm="right"
                      label-size="sm"
                      class="mb-0">
          <b-input-group size="sm">
            <b-form-input id="filter-input"
                          v-model="filter"
                          type="search"
                          placeholder="Type to Search"></b-form-input>
          </b-input-group>
        </b-form-group>
      </b-col>

      <b-col sm="12" md="6" class="my-1">
        <b-form-group v-model="sortDirection"
                      label="Filter On"
                      description="Leave all unchecked to filter on all data"
                      label-cols-sm="3"
                      label-align-sm="right"
                      label-size="sm"
                      class="mb-0"
                      v-slot="{ ariaDescribedby }">
          <b-form-checkbox-group v-model="filterOn"
                                 :aria-describedby="ariaDescribedby"
                                 class="mt-1">
            <b-form-checkbox value="gameName">Name</b-form-checkbox>
            <b-form-checkbox value="estimatedReleaseDate">Release Date</b-form-checkbox>
            <b-form-checkbox value="tags">Tags</b-form-checkbox>
          </b-form-checkbox-group>
        </b-form-group>
      </b-col>

      <b-col sm="12" md="6" class="my-1">
        <b-form-group label="Sort"
                      label-for="sort-by-select"
                      label-cols-sm="3"
                      label-align-sm="right"
                      label-size="sm"
                      class="mb-0"
                      v-slot="{ ariaDescribedby }">
          <b-input-group size="sm">
            <b-form-select id="sort-by-select"
                           v-model="sortBy"
                           :options="sortOptions"
                           :aria-describedby="ariaDescribedby"
                           class="w-75">
              <template #first>
                <option value="">-- none --</option>
              </template>
            </b-form-select>

            <b-form-select v-model="sortDesc"
                           :disabled="!sortBy"
                           :aria-describedby="ariaDescribedby"
                           size="sm"
                           class="w-25">
              <option :value="false">Asc</option>
              <option :value="true">Desc</option>
            </b-form-select>
          </b-input-group>
        </b-form-group>
      </b-col>

      <b-col sm="12" md="6" class="my-1">
        <b-form-group label="Per page"
                      label-for="per-page-select"
                      label-cols-sm="6"
                      label-cols-md="4"
                      label-cols-lg="3"
                      label-align-sm="right"
                      label-size="sm"
                      class="mb-0">
          <b-form-select id="per-page-select"
                         v-model="perPage"
                         :options="pageOptions"
                         size="sm"></b-form-select>
        </b-form-group>
      </b-col>

      <b-col sm="12" md="6" offset-md="6" lg="4" offset-lg="8" xl="3" offset-xl="9" class="my-1">
        <b-pagination v-model="currentPage"
                      :total-rows="totalRows"
                      :per-page="perPage"
                      align="fill"
                      size="sm"
                      class="my-0 pagination-dark"></b-pagination>
      </b-col>
    </b-row>
    
    <b-table :sort-by.sync="sortBy"
             :sort-desc.sync="sortDesc"
             :items="gameRows"
             :fields="gameFields"
             :filter="filter"
             :filter-included-fields="filterOn"
             bordered
             :small="tableIsSmall"
             responsive
             :per-page="perPage"
             :current-page="currentPage"
             striped
             @filtered="onFiltered">
      <template v-slot:cell(gameName)="data">
        <masterGamePopover :masterGame="data.item"></masterGamePopover>
      </template>
      <template v-slot:cell(maximumReleaseDate)="data">
        {{getReleaseDate(data.item)}}
      </template>
      <template v-slot:cell(criticScore)="data">
        <a v-if="data.item.openCriticID && data.item.criticScore" :href="openCriticLink(data.item)" target="_blank"><strong>OpenCritic <font-awesome-icon icon="external-link-alt" /></strong></a>
        <span v-else>--</span>
      </template>
      <template v-slot:cell(dateAdjustedHypeFactor)="data">
        {{data.item.dateAdjustedHypeFactor | score(1)}}
      </template>
      <template v-slot:cell(projectedOrRealFantasyPoints)="data">
        <template v-if="data.item.isReleased || !data.item.willRelease">
          {{data.item.projectedOrRealFantasyPoints | score(1)}}
        </template>
        <template v-else>
          <em>~{{data.item.projectedOrRealFantasyPoints | score(1)}}</em>
        </template>
      </template>
      <template v-slot:cell(eligiblePercentStandardGame)="data">
        {{data.item.eligiblePercentStandardGame | percent(1)}}
      </template>
      <template v-slot:cell(adjustedPercentCounterPick)="data">
        <span v-if="data.item.adjustedPercentCounterPick !== null">
          {{data.item.adjustedPercentCounterPick | percent(1)}}
        </span>
        <span v-else>
          N/A
        </span>
      </template>
      <template v-slot:cell(addedTimestamp)="data">
        {{data.item.addedTimestamp | date}}
      </template>
      <template v-slot:cell(tags)="data">
        <span v-for="(tag, index) in data.item.tags">
          <masterGameTagBadge :tagName="data.item.tags[index]" short="true"></masterGameTagBadge>
        </span>
      </template>
    </b-table>
    <b-pagination v-model="currentPage"
                  :total-rows="totalRows"
                  :per-page="perPage"
                  align="fill"
                  size="sm"
                  class="my-0 pagination-dark"></b-pagination>
  </div>
</template>
<script>
import Vue from 'vue';
import axios from 'axios';
import moment from 'moment';
import MasterGamePopover from '@/components/modules/masterGamePopover';
import MasterGamesTable from '@/components/modules/gameTables/masterGamesTable';
import MasterGameTagBadge from '@/components/modules/masterGameTagBadge';

export default {
  props: ['masterGames'],
  data() {
    return {
      gameFields: [
        { key: 'gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        { key: 'criticScore', label: 'Critic Score Link', thClass: ['bg-primary', 'md-screen-minimum'], tdClass: 'md-screen-minimum' },
        { key: 'dateAdjustedHypeFactor', label: 'Hype Factor', sortable: true, thClass: 'bg-primary' },
        { key: 'projectedOrRealFantasyPoints', label: 'Points', sortable: true, thClass: 'bg-primary' },
        { key: 'eligiblePercentStandardGame', label: '% Picked', sortable: true, thClass: ['bg-primary', 'md-screen-minimum'], tdClass: 'md-screen-minimum' },
        { key: 'adjustedPercentCounterPick', label: '% Counter Picked', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
        { key: 'tags', label: 'Tags', thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
        { key: 'addedTimestamp', label: 'Date Added', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' }
      ],
      totalRows: 1,
      currentPage: 1,
      perPage: 50,
      pageOptions: [10, 50, 100, 1000],
      sortBy: 'dateAdjustedHypeFactor',
      sortDesc: true,
      sortDirection: 'desc',
      filter: null,
      filterOn: [],
    };
  },
  components: {
    MasterGamePopover,
    MasterGamesTable,
    MasterGameTagBadge
  },
  computed: {
    sortOptions() {
      // Create an options list from our fields
      return this.gameFields
        .filter(f => f.sortable)
        .map(f => {
          return { text: f.label, value: f.key }
        })
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
      for (var i = 0; i < gameRows.length; ++i) {
        if (gameRows[i].error) {
          gameRows[i]._rowVariant = 'danger';
        }
      }
      return gameRows;
    }
  },
  methods: {
    getReleaseDate(game) {
      if (game.releaseDate) {
        return moment(game.releaseDate).format('YYYY-MM-DD');
      }
      return game.estimatedReleaseDate + ' (Estimated)';
    },
    openCriticLink(game) {
      return 'https://opencritic.com/game/' + game.openCriticID + '/a';
    },
    onFiltered(filteredItems) {
      // Trigger pagination to update the number of buttons/pages due to filtering
      this.totalRows = filteredItems.length;
      this.currentPage = 1;
    }
  },
  mounted() {
    this.totalRows = this.masterGames.length;
  },
};
</script>
<style scoped>
  .table-controls{
    margin-bottom: 15px;
  }
</style>
