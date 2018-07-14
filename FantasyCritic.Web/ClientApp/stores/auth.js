// auth.js
import axios from "axios";

export default {
    state: {
        jwt: null,
        expiration: null,
        redirect: "",
        emailAddress: ""
    },
    getters: {
        isAuthenticated(state) {
            if (state.jwt === null || state.expiration === null) {
                return false;
            }
            var expire = state.expiration;
            var now = new Date();
            var before = expire > now;

            return state.jwt && before;
        },
        token: (state) => state.jwt,
        redirect: (state) => state.redirect,
        emailAddress: (state) => state.emailAddress
    },
    actions: {
        doAuthentication(context, creds) {
            return new Promise(function (resolve, reject) {
                axios.post("/api/account/login", creds)
                    .then((res) => {
                        context.commit("setTokenInfo", res.data);
                    })
                    .catch(error => {
                        reject();
                    });
            });
        },
        logout(context) {
            return new Promise(function (resolve, reject) {
                context.commit("clearToken");
                resolve();
            });
        },
    },
    mutations: {
        setTokenInfo(state, tokenInfo) {
            localStorage.setItem('jwt_token', tokenInfo.token);
            localStorage.setItem('jwt_expiration', tokenInfo.expiration);
            state.jwt = tokenInfo.token;
            state.expiration = new Date(tokenInfo.expiration);
            axios.defaults.headers.common['Authorization'] = 'Bearer ' + tokenInfo.token;
        },
        clearToken(state) {
            localStorage.removeItem('jwt_token');
            localStorage.removeItem('jwt_expiration');
            state.jwt = null;
            state.expiration = null;
            state.emailAddress = "";
            axios.defaults.headers.common['Authorization'] = "";
        },
        setRedirect(state, path) {
            state.redirect = path;
        },
        clearRedirect(state) {
            state.redirect = "";
        }
    }
}
