<template>
  <div>
    <div class="reset-button-flex">
      <h5 class="help-text">Drag and Drop to Re-arrange</h5>
      <b-button variant="warning" class="reset-button" @click="resetValues">Reset Changes</b-button>
    </div>
    <div v-show="showWarning && !showDanger && !showPortDanger" class="alert alert-warning">You've chosen slightly non-standard settings. Be sure this is what you want.</div>
    <div v-show="showEarlyAccessWarning" class="alert alert-warning">
      The settings you've chosen will allow an early access game to be drafted/bid on, but only
      <em>before</em>
      the game enters early access. This is a reasonable choice, but be careful - it's very rare for a game to go enter early access and then see a full release within the same year. For this reason,
      allowing early access games is more of a pitfall than anything else.
    </div>
    <div v-show="showEarlyAccessDanger" class="alert alert-warning">If you ban 'Planned for Early Access' you must also ban 'Currently in Early Access'. See the FAQ page for an explanation.</div>
    <div v-show="showDanger && !showPortDanger" class="alert alert-danger">
      The settings you have selected are REALLY not recommended, unless you really know what you are doing and want a highly custom league.
    </div>
    <div v-show="showPortDanger" class="alert alert-danger">
      Please, please, don't allow the tag 'Port'. These games very very rarely get new OpenCritic pages. You're free to allow the tag, but please be aware that this is an "unsupported" feature.
    </div>
    <div class="tag-flex-container">
      <div class="tag-flex-drag">
        <div class="tag-drag-list bg-secondary">
          <span class="tag-header">Banned Tags</span>
          <VueDraggable v-model="internalValue.banned" class="tag-drag-items" group="tags" @update:model-value="onChange">
            <div v-for="element in internalValue.banned" :key="element" class="tag-drag-item">
              <font-awesome-icon icon="bars" />
              <masterGameTagBadge :tag-name="element"></masterGameTagBadge>
            </div>
          </VueDraggable>
        </div>
      </div>

      <div class="tag-flex-drag">
        <div class="tag-drag-list bg-secondary">
          <span class="tag-header">Allowed Tags</span>
          <VueDraggable v-model="internalValue.allowed" class="tag-drag-items" group="tags" @update:model-value="onChange">
            <div v-for="element in internalValue.allowed" :key="element" class="tag-drag-item">
              <font-awesome-icon icon="bars" />
              <masterGameTagBadge :tag-name="element"></masterGameTagBadge>
            </div>
          </VueDraggable>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import { VueDraggable } from 'vue-draggable-plus';

import type { LeagueTagOptionsViewModel, MasterGameTagViewModel } from '@/api/clients';
import { intersection } from '@/globalFunctions';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import { deepClone } from '@/utilities/deepClone';
import type { GameMode } from '@/utilities/leagueCreationPresets';
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

interface TagLists {
  banned: string[];
  allowed: string[];
  required: string[];
}

interface LeagueTagSelectorData {
  initialValue: TagLists;
  internalValue: TagLists;
}

export default defineComponent({
  components: {
    VueDraggable,
    MasterGameTagBadge
  },
  props: {
    modelValue: { type: Object as PropType<LeagueTagOptionsViewModel>, required: true },
    gameMode: { type: String as PropType<GameMode>, required: true }
  },
  emits: {
    'update:modelValue': (_value: LeagueTagOptionsViewModel) => true
  },
  data(): LeagueTagSelectorData {
    return {
      initialValue: {
        banned: [],
        allowed: [],
        required: []
      },
      internalValue: {
        banned: [],
        allowed: [],
        required: []
      }
    };
  },
  computed: {
    tagOptions(): MasterGameTagViewModel[] {
      const interLeagueStore = useInterLeagueStore();
      return interLeagueStore.allTags.filter((x) => !x.systemTagOnly);
    },
    showWarning(): boolean {
      let recommendedAllowedTags = ['Reimagining'];
      let recommendedBannedTags = ['DirectorsCut', 'ReleasedInternationally', 'CurrentlyInEarlyAccess'];
      let bannedIntersection = intersection(this.internalValue.banned, recommendedAllowedTags);
      let allowedIntersection = intersection(this.internalValue.allowed, recommendedBannedTags);
      return bannedIntersection.length > 0 || allowedIntersection.length > 0;
    },
    showDanger(): boolean {
      let recommendedAllowedTags = ['NewGame', 'NewGamingFranchise', 'WillReleaseInternationallyFirst'];
      let recommendedBannedTags = ['Port'];
      let bannedIntersection = intersection(this.internalValue.banned, recommendedAllowedTags);
      let allowedIntersection = intersection(this.internalValue.allowed, recommendedBannedTags);
      return bannedIntersection.length > 0 || allowedIntersection.length > 0;
    },
    showPortDanger(): boolean {
      let recommendedAllowedTags: string[] = [];
      let recommendedBannedTags = ['Port'];
      let bannedIntersection = intersection(this.internalValue.banned, recommendedAllowedTags);
      let allowedIntersection = intersection(this.internalValue.allowed, recommendedBannedTags);
      return bannedIntersection.length > 0 || allowedIntersection.length > 0;
    },
    showEarlyAccessWarning(): boolean {
      return this.internalValue.allowed.includes('PlannedForEarlyAccess') && !this.internalValue.allowed.includes('CurrentlyInEarlyAccess');
    },
    showEarlyAccessDanger(): boolean {
      return this.internalValue.allowed.includes('CurrentlyInEarlyAccess') && !this.internalValue.allowed.includes('PlannedForEarlyAccess');
    }
  },
  created() {
    this.initializeValues();
  },
  methods: {
    onChange(): void {
      this.$emit('update:modelValue', this.internalValue);
    },
    resetValues(): void {
      this.internalValue = deepClone(this.initialValue);
    },
    initializeValues(): void {
      this.initialValue = {
        banned: [],
        allowed: [],
        required: []
      };

      this.internalValue = {
        banned: [],
        allowed: [],
        required: []
      };

      this.tagOptions.forEach((tag) => {
        if (this.modelValue.banned.includes(tag.name)) {
          this.initialValue.banned.push(tag.name);
          return;
        }
        if (this.modelValue.required.includes(tag.name)) {
          this.initialValue.required.push(tag.name);
          return;
        }

        this.initialValue.allowed.push(tag.name);
      });

      this.internalValue = deepClone(this.initialValue);
    }
  }
});
</script>

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
  border-radius: 10px;
  padding: 5px;
}
.tag-drag-items {
  min-height: 40px;
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
