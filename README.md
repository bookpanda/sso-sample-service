# SSO: Sample Service

## What is SSO
Single Sign-On (SSO) is a user authentication process that allows a user to access multiple applications with one set of login credentials. This is a common practice in the enterprise, where a user can log in to their computer and, with a single login, gain access to all of the applications they need to do their job.

This project is a very simple implementation of SSO.

## Demo
#### CAS: https://sso-cas.bookpanda.dev
#### Service 1: https://sso-svc-1.bookpanda.dev
#### Service 2: https://sso-svc-2.bookpanda.dev
#### You can see that:
- Once you logged in to CAS either directly or through a service, you can access the ALL other services without having to do Google log in again.
- A logout from service 1 or 2 doesn't log you out from CAS.
- A logout from CAS logs you out from all services.

## Services
### CAS (Central Authentication Service)
This is the main service that handles the authentication of the user and also has a user management system. In real world, this would be something like the SSO part of [Okta](https://www.okta.com) or [HENNGE One](https://hennge.com/global/henngeone) service.
#### Components
#### 1. TGT (Ticket Granting Ticket) aka Session
They are issued by CAS to users. They are used by users to access their sessions. TGTs map to user's information in `sessions` table in CAS's database. TGTs are usually stored in the user's browser as HTTP only cookies named `CASTGC`.

#### 2. ST (Service Ticket)
They are issued by CAS to services. They are used by services to validate the user's session and get the user's information.

#### 3. Database
The main database that stores the user information and sessions. Here are some tables:
-  `users`: Stores the main user information. For this project, other services don't duplicate data from this table, so they would have to make a request to this service to get the common user information e.g. email, names. For service-specific information, they would be stored in their services' databases.
- `sessions`: TGT token of each session, expiry time, payload e.g. email, user_id, roles, etc.
- `service_tickets`: ST token, the TGT token it corresponds to, the service it was issued to, user_id, expiry time.

#### 4. Google OAuth
Used for Google login. You can use any other OAuth provider like Facebook, Github, etc. or even your own OAuth provider.

### Sample Service (Consumers of CAS SSO)
These are the services that are protected by CAS. They are the ones that the user wants to access. In real world, these would be the internal tools of a company e.g. HR tools, finance tools, etc.

## SSO Login Flow
1. The user attempts to access a resource or application that is protected by CAS e.g. internal tools.
2. User is redirected to CAS to do Google login.
3. If the Google login is successful, CAS creates a session (TGT) + Service Ticket (ST) and redirects the user back to the service with the ST.
4. The service receives the ST from the user and sends it to the CAS server for validation to ensure the ST is valid, not expired, issued to this exact service, and correctly associated with the user.
5. If the ST validation is successful, the CAS responds to service with user attributes e.g. user_id, email, roles, etc.
6. The service then creates a service-scoped session (in this case they are JWTs: `access_token` and `refresh_token` that are stored in a service's cache) for the user and allows the user to access the resource.

### SSO Auth Flow
If the user is already logged in to CAS, the user can access the service without having to log in again.
1. The service redirects the user to CAS to get a new ST.
2. CAS redirects the user back to the service with the ST.
3. The service then validates the ST with CAS and creates a new session for the user.

### SSO Logout Flow
- If it is a logout from a service, only the service's session (JWTs) is destroyed.
- If it is a logout from CAS, the session in CAS (`session` in database + `CASTGC` cookie) and all the other services' sessions (JWTs) are also destroyed.

## Setting up
### Repositories
-   [CAS](https://github.com/bookpanda/sso-cas)
-   [Sample service](https://github.com/bookpanda/sso-sample-service)

### Stack
-   ASP.NET Core
-   postgresql
-   redis
-   react

### Prerequisites
-   💻
-   Microsoft.NETCore.App 8.0.4 runtime or [later](https://dotnet.microsoft.com/en-us/download)
-   Node v20.9.0 or [later](https://nodejs.org/en) (recommend using nvm)
-   pnpm v9.5.0 or [later](https://pnpm.io)
-   docker
-   makefile

### Backend Setup
1. Go to `backend` directory.
2. Run `dotnet restore` to download all the dependencies.
3. Copy `.env.template` and paste it in the same directory as `.env`. Fill in the appropriate values.
4. Run `dotnet watch`.

### Frontend Setup
1. Go to `frontend` directory.
2. Run `pnpm i` to download all the dependencies.
3. Copy `.env.template` and paste it in the same directory as `.env`. Fill in the appropriate values.
4. Run `pnpm dev`.

### Running the whole backend stack
1. Run `make docker-qa` in the root directory to spin up databases, redis, CAS and service 1 + 2 APIs.
2. Run frontend services by running `pnpm dev` in `frontend` directory in [CAS](https://github.com/bookpanda/sso-cas) and [Sample service](https://github.com/bookpanda/sso-sample-service) repos. For service 2, just copy another [Sample service](https://github.com/bookpanda/sso-sample-service) and change `.env` and `port`.