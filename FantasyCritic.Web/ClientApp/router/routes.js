import Home from "components/pages/home";
import Login from "components/pages/login";
import ForgotPassword from "components/pages/forgotPassword";

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
  }
];
