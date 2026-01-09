# ChronoLog - Worktime Tracking Application

ChronoLog is a simple and efficient worktime tracking application designed to help individuals and teams monitor their productivity and manage their time effectively. With an intuitive interface and powerful features, ChronoLog makes it easy to log work hours, generate reports, and analyze time usage. You can use the application to directly track your times and copy the data to SAP for weekly or daily reporting.

## Features
- **Time Tracking**: Easily log your work hours with start and stop timers.
- **Project Management**: Organize your work by projects and tasks.
- **Reporting**: Generate detailed reports to analyze your time usage. You can use the reports to copy data to SAP.
- **Personal Dashboard**: View your overtime and vacation balance at a glance.
- **User Authentication**: Secure login using Azure Active Directory.
- **Dockerized Deployment**: Simple deployment using Docker and Docker Compose.

## Installation
### Prerequisites
- Docker and Docker Compose installed on your machine.
- An Azure account to create an App Registration for authentication.

### Steps
To install ChronoLog, follow these steps:

#### 1. Create an Azure App Registration
1. Go to the [Azure Portal](https://portal.azure.com/)
2. Navigate to "Azure Active Directory" > "App registrations" > "New registration"
3. Fill in the required details:
   - Name: ChronoLog
   - Supported account types: Accounts in this organizational directory only
   - Redirect URI: `Web` https://localhost:5001/signin-oidc (for the development environment, otherwise use your own domain)
   - Redirect URI: `Web` https://localhost:5001/signout-callback-oidc (for the development environment, otherwise use your own domain)
4. Click "Register"
5. After registration, go to "Certificates & secrets" and create a new client secret. Note down the secret value.
6. Go to "API permissions" and add the following Microsoft Graph permissions:
   - User.Read
7. Click "Grant admin consent for [Your Organization]"
8. Go to "Authentication" and then "Settings" 
   - Under "Implicit grant and hybrid flows", check the box for "ID tokens"
9. Note down the Application (client) ID and Directory (tenant) ID from the "Overview" section.

#### 2. Configure the Docker Environment
1. Create a `.env` file in the root directory of the project. You can use the provided `.env.example` as a template.
2. Fill in the following environment variables in the `.env` file:
   - `AZURE_AD_CLIENT_ID`: Your Azure App Registration Application (client) ID
   - `AZURE_AD_TENANT_ID`: Your Azure App Registration Directory (tenant) ID
   - `AZURE_AD_DOMAIN`: Your Azure AD domain (e.g., yourdomain.onmicrosoft.com)
   - `MYSQL_USER`: Database username (default: chronolog)
   - `MYSQL_PASSWORD`: Database password
   - `MYSQL_ROOT_PASSWORD`: Root database password
   - `MYSQL_DATABASE`: Database name (default: ChronoLog)
   - `MYSQL_HOST`: Database host (default: chronoLogDatabase for Docker Compose)
3. If you are using a **reverse Proxy**, make sure to change the following environment variables as well:
   - `REVERSE_PROXY_ENABLED`: Set to `true`
   - `REVERSE_PROXY_BASE_URL`: Set to your domain (e.g., `https://chronolog.yourdomain.com`)
4. Run the following command to start the application using Docker Compose:
   ```bash
   docker compose up -d
   ```

#### 3. User Configuration
You have to manually set an initial user as admin **after** the first login. To do this, execute the following command, replacing `<your_email_address>` with the email address of the user you want to set as admin:
```bash
docker exec -it chronolog-database mysql -u<MYSQL_USER> -p<MYSQL_PASSWORD> <MYSQL_DATABASE> -e "UPDATE Employees SET IsAdmin = 1 WHERE Email = '<your_email_address>';"
```
