# SpaceWatcher  
Little personal project to implement OAuth2/OIDC with Keycloak as IAM  

## Commandes  

### Visual Studio Code  
DEV :   
=> docker-compose -f docker-compose.dev.yml --env-file .env.dev up --build  
=> dotnet run --launch-profile Dev  
  
PROD :  
=> docker-compose -f docker-compose.prod.yml --env-file .env.prod up --build  
=> dotnet run --launch-profile Prod    

