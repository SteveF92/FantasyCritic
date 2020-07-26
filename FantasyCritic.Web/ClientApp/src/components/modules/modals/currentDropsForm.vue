<template>
  <b-modal id="currentDropsForm" ref="currentDropsFormRef" title="My Pending Drop Requests">
    <table class="table table-sm table-bordered table-striped">
      <thead>
        <tr class="bg-primary">
          <th scope="col" class="game-column">Game</th>
          <th scope="col">Cancel</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="drop in currentDrops">
          <td>{{drop.masterGame.gameName}}</td>
          <td class="select-cell">
            <b-button variant="danger" size="sm" v-on:click="cancelDrop(drop)">Cancel</b-button>
          </td>
        </tr>
      </tbody>
    </table>
  </b-modal>
</template>

<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    props: ['publisher','currentDrops'],
    data() {
      return {
        
      }
    },
    methods: {
      cancelDrop(dropRequest) {
        var model = {
          dropRequestID: dropRequest.dropRequestID
        };
        axios
          .post('/api/league/DeleteDropRequest', model)
          .then(response => {
            var dropInfo = {
              gameName: dropRequest.masterGame.gameName
            };
            this.$emit('dropCancelled', dropInfo);
          })
          .catch(response => {

          });
      }
    }
  }
</script>
