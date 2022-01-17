<template>
  <div :id="'publisher-' + publisher.publisherID" class="player-summary bg-secondary" v-bind:class="{ 'publisher-is-next': publisher.nextToDraft }">
    <div class="publisher-player-names">
      <div class="publisher-name-and-icon">
        <span v-if="publisher.publisherIcon && iconIsValid" class="publisher-icon">
          {{ publisher.publisherIcon }}
        </span>
        <div class="publisher-name">
          <router-link :to="{ name: 'publisher', params: { publisherid: publisher.publisherID }}">
            {{ publisher.publisherName }}
            <font-awesome-icon icon="info-circle" />
          </router-link>
        </div>
      </div>
      <div class="player-name">
        Player: {{publisher.playerName}}
        <b-button variant="secondary" v-if="userIsPublisher && isPlusUser" size="sm" v-on:click="sharePublisher">
          <font-awesome-icon icon="share-alt" size="lg" class="share-button" />
          <span>Get Image</span>
        </b-button>
      </div>
    </div>
    <table class="table table-striped">
      <thead>
        <tr class="bg-secondary">
          <th scope="col" class="game-column">Game</th>
          <th scope="col" class="score-column">Critic</th>
          <th scope="col" class="score-column">Points</th>
        </tr>
      </thead>
      <tbody>
        <playerGameSlotRow v-for="gameSlot in gameSlots" :minimal="true"
                           :gameSlot="gameSlot" :supportedYear="leagueYear.supportedYear" 
                           :hasSpecialSlots="leagueYear.hasSpecialSlots" :userIsPublisher="userIsPublisher"
                           v-bind:key="gameSlot.overallSlotNumber"></playerGameSlotRow>
          <tr class="minimal-game-row">
            <td id="total-description">
              <span id="total-description-text" v-if="!advancedProjections">
                Total Fantasy Points
              </span>
              <span id="total-description-text" v-else>
                Projected Fantasy Points
              </span>
            </td>
            <template v-if="!advancedProjections">
              <td id="total-column" class="bg-success" colspan="2">{{publisher.totalFantasyPoints | score}}</td>
            </template>
            <template v-else>
              <td id="total-column" class="bg-info" colspan="2"><em>~{{publisher.totalProjectedPoints | score}}</em></td>
            </template>
          </tr>
      </tbody>
    </table>
  </div>

</template>
<script>
import html2canvas from 'html2canvas';
import PlayerGameSlotRow from '@/components/modules/gameTables/playerGameSlotRow';
import GlobalFunctions from '@/globalFunctions';

export default {
  components: {
    PlayerGameSlotRow
  },
  props: ['publisher', 'leagueYear'],
  computed: {
    advancedProjections() {
      return this.$store.getters.advancedProjections;
    },
    userIsPublisher() {
      return this.$store.getters.userInfo && this.publisher.userID === this.$store.getters.userInfo.userID;
    },
    gameSlots() {
      if (!this.$store.getters.draftOrderView) {
        return this.publisher.gameSlots;
      }

      return _.orderBy(this.publisher.gameSlots, ['counterPick', 'publisherGame.timestamp'], ['asc', 'asc']);
    },
    iconIsValid() {
      return GlobalFunctions.publisherIconIsValid(this.publisher.publisherIcon);
    },
    isPlusUser() {
      return this.$store.getters.isPlusUser;
    }
  },
  methods: {
    sharePublisher() {
      let elementID = '#publisher-' + this.publisher.publisherID;
      html2canvas(document.querySelector(elementID)).then(canvas => {
        var dataUrl = canvas.toDataURL("png");
        var win = window.open();
        win.document.write("<h2>On mobile, long press on this image to share it. On desktop, you'll have to download it first.</h2>");
        win.document.write('<iframe src="' + dataUrl + '" frameborder="0" style="border:0; top:0px; left:0px; bottom:0px; right:0px; width:100%; height:100%;" allowfullscreen> </iframe>');
        win.document.title = this.publisher.publisherName;
      });
    }
  }
};
</script>
<style scoped>
  .player-summary {
    margin-bottom: 10px;
    padding: 5px;
    border: solid;
    border-width: 1px;
    padding: 0;
  }

  .publisher-is-next {
    border-color: #45621d;
    border-width: 5px;
    border-style: solid;
    padding: 0;
  }

  .publisher-is-next table thead tr.table-primary th {
    background-color: #ED9D2B;
  }

  .player-summary table {
    margin-bottom: 0;
  }

  .publisher-player-names {
    margin: 15px;
    margin-bottom: 0;
    display: flex;
    flex-wrap: wrap;
    justify-content: space-between;
  }

  .publisher-name-and-icon {
    display: flex;
    align-items:center;
  }

  .publisher-name {
    font-weight: bold;
    text-transform: uppercase;
    font-size: 1.1em;
    color: #D6993A;
    word-break: break-word;
  }

  .publisher-icon {
    font-size: 50px;
    line-height: 50px;
    padding-bottom: 5px;
  }

  .player-name {
    color: #D6993A;
    font-weight: bold;
  }

</style>
<style>
  .player-summary table th {
    border-top: none;
  }

  .player-summary table td {
    border: 1px solid white;
  }

  .player-summary table thead tr th {
    height: 35px;
    padding: 5px;
  }

  .player-summary table tbody tr td {
    height: 35px;
    padding: 5px;
  }

  .type-column {
    width: 30px;
    text-align: center;
  }
  .game-column {

  }

  .score-column {
    width: 30px;
    text-align: center;
    font-weight: bold;
    height: 20px;
    line-height: 20px;
  }

  #total-description {
    vertical-align: middle;
  }

  #total-description-text {
    float: right;
    text-align: right;
    display: table-cell;
    vertical-align: middle;
    font-weight: bold;
    font-size: 20px;
  }

  #total-column {
    text-align: center;
    font-weight: bold;
    font-size: 25px;
  }

  .share-button {
    color: white;
  }
</style>
