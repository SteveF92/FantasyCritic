<template>
  <b-modal id="removePlayerForm" ref="removePlayerFormRef" size="lg" title="Remove Player" hide-footer @hidden="clearData">
    Removing players?
  </b-modal>
</template>

<script>
  import axios from 'axios';
  export default {
    data() {
      return {

      };
    },
    props: ['league','leagueYear'],
    computed: {
      publishers() {
        return this.leagueYear.publishers;
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
      clearData() {

      }
    }
  };
</script>
