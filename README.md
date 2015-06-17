# ServiceStackGoogleAuthAzure
Reproduction of error 502 on Azure with Google Auth and ServiceStack

Setup manual:
1. Create Azure Web Site with new database and storage
1. Web config changes
```XML
  <appSettings>
	<add key="AppDb" value="{Azure connection string}" />
    <!-- Auth config Azure -->
    <add key="oauth.RedirectUrl" value="http://{yourAzureWebName}.azurewebsites.net/" />
    <add key="oauth.CallbackUrl" value="http://{yourAzureWebName}.azurewebsites.net/api/auth/{0}" />
    <!-- Facebook LOCALHOST App at: https://developers.facebook.com/apps -->
    <add key="oauth.facebook.Permissions" value="public_profile,email,user_friends" />
    <add key="oauth.facebook.AppId" value="{yourAppId}" />
    <add key="oauth.facebook.AppSecret" value="{yourAppSecret}" />
    <!-- Google LOCALHOST App at: https://console.developers.google.com/project -->
    <add key="oauth.GoogleOAuth.ConsumerKey" value="{yourAppId}"/>
    <add key="oauth.GoogleOAuth.ConsumerSecret" value="{yourAppSecret}" />
  </appSettings>	
```

Test publish on: http://googleauthtest.azurewebsites.net/