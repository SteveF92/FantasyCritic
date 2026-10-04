<template>
  <div class="royale-podium">
    <div class="podium-header">
      <template v-if="podium.isPreviousQuarter">
        <h2>Last Quarter's Champions</h2>
        <div class="podium-subtitle">
          <router-link :to="{ name: 'criticsRoyale', params: { year: podium.year, quarter: podium.quarter } }">{{ podium.year }}-Q{{ podium.quarter }}</router-link>
        </div>
      </template>
      <h2 v-else>{{ podium.year }}-Q{{ podium.quarter }} Champions</h2>
    </div>

    <div class="podium-stand">
      <div v-for="place in places" :key="place.ranking" class="podium-place" :class="`place-${place.ranking}`">
        <div class="podium-medal">{{ medals[place.ranking] }}</div>
        <div v-for="entry in place.entries" :key="entry.publisherID" class="podium-entry">
          <div class="podium-publisher">
            <span v-if="entry.publisherIcon && iconIsValid(entry.publisherIcon)" class="podium-publisher-icon">{{ entry.publisherIcon }}</span>
            <router-link :to="{ name: 'royalePublisher', params: { publisherid: entry.publisherID } }">{{ entry.publisherName }}</router-link>
          </div>
          <div class="podium-player">
            <router-link :to="{ name: 'royaleHistory', params: { userid: entry.userID } }">{{ entry.playerName }}</router-link>
          </div>
          <div class="podium-points">{{ entry.totalFantasyPoints | score(2) }} points</div>
        </div>
        <div class="podium-step">{{ place.ranking }}</div>
      </div>
    </div>
  </div>
</template>

<script>
import { publisherIconIsValid } from '@/globalFunctions';

const standOrder = [2, 1, 3];

export default {
  props: {
    podium: { type: Object, required: true }
  },
  data() {
    return {
      medals: { 1: '🥇', 2: '🥈', 3: '🥉' }
    };
  },
  computed: {
    places() {
      return standOrder
        .map((ranking) => ({
          ranking,
          entries: this.podium.entries.filter((x) => x.ranking === ranking)
        }))
        .filter((x) => x.entries.length > 0);
    }
  },
  methods: {
    iconIsValid(publisherIcon) {
      return publisherIconIsValid(publisherIcon);
    }
  }
};
</script>

<style scoped>
.royale-podium {
  background: #252525;
  border: 1px solid rgba(214, 153, 58, 0.35);
  border-radius: 8px;
  padding: 8px 10px 0;
  margin-bottom: 10px;
}

.podium-header {
  text-align: center;
  margin-bottom: 4px;
}

.podium-header h2 {
  font-size: 22px;
  margin-bottom: 0;
}

.podium-subtitle {
  font-size: 13px;
}

.podium-stand {
  display: flex;
  justify-content: center;
  align-items: flex-end;
  gap: 6px;
}

.podium-place {
  flex: 0 1 200px;
  min-width: 0;
  display: flex;
  flex-direction: column;
  align-items: stretch;
  text-align: center;
}

.podium-medal {
  font-size: 26px;
  line-height: 1.1;
}

.place-1 .podium-medal {
  font-size: 34px;
}

.podium-entry {
  margin-bottom: 4px;
  padding: 0 4px;
  line-height: 1.25;
  overflow-wrap: anywhere;
}

.podium-publisher {
  font-weight: bold;
  font-size: 14px;
}

.place-1 .podium-publisher {
  font-size: 16px;
}

.podium-publisher-icon {
  margin-right: 4px;
}

.podium-player {
  font-size: 13px;
}

.podium-points {
  font-size: 12px;
}

.podium-step {
  border-radius: 6px 6px 0 0;
  color: rgba(0, 0, 0, 0.55);
  font-size: 22px;
  font-weight: bold;
  display: flex;
  align-items: center;
  justify-content: center;
}

.place-1 .podium-step {
  height: 60px;
  background: #d6993a;
}

.place-2 .podium-step {
  height: 42px;
  background: #b4bac1;
}

.place-3 .podium-step {
  height: 30px;
  background: #b8733e;
}

@media only screen and (max-width: 576px) {
  .podium-publisher,
  .place-1 .podium-publisher {
    font-size: 13px;
  }

  .podium-player,
  .podium-points {
    font-size: 11px;
  }
}
</style>
