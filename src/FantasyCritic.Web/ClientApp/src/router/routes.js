import Welcome from '@/views/welcome.vue';
// import Home from '@/views/home.vue';
import About from '@/views/about.vue';
// import DiscordBot from '@/views/discordBot.vue';
// import HowToPlay from '@/views/howToPlay.vue';
// import Faq from '@/views/faq.vue';
import Contact from '@/views/contact.vue';
// import CreateLeague from '@/views/createLeague.vue';
// import League from '@/views/league.vue';
// import EditLeague from '@/views/editLeague.vue';
// import Publisher from '@/views/publisher.vue';
// import MasterGame from '@/views/masterGame.vue';
// import MasterGames from '@/views/masterGames.vue';
// import RecentMasterGameChanges from '@/views/recentMasterGameChanges.vue';
// import PublicLeagues from '@/views/publicLeagues.vue';
// import LeagueHistory from '@/views/leagueHistory.vue';
// import AdminConsole from '@/views/adminConsole.vue';
// import MasterGameRequest from '@/views/masterGameRequest.vue';
// import MasterGameChangeRequest from '@/views/masterGameChangeRequest.vue';
// import ActiveMasterGameRequests from '@/views/activeMasterGameRequests.vue';
// import ActiveMasterGameChangeRequests from '@/views/activeMasterGameChangeRequests.vue';
// import MasterGameCreator from '@/views/masterGameCreator.vue';
// import ActionProcessingDryRunResults from '@/views/actionProcessingDryRunResults.vue';
// import CriticsRoyale from '@/views/criticsRoyale.vue';
// import RoyalePublisher from '@/views/royalePublisher.vue';
import NotFound from '@/views/notFound.vue';
// import MasterGameEditor from '@/views/masterGameEditor.vue';
// import FantasyCriticPlus from '@/views/fantasyCriticPlus.vue';

export const routes = [
  { path: '/404', component: NotFound },
  { path: '/:pathMatch(.*)*', redirect: '/404' },
  {
    path: '/',
    component: Welcome,
    name: 'welcome',
    meta: {
      title: 'Welcome',
      isPublic: true,
      publicOnly: true
    }
  },
  // {
  //   path: '/home',
  //   component: Home,
  //   name: 'home',
  //   meta: {
  //     title: 'Home'
  //   }
  // },
  // {
  //   path: '/fantasyCriticPlus',
  //   component: FantasyCriticPlus,
  //   name: 'fantasyCriticPlus',
  //   meta: {
  //     title: 'Fantasy Critic Plus',
  //     isPublic: true
  //   }
  // },
  {
    path: '/about',
    component: About,
    name: 'about',
    meta: {
      title: 'About',
      isPublic: true
    }
  },
  // {
  //   path: '/discord-bot',
  //   component: DiscordBot,
  //   name: 'discordBot',
  //   meta: {
  //     title: 'Discord Bot',
  //     isPublic: true
  //   }
  // },
  // {
  //   path: '/howtoplay',
  //   component: HowToPlay,
  //   name: 'howtoplay',
  //   meta: {
  //     title: 'How to Play',
  //     isPublic: true
  //   }
  // },
  // {
  //   path: '/faq',
  //   component: Faq,
  //   name: 'faq',
  //   meta: {
  //     title: 'Frequently Asked Questions',
  //     isPublic: true
  //   }
  // },
  {
    path: '/contact',
    component: Contact,
    name: 'contact',
    meta: {
      title: 'Contact',
      isPublic: true
    }
  },
  // {
  //   path: '/games',
  //   component: MasterGames,
  //   name: 'masterGames',
  //   meta: {
  //     title: 'Games',
  //     isPublic: true
  //   }
  // },
  // {
  //   path: '/gameChanges',
  //   component: RecentMasterGameChanges,
  //   name: 'gameChanges',
  //   meta: {
  //     title: 'Game Changes',
  //     isPublic: true
  //   }
  // },
  // {
  //   path: '/masterGameRequest',
  //   component: MasterGameRequest,
  //   name: 'masterGameRequest',
  //   meta: {
  //     title: 'Master Game Request'
  //   }
  // },
  // {
  //   path: '/masterGameChangeRequest',
  //   component: MasterGameChangeRequest,
  //   name: 'masterGameChangeRequest',
  //   meta: {
  //     title: 'Master Game Correction'
  //   }
  // },
  // {
  //   path: '/publicLeagues',
  //   component: PublicLeagues,
  //   name: 'publicLeagues',
  //   meta: {
  //     title: 'Public Leagues',
  //     isPublic: true
  //   }
  // },
  // {
  //   path: '/createLeague',
  //   component: CreateLeague,
  //   name: 'createLeague',
  //   meta: {
  //     title: 'Create League'
  //   }
  // },
  // {
  //   path: '/league/:leagueid/:year',
  //   component: League,
  //   name: 'league',
  //   meta: {
  //     title: 'League',
  //     isPublic: true,
  //     delayScroll: true
  //   },
  //   props: (route) => {
  //     let parsedYear = Number.parseInt(route.params.year, 10);
  //     if (Number.isNaN(parsedYear)) {
  //       parsedYear = 0;
  //     }

  //     return {
  //       leagueid: route.params.leagueid,
  //       year: parsedYear
  //     };
  //   }
  // },
  // {
  //   path: '/editLeague/:leagueid/:year',
  //   component: EditLeague,
  //   name: 'editLeague',
  //   meta: {
  //     title: 'Edit League'
  //   },
  //   props: (route) => {
  //     let parsedYear = Number.parseInt(route.params.year, 10);
  //     if (Number.isNaN(parsedYear)) {
  //       parsedYear = 0;
  //     }

  //     return {
  //       leagueid: route.params.leagueid,
  //       year: parsedYear
  //     };
  //   }
  // },
  // {
  //   path: '/leagueHistory/:leagueid/:year',
  //   component: LeagueHistory,
  //   name: 'leagueHistory',
  //   meta: {
  //     title: 'League History',
  //     isPublic: true
  //   },
  //   props: (route) => {
  //     let parsedYear = Number.parseInt(route.params.year, 10);
  //     if (Number.isNaN(parsedYear)) {
  //       parsedYear = 0;
  //     }

  //     return {
  //       leagueid: route.params.leagueid,
  //       year: parsedYear
  //     };
  //   }
  // },
  // {
  //   path: '/publisher/:publisherid',
  //   component: Publisher,
  //   name: 'publisher',
  //   meta: {
  //     title: 'Publisher',
  //     isPublic: true
  //   },
  //   props: true
  // },
  // {
  //   path: '/mastergame/:mastergameid',
  //   component: MasterGame,
  //   name: 'mastergame',
  //   meta: {
  //     title: 'Master Game',
  //     isPublic: true
  //   },
  //   props: true
  // },
  // {
  //   path: '/mastergameeditor/:mastergameid',
  //   component: MasterGameEditor,
  //   name: 'masterGameEditor',
  //   meta: {
  //     title: 'Master Game Editor'
  //   },
  //   props: true
  // },
  // {
  //   path: '/adminConsole',
  //   component: AdminConsole,
  //   name: 'adminConsole',
  //   meta: {
  //     title: 'Admin Console',
  //     adminOnly: true
  //   }
  // },
  // {
  //   path: '/activeMasterGameRequests',
  //   component: ActiveMasterGameRequests,
  //   name: 'activeMasterGameRequests',
  //   meta: {
  //     title: 'Active Master Game Requests',
  //     adminOnly: true
  //   }
  // },
  // {
  //   path: '/activeMasterGameChangeRequests',
  //   component: ActiveMasterGameChangeRequests,
  //   name: 'activeMasterGameChangeRequests',
  //   meta: {
  //     title: 'Active Master Game Change Requests',
  //     adminOnly: true
  //   }
  // },
  // {
  //   path: '/masterGameCreator',
  //   component: MasterGameCreator,
  //   name: 'masterGameCreator',
  //   meta: {
  //     title: 'Master Game Creator',
  //     adminOnly: true
  //   }
  // },
  // {
  //   path: '/actionProcessingDryRunResults',
  //   component: ActionProcessingDryRunResults,
  //   name: 'actionProcessingDryRunResults',
  //   meta: {
  //     title: 'Action Processing Dry Run Results',
  //     adminOnly: true
  //   }
  // },
  // {
  //   path: '/criticsRoyale/:year?/:quarter?',
  //   component: CriticsRoyale,
  //   name: 'criticsRoyale',
  //   meta: {
  //     title: 'Critics Royale',
  //     isPublic: true
  //   },
  //   props: (route) => {
  //     if (!route.params.year || !route.params.quarter) {
  //       return;
  //     }

  //     const parsedYear = Number.parseInt(route.params.year, 10);
  //     if (Number.isNaN(parsedYear)) {
  //       return;
  //     }

  //     const parsedQuarter = Number.parseInt(route.params.quarter, 10);
  //     if (Number.isNaN(parsedQuarter)) {
  //       return;
  //     }

  //     return {
  //       year: parsedYear,
  //       quarter: parsedQuarter
  //     };
  //   }
  // },
  // {
  //   path: '/royalePublisher/:publisherid',
  //   component: RoyalePublisher,
  //   name: 'royalePublisher',
  //   meta: {
  //     title: 'Royale Publisher',
  //     isPublic: true
  //   },
  //   props: true
  // }
];
