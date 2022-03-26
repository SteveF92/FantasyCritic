<template>
  <b-modal id="tagOverridesModal" ref="tagOverridesModalRef" title="Tag Overrides" hide-footer>
    <table class="table table-sm table-responsive-sm table-bordered table-striped">
      <thead>
        <tr class="bg-primary">
          <th scope="col" class="game-column">Game</th>
          <th scope="col">Tags</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="tagOverride in tagOverrides" :key="tagOverride.masterGame.masterGameID">
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

<script>
import MasterGameTagBadge from '@/components/masterGameTagBadge';

export default {
  components: {
    MasterGameTagBadge
  },
  props: {
    tagOverrides: Array
  }
};
</script>
