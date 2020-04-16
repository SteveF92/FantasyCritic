<template>
  <div>
    <div class="header">
      <h2>Standings</h2>
      <span>
        <label class="projections-label">Advanced Projections</label>
        <toggle-button class="toggle" :class="{ 'toggle-on': advancedProjections }" v-model="advancedProjections" :sync="true"
                       :labels="{checked: 'On', unchecked: 'Off'}" :css-colors="true" :font-size="13" />
      </span>
    </div>


    <b-table :sort-by.sync="sortBy"
             :sort-desc.sync="sortDesc"
             :items="standings"
             :fields="standingFields"
             bordered
             small
             responsive
             striped>
      <template v-slot:cell(userName)="data">
        <span v-if="data.item.user">{{ data.item.user.displayName }}</span>
        <span v-if="!data.item.user">{{ data.item.inviteName }}</span>
      </template>
      <template v-slot:cell(publisher)="data">
        <span v-if="data.item.publisher">
            <router-link class="text-primary publisher-name" :to="{ name: 'publisher', params: { publisherid: data.item.publisher.publisherID }}">
              {{ data.item.publisher.publisherName }}
            </router-link>
            <span class="publisher-badge badge badge-pill badge-primary badge-info" v-show="data.item.publisher.autoDraft">Auto Draft</span>
          </span>
        <span v-if="data.item.publisher && showRemovePublisher">
          <b-button variant="danger" size="sm" v-on:click="removePublisher(data.item.publisher)">Remove Publisher</b-button>
        </span>
        <span v-if="data.item.user && !data.item.publisher">
          &lt;Publisher Not Created&gt;
          <span v-if="showRemove(data.item.user)">
            <b-button variant="danger" size="sm" v-on:click="removeUser(data.item.user)">Remove</b-button>
          </span>
        </span>
        <span v-if="!data.item.user">
          &lt;Invite Sent&gt;
          <span v-if="league.isManager">
            <b-button variant="danger" size="sm" v-on:click="rescindInvite(data.item.inviteID, data.item.inviteName)">Rescind Invite</b-button>
          </span>
        </span>
      </template>
      <template v-slot:cell(simpleProjectedFantasyPoints)="data">{{data.item.simpleProjectedFantasyPoints | score(2)}}</template>
      <template v-slot:cell(advancedProjectedFantasyPoints)="data">{{data.item.advancedProjectedFantasyPoints | score(2)}}</template>
      <template v-slot:cell(totalFantasyPoints)="data">{{data.item.totalFantasyPoints | score(2)}}</template>
      <template v-slot:cell(budget)="data">
        <span v-if="data.item.publisher">{{data.item.publisher.budget | money}}</span>
      </template>
    </b-table>
  </div>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";
  import { ToggleButton } from 'vue-js-toggle-button'

  export default {
    components: {
      ToggleButton
    },
    props: ['league', 'leagueYear'],
    data() {
      return {
        basicStandingFields: [
          { key: 'userName', label: 'User', thClass:'bg-primary' },
          { key: 'publisher', label: 'Publisher', thClass:'bg-primary' },
          { key: 'totalFantasyPoints', label: 'Points (Actual)', thClass:'bg-primary', sortable: true },
          { key: 'budget', label: 'Budget', thClass:'bg-primary' },
        ],
        projectionFields: [
          { key: 'simpleProjectedFantasyPoints', label: 'Points (Projected)', thClass:'bg-primary', sortable: true },
          { key: 'advancedProjectedFantasyPoints', label: 'Points (Projected)', thClass:'bg-primary', sortable: true },
        ],
        sortBy: 'totalFantasyPoints',
        sortDesc: true
      }
    },
    computed: {
      standingFields() {
        let copiedArray = this.basicStandingFields.slice(0);
        if (this.advancedProjections) {
          copiedArray.splice(2, 0, this.projectionFields[1]);
        } else {
          copiedArray.splice(2, 0, this.projectionFields[0]);
        }

        return copiedArray;
      },
      advancedProjections: {
        get() {
          return this.$store.getters.advancedProjections;
        },
        set(value) {
          if (value && this.sortBy === 'simpleProjectedFantasyPoints') {
            this.sortBy = 'advancedProjectedFantasyPoints';
          } else if (!value && this.sortBy === 'advancedProjectedFantasyPoints') {
            this.sortBy = 'simpleProjectedFantasyPoints';
          }
          this.$store.commit('setAdvancedProjections', value)
        }
      },
      standings() {
        let standings = this.leagueYear.players;
        if (!standings) {
            return [];
        }
        for (var i = 0; i < standings.length; ++i) {
          if (this.leagueYear.supportedYear.finished && this.topPublisher.publisherID === standings[i].publisher.publisherID) {
            standings[i]._rowVariant = 'success';
          }
        }
        return standings;
      },
      topPublisher() {
        if (this.leagueYear.publishers && this.leagueYear.publishers.length > 0) {
          return _.maxBy(this.leagueYear.publishers, 'totalFantasyPoints');
        }
      },
      showRemovePublisher() {
        if (!this.league.isManager) {
          return false;
        }

        if (this.leagueYear.playStatus.playStarted) {
          return false;
        }

        return true;
      }
    },
    methods: {
      showRemove(user) {
        if (!this.league.isManager) {
          return false;
        }

        let matchingLeagueLevelPlayer = _.find(this.league.players, function(item){
          return item.userID === user.userID;
        });

        return matchingLeagueLevelPlayer.removable;
      },
      
      removeUser(user) {
        var model = {
          leagueID: this.leagueYear.leagueID,
          userID: user.userID
        };
        axios
          .post('/api/leagueManager/RemovePlayer', model)
          .then(response => {
            let actionInfo = {
              message: 'User ' + user.displayName + ' has been removed from the league.',
              fetchLeague: true,
              fetchLeagueYear: true
            };
            this.$emit('actionTaken', actionInfo);
          })
          .catch(response => {

          });
      },
      removePublisher(publisher) {
        var model = {
          leagueID: this.leagueYear.leagueID,
          publisherID: publisher.publisherID
        };
        axios
          .post('/api/leagueManager/RemovePublisher', model)
          .then(response => {
            let actionInfo = {
              message: 'Publisher ' + publisher.publisherName + ' has been removed from the league.',
              fetchLeague: true,
              fetchLeagueYear: true
            };
            this.$emit('actionTaken', actionInfo);
          })
          .catch(response => {

          });
      },
      rescindInvite(inviteID, inviteName) {
        var model = {
          inviteID: inviteID
        };
        axios
          .post('/api/leagueManager/RescindInvite', model)
          .then(response => {
            let actionInfo = {
              message: 'The invite to ' + inviteName + ' has been rescinded.',
              fetchLeague: true,
              fetchLeagueYear: true
            };
            this.$emit('actionTaken', actionInfo);
          })
          .catch(response => {

          });
      }
    }
  }
</script>
<style scoped>
  .header{
    display:flex;
    justify-content: space-between;
    flex-wrap: wrap;
  }

   .projections-label {
    margin-top: 14px;
  }

  .toggle {
    margin-top: 4px;
  }

  .publisher-name {
    display: inline-block;
    word-wrap: break-word;
    max-width: 300px;
  }

  @media only screen and (min-width: 1550px) {
    .publisher-name {
      max-width: 650px;
    }
  }
  @media only screen and (max-width: 1549px) {
    .publisher-name {
      max-width: 150px;
    }
  }
  @media only screen and (max-width: 768px) {
    .publisher-name {
      max-width: 200px;
    }
  }
  div >>> tr.table-success td {
    font-weight: bolder;
  }
</style>
