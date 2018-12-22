<template>
  <b-modal id="changeLeagueNameForm" ref="changeLeagueNameFormRef" title="Change League Name" @hidden="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newleagueName" class="control-label">League Name</label>
        <input v-model="newleagueName" id="newleagueName" name="newleagueName" type="text" class="form-control input" />
      </div>
    </div>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Change Name" v-on:click="changeleagueName" :disabled="!newleagueName"/>
    </div>
  </b-modal>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    data() {
      return {
        newleagueName: "",
        errorInfo: ""
      }
    },
    props: ['league'],
    methods: {
      changeleagueName() {
        var model = {
          leagueID: this.league.leagueID,
          leagueName: this.newleagueName
        };
        axios
          .post('/api/leagueManager/changeleagueName', model)
          .then(response => {
            this.$refs.changeLeagueNameFormRef.hide();
            let actionInfo = {
              oldName: this.league.leagueName,
              newName: this.newleagueName,
              fetchLeagueYear: true
            };
            this.$emit('leagueNameChanged', actionInfo);
            this.newleagueName = "";
          })
          .catch(response => {
          });
      },
      clearData() {
        this.newleagueName = this.league.leagueName;
      }
    },
    mounted() {
      this.newleagueName = this.league.leagueName;
    }
  }
</script>
