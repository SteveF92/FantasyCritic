<template>
  <b-modal id="tag-overrides-modal" title="Tag Overrides" no-footer>
    <table class="table table-sm table-responsive-sm table-bordered table-striped">
      <thead>
        <tr class="bg-primary">
          <th scope="col" class="game-column">Game</th>
          <th scope="col">Tags</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="tagOverride in requiredLeagueYear.tagOverrides" :key="tagOverride.masterGame.masterGameID">
          <td>{{ tagOverride.masterGame.gameName }}</td>
          <td>
            <span v-for="tag in tagOverride.tags" :key="tag">
              <masterGameTagBadge :tag-name="tag"></masterGameTagBadge>
            </span>
          </td>
        </tr>
      </tbody>
    </table>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';

import { useLeagueStore } from '@/store/leagueStore';
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

export default defineComponent({
  components: {
    MasterGameTagBadge
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear'])
  }
});
</script>
