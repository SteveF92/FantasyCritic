<template>
  <b-modal id="editDraftOrderForm" ref="editDraftOrderFormRef" title="Set Draft Order" @hidden="clearData">
    <label>
      Drag and drop to change order.
    </label>
    <div class="fluid container draft-order-editor">
      <draggable class="list-group" element="ul" v-model="desiredDraftOrder" :options="dragOptions" @start="isDragging=true" @end="isDragging=false">
        <transition-group type="transition" :name="'flip-list'">
          <li class="draft-order-item" v-for="publisher in desiredDraftOrder" :key="publisher.draftPosition">
            <font-awesome-icon icon="bars" />
            <span class="badge">{{publisher.draftPosition}}</span>
            {{publisher.publisherName}} ({{publisher.playerName}})
          </li>
        </transition-group>
      </draggable>
    </div>
    <button type="button" name="button" @click="toggle">Toggle</button>
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
      },
      toggle() {
        this.shuffle()
      },
      shuffle() {
        const array = this.desiredDraftOrder
        this.desiredDraftOrder = [] // detach the watchers

        for (let i = array.length - 1; i > 0; i--) {
          let j = Math.floor(Math.random() * (i + 1))
          ;[array[i], array[j]] = [array[j], array[i]]
        }
        this.desiredDraftOrder = array
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
  .draft-order-editor {
    background-color: #414141;
    border-radius: 5px;
    padding-left: 20px;
    padding-right: 20px;
    padding-top: 5px;
    padding-bottom: 5px;
  }

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

  .draft-order-item {
    position: relative;
    display: block;
    padding: 10px 15px;
    margin-bottom: -1px;
    background-color: #5B6977 !important;
    border: 1px solid #ddd;
  }

  .draft-order-item i {
    cursor: pointer;
  }
</style>
