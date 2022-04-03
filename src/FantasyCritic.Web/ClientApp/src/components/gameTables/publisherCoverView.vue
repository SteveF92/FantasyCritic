<template>
  <div>
    <div class="cover-art-header">
      <a href="https://ggapp.io/" target="_blank">
        <strong>
          All Images Provided by GG|
          <font-awesome-icon icon="external-link-alt" />
        </strong>
      </a>

      <b-button v-if="userIsPublisher" variant="secondary" size="sm" @click="sharePublisher">
        <font-awesome-icon icon="share-alt" size="lg" class="share-button" />
        <span>Share</span>
      </b-button>
    </div>
    <div id="cover-art-container">
      <div v-for="gameSlot in gameSlots" :key="gameSlot.overallSlotNumber">
        <publisherCoverViewSlot :game-slot="gameSlot"></publisherCoverViewSlot>
      </div>
    </div>
  </div>
</template>

<script>
import html2canvas from 'html2canvas';

import PublisherMixin from '@/mixins/publisherMixin';
import PublisherCoverViewSlot from '@/components/gameTables/publisherCoverViewSlot';

export default {
  components: {
    PublisherCoverViewSlot
  },
  mixins: [PublisherMixin],
  methods: {
    sharePublisher() {
      let elementID = '#cover-art-container';
      const options = { allowTaint: false, useCORS: true, scale: 2 };
      html2canvas(document.querySelector(elementID), options).then(async (canvas) => {
        const dataUrl = canvas.toDataURL('png');
        const blob = await (await fetch(dataUrl)).blob();
        const filesArray = [
          new File([blob], 'myPublisher.png', {
            type: blob.type,
            lastModified: new Date().getTime()
          })
        ];
        const shareData = {
          files: filesArray,
          title: 'My Fantasy Critic Publisher'
        };
        navigator.share(shareData);
      });
    }
  }
};
</script>
<style scoped>
.cover-art-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

#cover-art-container {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 6px;
  background: #222222;
  border-radius: 5px;
  max-width: 1500px;
  padding: 10px;
}
</style>
