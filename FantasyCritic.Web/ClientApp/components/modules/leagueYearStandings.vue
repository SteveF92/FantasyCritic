<template>
  <div>
    <h3>Standings</h3>
    <table class="table table-bordered table-striped table-sm">
      <thead>
        <tr class="bg-primary">
          <th>User</th>
          <th>Publisher</th>
          <th>Points (Projected)</th>
          <th>Points (Actual)</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="player in leagueYear.players">
          <td>
            <span v-if="player.user">{{ player.user.displayName }}</span>
            <span v-if="!player.user">{{ player.invitedEmailAddress }}</span>
          </td>
          <td>
            <span v-if="player.publisher">
              <router-link class="text-primary" :to="{ name: 'publisher', params: { publisherid: player.publisher.publisherID }}">{{ player.publisher.publisherName }}</router-link>
              <span v-if="showRemove && league.leagueManager.userID !== player.user.userID"">
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
              <b-button variant="danger" size="sm" v-on:click="rescindInvite(player.invitedEmailAddress)">Rescind Invite</b-button>
            </span>
          </td>
          <td>{{player.projectedFantasyPoints | score(2)}}</td>
          <td>{{player.totalFantasyPoints | score(2)}}</td>
        </tr>
      </tbody>
    </table>
</div>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    props: ['league', 'leagueYear'],
    computed: {
      showRemove() {
        return (this.league.isManager && this.league.neverStarted);
      }
    },
    methods: {
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
      rescindInvite(emailAddress) {
        var model = {
          leagueID: this.league.leagueID,
          inviteEmail: emailAddress
        };
        axios
          .post('/api/leagueManager/RescindInvite', model)
          .then(response => {
            let actionInfo = {
              message: 'The invite to ' + emailAddress + ' has been rescinded.',
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
