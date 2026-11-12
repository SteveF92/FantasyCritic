<template>
  <div>
    <div v-if="userInfo && !userInfo.emailConfirmed" class="alert alert-warning">
      <div>Your email address has not been confirmed. You cannot accept league invites via email until you do so.</div>
      <div>Check your email account for an email from us.</div>
      <span>
        If you are having issues, check out our
        <a href="/faq#technical" class="text-secondary" target="_blank">FAQ</a>
        page.
      </span>
    </div>

    <div class="col-md-10 offset-md-1 col-sm-12">
      <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
      <div v-if="userInfo" class="text-well welcome-area">
        <h2 class="welcome-header">Welcome {{ userInfo.displayName }}!</h2>
        <div class="row welcome-main">
          <div class="col-12 col-lg-4 mb-3 mb-lg-0 welcome-actions-col">
            <div class="welcome-actions" role="toolbar" aria-label="Quick actions">
              <b-button size="sm" variant="outline-primary" :to="{ name: 'createLeague' }" class="welcome-action-btn">Create a League</b-button>
              <b-button v-if="isPlusUser" size="sm" variant="outline-primary" :to="{ name: 'createConference' }" class="welcome-action-btn">Create a Conference</b-button>
              <b-button size="sm" variant="outline-primary" :to="{ name: 'criticsRoyale' }" class="welcome-action-btn">Play Critics Royale</b-button>
              <b-button v-show="isFactChecker || isAdmin" size="sm" variant="outline-warning" :to="{ name: 'adminConsole' }" class="welcome-action-btn">
                Admin Console
                <b-badge v-if="adminTaskTotalCount" variant="danger" class="ms-1">{{ adminTaskTotalCount }}</b-badge>
              </b-button>
            </div>
          </div>
          <div class="col-lg-8 col-12 welcome-announcements-col">
            <SiteAnnouncementsWidget :announcements="homeAnnouncementPreview" />
          </div>
        </div>
      </div>

      <div class="row">
        <div class="col-lg-8 col-md-12">
          <b-card v-if="userInfo" title="Leagues">
            <b-tabs pills class="league-tabs">
              <b-tab title="My Leagues" title-item-class="tab-header">
                <div v-if="myStandardLeagues.length > 0">
                  <leagueTable :leagues="myStandardLeagues" :league-icon="'user'" :show-archive="true"></leagueTable>
                </div>
                <div v-else>
                  <label>You are not in any leagues! Why not create one?</label>
                </div>
              </b-tab>
              <b-tab v-if="invitedLeagues.length > 0" title-item-class="tab-header">
                <template #title>
                  League Invites
                  <font-awesome-icon icon="exclamation-circle" size="lg" />
                </template>
                <leagueInviteTable :invites="invitedLeagues"></leagueInviteTable>
              </b-tab>
              <b-tab v-if="myConferences.length > 0" title="My Conferences" title-item-class="tab-header">
                <conferenceTable :conferences="myConferences"></conferenceTable>
              </b-tab>
              <b-tab title="Followed Leagues" title-item-class="tab-header">
                <div v-if="myFollowedLeagues.length > 0">
                  <leagueTable :leagues="myFollowedLeagues" :league-icon="'users'" :show-unfollow="true" @league-unfollowed="onLeagueUnfollowed"></leagueTable>
                </div>
                <div v-else>
                  <label>You are not following any public leagues!</label>
                </div>
              </b-tab>
              <b-tab v-if="myArchivedLeagues.length > 0" title="Archived Leagues" title-item-class="tab-header">
                <leagueTable :leagues="myArchivedLeagues" :league-icon="'archive'" :show-un-archive="true"></leagueTable>
              </b-tab>
              <b-tab v-if="myTestLeagues.length > 0" title="Test Leagues" title-item-class="tab-header">
                <leagueTable :leagues="myTestLeagues" :league-icon="'atom'"></leagueTable>
              </b-tab>
            </b-tabs>
          </b-card>
        </div>

        <div class="col-lg-4 col-md-12">
          <hr class="d-md-block d-lg-none" />
          <TopBidsAndDrops :top-bids-and-drops="topBidsAndDrops" :process-date="topBidsAndDropsProcessDate"></TopBidsAndDrops>
        </div>
      </div>

      <hr />

      <div class="row">
        <div class="col-lg-8 col-md-12">
          <b-card v-if="gameNews">
            <gameNews :game-news="gameNews" mode="user" />
          </b-card>
        </div>

        <div class="col-lg-4 col-md-12">
          <hr class="d-md-block d-lg-none" />
          <b-card title="Popular Public Leagues">
            <h5><router-link :to="{ name: 'publicLeagues' }">View All</router-link></h5>

            <div v-if="publicLeagues.length > 0" class="row">
              <div class="col-12 px-0">
                <b-table v-model:sort-by="sortBy" :items="publicLeagues" :fields="publicLeagueFields" bordered striped responsive small>
                  <template #cell(leagueName)="data">
                    <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: selectedYear } }">{{ data.item.leagueName }}</router-link>
                    <font-awesome-icon
                      v-if="!data.item.anyDraftStarted"
                      v-b-popover.hover.top="'This league has not started a draft yet for this year.'"
                      icon="hourglass-half"
                      class="not-started-icon" />
                  </template>
                </b-table>
              </div>
            </div>
          </b-card>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import { type BTableSortBy, type TableField } from 'bootstrap-vue-next';

import {
  combinedDataClient,
  generalClient,
  type CompleteLeagueInviteViewModel,
  type GameNewsViewModel,
  type LeagueWithStatusViewModel,
  type MinimalConferenceViewModel,
  type PublicLeagueYearViewModel,
  type SiteAnnouncementViewModel,
  type TopBidsAndDropsGameViewModel
} from '@/api/clients';
import { useAdminStore } from '@/store/adminStore';
import { useAuthStore } from '@/store/authStore';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import ConferenceTable from '@/components/conferenceTable.vue';
import GameNews from '@/components/gameNews.vue';
import LeagueInviteTable from '@/components/leagueInviteTable.vue';
import LeagueTable from '@/components/leagueTable.vue';
import SiteAnnouncementsWidget from '@/components/siteAnnouncementsWidget.vue';
import TopBidsAndDrops from '@/components/topBidsAndDropsWidget.vue';

interface HomeData {
  errorInfo: string;
  myLeagues: LeagueWithStatusViewModel[];
  invitedLeagues: CompleteLeagueInviteViewModel[];
  myFollowedLeagues: LeagueWithStatusViewModel[];
  myConferences: MinimalConferenceViewModel[];
  selectedYear: number;
  publicLeagues: PublicLeagueYearViewModel[];
  sortBy: BTableSortBy[];
  gameNews: GameNewsViewModel | null;
  topBidsAndDropsProcessDate: string | null;
  topBidsAndDrops: TopBidsAndDropsGameViewModel[] | null;
  siteAnnouncements: SiteAnnouncementViewModel[];
}

export default defineComponent({
  setup() {
    const publicLeagueFields: TableField<PublicLeagueYearViewModel>[] = [
      { key: 'leagueName', label: 'Name', sortable: true, thClass: 'bg-primary' },
      { key: 'numberOfFollowers', label: 'Followers', sortable: true, thClass: 'bg-primary' }
    ];

    return { publicLeagueFields };
  },
  components: {
    LeagueTable,
    LeagueInviteTable,
    ConferenceTable,
    GameNews,
    SiteAnnouncementsWidget,
    TopBidsAndDrops
  },
  data(): HomeData {
    return {
      errorInfo: '',
      myLeagues: [],
      invitedLeagues: [],
      myFollowedLeagues: [],
      myConferences: [],
      selectedYear: useInterLeagueStore().supportedYears.filter((x) => x.openForPlay)[0].year,
      publicLeagues: [],
      sortBy: [{ key: 'numberOfFollowers', order: 'desc' }],
      gameNews: null,
      topBidsAndDropsProcessDate: null,
      topBidsAndDrops: null,
      siteAnnouncements: []
    };
  },
  computed: {
    ...mapState(useAuthStore, ['isPlusUser', 'userInfo', 'isAdmin', 'isFactChecker']),
    ...mapState(useAdminStore, ['adminTaskCounts']),
    adminTaskTotalCount() {
      return this.adminTaskCounts ? this.adminTaskCounts.totalCount : 0;
    },
    myStandardLeagues() {
      return this.myLeagues.filter((x) => !x.testLeague && !x.archived);
    },
    myArchivedLeagues() {
      return this.myLeagues.filter((x) => !x.testLeague && x.archived);
    },
    myTestLeagues() {
      return this.myLeagues.filter((x) => x.testLeague);
    },
    homeAnnouncementPreview() {
      return this.siteAnnouncements.slice(0, 1);
    }
  },
  async created() {
    const adminStore = useAdminStore();
    await Promise.all([this.fetchHomePageData(), this.fetchSiteAnnouncements(), adminStore.fetchAdminTaskCounts()]);
  },
  methods: {
    async fetchSiteAnnouncements() {
      try {
        this.siteAnnouncements = await generalClient.siteAnnouncements();
      } catch {
        this.siteAnnouncements = [];
      }
    },
    onLeagueUnfollowed(leagueID: string) {
      this.myFollowedLeagues = this.myFollowedLeagues.filter((x) => x.leagueID !== leagueID);
    },
    async fetchHomePageData() {
      try {
        const response = await combinedDataClient.homePageData();

        //My Leagues
        this.myLeagues = response.myLeagues.filter((x) => x.userIsInLeague);
        this.myFollowedLeagues = response.myLeagues.filter((x) => x.userIsFollowingLeague);

        //Invited Leagues
        this.invitedLeagues = response.myInvites;

        //My Conferences
        this.myConferences = response.myConferences;

        //Game News
        this.gameNews = response.myGameNews;

        //Top Bids and Drops
        if (response.topBidsAndDrops) {
          this.topBidsAndDropsProcessDate = response.topBidsAndDrops.processDate;
          const allData = response.topBidsAndDrops.data;
          const yearWithMostData = response.topBidsAndDrops.yearWithMostData;
          this.topBidsAndDrops = allData[yearWithMostData];
        } else {
          this.topBidsAndDrops = [];
        }

        //Public Leagues
        this.publicLeagues = response.publicLeagues;
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to load your home page.');
      }
    }
  }
});
</script>

<style scoped>
.welcome-header {
  text-align: center;
}
.not-started-icon {
  margin-left: 8px;
  color: #888;
}
.welcome-area {
  margin-top: 10px;
  margin-bottom: 20px;
}

.welcome-actions-col {
  display: flex;
  justify-content: center;
  /* Size the button toolbar by column width so we can switch grid columns without tying it to full-page breakpoints */
  container-type: inline-size;
  container-name: welcome-actions;
}

.welcome-actions {
  display: grid;
  grid-template-columns: 1fr;
  gap: 0.5rem;
}

/* Wide enough actions column: two compact columns */
@container welcome-actions (min-width: 26rem) {
  .welcome-actions {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

/* Odd count in 2-col mode: last button spans full width */
@container welcome-actions (min-width: 26rem) {
  .welcome-actions .welcome-action-btn:last-child:nth-child(odd) {
    grid-column: 1 / -1;
  }
}

@media (min-width: 992px) {
  /* Equal-height columns (row default stretch); center the button grid vertically vs. announcements */
  .welcome-actions-col {
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: stretch;
  }

  .welcome-actions {
    margin-top: 0;
    max-width: none;
  }
}

div :deep(div.card) {
  background: rgba(50, 50, 50, 0.8);
}

div :deep(.tab-header) {
  margin-bottom: 5px;
}

div :deep(.tab-header .nav-link) {
  border-radius: 0px;
  font-weight: bolder;
  color: white;
}

div :deep(.welcome-action-btn) {
  padding-top: 10px;
}
</style>
