import Welcome from "components/pages/welcome";
import Home from "components/pages/home";
import Login from "components/pages/login";
import Register from "components/pages/register";
import About from "components/pages/about";
import HowToPlay from "components/pages/howtoplay";
import Faq from "components/pages/faq";
import Contact from "components/pages/contact";
import ManageUser from "components/pages/manageUser";
import CreateLeague from "components/pages/createLeague";
import League from "components/pages/league";
import EditLeague from "components/pages/editLeague";
import Publisher from "components/pages/publisher";
import MasterGame from "components/pages/masterGame";
import ConfirmEmail from "components/pages/confirmEmail";
import ForgotPassword from "components/pages/forgotPassword";
import ResetPassword from "components/pages/resetPassword";
import ChangeEmail from "components/pages/changeEmail";
import MasterGames from "components/pages/masterGames";
import PublicLeagues from "components/pages/publicLeagues";
import LeagueHistory from "components/pages/leagueHistory";
import AdminConsole from "components/pages/adminConsole";
import MasterGameRequest from "components/pages/masterGameRequest";
import MasterGameChangeRequest from "components/pages/masterGameChangeRequest";
import ActiveMasterGameRequests from "components/pages/activeMasterGameRequests";
import ActiveMasterGameChangeRequests from "components/pages/activeMasterGameChangeRequests";
import MasterGameCreator from "components/pages/masterGameCreator";
import CurrentFailingBids from "components/pages/currentFailingBids";
import CriticsRoyale from "components/pages/criticsRoyale";
import RoyalePublisher from "components/pages/royalePublisher";
import NotFound from "components/pages/notFound";

export const routes = [
  { path: '/404', component: NotFound },
  { path: '*', redirect: '/404' },  
  {
    path: "/",
    component: Welcome,
    name: "welcome",
    meta: {
      title: "Welcome",
      isPublic: true,
      publicOnly: true
    }
  },
  {
    path: "/login",
    component: Login,
    name: "login",
    meta: {
      title: "Login",
      isPublic: true,
      publicOnly: true
    }
  },
  {
    path: "/home",
    component: Home,
    name: "home",
    meta: {
      title: "Home"
    }
  },
  {
    path: "/forgotPassword",
    component: ForgotPassword,
    name: "forgotPassword",
    meta: {
      title: "Forgot Password",
      isPublic: true,
      publicOnly: true
    }
  },
  {
    path: "/resetPassword",
    component: ResetPassword,
    name: "resetPassword",
    meta: {
      title: "Reset Password",
      isPublic: true,
      publicOnly: true
    }
  },
  {
    path: "/changeEmail",
    component: ChangeEmail,
    name: "changeEmail",
    meta: {
      title: "Change Email",
      isPublic: false
    }
  },
  {
    path: "/register",
    component: Register,
    name: "register",
    meta: {
      title: "Register",
      isPublic: true,
      publicOnly: true
    }
  },
  {
    path: "/about",
    component: About,
    name: "about",
    meta: {
      title: "About",
      isPublic: true
    }
  },
  {
    path: "/howtoplay",
    component: HowToPlay,
    name: "howtoplay",
    meta: {
      title: "How to Play",
      isPublic: true
    }
  },
  {
    path: "/faq",
    component: Faq,
    name: "faq",
    meta: {
      title: "Frequently Asked Questions",
      isPublic: true
    }
  },
  {
    path: "/contact",
    component: Contact,
    name: "contact",
    meta: {
      title: "Contact",
      isPublic: true
    }
  },
  {
    path: "/games",
    component: MasterGames,
    name: "masterGames",
    meta: {
      title: "Games",
      isPublic: true
    }
  },
  {
    path: "/masterGameRequest",
    component: MasterGameRequest,
    name: "masterGameRequest",
    meta: {
      title: "Request a Master Game"
    }
  },
  {
    path: "/masterGameChangeRequest",
    component: MasterGameChangeRequest,
    name: "masterGameChangeRequest",
    meta: {
      title: "Request a Master Game Change"
    }
  },
  {
    path: "/publicLeagues",
    component: PublicLeagues,
    name: "publicLeagues",
    meta: {
      title: "Public Leagues",
      isPublic: true
    }
  },
  {
    path: "/manageUser",
    component: ManageUser,
    name: "manageUser",
    meta: {
      title: "Manage User"
    }
  },
  {
    path: "/confirmEmail",
    component: ConfirmEmail,
    name: "confirmEmail",
    meta: {
      title: "Confirm Email",
      isPublic: true
    }
  },
  {
    path: "/createLeague",
    component: CreateLeague,
    name: "createLeague",
    meta: {
      title: "Create League"
    }
  },
  {
    path: "/league/:leagueid/:year",
    component: League,
    name: "league",
    meta: {
      title: "League",
      isPublic: true
    },
    props: (route) => ({
      leagueid: route.params.leagueid,
      year: route.params.year
    })
  },
  {
    path: "/editLeague/:leagueid/:year",
    component: EditLeague,
    name: "editLeague",
    meta: {
      title: "Edit League"
    },
    props: (route) => ({
      leagueid: route.params.leagueid,
      year: route.params.year
    })
  },
  {
    path: "/leagueHistory/:leagueid/:year",
    component: LeagueHistory,
    name: "leagueHistory",
    meta: {
      title: "League History",
      isPublic: true
    },
    props: (route) => ({
      leagueid: route.params.leagueid,
      year: route.params.year
    })
  },
  {
    path: "/publisher/:publisherid",
    component: Publisher,
    name: "publisher",
    meta: {
      title: "Publisher",
      isPublic: true
    },
    props: (route) => ({
      publisherid: route.params.publisherid
    })
  },
  {
    path: "/mastergame/:mastergameid",
    component: MasterGame,
    name: "mastergame",
    meta: {
      title: "Master Game",
      isPublic: true
    },
    props: (route) => ({
      mastergameid: route.params.mastergameid
    })
  },
  {
    path: "/adminConsole",
    component: AdminConsole,
    name: "adminConsole",
    meta: {
      title: "Admin Console",
      isPublic: false,
      adminOnly: true
    }
  },
  {
    path: "/activeMasterGameRequests",
    component: ActiveMasterGameRequests,
    name: "activeMasterGameRequests",
    meta: {
      title: "Active Master Game Requests",
      isPublic: false,
      adminOnly: true
    }
  },
  {
    path: "/activeMasterGameChangeRequests",
    component: ActiveMasterGameChangeRequests,
    name: "activeMasterGameChangeRequests",
    meta: {
      title: "Active Master Game Change Requests",
      isPublic: false,
      adminOnly: true
    }
  },
  {
    path: "/masterGameCreator",
    component: MasterGameCreator,
    name: "masterGameCreator",
    meta: {
      title: "Master Game Creator",
      isPublic: false,
      adminOnly: true
    }
  },
  {
    path: "/currentFailingBids",
    component: CurrentFailingBids,
    name: "currentFailingBids",
    meta: {
      title: "Current Failing Bids",
      isPublic: false,
      adminOnly: true
    }
  },
  {
    path: "/criticsRoyale/:year/:quarter",
    component: CriticsRoyale,
    name: "criticsRoyale",
    meta: {
      title: "Critics Royale",
      isPublic: true
    },
    props: (route) => ({
      year: route.params.year,
      quarter: route.params.quarter
    })
  },
  {
    path: "/royalePublisher/:publisherid",
    component: RoyalePublisher,
    name: "royalePublisher",
    meta: {
      title: "Royale Publisher",
      isPublic: true
    },
    props: (route) => ({
      publisherid: route.params.publisherid
    })
  },
];
