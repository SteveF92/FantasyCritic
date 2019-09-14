## Fantasy Critic
Fantasy Critic is like fantasy football...but for video games! You and several friends will create your own virtual video game “Publishers”,
          and then you will assemble a roster of the games you believe will review the best. Similar to fantasy football, the players will alternate
          picking games in a snake draft style to start the year. At the end of the year, the winner is the team with the best lineup of video games
          based on scores from <a href="https://opencritic.com/">opencritic.com</a>.

You can see the site at  <a href="https://www.fantasycritic.games/">fantasycritic.games</a>

### ClientApp
ClientApp is the root directory for the front-end UI. In it contains a Vue.js client application. See the official [Vue.js documentation](https://vuejs.org/) for more
information. The project is configured to start its own instance of the client app in the background when the ASP.NET Core app starts in development mode. The client
app can also be run via webpack dev server, instead of the ASP.NET core app. Note, when using the webpack dev server, the client app will point the 
[official website](https://www.fantasycritic.games/), instead of the local ASP.NET core app.
```sh
> npm run client
```