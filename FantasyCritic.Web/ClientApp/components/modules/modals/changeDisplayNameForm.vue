<template>
  <b-modal id="changeDisplayNameForm" ref="changeDisplayNameFormRef" title="Change Display Name" @hidden="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newDisplayName" class="control-label">New Display Name</label>
        <input v-model="newDisplayName" id="newDisplayName" name="newDisplayName"  class="form-control input" />
      </div>
    </div>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Change Display Name" v-on:click="changeDisplayName" :disabled="!formValid" />
    </div>
  </b-modal>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    data() {
      return {
        newDisplayName: "",
        errorInfo: ""
      }
    },
    computed: {
      formValid() {
        return this.newDisplayName;
      }
    },
    methods: {
      changeDisplayName() {
        let changeInfo = {
          newDisplayName: this.newDisplayName,
        };
        this.$store.dispatch("changeDisplayName", changeInfo)
          .then(() => {
            this.$refs.changeDisplayNameFormRef.hide();
            this.$emit('displayNameChanged', changeInfo);
            this.clearData();
          })
          .catch(returnedError => {
          });
      },
      clearData() {
        this.newDisplayName = "";
      }
    }
  }
</script>
