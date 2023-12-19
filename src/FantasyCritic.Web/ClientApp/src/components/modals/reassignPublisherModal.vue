<template>
  <b-modal id="reassignPublisherModal" ref="reassignPublisherModalRef" title="Reassign a Publisher" @hidden="clearData">
    <div v-show="errorInfo" class="alert alert-danger">
      {{ errorInfo }}
    </div>

    <div class="alert alert-warning">
      This feature will allow you to replace the user of one of the leagues publishers with a different player who is currently inactive (does not have a publisher for this year). If you need to
      invite a new player, you can do so by clicking "Invite a Player".
    </div>
    <div class="alert alert-warning">
      If you use this option, the following will happen:
      <ol>
        <li>The new player will take ownership of the publisher selected.</li>
        <li>The old player will be marked as inactive in the current year.</li>
      </ol>
    </div>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="publisherToReassign" class="control-label">Publisher to Reassign</label>
        <b-form-select v-model="publisherToReassign">
          <option v-for="publisher in publishers" :key="publisher.publisherID" :value="publisher">
            {{ publisher.publisherName }}
          </option>
        </b-form-select>
      </div>

      <div v-if="inactiveUsers.length > 0" class="form-group">
        <label for="newUser" class="control-label">New User to Assign Publisher To</label>
        <b-form-select v-model="newUser">
          <option v-for="user in inactiveUsers" :key="user.id" :value="user">
            {{ user.displayName }}
          </option>
        </b-form-select>
      </div>
      <div v-else class="alert alert-warning">There are no inactive players in this league. Invite one by going to 'Invite a Player'.</div>
    </div>

    <div v-if="readyToConfirm">
      <div class="alert alert-warning">If you are sure you want do this, please type "Reassign Publisher" below.</div>
      <input v-model="reassignConfirmation" type="text" class="form-control input" />
    </div>

    <template #modal-footer>
      <input class="btn btn-primary" value="Reassign Publisher" @click="reassignPublisher" :disabled="!readyToReassign" />
    </template>
  </b-modal>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      publisherToReassign: null,
      newUser: null,
      reassignConfirmation: '',
      errorInfo: ''
    };
  },
  computed: {
    readyToConfirm() {
      return !!this.publisherToReassign && !!this.newUser;
    },
    readyToReassign() {
      return this.readyToConfirm && this.reassignConfirmation === 'Reassign Publisher';
    },
    inactiveUsers() {
      const allPlayers = this.leagueYear.league.players;
      const activePlayerUserIDs = this.leagueYear.players.filter((x) => !!x.user).map((x) => x.user.userID);
      return allPlayers.filter((x) => !activePlayerUserIDs.includes(x.userID));
    }
  },
  methods: {
    async reassignPublisher() {
      const model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        publisherID: this.publisherToReassign.publisherID,
        newUserID: this.newUser.userID
      };

      try {
        await axios.post('/api/leagueManager/ReassignPublisher', model);
        this.notifyAction('Publisher has been reassigned.');
        this.clearData();
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    clearData() {
      this.publisherToReassign = null;
      this.newUser = null;
      this.errorInfo = '';
    }
  }
};
</script>
<style scoped>
.email-form {
  margin-bottom: 10px;
}
.text-black {
  color: black !important;
}
.display-number-label {
  font-size: 35px;
  margin-right: 3px;
}
.invite-link {
  display: flex;
}
</style>
