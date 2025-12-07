-- Database Migration Script v1.0.0
-- Initial schema creation for SCADA system

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Function to update timestamps
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- =============================================
-- TAGS TABLE (from ScadaCore)
-- =============================================

CREATE TABLE IF NOT EXISTS tags (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) UNIQUE NOT NULL,
    description TEXT,
    data_type VARCHAR(50) NOT NULL,
    units VARCHAR(50),
    min_value DOUBLE PRECISION,
    max_value DOUBLE PRECISION,
    scale_factor DOUBLE PRECISION DEFAULT 1.0,
    offset DOUBLE PRECISION DEFAULT 0.0,
    scan_rate INTEGER DEFAULT 1000,
    site VARCHAR(100),
    device VARCHAR(100),
    is_enabled BOOLEAN DEFAULT true,
    log_history BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_tags_name ON tags(name);
CREATE INDEX idx_tags_site_device ON tags(site, device);
CREATE INDEX idx_tags_enabled ON tags(is_enabled);

CREATE TRIGGER update_tags_updated_at
    BEFORE UPDATE ON tags
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- =============================================
-- ALARM RULES TABLE
-- =============================================

CREATE TABLE IF NOT EXISTS alarm_rules (
    id SERIAL PRIMARY KEY,
    tag_id INTEGER REFERENCES tags(id) ON DELETE CASCADE,
    alarm_type VARCHAR(50) NOT NULL,
    threshold DOUBLE PRECISION NOT NULL,
    priority VARCHAR(50) NOT NULL,
    message TEXT,
    is_enabled BOOLEAN DEFAULT true,
    deadband DOUBLE PRECISION DEFAULT 0.0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_alarm_rules_tag_id ON alarm_rules(tag_id);
CREATE INDEX idx_alarm_rules_enabled ON alarm_rules(is_enabled);

-- =============================================
-- USERS TABLE (Authentication)
-- =============================================

CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    username VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    role VARCHAR(50) NOT NULL DEFAULT 'Viewer',
    is_active BOOLEAN DEFAULT true,
    mfa_enabled BOOLEAN DEFAULT false,
    mfa_secret VARCHAR(255),
    last_login TIMESTAMP,
    failed_login_attempts INTEGER DEFAULT 0,
    locked_until TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_active ON users(is_active);

CREATE TRIGGER update_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- =============================================
-- SESSIONS TABLE
-- =============================================

CREATE TABLE IF NOT EXISTS sessions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
    token VARCHAR(500) NOT NULL,
    refresh_token VARCHAR(500),
    ip_address VARCHAR(45),
    user_agent TEXT,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_sessions_user_id ON sessions(user_id);
CREATE INDEX idx_sessions_token ON sessions(token);
CREATE INDEX idx_sessions_expires_at ON sessions(expires_at);

-- =============================================
-- AUDIT LOGS TABLE
-- =============================================

CREATE TABLE IF NOT EXISTS audit_logs (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(id),
    action VARCHAR(100) NOT NULL,
    entity_type VARCHAR(100),
    entity_id VARCHAR(100),
    details JSONB,
    ip_address VARCHAR(45),
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX idx_audit_logs_timestamp ON audit_logs(timestamp DESC);
CREATE INDEX idx_audit_logs_entity ON audit_logs(entity_type, entity_id);
CREATE INDEX idx_audit_logs_action ON audit_logs(action);

-- =============================================
-- REPORT TEMPLATES TABLE
-- =============================================

CREATE TABLE IF NOT EXISTS report_templates (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    template_type VARCHAR(50),
    configuration JSONB,
    created_by INTEGER REFERENCES users(id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_report_templates_type ON report_templates(template_type);

CREATE TRIGGER update_report_templates_updated_at
    BEFORE UPDATE ON report_templates
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- =============================================
-- SCHEDULED REPORTS TABLE
-- =============================================

CREATE TABLE IF NOT EXISTS scheduled_reports (
    id SERIAL PRIMARY KEY,
    template_id INTEGER REFERENCES report_templates(id) ON DELETE CASCADE,
    schedule_cron VARCHAR(100),
    recipients TEXT[],
    is_enabled BOOLEAN DEFAULT true,
    last_run TIMESTAMP,
    next_run TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_scheduled_reports_next_run ON scheduled_reports(next_run);
CREATE INDEX idx_scheduled_reports_enabled ON scheduled_reports(is_enabled);

-- =============================================
-- SEED DATA
-- =============================================

-- Insert default admin user
INSERT INTO users (email, username, password_hash, first_name, last_name, role, is_active)
VALUES (
    'admin@scada.local',
    'admin',
    '$2a$12$LExrX/zX6Y4H0MqHzrF1xuNS9VgxLVy1KwJZ8k9BhPa9X5xT3XnzS',  -- Admin123!
    'System',
    'Administrator',
    'Administrator',
    true
) ON CONFLICT (email) DO NOTHING;

-- Insert sample tags
INSERT INTO tags (name, description, data_type, units, min_value, max_value, scan_rate, site, device, is_enabled, log_history)
VALUES
    ('SITE01.TURBINE01.WindSpeed', 'Wind turbine blade speed', 'Float', 'm/s', 0, 50, 1000, 'SITE01', 'TURBINE01', true, true),
    ('SITE01.TURBINE01.PowerOutput', 'Turbine power generation', 'Float', 'kW', 0, 5000, 1000, 'SITE01', 'TURBINE01', true, true),
    ('SITE02.SOLAR01.Voltage', 'Solar panel voltage', 'Float', 'V', 0, 1000, 500, 'SITE02', 'SOLAR01', true, true),
    ('SITE02.SOLAR01.Current', 'Solar panel current', 'Float', 'A', 0, 100, 500, 'SITE02', 'SOLAR01', true, true),
    ('SITE01.BATTERY01.StateOfCharge', 'Battery state of charge percentage', 'Float', '%', 0, 100, 2000, 'SITE01', 'BATTERY01', true, true)
ON CONFLICT (name) DO NOTHING;

-- Insert sample alarm rules
INSERT INTO alarm_rules (tag_id, alarm_type, threshold, priority, message, deadband, is_enabled)
SELECT id, 'High', 40.0, 'Medium', 'Wind speed is too high', 2.0, true
FROM tags WHERE name = 'SITE01.TURBINE01.WindSpeed'
ON CONFLICT DO NOTHING;

INSERT INTO alarm_rules (tag_id, alarm_type, threshold, priority, message, deadband, is_enabled)
SELECT id, 'Low', 500.0, 'Low', 'Power output is below threshold', 50.0, true
FROM tags WHERE name = 'SITE01.TURBINE01.PowerOutput'
ON CONFLICT DO NOTHING;

INSERT INTO alarm_rules (tag_id, alarm_type, threshold, priority, message, deadband, is_enabled)
SELECT id, 'LowLow', 20.0, 'Critical', 'Battery critically low!', 5.0, true
FROM tags WHERE name = 'SITE01.BATTERY01.StateOfCharge'
ON CONFLICT DO NOTHING;

-- Audit the migration
INSERT INTO audit_logs (user_id, action, entity_type, details)
SELECT id, 'DATABASE_MIGRATION', 'Schema', '{"version": "1.0.0", "description": "Initial schema creation"}'::jsonb
FROM users WHERE username = 'admin';

-- Display migration summary
DO $$
BEGIN
    RAISE NOTICE 'Migration completed successfully!';
    RAISE NOTICE 'Tables created: tags, alarm_rules, users, sessions, audit_logs, report_templates, scheduled_reports';
    RAISE NOTICE 'Default admin user: admin@scada.local / Admin123!';
    RAISE NOTICE 'Sample tags inserted: %', (SELECT COUNT(*) FROM tags);
END $$;
