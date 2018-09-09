import Home from "components/pages/home";
import Login from "components/pages/login";
import Register from "components/pages/register";
import About from "components/pages/about";
import Contact from "components/pages/contact";
import ManageUser from "components/pages/manageUser";
import CreateLeague from "components/pages/createLeague";
import League from "components/pages/league";
import Publisher from "components/pages/publisher";
import ConfirmEmail from "components/pages/confirmEmail";
import ForgotPassword from "components/pages/forgotPassword";
import ResetPassword from "components/pages/resetPassword";

export const routes = [
  {
    path: "/",
    component: Home,
    name: "home"
  },
  {
    path: "/login",
    component: Login,
    name: "login",
    meta: {
      isPublic: true
    }
  },
  {
    path: "/forgotPassword",
    component: ForgotPassword,
    name: "forgotPassword",
    meta: {
      isPublic: true
    }
  },
  {
    path: "/resetPassword",
    component: ResetPassword,
    name: "resetPassword",
    meta: {
      isPublic: true
    }
  },
  {
    path: "/register",
    component: Register,
    name: "register",
    meta: {
      isPublic: true
    }
  },
  {
    path: "/about",
    component: About,
    name: "about",
    meta: {
      isPublic: true
    }
  },
  {
    path: "/contact",
    component: Contact,
    name: "contact",
    meta: {
      isPublic: true
    }
  },
  {
    path: "/manageUser",
    component: ManageUser,
    name: "manageUser"
  },
  {
    path: "/confirmEmail",
    component: ConfirmEmail,
    name: "confirmEmail"
  },
  {
    path: "/createLeague",
    component: CreateLeague,
    name: "createLeague"
  },
  {
    path: "/league/:leagueid/:year",
    component: League,
    name: "league",
    props: (route) => ({
      leagueid: route.params.leagueid,
      year: route.params.year
    })
  },
  {
    path: "/publisher/:publisherid",
    component: Publisher,
    name: "publisher",
    props: (route) => ({
      publisherid: route.params.publisherid
    })
  }
];
