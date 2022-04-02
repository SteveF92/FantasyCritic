<template>
  <div>
    <div class="slot-area" :style="slotColor">
      <div class="game-text">{{ slotLabel }}</div>
      <div class="game-image-area">
        <a :id="popoverID" href="javascript:;" class="no-link-style">
          <img v-if="game && masterGame && masterGame.ggToken && masterGame.ggCoverArtFileName" :src="ggCoverArtLink" alt="Cover Image" class="game-image" />
          <div v-if="game && !(masterGame && masterGame.ggToken && masterGame.ggCoverArtFileName)" class="game-text game-name">
            {{ gameName }}
          </div>
        </a>

        <font-awesome-layers v-if="!game" class="fa-8x no-game-image">
          <font-awesome-icon :icon="['far', 'square']" />
          <font-awesome-layers-text class="game-text empty-slot-text" value="Slot Empty" />
        </font-awesome-layers>
      </div>

      <div class="bottom-text-area">
        <div v-if="game" class="game-text">{{ dateText }}</div>
        <div v-if="game" class="game-text">{{ scoreText }}</div>
      </div>
    </div>

    <b-popover :target="popoverID" triggers="click blur" custom-class="master-game-popover">
      <div class="mg-popover">
        <masterGameSummary :master-game="masterGame" hide-image></masterGameSummary>
      </div>
    </b-popover>
  </div>
</template>
<script>
import PublisherMixin from '@/mixins/publisherMixin';
import MasterGameSummary from '@/components/masterGameSummary';
import GlobalFunctions from '@/globalFunctions';

export default {
  components: {
    MasterGameSummary
  },
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
    gameName() {
      if (this.masterGame) {
        return this.masterGame.gameName;
      }

      return this.game.gameName;
    },
    ggCoverArtLink() {
      if (this.masterGame.ggCoverArtFileName) {
        return `https://ggapp.imgix.net/media/games/${this.masterGame.ggToken}/${this.masterGame.ggCoverArtFileName}?w=165&dpr=1&fit=crop&auto=compress&q=95`;
      }
      return null;
    },
    popoverID() {
      return `mg-popover-${this._uid}`;
    },
    dateText() {
      return GlobalFunctions.formatPublisherGameReleaseDate(this.game, true);
    },
    scoreText() {
      if (this.game.criticScore) {
        return `Score: ${GlobalFunctions.roundNumber(this.game.criticScore, 2)}`;
      }

      return `Projection: ~${GlobalFunctions.roundNumber(this.game.masterGame.projectedFantasyPoints, 2)}`;
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
  padding-left: 12px;
  padding-right: 12px;
}

.game-text {
  text-shadow: 1px 0 0 #000, 0 -1px 0 #000, 0 1px 0 #000, -1px 0 0 #000;
  font-weight: bolder;
  text-align: center;
}

.game-name {
  font-size: 25px;
}
.game-image-area {
  width: 165px;
  height: 248px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
}

.empty-slot-text {
  font-size: 25px;
}

.game-image {
  display: block;
  margin: auto;
  border-radius: 5%;
}

.bottom-text-area {
  height: 50px;
}

.no-link-style {
  color: white;
  text-decoration: none;
}
</style>
