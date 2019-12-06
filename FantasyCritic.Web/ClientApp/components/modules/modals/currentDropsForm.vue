<template>
  <b-modal id="currentDropsForm" ref="currentDropsFormRef" title="My Current Drop Requests" @hidden="clearData">
    <table class="table table-sm table-responsive-sm table-bordered table-striped">
      <thead>
        <tr class="bg-primary">
          <th scope="col" class="game-column">Game</th>
          <th scope="col">Cancel</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="drop in desiredDropPriorities" :key="drop.priority">
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
          dropID: dropRequest.dropID
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
