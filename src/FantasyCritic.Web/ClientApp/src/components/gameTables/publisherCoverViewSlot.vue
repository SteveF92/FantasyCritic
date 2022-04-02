<template>
  <div v-if="game" class="game-image-area">
    <div v-if="masterGame && masterGame.ggToken && masterGame.ggCoverArtFileName" class="gg-image-area">
      <img v-show="masterGame.ggCoverArtFileName" :src="ggCoverArtLink" alt="Cover Image" class="game-image" />
    </div>
    <font-awesome-layers v-if="!masterGame.ggCoverArtFileName" class="fa-8x no-game-image">
      <font-awesome-icon :icon="['far', 'square']" />
      <font-awesome-layers-text transform="shrink-14" value="No image found" />
    </font-awesome-layers>
  </div>
</template>
<script>
import PublisherMixin from '@/mixins/publisherMixin';

export default {
  mixins: [PublisherMixin],
  props: {
    gameSlot: { type: Object, required: true }
  },
  computed: {
    game() {
      return this.gameSlot.publisherGame;
    },
    masterGame() {
      if (!this.game) {
        return;
      }
      if (!this.game.masterGame) {
        return;
      }
      return this.game.masterGame;
    },
    ggCoverArtLink() {
      if (this.masterGame.ggCoverArtFileName) {
        return `https://ggapp.imgix.net/media/games/${this.masterGame.ggToken}/${this.masterGame.ggCoverArtFileName}?w=165&dpr=1&fit=crop&auto=compress&q=95`;
      }
      return null;
    }
  }
};
</script>
<style scoped>
.game-image {
  display: block;
  margin: auto;
  border-radius: 5%;
}
</style>
