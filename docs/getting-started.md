# Getting Started with ChronoLog

This guide will walk you through the complete setup process for ChronoLog, from choosing an authentication provider to accessing your first tracked worktime.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Step 1: Choose an Authentication Provider](#step-1-choose-an-authentication-provider)
  - [Option A: Azure AD (Microsoft Entra ID)](#option-a-azure-ad-microsoft-entra-id)
  - [Option B: Keycloak](#option-b-keycloak)
- [Step 2: Environment Configuration](#step-2-environment-configuration)
- [Step 3: Deploy with Docker](#step-3-deploy-with-docker)
- [Step 4: Initial User Setup](#step-4-initial-user-setup)
- [Step 5: First Login and Configuration](#step-5-first-login-and-configuration)
- [Troubleshooting](#troubleshooting)
- [Next Steps](#next-steps)

## Prerequisites

Before you begin, ensure you have the following:

- **Docker** and **Docker Compose**
- **One** of the following authentication providers:
  - An **Azure account** with permissions to create App Registrations (for Azure AD)
  - A running **Keycloak** instance (for Keycloak)
- Basic knowledge of command-line operations
- (Optional) A domain name and reverse proxy (nginx, Apache, etc.) for production deployment

### Verifying Prerequisites

```bash
# Check Docker version
docker --version

# Check Docker Compose version
docker compose version
```

## Step 1: Choose an Authentication Provider

ChronoLog supports two authentication providers. Exactly **one** is active at a time. Choose the one that fits your infrastructure:

| Provider | Best for |
|---|---|
| **Azure AD** | Organizations using Microsoft 365 / Entra ID |
| **Keycloak** | Self-hosted / open-source identity management |

---

### Option A: Azure AD (Microsoft Entra ID)

#### A.1 Create the App Registration

1. Navigate to the [Azure Portal](https://portal.azure.com/)
2. Go to **Azure Active Directory** → **App registrations** → **New registration**
3. Fill in the registration form:
   - **Name**: `ChronoLog`
   - **Supported account types**: `Accounts in this organizational directory only`
   - **Redirect URI**:
     - Type: `Web`
     - For local development: `https://localhost:5001/signin-oidc`
     - For production: `https://your-domain.com/signin-oidc`
4. Click **Register**

#### A.2 Note Important Values

From the **Overview** page, note:
- **Application (client) ID** → `AZURE_AD_CLIENT_ID`
- **Directory (tenant) ID** → `AZURE_AD_TENANT_ID`
- **Primary domain** (e.g., `yourdomain.onmicrosoft.com`) → `AZURE_AD_DOMAIN`

#### A.3 Create a Client Secret

1. Go to **Certificates & secrets** → **New client secret**
2. Set a description and expiration period
3. Click **Add** and immediately copy the **Value** → `AZURE_AD_CLIENT_SECRET`

#### A.4 Configure Authentication

1. Go to **Authentication** → add redirect URIs:
   - `https://your-domain.com/signin-oidc`
   - `https://your-domain.com/signout-callback-oidc`
2. Under **Implicit grant**, enable **ID tokens**
3. Click **Save**

#### A.5 Configure API Permissions

1. Go to **API permissions** → **Add a permission** → **Microsoft Graph** → **Delegated**
2. Add `User.Read`
3. Click **Grant admin consent for [Your Organization]**

---

### Option B: Keycloak

#### B.1 Create a Realm

1. Open the Keycloak Admin Console
2. Click the realm dropdown (top left) → **Create realm**
3. Set **Realm name** to `chronolog` (or your preferred name)
4. Click **Create**

#### B.2 Create the Client

1. In your realm, go to **Clients** → **Create client**
2. Fill in:
   - **Client type**: `OpenID Connect`
   - **Client ID**: `chronolog-app`
3. Click **Next**
4. Enable **Client authentication** → Click **Next** → **Save**

#### B.3 Configure Redirect URIs

In the client's **Settings** tab:

| Field | Value |
|---|---|
| **Valid redirect URIs** | `https://your-domain.com/signin-oidc` |
| **Valid post logout redirect URIs** | `https://your-domain.com/signout-callback-oidc` |
| **Web origins** | `https://your-domain.com` |

For local development, also add:
- `https://localhost:5001/signin-oidc`
- `https://localhost:5001/signout-callback-oidc`

#### B.4 Configure Logout (Advanced Tab)

| Setting | Value |
|---|---|
| **Front channel logout** | `ON` |
| **Front-Channel Logout URL** | `https://your-domain.com/signout-callback-oidc` |

#### B.5 Get the Client Secret

Go to **Credentials** tab → copy **Client secret** → `KEYCLOAK_CLIENT_SECRET`

#### B.6 Note the Authority URL

The authority URL follows this pattern:
```
https://keycloak.yourdomain.com/realms/chronolog
```
→ `KEYCLOAK_AUTHORITY`

#### B.7 Assign Client Scopes

In **Client scopes** tab, ensure these are assigned:
- `openid`
- `profile`
- `email`

---

## Step 2: Environment Configuration

### 2.1 Clone or Download ChronoLog

```bash
git clone https://github.com/fheinke/ChronoLog.git
cd ChronoLog
```

### 2.2 Create Environment File

```bash
cp .env.example .env
```

### 2.3 Configure Environment Variables

```bash
nano .env
```

#### Authentication Provider

Set the active provider — only one should be configured:

**For Azure AD:**
```bash
AUTH_PROVIDER="AzureAd"
AZURE_AD_DOMAIN="yourdomain.onmicrosoft.com"
AZURE_AD_TENANT_ID="12345678-1234-1234-1234-123456789012"
AZURE_AD_CLIENT_ID="87654321-4321-4321-4321-210987654321"
AZURE_AD_CLIENT_SECRET="YourClientSecretValue"
```

**For Keycloak:**
```bash
AUTH_PROVIDER="Keycloak"
KEYCLOAK_AUTHORITY="https://keycloak.yourdomain.com/realms/chronolog"
KEYCLOAK_CLIENT_ID="chronolog-app"
KEYCLOAK_CLIENT_SECRET="YourKeycloakClientSecret"
```
> **Docker Compose note:** The provided `compose.yaml` must pass these values into the ChronoLog container environment/configuration (e.g., `AuthProvider`, `Keycloak__Authority`, `Keycloak__ClientId`, `Keycloak__ClientSecret`). If you don't add those mappings, ChronoLog will default to `AzureAd`.

#### Database Configuration

```bash
MYSQL_USER="chronolog"
MYSQL_PASSWORD="YourSecurePassword123!"
MYSQL_ROOT_PASSWORD="YourSecureRootPassword456!"
MYSQL_DATABASE="ChronoLog"
MYSQL_HOST="chronoLogDatabase"
```

**Security Best Practices:**
- Use strong passwords (16+ characters)
- Never commit the `.env` file to version control

#### Reverse Proxy Configuration (Production Only)
If deploying behind a reverse proxy (nginx, Apache, Traefik, etc.):
```bash
REVERSE_PROXY_ENABLED="true"
REVERSE_PROXY_BASE_URL="https://chronolog.yourdomain.com"
REVERSE_PROXY_KNOWN_PROXIES="172.16.0.0/12"
```

For local development:
```bash
REVERSE_PROXY_ENABLED="false"
```

## Step 3: Deploy with Docker

### 3.1 Pull the Docker Image

```bash
docker compose pull
```

### 3.2 Start the Application

```bash
docker compose up -d
```

### 3.3 Verify Deployment

```bash
docker compose ps
```

You should see:
- `chronolog` - Running on port 8080
- `chronolog-database` - MySQL database

```bash
docker compose logs -f chronolog
```

### 3.4 Wait for Database Migration

On first startup, ChronoLog automatically runs database migrations. Watch the logs for:

```
info: Program[0]
      Database migration completed successfully.
```

### 3.5 Verify Health Check

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

1. Click the **Sign In** button (or you will be redirected automatically)
2. Authenticate with your **Azure AD** or **Keycloak** credentials
3. Grant consent if prompted
4. You'll be redirected to the ChronoLog dashboard

### 4.3 Grant Admin Privileges

After your first login, run:

```bash
docker exec -it chronolog-database mysql -uchronolog -p
```

When prompted, enter your `MYSQL_PASSWORD` from the `.env` file.

Then execute:

```sql
USE ChronoLog;

-- View all users
SELECT EmployeeId, Name, Email, IsAdmin FROM Employees;

-- Grant admin privileges (replace with your email)
UPDATE Employees SET IsAdmin = 1 WHERE Email = 'your-email@yourdomain.com';

-- Verify the change
SELECT Name, Email, IsAdmin FROM Employees WHERE Email = 'your-email@yourdomain.com';

EXIT;
```

**Alternative one-liner:**

```bash
docker exec -it chronolog-database mysql -uchronolog -p<YOUR_PASSWORD> ChronoLog -e "UPDATE Employees SET IsAdmin = 1 WHERE Email = 'your-email@yourdomain.com';"
```

⚠️ **Important**: Replace `<YOUR_PASSWORD>` with your actual MySQL user password (no spaces after `-p`).

## Step 5: First Login and Configuration

### 5.1 Verify Admin Access

1. Log out and log back in to refresh your session
2. You should now see the **User Management** menu item in the sidebar

### 5.2 Create Projects

1. Navigate to **Projects**
2. Click **Add New Project**
3. Fill in project details and click **Save**

### 5.3 Configure Employee Settings

As an admin, you can configure employee-specific settings in **User Management**.

### 5.4 Start Tracking Time

1. Navigate to **Dashboard**
2. Click on a specific day to log time
3. Select the workday type and add worktime entries or project allocations
4. Use the **Time Balance** view to see your overtime/undertime

## Troubleshooting

### Application Won't Start

```bash
docker compose logs chronolog
```

**Common issues:**
- Database connection failed: Verify `MYSQL_*` variables in `.env`
- Auth provider error: Check `AUTH_PROVIDER` and corresponding credentials
- Port already in use: Change port mapping in `compose.yaml`

### Authentication Errors (Azure AD)

**"Redirect URI mismatch":**
- Ensure the redirect URI in Azure AD matches your actual URL
- Remember to add the `/signin-oidc` path

**"Admin consent required":**
- Go to Azure AD → App Registration → API permissions
- Click "Grant admin consent for [Organization]"

### Authentication Errors (Keycloak)

**Redirect loop after login:**
- Verify `Valid redirect URIs` in Keycloak client settings includes `/signin-oidc`
- Check `KEYCLOAK_AUTHORITY` points to the correct realm URL

**User created multiple times in database:**
- Verify the Keycloak client has `email` and `profile` scopes assigned
- Do not change user IDs in Keycloak — the `sub` claim must remain stable

**"The remote signout request was ignored":**
- Enable Front channel logout in Keycloak client → Advanced → Logout Settings
- Set Front-Channel Logout URL to `https://your-domain.com/signout-callback-oidc`

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
- Verify auth provider redirect URIs use HTTPS

## Next Steps

- **[Configuration Guide](configuration.md)** - Advanced configuration options
- **[API Reference](api-reference.md)** - Integrate with external systems

## Getting Help

- **Documentation**: Browse the `/docs` directory
- **API Docs**: Access Swagger UI at `http://localhost:5001/swagger`

---

**Welcome to ChronoLog!** You're now ready to track your time efficiently. 🎉
