<template>
  <b-modal id="changeUserNameForm" ref="changeUserNameRef" title="Change Username" @hidden="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newUserName" class="control-label">New Username</label>
        <input v-model="newUserName" id="newUserName" name="newUserName"  class="form-control input" />
      </div>
    </div>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Change Username" v-on:click="changeUserName" :disabled="!formValid" />
    </div>
  </b-modal>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    data() {
      return {
        newUserName: "",
        errorInfo: ""
      }
    },
    computed: {
      formValid() {
        return this.newUserName;
      }
    },
    methods: {
      changeUserName() {
        let changeInfo = {
          newUserName: this.newUserName,
        };
        this.$store.dispatch("changeUserName", changeInfo)
          .then(() => {
            this.$refs.changeUserNameRef.hide();
            this.$emit('userNameChanged', changeInfo);
            this.clearData();
          })
          .catch(returnedError => {
          });
      },
      clearData() {
        this.newUserName = "";
      }
    }
  }
</script>
