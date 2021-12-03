<template>
  <div>
    <ul>
      <li v-for="specialGameSlot in value">
        {{specialGameSlot.specialSlotPosition}}
        <ul>
          <li v-for="requiredTag in specialGameSlot.requiredTags">
            {{requiredTag}}
          </li>
        </ul>
      </li>
    </ul>
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

      }
    },
    computed: {
      tagOptions() {
        return this.$store.getters.allTags;
      }
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
