<template>
  <b-modal id="editDraftOrderForm" ref="editDraftOrderFormRef" title="Set Draft Order" @hidden="clearData">
    Drag and drop to change order.
    <br />
    <br />
    <div class="fluid container">
      <draggable class="list-group" element="ul" v-model="desiredDraftOrder" :options="dragOptions" @start="isDragging=true" @end="isDragging=false">
        <transition-group type="transition" :name="'flip-list'">
          <li class="list-group-item" v-for="publisher in desiredDraftOrder" :key="publisher.draftPosition">
            <font-awesome-icon icon="bars" />
            <span class="badge">{{publisher.draftPosition}}</span>
            {{publisher.publisherName}}
          </li>
        </transition-group>
      </draggable>
    </div>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Set Draft Order" v-on:click="setDraftOrder" />
    </div>
  </b-modal>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    import draggable from 'vuedraggable';

  export default {
    components: {
      draggable,
    },
    props: ['leagueYear'],
      data() {
        return {
          desiredDraftOrder: [],
          isDragging: false,
          delayedDragging: false
        }
    },
    computed: {
      dragOptions() {
        return {
          animation: 0,
          group: "description",
          disabled: false
        };
      }
    },
    methods: {
      setDraftOrder() {
        let desiredDraftOrderIDs = this.desiredDraftOrder.map(function (v) {
          return v.publisherID;
        });
        var model = {
          leagueID: this.leagueYear.leagueID,
          year: this.leagueYear.year,
          publisherDraftPositions: desiredDraftOrderIDs
        };
        axios
          .post('/api/leagueManager/SetDraftOrder', model)
          .then(response => {
            this.$refs.editDraftOrderFormRef.hide();
            this.$emit('draftOrderEdited');
          })
          .catch(response => {

          });
      },
      clearData() {
        this.desiredDraftOrder = this.leagueYear.publishers;
      }
    },
    mounted() {
      this.clearData();
    },
    watch: {
      isDragging(newValue) {
        if (newValue) {
          this.delayedDragging = true;
          return;
        }
        this.$nextTick(() => {
          this.delayedDragging = false;
        });
      }
    }
  }
</script>
<style>
  .flip-list-move {
    transition: transform 0.5s;
  }

  .no-move {
    transition: transform 0s;
  }

  .ghost {
    opacity: 0.5;
    background: #c8ebfb;
  }

  .list-group {
    min-height: 20px;
  }

  .list-group-item {
    position: relative;
    display: block;
    padding: 10px 15px;
    margin-bottom: -1px;
    background-color: #5B6977;
    border: 1px solid #ddd;
  }

  .list-group-item i {
    cursor: pointer;
  }
</style>
