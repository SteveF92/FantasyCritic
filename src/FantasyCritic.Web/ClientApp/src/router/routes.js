import Welcome from '@/views/welcome';
import Home from '@/views/home';
import About from '@/views/about';
import HowToPlay from '@/views/howToPlay';
import Faq from '@/views/faq';
import Contact from '@/views/contact';
import CreateLeague from '@/views/createLeague';
import League from '@/views/league';
import EditLeague from '@/views/editLeague';
import Publisher from '@/views/publisher';
import MasterGame from '@/views/masterGame';
import MasterGames from '@/views/masterGames';
import PublicLeagues from '@/views/publicLeagues';
import LeagueHistory from '@/views/leagueHistory';
import AdminConsole from '@/views/adminConsole';
import MasterGameRequest from '@/views/masterGameRequest';
import MasterGameChangeRequest from '@/views/masterGameChangeRequest';
import ActiveMasterGameRequests from '@/views/activeMasterGameRequests';
import ActiveMasterGameChangeRequests from '@/views/activeMasterGameChangeRequests';
import MasterGameCreator from '@/views/masterGameCreator';
import ActionProcessingDryRunResults from '@/views/actionProcessingDryRunResults';
import CriticsRoyale from '@/views/criticsRoyale';
import RoyalePublisher from '@/views/royalePublisher';
import NotFound from '@/views/notFound';
import MasterGameEditor from '@/views/masterGameEditor';
import FantasyCriticPlus from '@/views/fantasyCriticPlus';

export const routes = [
  { path: '/404', component: NotFound },
  { path: '*', redirect: '/404' },
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
  {
    path: '/home',
    component: Home,
    name: 'home',
    meta: {
      title: 'Home'
    }
  },
  {
    path: '/fantasyCriticPlus',
    component: FantasyCriticPlus,
    name: 'fantasyCriticPlus',
    meta: {
      title: 'Fantasy Critic Plus',
      isPublic: true
    }
  },
  {
    path: '/about',
    component: About,
    name: 'about',
    meta: {
      title: 'About',
      isPublic: true
    }
  },
  {
    path: '/howtoplay',
    component: HowToPlay,
    name: 'howtoplay',
    meta: {
      title: 'How to Play',
      isPublic: true
    }
  },
  {
    path: '/faq',
    component: Faq,
    name: 'faq',
    meta: {
      title: 'Frequently Asked Questions',
      isPublic: true
    }
  },
  {
    path: '/contact',
    component: Contact,
    name: 'contact',
    meta: {
      title: 'Contact',
      isPublic: true
    }
  },
  {
    path: '/games',
    component: MasterGames,
    name: 'masterGames',
    meta: {
      title: 'Games',
      isPublic: true
    }
  },
  {
    path: '/masterGameRequest',
    component: MasterGameRequest,
    name: 'masterGameRequest',
    meta: {
      title: 'Master Game Request'
    }
  },
  {
    path: '/masterGameChangeRequest',
    component: MasterGameChangeRequest,
    name: 'masterGameChangeRequest',
    meta: {
      title: 'Master Game Correction'
    }
  },
  {
    path: '/publicLeagues',
    component: PublicLeagues,
    name: 'publicLeagues',
    meta: {
      title: 'Public Leagues',
      isPublic: true
    }
  },
  {
    path: '/createLeague',
    component: CreateLeague,
    name: 'createLeague',
    meta: {
      title: 'Create League'
    }
  },
  {
    path: '/league/:leagueid/:year',
    component: League,
    name: 'league',
    meta: {
      title: 'League',
      isPublic: true
    },
    props: (route) => {
      let parsedYear = Number.parseInt(route.params.year, 10);
      if (Number.isNaN(parsedYear)) {
        parsedYear = 0;
      }

      return {
        leagueid: route.params.leagueid,
        year: parsedYear
      };
    }
  },
  {
    path: '/editLeague/:leagueid/:year',
    component: EditLeague,
    name: 'editLeague',
    meta: {
      title: 'Edit League'
    },
    props: (route) => {
      let parsedYear = Number.parseInt(route.params.year, 10);
      if (Number.isNaN(parsedYear)) {
        parsedYear = 0;
      }

      return {
        leagueid: route.params.leagueid,
        year: parsedYear
      };
    }
  },
  {
    path: '/leagueHistory/:leagueid/:year',
    component: LeagueHistory,
    name: 'leagueHistory',
    meta: {
      title: 'League History',
      isPublic: true
    },
    props: (route) => {
      let parsedYear = Number.parseInt(route.params.year, 10);
      if (Number.isNaN(parsedYear)) {
        parsedYear = 0;
      }

      return {
        leagueid: route.params.leagueid,
        year: parsedYear
      };
    }
  },
  {
    path: '/publisher/:publisherid',
    component: Publisher,
    name: 'publisher',
    meta: {
      title: 'Publisher',
      isPublic: true
    },
    props: (route) => ({
      publisherid: route.params.publisherid
    })
  },
  {
    path: '/mastergame/:mastergameid',
    component: MasterGame,
    name: 'mastergame',
    meta: {
      title: 'Master Game',
      isPublic: true
    },
    props: (route) => ({
      mastergameid: route.params.mastergameid
    })
  },
  {
    path: '/mastergameeditor/:mastergameid',
    component: MasterGameEditor,
    name: 'masterGameEditor',
    meta: {
      title: 'Master Game Editor',
      isPublic: false
    },
    props: (route) => ({
      mastergameid: route.params.mastergameid
    })
  },
  {
    path: '/adminConsole',
    component: AdminConsole,
    name: 'adminConsole',
    meta: {
      title: 'Admin Console',
      isPublic: false,
      adminOnly: true
    }
  },
  {
    path: '/activeMasterGameRequests',
    component: ActiveMasterGameRequests,
    name: 'activeMasterGameRequests',
    meta: {
      title: 'Active Master Game Requests',
      isPublic: false,
      adminOnly: true
    }
  },
  {
    path: '/activeMasterGameChangeRequests',
    component: ActiveMasterGameChangeRequests,
    name: 'activeMasterGameChangeRequests',
    meta: {
      title: 'Active Master Game Change Requests',
      isPublic: false,
      adminOnly: true
    }
  },
  {
    path: '/masterGameCreator',
    component: MasterGameCreator,
    name: 'masterGameCreator',
    meta: {
      title: 'Master Game Creator',
      isPublic: false,
      adminOnly: true
    }
  },
  {
    path: '/actionProcessingDryRunResults',
    component: ActionProcessingDryRunResults,
    name: 'actionProcessingDryRunResults',
    meta: {
      title: 'Action Processing Dry Run Results',
      isPublic: false,
      adminOnly: true
    }
  },
  {
    path: '/criticsRoyale/:year?/:quarter?',
    component: CriticsRoyale,
    name: 'criticsRoyale',
    meta: {
      title: 'Critics Royale',
      isPublic: true
    },
    props: (route) => {
      if (!route.params.year || !route.params.quarter) {
        return;
      }

      let parsedYear = Number.parseInt(route.params.year, 10);
      if (Number.isNaN(parsedYear)) {
        return;
      }

      let parsedQuarter = Number.parseInt(route.params.quarter, 10);
      if (Number.isNaN(parsedQuarter)) {
        return;
      }

      return {
        year: parsedYear,
        quarter: parsedQuarter
      };
    }
  },
  {
    path: '/royalePublisher/:publisherid',
    component: RoyalePublisher,
    name: 'royalePublisher',
    meta: {
      title: 'Royale Publisher',
      isPublic: true
    },
    props: (route) => ({
      publisherid: route.params.publisherid
    })
  }
];
