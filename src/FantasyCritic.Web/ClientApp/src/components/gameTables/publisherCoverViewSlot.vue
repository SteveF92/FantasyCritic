<template>
  <div class="slot-area" :style="slotColor">
    <label class="slot-label">{{ slotLabel }}</label>
    <div v-if="game" class="game-image-area">
      <img v-if="masterGame && masterGame.ggToken && masterGame.ggCoverArtFileName" :src="ggCoverArtLink" alt="Cover Image" class="game-image" />
      <font-awesome-layers v-else class="fa-8x no-game-image">
        <font-awesome-icon :icon="['far', 'square']" />
        <font-awesome-layers-text transform="shrink-14" value="No image found" />
      </font-awesome-layers>
    </div>
    <div v-else class="empty-slot-area">
      <font-awesome-layers class="fa-8x no-game-image">
        <font-awesome-icon :icon="['far', 'square']" />
        <font-awesome-layers-text transform="shrink-14" value="Slot Empty" />
      </font-awesome-layers>
    </div>
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
    },
    slotLabel() {
      if (this.gameSlot.counterPick) {
        return 'CPK';
      }

      if (!this.gameSlot.specialSlot) {
        return 'REG';
      }

      if (this.gameSlot.specialSlot.requiredTags.length === 1) {
        const singleTag = this.getTag(this.gameSlot.specialSlot.requiredTags[0]);
        return singleTag.shortName;
      }

      return 'FLX';
    },
    slotColor() {
      if (this.gameSlot.counterPick) {
        return {
          backgroundColor: '#aa1e1e'
        };
      }

      if (!this.gameSlot.specialSlot) {
        return {
          backgroundColor: '#cccccc'
        };
      }

      if (this.gameSlot.specialSlot.requiredTags.length === 1) {
        const singleTag = this.getTag(this.gameSlot.specialSlot.requiredTags[0]);
        return {
          backgroundColor: '#' + singleTag.badgeColor
        };
      }

      return this.getMultiBadgeColor(this.gameSlot.specialSlot.requiredTags);
    }
  },
  methods: {
    getTag(tagName) {
      let allTags = this.$store.getters.allTags;
      let singleTag = _.filter(allTags, { name: tagName });
      return singleTag[0];
    },
    getMultiBadgeColor(tagNames) {
      let colorString = '';
      let stripeWidth = 100.0 / tagNames.length;
      for (let i = 0; i < tagNames.length; i++) {
        let tag = this.getTag(tagNames[i]);
        let badgeColor = tag.badgeColor;

        colorString += `#${badgeColor} ${i * stripeWidth}%, #${badgeColor} ${(i + 1) * stripeWidth}%,`;
      }

      colorString = colorString.substring(0, colorString.length - 1);
      let backGroundString = `linear-gradient(135deg, ${colorString})`;

      return {
        backgroundImage: backGroundString,
        color: 'white'
      };
    },
    getFlexText(requiredTags) {
      let tagElements = requiredTags.map((x) => `<li>${this.getTag(x).readableName}</li>`);
      let joinedTagElements = tagElements.join('');
      return {
        html: true,
        title: () => {
          return 'Flex Slot';
        },
        content: () => {
          return `<p>A game in this slot must have at least one of these tags:</p><ul>${joinedTagElements}</ul>`;
        }
      };
    }
  }
};
</script>
<style scoped>
.slot-area {
  border-radius: 5%;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 10px;
  padding-top: 0;
}

.slot-label {
  text-shadow: 1px 0 0 #000, 0 -1px 0 #000, 0 1px 0 #000, -1px 0 0 #000;
  margin: 0;
}

.regular-slot {
  background-color: #cccccc;
}

.counter-pick-slot {
  background-color: #aa1e1e;
}

.empty-slot-area {
}

.game-image {
  display: block;
  margin: auto;
  border-radius: 5%;
}

.no-game-image {
  width: 165px;
  height: 248px;
}
</style>
