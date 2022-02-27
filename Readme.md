# Serene Marine - Beach Clean Up

![Serene Marine - Home page](https://user-images.githubusercontent.com/60667206/155885803-d1511fcc-d72a-475d-9a39-51f7e878d159.png)

1. [Summary](#summary)
    1. [Key Notes](#key-notes)
    2. [Tech Stack](#tech-stack)
    3. [Login Details for Admin](#login-details-for-admin)
2. [Getting Started](#getting-started)
    1. [Requirements](#requirements)
    2. [Steps](#steps)
    4. [Issues & Solutions](#issues-and-solutions)
    
## Summary

Final year University project aiming to increase the participation of beach clean up events by providing an easy-to-use platform for people to find and join events. Provides additional features such as Petitions, Threads and Donation channels.

### Key Notes

- Project has been configured and updated to run locally.
- Database has been setup to use MongoDB, make sure check your connection string.
- When running the project for the first time, it will create the database and load default data.
- MapBox GL JS has been integrated for marking and setting locations for events.
- Admins can CRUD Events, Petitions and Threads. 
- Users can participate in Events, Petitions and Threads. 
- Anonymous users can join events using their email address.

### Tech Stack
1. ASP.NET core WebApi (*API*)
2. ASP.NET Core WebApp MVC (*front-end & back-end*)
3. MongoDB (*database*)

### Login Details for Admin
|Email| Password |
|---|---|
| admin@serenemarine.com | 123456|

## Getting Started

### Requirements
-	[Visual Studio](https://visualstudio.microsoft.com/vs/community/)
-	[Asp.Net](https://dotnet.microsoft.com/apps/aspnet)
-	[IIS Express](https://www.microsoft.com/en-us/download/details.aspx?id=48264)
-	[Internet Browser](https://www.google.com/chrome/) (Google Chrome)
-	[MongoDB Atlas](https://www.mongodb.com/try) or On-premise [MongoDB Locally](https://www.mongodb.com/try/download/community) (Instructions at **Steps 2. Setup MongoDB**)

### Steps

#### 1. Visual Studio
1.  [Clone](https://docs.github.com/en/repositories/creating-and-managing-repositories/cloning-a-repository#cloning-a-repository)  or [Fork](https://docs.github.com/en/get-started/quickstart/fork-a-repo#forking-a-repository) the repo.
2.	In the folder, double-click the .sln file to open the solution.
3.  Set the Solution to startup using Multiple Projects:
    1. In the Solution Explorer, Right-click the 'Solution SereneMarine' and select 'Properties'
    2. In the left-menu tab under 'Common Properties' select 'Startup Project'
    3. Choose 'Multiple startup projects' radio checkbox option.
    4. Set both projects to Action -> Start.
    Make sure WebApi starts before the website.
    5. Make Sure <b>WebApi</b> is above <b>SereneMarine_Web</b>.
4.	Clean and Rebuild the entire Solution.

#### 2. Setup Your MongoDB

##### 2.1 MongoDB Atlas (Cloud Version, easiest option and less setup)
1. [Create an organization](https://docs.atlas.mongodb.com/tutorial/manage-organizations/)
2. [Create a Project](https://docs.atlas.mongodb.com/tutorial/manage-projects/#create-a-project)
3. [Create a Free Shared Cluster](https://docs.atlas.mongodb.com/tutorial/deploy-free-tier-cluster/)
4. [Add your connection IP Address to IP Access List](https://docs.atlas.mongodb.com/security/add-ip-address-to-list/)
5. [Create a Database User for Your Cluster](https://docs.atlas.mongodb.com/tutorial/create-mongodb-user-for-cluster/#create-a-database-user-for-your-cluster)

##### 2.2 On-premise MongoDB (local version, requires setup)
1. [Download MongoDB Community Server](https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.4-signed.msi)
2. [Run the MongoDB installer](https://docs.mongodb.com/manual/tutorial/install-mongodb-on-windows/#run-the-mongodb-installer)
3. [Follow the MongoDB Community Edition installation wizard](https://docs.mongodb.com/manual/tutorial/install-mongodb-on-windows/#follow-the-mongodb-edition-installation-wizard) and **uncheck** 'Install MongoD as a Service'.
4. Make sure the mongo database server is running
    1. Locate your MongoDB Server install and double-click the 'mongod.exe' file. This is usually located in 
        1. 'C:\Program Files\MongoDB\Server\<version>\bin\mongod.exe'

#### 3. Update Database Connection String

1. In Solution Explorer, drop-down the <b>WebAPI</b> project and double-click the 'appsettings.json' file.
2. Under 'UserDatabaseSettings', replace the field value for 'ConnectionString' with your connection string.
    1. MongoDB Atlas (cloud)
        1. In your Database Deployments View, click the 'Connect' button for the cluster you created.
        2. Select 'Connect your application'
        3. For Driver, select C# /.NET
        4. For Version, select 2.5 or later
        5. Make sure 'Include full drive code example' is **unchecked**.
        6. Copy the connection string provided.
3. If there is a \<password\> tag in your connection string, make sure to replace this with your password for the user.

#### 4. Update MapBox Api Key String

1. Follow [How-to add url Restrictions to access token](https://blog.mapbox.com/url-restrictions-for-access-tokens-5f7f7eb90092) and get copy your token.
2. In the Solution Explorer, drop-down the <b>SereneMarine_Web</b> project and double-click the 'appsettings.json' file.
3. Under 'AppSettings', replace the value for the key 'MapboxAPIkey'.

### Issues and Solutions
1. Project does not run both files
    * Make sure that for your project selection (next to the Start button), that it is 'Multiple Startup Projects'.
    
2. No option for 'Multiple Startup Projects'.
    * In the Solution Explorer, Right-click the 'Solution Serenemarine' and select 'Properties'
    * In the left-menu tab, dropdown Common Properties, select 'Startup Project', and then select 'Multiple startup projects' radio checkbox option.
    * Set both projects to Action -> Start.
    * Make sure WebApi starts before the website.
