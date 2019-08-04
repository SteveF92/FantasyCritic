<template>
  <div>
    <div class="header">
      <h2>Standings</h2>
      <span v-if="false">
        <label class="projections-label">Advanced Projections</label>
        <toggle-button class="toggle" :class="{ 'toggle-on': advancedProjections }" v-model="advancedProjections" :sync="true"
                       :labels="{checked: 'On', unchecked: 'Off'}" :css-colors="true" :font-size="13"/>
      </span>
    </div>
    
    <div class="table-responsive">
      <table class="table table-bordered table-striped table-sm">
        <thead>
          <tr class="bg-primary">
            <th>User</th>
            <th>Publisher</th>
            <th>Points (Projected)</th>
            <th>Points (Actual)</th>
            <th>Budget</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="player in sortedPlayers">
            <td>
              <span v-if="player.user">{{ player.user.displayName }}</span>
              <span v-if="!player.user">{{ player.inviteName }}</span>
            </td>
            <td>
              <span v-if="player.publisher">
                <router-link class="text-primary publisher-name" :to="{ name: 'publisher', params: { publisherid: player.publisher.publisherID }}">{{ player.publisher.publisherName }}</router-link>
                <span v-if="showRemove && league.leagueManager.userID !== player.user.userID">
                  <b-button variant="danger" size="sm" v-on:click="removeUser(player.user)">Remove</b-button>
                </span>
              </span>
              <span v-if="player.user && !player.publisher">
                &lt;Publisher Not Created&gt;
                <span v-if="showRemove && league.leagueManager.userID !== player.user.userID">
                  <b-button variant="danger" size="sm" v-on:click="removeUser(player.user)">Remove</b-button>
                </span>
              </span>
              <span v-if="!player.user">
                &lt;Invite Sent&gt;
                <span v-if="showRemove">
                  <b-button variant="danger" size="sm" v-on:click="rescindInvite(player.inviteID, player.inviteName)">Rescind Invite</b-button>
                </span>
              </span>
            </td>
            <td>{{getProjectedPoints(player) | score(2)}}</td>
            <td>{{player.totalFantasyPoints | score(2)}}</td>
            <td>
              <span v-if="player.publisher">{{player.publisher.budget | money}}</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
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
    computed: {
      showRemove() {
        return (this.league.isManager && this.league.neverStarted);
      },
      sortedPlayers() {
        let vueObject = this;
        return _.sortBy(this.leagueYear.players, [function (x) { return vueObject.getProjectedPoints(x); }]).reverse();
        return;
      },
      advancedProjections: {
        get() {
          return this.$store.getters.advancedProjections;
        },
        set (value) {
          this.$store.commit('setAdvancedProjections', value)
        }
      }
    },
    methods: {
      getProjectedPoints(player) {
        if (this.advancedProjections) {
          return player.advancedProjectedFantasyPoints;
        }
        return player.simpleProjectedFantasyPoints;
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
  }

   .projections-label {
    margin-top: 14px;
  }

  .toggle {
    margin-top: 4px;
  }

  .publisher-name {
      display: block;
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

</style>
