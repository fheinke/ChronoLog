# Getting Started with ChronoLog

This guide will walk you through the complete setup process for ChronoLog, from creating an Azure AD App Registration to accessing your first tracked worktime.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Step 1: Azure AD App Registration](#step-1-azure-ad-app-registration)
- [Step 2: Environment Configuration](#step-2-environment-configuration)
- [Step 3: Deploy with Docker](#step-3-deploy-with-docker)
- [Step 4: Initial User Setup](#step-4-initial-user-setup)
- [Step 5: First Login and Configuration](#step-5-first-login-and-configuration)
- [Troubleshooting](#troubleshooting)
- [Next Steps](#next-steps)

## Prerequisites

Before you begin, ensure you have the following:

- **Docker** and **Docker Compose**
- An **Azure account** with permissions to create App Registrations in Azure Active Directory
- Basic knowledge of command-line operations
- (Optional) A domain name and reverse proxy (nginx, Apache, etc.) for production deployment

### Verifying Prerequisites

```bash
# Check Docker version
docker --version

# Check Docker Compose version
docker compose version
```

## Step 1: Azure AD App Registration

ChronoLog uses Microsoft Entra ID (formerly Azure Active Directory) for authentication. You need to create an App Registration to enable secure login.

### 1.1 Create the App Registration

1. Navigate to the [Azure Portal](https://portal.azure.com/)
2. Go to **Azure Active Directory** → **App registrations** → **New registration**
3. Fill in the registration form:
   - **Name**: `ChronoLog` (or your preferred name)
   - **Supported account types**: `Accounts in this organizational directory only`
   - **Redirect URI**: 
     - Type: `Web`
     - For local development: `https://localhost:5001/signin-oidc`
     - For production: `https://your-domain.com/signin-oidc`
4. Click **Register**

### 1.2 Note Important Values

After registration, navigate to the **Overview** page and note down:

- **Application (client) ID**: This is your `AZURE_AD_CLIENT_ID`
- **Directory (tenant) ID**: This is your `AZURE_AD_TENANT_ID`
- **Azure AD domain**: Typically `yourdomain.onmicrosoft.com` (found under "Primary domain")

### 1.3 Create a Client Secret

1. Go to **Certificates & secrets** → **New client secret**
2. Add a description (e.g., `ChronoLog Secret`)
3. Set an expiration period (e.g., `6 months`, `12 months`, or `24 months`)
4. Click **Add**
5. Note down the **Value** of the client secret immediately (this is your `AZURE_AD_CLIENT_SECRET`). You won't be able to see it again.

### 1.4 Configure Authentication

1. In your App Registration, go to **Authentication**
2. Add additional redirect URIs if needed:
   - Sign-in: `https://your-domain.com/signin-oidc`
   - Sign-out: `https://your-domain.com/signout-callback-oidc`
3. Under **Settings** -> **Implicit grant and hybrid flows**, enable:
   - ✅ **ID tokens** (used for user authentication)
4. Click **Save**

### 1.5 Configure API Permissions

1. Go to **API permissions**
2. Click **Add a permission** → **Microsoft Graph** → **Delegated permissions**
3. Add the following permissions:
   - `User.Read` (allows reading basic user profile)
4. Click **Grant admin consent for [Your Organization]**
   - This requires admin privileges
   - Ensures all users can sign in without individual consent

### 1.6 (Optional) Expose an API for External Integrations

If you plan to use the ChronoLog API from external applications:

1. Go to **Expose an API**
2. Click **Add a scope**
3. Accept the default Application ID URI (`api://<client-id>`) or customize it
4. Define the scope:
   - **Scope name**: `access_as_user`
   - **Who can consent**: `Admins and users`
   - **Admin consent display name**: `Access ChronoLog API`
   - **Admin consent description**: `Allows the app to access ChronoLog API on behalf of the signed-in user`
   - **User consent display name**: `Access your ChronoLog data`
   - **User consent description**: `Allow ChronoLog to access your time tracking data`
   - **State**: `Enabled`
5. Click **Add scope**

## Step 2: Environment Configuration

### 2.1 Clone or Download ChronoLog

```bash
# If using Git
git clone https://github.com/fheinke/ChronoLog.git
cd ChronoLog

# Or download and extract the release package
```

### 2.2 Create Environment File

Create a `.env` file in the root directory by copying the example:

```bash
cp .env.example .env
```

### 2.3 Configure Environment Variables

Edit the `.env` file with your preferred text editor:

```bash
nano .env
```

Configure the following variables:

#### Azure AD Configuration

```bash
AZURE_AD_DOMAIN="yourdomain.onmicrosoft.com"
AZURE_AD_TENANT_ID="12345678-1234-1234-1234-123456789012"
AZURE_AD_CLIENT_ID="87654321-4321-4321-4321-210987654321"
AZURE_AD_CLIENT_SECRET="YourClientSecretValue"
```

Replace with your actual values from Step 1.2.

#### Database Configuration

```bash
MYSQL_USER="chronolog"
MYSQL_PASSWORD="YourSecurePassword123!"
MYSQL_ROOT_PASSWORD="YourSecureRootPassword456!"
MYSQL_DATABASE="ChronoLog"
MYSQL_HOST="chronoLogDatabase"
```

**Security Best Practices:**
- Use strong passwords (16+ characters with mixed case, numbers, and symbols)
- Never commit the `.env` file to version control
- Keep the root password different from the user password

#### Reverse Proxy Configuration (Production Only)

If deploying behind a reverse proxy (nginx, Apache, Traefik, etc.):

```bash
REVERSE_PROXY_ENABLED="true"
REVERSE_PROXY_BASE_URL="https://chronolog.yourdomain.com"
```

For local development:

```bash
REVERSE_PROXY_ENABLED="false"
REVERSE_PROXY_BASE_URL=""
```

## Step 3: Deploy with Docker

### 3.1 Pull the Docker Image

```bash
docker compose pull
```

This downloads the latest ChronoLog image from GitHub Container Registry.

### 3.2 Start the Application

```bash
docker compose up -d
```

The `-d` flag runs containers in detached mode (background).

### 3.3 Verify Deployment

Check that containers are running:

```bash
docker compose ps
```

You should see:
- `chronolog` - Running on port 8080
- `chronolog-database` - MySQL database

Check application logs:

```bash
# View ChronoLog application logs
docker compose logs chronolog

# Follow logs in real-time
docker compose logs -f chronolog
```

### 3.4 Wait for Database Migration

On first startup, ChronoLog automatically runs database migrations. This may take 30-60 seconds. Watch the logs for:

```
info: Program[0]
      Database migration completed successfully.
```

### 3.5 Verify Health Check

ChronoLog includes a health check endpoint:

```bash
curl http://localhost:8080/.well-known/readiness
```

Expected response: `Healthy`

## Step 4: Initial User Setup

After the first user logs in, you must manually grant admin privileges to at least one user.

### 4.1 Access the Application

Open your browser and navigate to:
- Local: `http://localhost:8080`
- Production: `https://your-domain.com`

### 4.2 Sign In

1. Click the **Sign In** button
2. Authenticate with your Azure AD credentials
3. Grant consent if prompted
4. You'll be redirected to the ChronoLog dashboard

### 4.3 Grant Admin Privileges

After your first login, exit the application and run this command:

```bash
docker exec -it chronolog-database mysql -uchronolog -p
```

When prompted, enter your `MYSQL_PASSWORD` from the `.env` file.

Then execute:

```sql
USE ChronoLog;

-- View all users
SELECT Id, Name, Email, IsAdmin, IsProjectManager FROM Employees;

-- Grant admin privileges (replace with your email)
UPDATE Employees SET IsAdmin = 1 WHERE Email = 'your-email@yourdomain.com';

-- Verify the change
SELECT Name, Email, IsAdmin FROM Employees WHERE Email = 'your-email@yourdomain.com';

-- Exit MySQL
EXIT;
```

**Alternative one-liner:**

```bash
docker exec -it chronolog-database mysql -uchronolog -p<YOUR_PASSWORD> ChronoLog -e "UPDATE Employees SET IsAdmin = 1 WHERE Email = 'your-email@yourdomain.com';"
```

⚠️ **Important**: Replace `<YOUR_PASSWORD>` with your actual mysql user password (no spaces after `-p`).

## Step 5: First Login and Configuration

### 5.1 Verify Admin Access

1. Log out and log back in to refresh your session
2. You should now see the **User Management** menu item in the sidebar
3. Navigate to **User Management** to manage other users

### 5.2 Create Projects

1. Navigate to **Projects**
2. Click **Add New Project**
3. Fill in project details:
   - Name
   - Description
   - Default SAP Response
   - SAP Project ID / Cost Center
4. Click **Save**

### 5.3 Configure Employee Settings

As an admin, you can configure employee-specific settings:

1. Go to **User Management**
2. Select a user to edit
3. Configure:
   - **Roles**: Admin, Project Manager

### 5.5 Start Tracking Time

1. Navigate to **Dashboard**
2. Click on a specific day to log time
3. Select the workday type and add worktime entries or project allocations
4. Use the **Time Balance** view to see your overtime/undertime

## Troubleshooting

### Application Won't Start

**Check logs:**
```bash
docker compose logs chronolog
```

**Common issues:**
- Database connection failed: Verify `MYSQL_*` variables in `.env`
- Azure AD error: Double-check `AZURE_AD_*` values
- Port already in use: Change port mapping in `compose.yaml`

### Authentication Errors

**"Redirect URI mismatch":**
- Ensure the redirect URI in Azure AD matches your actual URL
- Include both HTTP/HTTPS variants for development
- Remember to add the `/signin-oidc` path

**"Admin consent required":**
- Go to Azure AD → App Registration → API permissions
- Click "Grant admin consent for [Organization]"

### Database Issues

**Reset database (⚠️ deletes all data):**
```bash
docker compose down -v
docker compose up -d
```

**Access database directly:**
```bash
docker exec -it chronolog-database mysql -uchronolog -p
```

### Can't Set Admin User

**Verify user exists:**
```bash
docker exec -it chronolog-database mysql -uchronolog -p<PASSWORD> ChronoLog -e "SELECT * FROM Employees;"
```

**Check for typos in email address** - it's case-sensitive.

### Reverse Proxy Issues

**502 Bad Gateway:**
- Verify `REVERSE_PROXY_ENABLED=true` in `.env`
- Ensure proxy headers are correctly forwarded:
  ```nginx
  proxy_set_header Host $host;
  proxy_set_header X-Forwarded-Proto $scheme;
  proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
  ```

**Redirect loops:**
- Check that HTTPS is correctly forwarded
- Verify Azure AD redirect URIs use HTTPS

## Next Steps

- **[Configuration Guide](configuration.md)** - Advanced configuration options
- **[API Reference](api-reference.md)** - Integrate with external systems

## Getting Help

- **Documentation**: Browse the `/docs` directory
- **API Docs**: Access Swagger UI at `http://localhost:5001/swagger`

---

**Welcome to ChronoLog!** You're now ready to track your time efficiently. 🎉
