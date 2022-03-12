<template>
  <div>
    <div class="reset-button-flex">
      <h5 class="help-text">Drag and Drop to Re-arrange</h5>
      <b-button variant="warning" class="reset-button" v-on:click="resetValues">Reset Changes</b-button>
    </div>
    <div class="alert alert-warning" v-show="showWarning && !showDanger && !showPortDanger">You've chosen slightly non-standard settings. Be sure this is what you want.</div>
    <div class="alert alert-danger" v-show="showDanger && !showPortDanger">
      The settings you have selected are REALLY not recommended, unless you really know what you are doing and want a highly custom league.
    </div>
    <div class="alert alert-danger" v-show="showPortDanger">
      Please, please, don't allow the tag 'Port'. These games very very rarely get new Open Critic pages, so we usually end up assigning the points from the original game. You're free to allow the
      tag, but please be aware that this is an "unsupported" feature.
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
  props: ['value', 'gameMode'],
  components: {
    draggable,
    MasterGameTagBadge
  },
  data() {
    return {
      allowed: [],
      banned: [],
      required: [],
      initialValue: null
    };
  },
  computed: {
    tagOptions() {
      return _.filter(this.$store.getters.allTags, (x) => !x.systemTagOnly);
    },
    showWarning() {
      if (this.gameMode === 'Beginner') {
        return false;
      }
      let recommendedAllowedTags = ['Reimagining'];
      let recommendedBannedTags = ['DirectorsCut', 'ReleasedInternationally', 'CurrentlyInEarlyAccess'];
      let bannedIntersection = _.intersection(this.banned, recommendedAllowedTags);
      let allowedIntersection = _.intersection(this.allowed, recommendedBannedTags);
      return bannedIntersection.length > 0 || allowedIntersection.length > 0;
    },
    showDanger() {
      let recommendedAllowedTags = ['NewGame', 'NewGamingFranchise', 'PlannedForEarlyAccess', 'WillReleaseInternationallyFirst'];
      let recommendedBannedTags = ['Port'];
      let bannedIntersection = _.intersection(this.banned, recommendedAllowedTags);
      let allowedIntersection = _.intersection(this.allowed, recommendedBannedTags);
      return bannedIntersection.length > 0 || allowedIntersection.length > 0;
    },
    showPortDanger() {
      let recommendedAllowedTags = [];
      let recommendedBannedTags = ['Port'];
      let bannedIntersection = _.intersection(this.banned, recommendedAllowedTags);
      let allowedIntersection = _.intersection(this.allowed, recommendedBannedTags);
      return bannedIntersection.length > 0 || allowedIntersection.length > 0;
    }
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

      this.tagOptions.forEach((tag) => {
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
  },
  watch: {
    value: function () {
      this.initialValue = _.cloneDeep(this.value);
      this.updateInternal();
    }
  }
};
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
.tag-flex-drag {
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
  background-color: #5b6977 !important;
  border: 1px solid #ddd;
}
.tag-header {
  padding-left: 10px;
  font-size: 20px;
  font-weight: bold;
  color: #d6993a;
}
</style>
