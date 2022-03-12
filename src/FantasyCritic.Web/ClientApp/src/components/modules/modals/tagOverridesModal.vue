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
        <tr v-for="tagOverride in tagOverrides">
          <td>{{ tagOverride.masterGame.gameName }}</td>
          <td>
            <span v-for="(tag, index) in tagOverride.tags">
              <masterGameTagBadge :tagName="tagOverride.tags[index]"></masterGameTagBadge>
            </span>
          </td>
        </tr>
      </tbody>
    </table>
  </b-modal>
</template>

<script>
import MasterGameTagBadge from '@/components/modules/masterGameTagBadge';

export default {
  components: {
    MasterGameTagBadge
  },
  props: ['tagOverrides']
};
</script>
