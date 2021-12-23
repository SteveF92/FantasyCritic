<template>
  <div>
    <div class="alert alert-info">
      If you choose more than one tag for a single slot, then a game only needs to have at least ONE of those tags, not all of them.
      You can use this to say "this game must be a remake or a remaster", for example.
    </div>
    <div v-show="showNoTagsWarning" class="alert alert-warning">
      All of your special game slots should have at least one tag listed.
    </div>
    <div v-show="showPortDanger" class="alert alert-danger">
      Please, please, don't allow the tag 'Port'. These games very very rarely get new Open Critic pages, so we usually end up assigning the points from the original game.
      You're free to allow the tag, but please be aware that this is an "unsupported" feature.
    </div>
    <div v-for="specialGameSlot in internalValue" class="special-game-slot">
      <div class="special-slot-header">
        <h4>Special Slot {{specialGameSlot.specialSlotPosition + 1}}</h4>
        <b-button variant="danger" size="sm" v-on:click="removeSlot(specialGameSlot)">Remove</b-button>
      </div>
      <masterGameTagSelector v-model="specialGameSlot.requiredTags" @input="handleInput"></masterGameTagSelector>
    </div>
    <div class="add-slot-button-row">
      <b-button variant="primary" v-on:click="addSlot" class="add-slot-button">Add Special Slot</b-button>
    </div>
  </div>
</template>

<script>
  import draggable from 'vuedraggable';
  import MasterGameTagSelector from '@/components/modules/masterGameTagSelector';

  export default {
    props: ['value'],
    components: {
      draggable,
      MasterGameTagSelector
    },
    data() {
      return {
        internalValue: []
      }
    },
    computed: {
      tagOptions() {
        return this.$store.getters.allTags;
      },
      showNoTagsWarning() {
        return _.some(this.internalValue, x => x.requiredTags.length === 0);
      },
      showPortDanger() {
        return _.some(this.internalValue, x => _.some(x.requiredTags, y => y.name === "Port"));
      }
    },
    methods: {
      handleInput() {
        let returnValue = [];
        this.internalValue.forEach(singleValue => {
          let newReturnItem = {
            specialSlotPosition: singleValue.specialSlotPosition,
            requiredTags: singleValue.requiredTags.map(v => v.name)
          };
          returnValue.push(newReturnItem);
        });

        this.$emit('input', returnValue);
      },
      getTags(tagNames) {
        let allTags = this.$store.getters.allTags;
        let matchingTags = [];
        tagNames.forEach(tagName => {
          let match = _.filter(allTags, { 'name': tagName });
          matchingTags.push(match[0]);
        });

        return matchingTags;
      },
      removeSlot(specialSlot) {
        this.internalValue = _.filter(this.internalValue, x => x.specialSlotPosition !== specialSlot.specialSlotPosition);

        let slotNumber = 0;
        this.internalValue.forEach(singleValue => {
          singleValue.specialSlotPosition = slotNumber;
          slotNumber++;
        });
        this.handleInput();
      },
      addSlot() {
        let newItem = {
          specialSlotPosition: this.internalValue.length,
          requiredTags: []
        };
        this.internalValue.push(newItem);
        this.handleInput();
      },
      updateInternal() {
        this.internalValue = [];

        this.value.forEach(singleValue => {
          let newInternalItem = {
            specialSlotPosition: singleValue.specialSlotPosition,
            requiredTags: this.getTags(singleValue.requiredTags)
          };
          this.internalValue.push(newInternalItem);
        });
      }
    },
    mounted() {
      this.updateInternal();
    },
    watch: {
      value() {
        this.updateInternal();
      }
    }
  }
</script>

<style scoped>
  .special-game-slot {
    background: #404040;
    border-radius: 10px;
    padding: 10px;
    margin-bottom: 15px;
  }
  .special-slot-header {
    width: 100%;
    display: flex;
    justify-content: space-between;
    margin-bottom: 5px;
  }
  .add-slot-button-row{
      display: flex;
      flex-direction: row-reverse;
  }
</style>
