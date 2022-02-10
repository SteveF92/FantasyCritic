<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div v-if="forbidden">
        <div class="alert alert-danger" role="alert">
          You do not have permission to view this league.
        </div>
      </div>
      
      <div v-if="league">
        <h1>League History: {{league.leagueName}} (Year {{year}})</h1>
        <hr />
        <div v-if="leagueYear && leagueYear.managerMessages && leagueYear.managerMessages.length > 0">
          <h2>Manager's Messages</h2>
          <div class="alert alert-info" v-for="message in leagueYear.managerMessages">
            <b-button class="delete-button" variant="warning" v-if="league.isManager" v-on:click="deleteMessage(message)">Delete</b-button>
            <h5>{{message.timestamp | dateTime}}</h5>
            <div class="preserve-whitespace">{{message.messageText}}</div>
          </div>
          <hr />
        </div>

        <h2>Detailed Bid/Drop Results</h2>
        <div v-for="leagueActionSet in leagueActionSets" class="history-table">
          <collapseCard>
            <div slot="header">{{leagueActionSet.processTime | longDate}}</div>
            <div slot="body">
              <leagueActionSet :leagueActionSet="leagueActionSet" :mode="'leagueHistory'"></leagueActionSet>
            </div>
          </collapseCard>
        </div>

        <h2>Full Actions History</h2>
        <div class="history-table">
          <b-table :sort-by.sync="sortBy"
                   :sort-desc.sync="sortDesc"
                   :items="leagueActions"
                   :fields="actionFields"
                   bordered
                   striped
                   responsive>
            <template v-slot:cell(timestamp)="data">
              {{data.item.timestamp | dateTime}}
            </template>
            <template v-slot:cell(managerAction)="data">
              {{data.item.managerAction | yesNo}}
            </template>
          </b-table>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
import Vue from 'vue';
import axios from 'axios';
import LeagueActionSet from '@/components/modules/leagueActionSet';
import CollapseCard from '@/components/modules/collapseCard';

export default {
  data() {
    return {
      errorInfo: '',
      league: null,
      leagueYear: null,
      leagueActions: [],
      leagueActionSets: [],
      actionFields: [
        { key: 'publisherName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'timestamp', label: 'Timestamp', sortable: true, thClass: 'bg-primary' },
        { key: 'actionType', label: 'Action Type', sortable: true, thClass: 'bg-primary' },
        { key: 'description', label: 'Description', thClass: 'bg-primary' },
        { key: 'managerAction', label: 'Manager Action?', thClass: 'bg-primary' },
      ],
      sortBy: 'timestamp',
      sortDesc: true,
      forbidden: false,
      lastID: 1
    };
  },
  components: {
    LeagueActionSet,
    CollapseCard
  },
  props: ['leagueid', 'year'],
  methods: {
    getCollapseID() {
      let thisID = this.lastID;
      this.lastID = this.lastID + 1;
      return thisID;
    },
    fetchLeagueActions() {
      axios
        .get('/api/League/GetLeagueActions?leagueID=' + this.leagueid + '&year=' + this.year)
        .then(response => {
          this.leagueActions = response.data;
        })
        .catch(returnedError => (this.error = returnedError));
    },
    fetchLeagueActionSets() {
      axios
        .get('/api/League/GetLeagueActionSets?leagueID=' + this.leagueid + '&year=' + this.year)
        .then(response => {
          this.leagueActionSets = response.data;
        })
        .catch(returnedError => (this.error = returnedError));
    },
    fetchLeague() {
      axios
        .get('/api/League/GetLeague/' + this.leagueid)
        .then(response => {
          this.league = response.data;
        })
        .catch(returnedError => {
          this.error = returnedError;
          this.forbidden = (returnedError.response.status === 403);
        });
    },
    fetchLeagueYear() {
      let queryURL = '/api/League/GetLeagueYear?leagueID=' + this.leagueid + '&year=' + this.year;
      axios
        .get(queryURL)
        .then(response => {
          this.leagueYear = response.data;
          this.selectedYear = this.leagueYear.year;
        })
        .catch(returnedError => {
        });
    },
    deleteMessage(message) {
      var model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year,
        messageID: message.messageID
      };
      axios
        .post('/api/leagueManager/DeleteManagerMessage', model)
        .then(response => {
          this.fetchLeagueYear();
        })
        .catch(response => {
        });
    }
  },
  mounted() {
    this.fetchLeague();
    this.fetchLeagueYear();
    this.fetchLeagueActions();
    this.fetchLeagueActionSets();
  }
};
</script>
<style scoped>
  .history-table {
    margin-left: 15px;
    margin-right: 15px;
  }
  .delete-button{
      float:right;
  }
</style>
