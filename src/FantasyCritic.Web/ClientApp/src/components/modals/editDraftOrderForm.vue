<template>
  <b-modal id="editDraftOrderForm" ref="editDraftOrderFormRef" title="Set Draft Order" @hidden="clearData">
    <label>Drag and drop to change order.</label>
    <div class="fluid container draft-order-editor">
      <draggable class="list-group" element="ul" v-model="desiredDraftOrder" :options="dragOptions" @start="isDragging = true" @end="isDragging = false" handle=".handle">
        <transition-group type="transition" :name="'flip-list'">
          <li class="draft-order-item" v-for="publisher in desiredDraftOrder" :key="publisher.draftPosition">
            <font-awesome-icon icon="bars" class="handle" />
            <span class="badge">{{ publisher.draftPosition }}</span>
            {{ publisher.publisherName }} ({{ publisher.playerName }})
          </li>
        </transition-group>
      </draggable>
    </div>

    <b-button variant="info" size="sm" v-on:click="shuffleOrder">Randomize draft order</b-button>

    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Set Draft Order" v-on:click="setDraftOrder" />
    </div>
  </b-modal>
</template>

<script>
import axios from 'axios';
import draggable from 'vuedraggable';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  components: {
    draggable
  },
  mixins: [LeagueMixin],
  data() {
    return {
      desiredDraftOrder: [],
      isDragging: false,
      delayedDragging: false
    };
  },
  computed: {
    dragOptions() {
      return {
        animation: 0,
        group: 'description',
        disabled: false
      };
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
  },
  methods: {
    setDraftOrder() {
      let desiredDraftOrderIDs = this.desiredDraftOrder.map((v) => v.publisherID);
      var model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        publisherDraftPositions: desiredDraftOrderIDs
      };
      axios
        .post('/api/leagueManager/SetDraftOrder', model)
        .then(() => {
          this.$refs.editDraftOrderFormRef.hide();
          this.notifyAction('Draft order has been changed.');
        })
        .catch(() => {});
    },
    clearData() {
      this.desiredDraftOrder = this.leagueYear.publishers;
    },
    /**
     * On randomize, shuffle current `desiredDraftOrder` array
     * Uses Fisher–Yates_shuffle algorithm to randomize the publishers
     */
    shuffleOrder() {
      const array = this.desiredDraftOrder;
      this.desiredDraftOrder = []; // detach the watchers

      for (let i = array.length - 1; i > 0; i--) {
        let j = Math.floor(Math.random() * (i + 1));
        [array[i], array[j]] = [array[j], array[i]];
      }
      this.desiredDraftOrder = array;
    }
  }
};
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
  background-color: #5b6977 !important;
  border: 1px solid #ddd;
}

.draft-order-item i {
  cursor: pointer;
}
</style>
