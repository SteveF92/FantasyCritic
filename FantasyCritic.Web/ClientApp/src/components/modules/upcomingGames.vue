<template>
  <div v-if="upcomingGames && upcomingGames.length > 0">
    <b-table :sort-by.sync="sortBy"
              :sort-desc.sync="sortDesc"
              :items="upcomingGames"
              :fields="upcomingGamesFields"
              bordered
              striped
              responsive
              small>
      <template v-slot:cell(gameName)="data">
        <masterGamePopover :masterGame="data.item.masterGame"></masterGamePopover>
      </template>
      <template v-slot:cell(maximumReleaseDate)="data">
        {{getReleaseDate(data.item)}}
      </template>
      <template v-slot:cell(league)="data">
        <router-link v-if="data.item.leagueID" :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: data.item.year }}">{{data.item.leagueName}}</router-link>
        <span v-else>{{data.item.leagueName}}</span>
      </template>
      <template v-slot:cell(publisher)="data">
        <span v-if="!data.item.counterPickPublisherID">
          <router-link :to="{ name: 'publisher', params: { publisherid: data.item.publisherID }}">{{ data.item.publisherName }}</router-link>
        </span>
        <span v-else>
          <router-link :to="{ name: 'publisher', params: { publisherid: data.item.publisherID }}">{{ data.item.publisherName }}</router-link>
          - Counter Picked by:
          <router-link :to="{ name: 'publisher', params: { publisherid: data.item.counterPickPublisherID }}">{{ data.item.counterPickPublisherName }}</router-link>
        </span>
      </template>
    </b-table>
  </div>
</template>
<script>
import moment from 'moment';
import MasterGamePopover from '@/components/modules/masterGamePopover';

export default {
  props: ['upcomingGames', 'mode'],
  data() {
    return {
      sortBy: 'maximumReleaseDate',
      sortDesc: false,
      baseUpcomingGamesFields: [
        { key: 'gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
      ],
      userUpcomingGamesFields: [
        { key: 'league', label: 'League', sortable: true, thClass: ['bg-primary'] },
      ],
      leagueUpcomingGamesFields: [
        { key: 'publisher', label: 'Publisher', sortable: true, thClass: ['bg-primary'] },
      ],
    };
  },
  computed: {
    upcomingGamesFields() {
      if (this.mode === 'user') {
        return this.baseUpcomingGamesFields.concat(this.userUpcomingGamesFields);
      } else if (this.mode === 'league') {
        return this.baseUpcomingGamesFields.concat(this.leagueUpcomingGamesFields);
      }

      return this.baseUpcomingGamesFields;
    }
  },
  components: {
    MasterGamePopover,
  },
  methods: {
    getReleaseDate(game) {
      if (game.releaseDate) {
        return moment(game.releaseDate).format('YYYY-MM-DD');
      }
      return game.estimatedReleaseDate + ' (Estimated)';
    }
  }
};
</script>
