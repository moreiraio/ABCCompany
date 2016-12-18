# ABCCompany
> Livit Exercice.


### Business Requirements

Develop an API back-end using ASP.NET Core, which can:
1. Allow the users to login with their Google email.
2. Add an absence request to the system.
3. Allow the Admin can approve or decline the request.
4. Approved requests are shared automatically in the user’s calendar.### Software PlatformFor plataform as requested it was used ASP.NET Core```
 asp.core
 version": "1.1.0
```

### Configuration

On the appsettings.json file the folowing settings need to be configured:
```
 "AdminUser": "" -> the gmail email for the adminuser
 "ClientId": "" -> google api clientid
 "ClientSecret": "" -> google api client secret
 "GroupId": "" -> the gmail group that contains the emails of the admins
```
### Installation and Setup

After gettinf the source code open an command line on the source folder and run the following commands:
 
setup the database and create the model
```sh
dotnet ef migrations add v1
dotnet ef database update
```

to run the application just
```sh
dotnet run
```

### Running the application


```sh
dotnet ef migrations add v1
dotnet ef database update
```

```sh
dotnet run
```


## About

Vitor Moreira [@vitormoreira] (https://twitter.com/vitormoreira)  vitor.moreira@gmail.com

[https://github.com/vrmmoreira/ABCCompany](https://github.com/vrmmoreira/)

