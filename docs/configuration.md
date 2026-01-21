# ChronoLog Configuration Guide

This guide covers all available configuration options for ChronoLog, including environment variables, deployment scenarios, security settings, and advanced configurations.

## Table of Contents

- [Environment Variables](#environment-variables)
- [Azure AD Configuration](#azure-ad-configuration)
- [Database Configuration](#database-configuration)
- [Reverse Proxy Setup](#reverse-proxy-setup)
- [Production Deployment](#production-deployment)
- [Security Best Practices](#security-best-practices)
- [Advanced Configuration](#advanced-configuration)
- [Backup and Recovery](#backup-and-recovery)
- [Monitoring and Health Checks](#monitoring-and-health-checks)

## Environment Variables

ChronoLog is configured entirely through environment variables defined in the `.env` file. This approach allows for secure, deployment-specific configuration without modifying code.

### Complete Environment Variable Reference

```bash
# ============================================
# Azure AD Authentication
# ============================================
AZURE_AD_DOMAIN="yourdomain.onmicrosoft.com"
AZURE_AD_TENANT_ID="your-tenant-id"
AZURE_AD_CLIENT_ID="your-client-id"
AZURE_AD_CLIENT_SECRET="your-client-secret"

# ============================================
# Database Configuration
# ============================================
MYSQL_USER="chronolog"
MYSQL_PASSWORD="your-secure-password"
MYSQL_ROOT_PASSWORD="your-root-password"
MYSQL_DATABASE="ChronoLog"
MYSQL_HOST="chronoLogDatabase"

# ============================================
# Reverse Proxy Configuration (Production)
# ============================================
REVERSE_PROXY_ENABLED="false"
REVERSE_PROXY_BASE_URL="https://chronolog.yourdomain.com"
```

### Environment Variable Details

| Variable | Required | Default | Description |
|----------|----------|-------|-------------|
| `AZURE_AD_DOMAIN` | ✅ Yes | - | Your Azure AD domain (e.g., `contoso.onmicrosoft.com`) |
| `AZURE_AD_TENANT_ID` | ✅ Yes | - | Azure AD Tenant ID (GUID) |
| `AZURE_AD_CLIENT_ID` | ✅ Yes | - | Azure AD Application (Client) ID |
| `AZURE_AD_CLIENT_SECRET` | ✅ Yes | - | Azure AD Application Client Secret |
| `MYSQL_USER` | ✅ Yes | - | MySQL database user |
| `MYSQL_PASSWORD` | ✅ Yes | - | MySQL user password (min 8 chars recommended) |
| `MYSQL_ROOT_PASSWORD` | ✅ Yes | - | MySQL root password |
| `MYSQL_DATABASE` | ✅ Yes | `ChronoLog` | Database name |
| `MYSQL_HOST` | ✅ Yes | `chronoLogDatabase` | Database host (container name in Docker) |
| `REVERSE_PROXY_ENABLED` | ❌ No | `false` | Enable reverse proxy support |
| `REVERSE_PROXY_BASE_URL` | ❌ No | - | Full base URL when behind reverse proxy |

## Azure AD Configuration

### Basic Setup

Azure AD (Microsoft Entra ID) provides enterprise-grade authentication for ChronoLog.

**Required Configuration:**

1. **Domain**: Your organization's Azure AD domain
   ```bash
   AZURE_AD_DOMAIN="contoso.onmicrosoft.com"
   ```

2. **Tenant ID**: Found in Azure Portal → Azure AD → Overview
   ```bash
   AZURE_AD_TENANT_ID="12345678-1234-1234-1234-123456789012"
   ```

3. **Client ID**: From your App Registration → Overview
   ```bash
   AZURE_AD_CLIENT_ID="87654321-4321-4321-4321-210987654321"
   ```
   
4. **Client Secret**: Create a new client secret in App Registration → Certificates & secrets
   ```bash
   AZURE_AD_CLIENT_SECRET="your-client-secret"
   ```

### Redirect URIs

Configure these in Azure Portal → App Registration → Authentication:

**For Development:**
- `http://localhost:5001/signin-oidc`
- `http://localhost:5001/signout-callback-oidc`

**For Production:**
- `https://your-domain.com/signin-oidc`
- `https://your-domain.com/signout-callback-oidc`

### API Permissions

Required Microsoft Graph permissions:

| Permission | Type | Purpose |
|------------|------|---------|
| `User.Read` | Delegated | Read user profile information |

### Multi-Tenant vs Single-Tenant

**Single-Tenant (Recommended):**
- Only users from your organization can sign in
- More secure for internal applications
- Easier to manage

**Multi-Tenant:**
- Users from any Azure AD organization can sign in
- Requires additional consent flow
- Not recommended for ChronoLog's typical use case

### Token Configuration

ChronoLog uses **ID tokens** for authentication:
- Enable in Azure Portal → App Registration → Authentication → Settings
- Check **ID tokens**

## Database Configuration

### MySQL Configuration

ChronoLog uses MySQL 8 with UTF-8 support.

#### Character Set

The default configuration uses:
```yaml
character-set-server: utf8mb4
collation-server: utf8mb4_unicode_ci
```

This ensures proper support for international characters and emojis.

#### Connection String

Generated automatically from environment variables:
```
server=${MYSQL_HOST};uid=${MYSQL_USER};pwd=${MYSQL_PASSWORD};database=${MYSQL_DATABASE}
```

#### Database Security

**Best Practices:**

1. **Use strong passwords**
   - Minimum 16 characters
   - Mix of uppercase, lowercase, numbers, symbols

2. **Restrict database access**
   - In Docker Compose, database is only accessible to the app container
   - Don't expose port `3306` to the host unless necessary

3. **Regular backups**
   - See [Backup and Recovery](#backup-and-recovery) section

### Database Migrations

ChronoLog automatically applies database migrations on startup.

**Migration Process:**
1. Application starts
2. Connects to database
3. Checks for pending migrations
4. Applies migrations in order
5. Logs completion or errors

## Reverse Proxy Setup

### When to Use a Reverse Proxy

Use a reverse proxy in production environments for:
- **SSL/TLS termination** (HTTPS)
- **Load balancing** (future scaling)
- **Security** (hide internal architecture)
- **Multiple applications** on one server

### Enabling Reverse Proxy Support

In `.env`:
```bash
REVERSE_PROXY_ENABLED="true"
REVERSE_PROXY_BASE_URL="https://chronolog.yourdomain.com"
```

### Nginx Configuration

**Recommended Nginx configuration:**

```nginx
map $http_connection $connection_upgrade {
  "~*Upgrade" $http_connection;
  default keep-alive;
}

server {
  listen 443 ssl http2;
  listen [::]:443 ssl http2;
  server_name chronolog.yourdomain.com;

  # SSL Configuration
  ssl_certificate /etc/letsencrypt/live/chronolog.yourdomain.com/fullchain.pem;
  ssl_certificate_key /etc/letsencrypt/live/chronolog.yourdomain.com/privkey.pem;
  ssl_protocols TLSv1.2 TLSv1.3;
  ssl_ciphers 'ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305:ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256';
  ssl_prefer_server_ciphers on;

  # Proxy settings
  location / {
    proxy_pass         http://localhost:8080;
    proxy_http_version 1.1;
    
    # Essential headers for ASP.NET Core
    proxy_set_header   Host $host;
    proxy_set_header   X-Real-IP $remote_addr;
    proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header   X-Forwarded-Proto $scheme;
    proxy_set_header   X-Forwarded-Host $host;
    
    # WebSocket support (for Blazor)
    proxy_set_header   Upgrade $http_upgrade;
    proxy_set_header   Connection $connection_upgrade;
    proxy_cache_bypass $http_upgrade;
    
    # Timeouts
    proxy_connect_timeout 60s;
    proxy_send_timeout 60s;
    proxy_read_timeout 60s;
  }
}

# Redirect HTTP to HTTPS
server {
  listen 80;
  listen [::]:80;
  server_name chronolog.yourdomain.com;
  return 301 https://$host$request_uri;
}
```

### Apache Configuration

```apache
<VirtualHost *:443>
    ServerName chronolog.yourdomain.com
    
    SSLEngine on
    SSLCertificateFile /etc/letsencrypt/live/chronolog.yourdomain.com/fullchain.pem
    SSLCertificateKeyFile /etc/letsencrypt/live/chronolog.yourdomain.com/privkey.pem
    
    ProxyPreserveHost On
    ProxyPass / http://localhost:8080/
    ProxyPassReverse / http://localhost:8080/
    
    RequestHeader set X-Forwarded-Proto "https"
    RequestHeader set X-Forwarded-Host "chronolog.yourdomain.com"
    
    # WebSocket support
    RewriteEngine on
    RewriteCond %{HTTP:Upgrade} websocket [NC]
    RewriteCond %{HTTP:Connection} upgrade [NC]
    RewriteRule ^/?(.*) "ws://localhost:8080/$1" [P,L]
</VirtualHost>

<VirtualHost *:80>
    ServerName chronolog.yourdomain.com
    Redirect permanent / https://chronolog.yourdomain.com/
</VirtualHost>
```

### Traefik Configuration

```yaml
# docker-compose.yml with Traefik
services:
  chronoLog:
    # ... existing configuration ...
    labels:
      - "traefik.enable=true"
      - "traefik.http.routers.chronolog.rule=Host(`chronolog.yourdomain.com`)"
      - "traefik.http.routers.chronolog.entrypoints=websecure"
      - "traefik.http.routers.chronolog.tls.certresolver=letsencrypt"
      - "traefik.http.services.chronolog.loadbalancer.server.port=80"
```

### Troubleshooting Reverse Proxy

**Issue: Redirect loops**
- Solution: Ensure `X-Forwarded-Proto` header is set correctly
- Verify `REVERSE_PROXY_ENABLED="true"` in `.env`

**Issue: 502 Bad Gateway**
- Check that ChronoLog container is running: `docker compose ps`
- Verify network connectivity: `curl http://localhost:8080`
- Check logs: `docker compose logs chronolog`

**Issue: Authentication redirects to HTTP instead of HTTPS**
- Update redirect URIs in Azure AD to use HTTPS
- Verify `REVERSE_PROXY_BASE_URL` uses `https://`
- Check nginx/apache is sending `X-Forwarded-Proto: https`

## Production Deployment

### Pre-Deployment Checklist

- [ ] Strong passwords set for all accounts
- [ ] Azure AD redirect URIs updated with production URLs
- [ ] SSL/TLS certificate obtained and configured
- [ ] Reverse proxy properly configured
- [ ] `.env` file configured with production values
- [ ] Firewall rules configured (only 80/443 open to public)
- [ ] Backup strategy implemented
- [ ] Monitoring and alerting configured

### Docker Compose Production Configuration

**Recommended changes for production:**

```yaml
services:
  chronoLog:
    image: ghcr.io/fheinke/chronolog:stable  # Use stable tag
    container_name: chronolog
    restart: always  # Changed from unless-stopped
    
    # Resource limits
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 1G
    
    # ... rest of configuration ...
    
  chronoLogDatabase:
    image: mysql:8
    container_name: chronolog-database
    restart: always
    
    # Resource limits
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 4G
        reservations:
          cpus: '1'
          memory: 2G
    
    # ... rest of configuration ...
```

### Logging

**View logs:**
```bash
docker compose logs -f chronolog
```

**Log retention in production:**
```yaml
services:
  chronoLog:
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"
```

### Updates and Maintenance

**Updating ChronoLog:**

```bash
# Pull latest image
docker compose pull

# Restart with new image
docker compose up -d

# Check logs
docker compose logs -f chronolog
```

**Scheduled maintenance:**
- Plan updates during low-usage periods
- Always backup database before updating
- Test updates in a staging environment first

## Security Best Practices

### Application Security

1. **Always use HTTPS in production**
   - Obtain certificates from Let's Encrypt or your certificate provider
   - Configure HSTS headers in your reverse proxy

2. **Keep software updated**
   - Regularly pull latest ChronoLog images
   - Subscribe to GitHub releases for security notifications

3. **Restrict network access**
   - Don't expose MySQL port (3306) to the internet
   - Use firewall rules to limit access

### Database Security

1. **Password security**
   - Use randomly generated passwords

2. **Backup encryption**
   - Encrypt database backups
   - Store backups in secure location
   - Test restore procedures regularly

### Rate Limiting

Consider implementing rate limiting in your reverse proxy:

**Nginx example:**
```nginx
limit_req_zone $binary_remote_addr zone=chronolog:10m rate=10r/s;

location / {
    limit_req zone=chronolog burst=20 nodelay;
    proxy_pass http://localhost:8080;
    # ... other settings ...
}
```

## Advanced Configuration

### External Database

To use an external MySQL server instead of Docker:

1. Update `.env`:
   ```bash
   MYSQL_HOST="your-mysql-server.com"
   MYSQL_USER="chronolog"
   MYSQL_PASSWORD="password"
   MYSQL_DATABASE="ChronoLog"
   ```

2. Modify `compose.yaml` to remove the database service

3. Ensure firewall allows connection from ChronoLog server

## Backup and Recovery

### Database Backup

**Automated daily backup script:**

```bash
#!/bin/bash
# backup-chronolog.sh

BACKUP_DIR="/backups/chronolog"
DATE=$(date +%Y%m%d_%H%M%S)
MYSQL_USER="chronolog"
MYSQL_PASSWORD="your-password"
MYSQL_DATABASE="ChronoLog"

# Create backup directory
mkdir -p "$BACKUP_DIR"

# Backup database
docker exec chronolog-database mysqldump \
  -u"$MYSQL_USER" \
  -p"$MYSQL_PASSWORD" \
  "$MYSQL_DATABASE" \
  > "$BACKUP_DIR/chronolog_$DATE.sql"

# Compress backup
gzip "$BACKUP_DIR/chronolog_$DATE.sql"

# Remove backups older than 30 days
find "$BACKUP_DIR" -name "*.sql.gz" -mtime +30 -delete

echo "Backup completed: chronolog_$DATE.sql.gz"
```

**Schedule with cron:**
```bash
# Edit crontab
crontab -e

# Add daily backup at 2 AM
0 2 * * * /path/to/backup-chronolog.sh >> /var/log/chronolog-backup.log 2>&1
```

### Restore from Backup

```bash
# Extract backup
gunzip chronolog_20260121_020000.sql.gz

# Stop application
docker compose stop chronolog

# Restore database
docker exec -i chronolog-database mysql \
  -uchronolog \
  -p"your-password" \
  ChronoLog < chronolog_20260121_020000.sql

# Start application
docker compose start chronolog
```

### Volume Backup

Backup the entire database volume:

```bash
# Create backup
docker run --rm \
  -v chronolog_chronolog-data:/data \
  -v $(pwd):/backup \
  alpine tar czf /backup/chronolog-volume-$(date +%Y%m%d).tar.gz -C /data .

# Restore backup
docker run --rm \
  -v chronolog_chronolog-data:/data \
  -v $(pwd):/backup \
  alpine sh -c "rm -rf /data/* && tar xzf /backup/chronolog-volume-20260121.tar.gz -C /data"
```

## Monitoring and Health Checks

### Built-in Health Check

ChronoLog includes a health check endpoint:

**Endpoint:** `/.well-known/readiness`

**Check health:**
```bash
curl http://localhost:8080/.well-known/readiness
```

**Response:**
- `200 OK`: Application healthy
- `503 Service Unavailable`: Application unhealthy

### Docker Health Check

Configured automatically in `compose.yaml`:

```yaml
healthcheck:
  test: ["CMD-SHELL", "curl -f http://localhost:80/.well-known/readiness || exit 1"]
  interval: 30s
  timeout: 10s
  retries: 5
  start_period: 30s
```

### External Monitoring

**Uptime monitoring services:**
- UptimeRobot
- Pingdom
- StatusCake

**Monitor this endpoint:**
```
https://chronolog.yourdomain.com/.well-known/readiness
```

### Log Monitoring

**Important log patterns to monitor:**

- `Database migration completed` - Successful startup
- `An error occurred while applying database migrations` - Migration failure
- `Authentication failed` - Azure AD issues
- `Unable to connect to any of the specified MySQL hosts` - Database connectivity

### Alerting

Set up alerts for:
- Service downtime (health check failure)
- High error rates in logs
- Database connection failures
- Disk space warnings (for MySQL volume)
- SSL certificate expiration

## Troubleshooting Configuration Issues

### Environment Variables Not Applied

**Symptom:** Changes to `.env` not reflected in application

**Solution:**
```bash
# Recreate containers with new environment
docker compose down
docker compose up -d
```

### Azure AD Configuration Errors

**Symptom:** "AADSTS50011: The redirect URI does not match"

**Solution:**
- Verify redirect URIs in Azure Portal exactly match your domain
- Include `/signin-oidc` path
- Check for HTTP vs HTTPS mismatch

### Database Connection Errors

**Symptom:** "Unable to connect to any of the specified MySQL hosts"

**Solution:**
- Verify `MYSQL_HOST` matches the database service name
- Check `MYSQL_USER` and `MYSQL_PASSWORD` are correct
- Ensure database container is healthy: `docker compose ps`

---

For installation instructions, see [Getting Started Guide](getting-started.md).
For API documentation, see [API Reference](api-reference.md).
