import Welcome from "components/pages/welcome";
import Home from "components/pages/home";
import Login from "components/pages/login";
import Register from "components/pages/register";
import About from "components/pages/about";
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

export const routes = [
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
    path: "/contact",
    component: Contact,
    name: "contact",
    meta: {
      title: "Contact",
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
      title: "Confirm Email"
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
      title: "League"
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
    path: "/publisher/:publisherid",
    component: Publisher,
    name: "publisher",
    meta: {
      title: "Publisher"
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
      title: "Master Game"
    },
    props: (route) => ({
      mastergameid: route.params.mastergameid
    })
  }
];
