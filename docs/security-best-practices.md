# Enterprise SCADA System - Security Best Practices

## Executive Summary

This document outlines security best practices for deploying and operating the Enterprise SCADA system in production environments. Following these guidelines is critical for protecting mission-critical infrastructure.

## Network Security

### Network Segmentation

**Requirement**: Separate OT (Operational Technology) from IT networks

```
Internet
    │
    ├─── DMZ Zone (Firewall)
    │       └─── Frontend (Public Access)
    │
    ├─── Application Zone (Firewall)
    │       ├─── API Gateway
    │       ├─── Backend Services
    │       └─── Auth Service
    │
    └─── Data Zone (Firewall)
            ├─── PostgreSQL
            ├─── InfluxDB
            ├─── RabbitMQ
            └─── Redis

Field Devices (OT Network - Air-gapped or separate IP range)
    ├─── PLCs
    ├─── RTUs
    └─── Sensors
```

### Firewall Rules

**External Access** (Public-facing):
```bash
# Allow HTTPS only
iptables -A INPUT -p tcp --dport 443 -j ACCEPT
iptables -A INPUT -p tcp --dport 80 -j REDIRECT --to-port 443

# Block direct database access
iptables -A INPUT -p tcp --dport 5432 -j DROP   # PostgreSQL
iptables -A INPUT -p tcp --dport 8086 -j DROP   # InfluxDB
iptables -A INPUT -p tcp --dport 6379 -j DROP   # Redis
```

**Internal Network** (Service-to-Service):
```bash
# Allow only necessary service communication
iptables -A INPUT -s 10.0.1.0/24 -p tcp --dport 5432 -j ACCEPT  # Database subnet
iptables -A INPUT -s 10.0.2.0/24 -p tcp --dport 5672 -j ACCEPT  # Message queue subnet
```

## Application Security

### Authentication & Authorization

**Password Policy**:
- Minimum 12 characters
- Require uppercase, lowercase, numbers, symbols
- Password expiration: 90 days
- Prevent password reuse (last 5 passwords)
- Account lockout: 5 failed attempts

**Implementation**:
```csharp
// In AuthService
public class PasswordPolicy
{
    public const int MinLength = 12;
    public const int RequireUppercase = 1;
    public const int RequireLowercase = 1;
    public const int RequireDigit = 1;
    public const int RequireSymbol = 1;
    public const int PasswordHistoryCount = 5;
    public const int MaxFailedAttempts = 5;
    public const int LockoutDurationMinutes = 30;
}
```

**Multi-Factor Authentication (MFA)**:
- ✅ MANDATORY for Administrator and Engineer roles
- ✅ Recommended for Operator role
- ✅ TOTP-based (Google Authenticator, Authy)

**JWT Token Security**:
```env
# Use 256-bit secret (32 bytes base64)
JWT_SECRET=$(openssl rand -base64 32)

# Short expiry times
JWT_EXPIRY_MINUTES=15  # Access token
JWT_REFRESH_EXPIRY_DAYS=7  # Refresh token

# Rotate secrets quarterly
```

### API Security

**Rate Limiting**:
```csharp
// Add to API Gateway
services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

**Input Validation**:
```csharp
// Validate all inputs
public class CreateTagRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    [RegularExpression(@"^[A-Z0-9.]+$")]  // Alphanumeric and dots only
    public string Name { get; set; }

    [Range(-999999, 999999)]
    public double? MinValue { get; set; }
}
```

**CORS Configuration**:
```csharp
// Restrict origins in production
services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://scada.yourdomain.com")  // NEVER use *
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

## Data Security

### Encryption

**At Rest**:
```bash
# Database encryption
# PostgreSQL - Enable TDE (Transparent Data Encryption)
# InfluxDB - Enable encryption

# File system encryption (Linux)
sudo cryptsetup luksFormat /dev/sdb
sudo cryptsetup luksOpen /dev/sdb scada_data
sudo mkfs.ext4 /dev/mapper/scada_data
```

**In Transit**:
```nginx
# NGINX TLS Configuration
ssl_protocols TLSv1.3 TLSv1.2;
ssl_ciphers 'ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384';
ssl_prefer_server_ciphers on;
ssl_session_cache shared:SSL:10m;
ssl_session_timeout 10m;
ssl_stapling on;
ssl_stapling_verify on;

# HTTP Strict Transport Security
add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
```

**Secrets Management**:

```bash
# Use HashiCorp Vault or Kubernetes Secrets

# Kubernetes Secrets (base64 encoded)
kubectl create secret generic scada-secrets \
  --from-literal=jwt-secret=$(openssl rand -base64 32) \
  --from-literal=db-password=$(openssl rand -base64 24) \
  --from-literal=influxdb-token=$(openssl rand -hex 32) \
  -n scada-system

# NEVER commit secrets to Git!
```

### Sensitive Data Handling

**PII Protection**:
```csharp
// Mask sensitive data in logs
public class SensitiveDataFilter : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // Mask emails, passwords, tokens
    }
}
```

**Audit Logging**:
```sql
-- Log ALL administrative actions
CREATE TABLE audit_logs (
    id SERIAL PRIMARY KEY,
    user_id INTEGER,
    action VARCHAR(100),
    entity_type VARCHAR(100),
    entity_id VARCHAR(100),
    details JSONB,
    ip_address VARCHAR(45),
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Retention: 7 years (compliance requirement)
```

## Infrastructure Security

### Container Security

**Image Scanning**:
```yaml
# .github/workflows/security.yml
- name: Run Trivy vulnerability scanner
  uses: aquasecurity/trivy-action@master
  with:
    image-ref: 'scada-core:latest'
    format: 'sarif'
    severity: 'CRITICAL,HIGH'
```

**Runtime Security**:
```yaml
# kubernetes/security-policy.yaml
apiVersion: policy/v1beta1
kind: PodSecurityPolicy
metadata:
  name: scada-restricted
spec:
  privileged: false
  allowPrivilegeEscalation: false
  requiredDropCapabilities:
    - ALL
  runAsUser:
    rule: MustRunAsNonRoot
  seLinux:
    rule: RunAsAny
  fsGroup:
    rule: RunAsAny
```

### Kubernetes Security

**Network Policies**:
```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: scada-core-policy
  namespace: scada-system
spec:
  podSelector:
    matchLabels:
      app: scada-core
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - podSelector:
        matchLabels:
          app: api-gateway
    ports:
    - protocol: TCP
      port: 5000
  egress:
  - to:
    - podSelector:
        matchLabels:
          app: postgres
    ports:
    - protocol: TCP
      port: 5432
```

**RBAC**:
```yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: scada-deployer
  namespace: scada-system
rules:
- apiGroups: ["apps"]
  resources: ["deployments"]
  verbs: ["get", "list", "update", "patch"]
```

## Compliance & Standards

### ISA/IEC 62443 (Industrial Cybersecurity)

**Compliance Checklist**:
- ✅ Network segmentation (OT/IT separation)
- ✅ Role-Based Access Control (RBAC)
- ✅ Multi-Factor Authentication (MFA)
- ✅ Audit logging
- ✅ Encryption (data in transit and at rest)
- ✅ Security patching process
- ✅ Incident response plan

### NERC CIP (Critical Infrastructure Protection)

**Requirements**:
- ✅ Annual security assessments
- ✅ Personnel training and background checks
- ✅ Physical security controls
- ✅ Electronic security perimeter
- ✅ System security management

## Incident Response

### Detection

**SIEM Integration**:
```bash
# Forward logs to Splunk/ELK
# Configure alerts for:
- Multiple failed login attempts
- Privilege escalation
- Unusual API calls
- Abnormal data access patterns
- Configuration changes
```

### Response Plan

**Severity Levels**:
| Level | Description | Response Time |
|-------|-------------|---------------|
| **Critical** | Active breach, data exfiltration | Immediate (< 15 min) |
| **High** | Attempted breach, system compromise | 1 hour |
| **Medium** | Suspicious activity, policy violation | 4 hours |
| **Low** | Anomalies, potential vulnerabilities | 24 hours |

**Incident Workflow**:
1. **Detect**: Automated alerts, user reports
2. **Contain**: Isolate affected systems
3. **Eradicate**: Remove threat, patch vulnerabilities
4. **Recover**: Restore from clean backups
5. **Post-Mortem**: Document lessons learned

## Security Maintenance

### Patching Schedule

| Component | Patch Schedule | Window |
|-----------|---------------|--------|
| OS packages | Monthly | 1st Sunday, 2-5 AM |
| Docker images | Quarterly | Scheduled maintenance |
| Dependencies | As released (critical) | Emergency window |

### Vulnerability Management

```bash
# Weekly vulnerability scans
trivy image --severity HIGH,CRITICAL scada-core:latest

# Dependency checks
dotnet list package --vulnerable
npm audit
```

### Security Training

**Required Training**:
- Security Awareness (Annual, All Users)
- Phishing Simulation (Quarterly, All Users)
- Secure Coding (Annual, Developers)
- Incident Response (Bi-Annual, Admins)

## Monitoring & Alerts

### Security Metrics

**Track**:
- Failed login attempts per hour
- API calls from unauthorized IPs
- Database query anomalies
- Unusual data export volumes
- Certificate expiration (30 day warning)

**Alerts**:
```yaml
# Prometheus alert rules
groups:
- name: security
  rules:
  - alert: HighFailedLoginRate
    expr: rate(failed_login_attempts[5m]) > 0.1
    for: 5m
    annotations:
      summary: "High failed login rate detected"

  - alert: UnauthorizedAPIAccess
    expr: unauthorized_api_calls > 10
    annotations:
      summary: "Unauthorized API access attempts"
```

## Contact & Resources

**Security Team**:
- Email: security@scada-system.com
- Emergency: +1-XXX-XXX-XXXX (24/7)

**Resources**:
- CISA ICS Advisories: https://www.cisa.gov/uscert/ics
- ISA/IEC 62443: https://www.isa.org/standards-and-publications/isa-standards/
- OWASP Top 10: https://owasp.org/www-project-top-ten/

## Revision History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-01-01 | Initial release |
