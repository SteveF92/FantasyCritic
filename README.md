# Fantasy Critic
Fantasy Critic is like fantasy football...but for video games! You and several friends will create your own virtual video game “Publishers”,
          and then you will assemble a roster of the games you believe will review the best. Similar to fantasy football, the players will alternate
          picking games in a snake draft style to start the year. At the end of the year, the winner is the team with the best lineup of video games
          based on scores from <a href="https://opencritic.com/">opencritic.com</a>.

You can see the site at  <a href="https://www.fantasycritic.games/">fantasycritic.games</a>

## Development Setup

### Option 1: Running the client side only.
ClientApp is the root directory for the front-end UI. In it contains a Vue.js client application. See the official [Vue.js documentation](https://vuejs.org/) for more
information. The project is configured to start its own instance of the client app in the background when the ASP.NET Core app starts in development mode. The client
app can also be run via webpack dev server, instead of the ASP.NET core app. Note, when using the webpack dev server, the client app will point to the 
[official website](https://www.fantasycritic.games/), instead of the local ASP.NET core app.
```sh
> npm run client
```

### Option 2: Full Setup
You'll need to install a few things:

* .NET 6.0
* A C# IDE (I use Visual Studio 2022)
* MySQL

Once you have everything installed, you'll need to run the MySQL scripts to get your database set up. You can find them in "Database" folder. Start with the folder in "SchemaCreates" with the highest version number. Run "FCTableCreate.sql" and then "FCDataInsert.sql" on your local MySQL server. Then check the "SchemaUpdates" folder. If there are any files with a version number higher than the SchemaCreate that you ran, you'll need to run those too. The naming scheme is:

    FC_{FromVersion}-{ToVersion}_{Description}.sql
    
You're looking for any file with a {ToVersion} higher than your "SchemaCreate" folder version.

Once your database is setup, next up is to get the code running. The best way to make sure the code is at least building is to run the unit tests. Assuming they pass, you can move onto running the web site. You'll need to set up "User Secrets", which you can find documentation on here:

https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows

For most use cases, the only user secret you'll need to set is the MySQL connection string, which will be to your local MySQL server.

With that all set up, the site should run like normal via Visual Studio or via 'dotnet run' on the command line.

#### Some extra notes

* In the "Utilities" Solution folder, you'll find the project "FantasyCritic.MasterGameUpdater". This will update your local database with the latest game data from the actual Fantasy Critic site. Things like new announcements, release dates, etc. Reminder: You'll need to setup "user secrets" here too.

* I use full Visual Studio for server side development, but I find that Visual Studio Code works better when I'm working on the client side (Vue.JS) code.
