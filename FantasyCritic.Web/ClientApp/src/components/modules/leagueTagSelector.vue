<template>
  <div>
    <div class="reset-button-flex">
      <h5 class="help-text">Drag and Drop to Rearrange</h5>
      <b-button variant="warning" class="reset-button" v-on:click="resetValues">Reset Changes</b-button>
    </div>
    <div class="tag-flex-container">
      <div class="tag-flex-drag">
        <draggable class="tag-drag-list" :list="banned" group="tags" @change="updateValue">
          <div class="tag-drag-item" v-for="(element, index) in banned" :key="element">
            <font-awesome-icon icon="bars" />
            <masterGameTagBadge :tagName="element"></masterGameTagBadge>
          </div>
          <span slot="header" class="tag-header">Banned Tags</span>
        </draggable>
      </div>

      <div class="tag-flex-drag">
        <draggable class="tag-drag-list" :list="allowed" group="tags" @change="updateValue">
          <div class="tag-drag-item" v-for="(element, index) in allowed" :key="element">
            <font-awesome-icon icon="bars" />
            <masterGameTagBadge :tagName="element"></masterGameTagBadge>
          </div>
          <span slot="header" class="tag-header">Allowed Tags</span>
        </draggable>
      </div>

      <!--<div class="tag-flex-drag">
        <draggable class="tag-drag-list" :list="required" group="tags" @change="updateValue">
          <div class="tag-drag-item" v-for="(element, index) in required" :key="element">
            <masterGameTagBadge :tagName="element"></masterGameTagBadge>
          </div>
          <span slot="header" class="tag-header">Required Tags</span>
        </draggable>
      </div>-->
    </div>
  </div>
</template>

<script>
  import draggable from 'vuedraggable';
  import MasterGameTagBadge from '@/components/modules/masterGameTagBadge';

  export default {
    props: ['value'],
    components: {
      draggable,
      MasterGameTagBadge
    },
    data() {
      return {
        allowed: [
        ],
        banned: [
        ],
        required: [
        ],
        initialValue: null
      }
    },
    computed: {
      tagOptions() {
        return this.$store.getters.allTags;
      },
    },
    methods: {
      handleInput(e) {
        this.$emit('input', this.value);
      },
      updateValue() {
        this.value.banned = _.cloneDeep(this.banned);
        this.value.required = _.cloneDeep(this.required);
      },
      resetValues() {
        this.value.banned = _.cloneDeep(this.initialValue.banned);
        this.value.required = _.cloneDeep(this.initialValue.required);
        this.updateInternal();
      },
      updateInternal() {
        this.allowed = [];
        this.banned = [];
        this.required = [];

        this.tagOptions.forEach(tag => {
          if (this.value.banned.includes(tag.name)) {
            this.banned.push(tag.name);
            return;
          }
          if (this.value.required.includes(tag.name)) {
            this.required.push(tag.name);
            return;
          }

          this.allowed.push(tag.name);
        });
      }
    },
    mounted() {
      this.initialValue = _.cloneDeep(this.value);
      this.updateInternal();
    }
  }
</script>

<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>
<style>
  .reset-button-flex {
    display: flex;
    justify-content: space-between;
    margin-bottom: 10px;
  }

  .tag-flex-container {
    display: flex;
  }
  .tag-flex-drag{
    flex-grow: 1;
    margin: 3px;
  }
  .tag-drag-list {
    background: #404040;
    border-radius: 10px;
    padding: 5px;
  }
  .tag-drag-item {
    margin: 10px;
    position: relative;
    display: block;
    padding: 10px 15px;
    margin-bottom: -1px;
    background-color: #5B6977 !important;
    border: 1px solid #ddd;
  }
  .tag-header {
    padding-left: 10px;
    font-size: 20px;
    font-weight: bold;
    color: #D6993A
  }
</style>
