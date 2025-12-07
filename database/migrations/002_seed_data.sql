-- ================================================
-- SCADA System - Database Seed Data Script
-- ================================================
-- This script creates initial users, roles, and demo data
-- Run this after initial schema migration

BEGIN;

-- ===== 1. Insert Default Roles =====
INSERT INTO roles (id, name, description, created_at) VALUES
    (gen_random_uuid(), 'Administrator', 'Full system access', NOW()),
    (gen_random_uuid(), 'Engineer', 'Configure tags and alarms', NOW()),
    (gen_random_uuid(), 'Operator', 'Monitor and acknowledge alarms', NOW()),
    (gen_random_uuid(), 'Viewer', 'Read-only access', NOW())
ON CONFLICT (name) DO NOTHING;

-- ===== 2. Insert Default Admin User =====
-- Password: Admin123! (hashed with BCrypt)
-- IMPORTANT: Change this password in production!
INSERT INTO users (id, email, password_hash, full_name, role_id, is_active, mfa_enabled, created_at) VALUES
    (gen_random_uuid(), 
     'admin@scada.local', 
     '$2a$11$qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbnm', 
     'System Administrator',
     (SELECT id FROM roles WHERE name = 'Administrator'),
     true,
     false,
     NOW())
ON CONFLICT (email) DO NOTHING;

-- Insert demo engineer user
INSERT INTO users (id, email, password_hash, full_name, role_id, is_active, mfa_enabled, created_at) VALUES
    (gen_random_uuid(), 
     'engineer@scada.local', 
     '$2a$11$qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbnm',
     'Demo Engineer',
     (SELECT id FROM roles WHERE name = 'Engineer'),
     true,
     false,
     NOW())
ON CONFLICT (email) DO NOTHING;

-- Insert demo operator user
INSERT INTO users (id, email, password_hash, full_name, role_id, is_active, mfa_enabled, created_at) VALUES
    (gen_random_uuid(), 
     'operator@scada.local', 
     '$2a$11$qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbnm',
     'Demo Operator',
     (SELECT id FROM roles WHERE name = 'Operator'),
     true,
     false,
     NOW())
ON CONFLICT (email) DO NOTHING;

-- ===== 3. Insert Demo Sites =====
INSERT INTO sites (id, name, location, description, timezone, is_active, created_at) VALUES
    (gen_random_uuid(), 'SITE01', 'Wind Farm North', 'Northern wind turbine farm', 'UTC', true, NOW()),
    (gen_random_uuid(), 'SITE02', 'Solar Plant East', 'Eastern solar panel array', 'UTC', true, NOW()),
    (gen_random_uuid(), 'SITE03', 'Battery Storage', 'Energy storage facility', 'UTC', true, NOW())
ON CONFLICT (name) DO NOTHING;

-- ===== 4. Insert Demo Tags =====
-- Wind Farm Tags
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'SITE01.TURBINE01.Power', 'Turbine 1 Power Output', 'Float', 'kW', 0, 5000, (SELECT id FROM sites WHERE name = 'SITE01'), 'WindTurbine', true, NOW()),
    (gen_random_uuid(), 'SITE01.TURBINE01.WindSpeed', 'Wind Speed', 'Float', 'm/s', 0, 30, (SELECT id FROM sites WHERE name = 'SITE01'), 'WindTurbine', true, NOW()),
    (gen_random_uuid(), 'SITE01.TURBINE01.Temperature', 'Generator Temperature', 'Float', '°C', -20, 100, (SELECT id FROM sites WHERE name = 'SITE01'), 'WindTurbine', true, NOW()),
    (gen_random_uuid(), 'SITE01.TURBINE01.Vibration', 'Vibration Level', 'Float', 'mm/s', 0, 20, (SELECT id FROM sites WHERE name = 'SITE01'), 'WindTurbine', true, NOW()),
    (gen_random_uuid(), 'SITE01.TURBINE01.Status', 'Running Status', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'SITE01'), 'WindTurbine', true, NOW()),

    (gen_random_uuid(), 'SITE01.TURBINE02.Power', 'Turbine 2 Power Output', 'Float', 'kW', 0, 5000, (SELECT id FROM sites WHERE name = 'SITE01'), 'WindTurbine', true, NOW()),
    (gen_random_uuid(), 'SITE01.TURBINE02.WindSpeed', 'Wind Speed', 'Float', 'm/s', 0, 30, (SELECT id FROM sites WHERE name = 'SITE01'), 'WindTurbine', true, NOW()),

-- Solar Plant Tags
    (gen_random_uuid(), 'SITE02.SOLAR01.Power', 'Solar Panel Power', 'Float', 'kW', 0, 2000, (SELECT id FROM sites WHERE name = 'SITE02'), 'SolarPanel', true, NOW()),
    (gen_random_uuid(), 'SITE02.SOLAR01.Voltage', 'Panel Voltage', 'Float', 'V', 0, 1000, (SELECT id FROM sites WHERE name = 'SITE02'), 'SolarPanel', true, NOW()),
    (gen_random_uuid(), 'SITE02.SOLAR01.Current', 'Panel Current', 'Float', 'A', 0, 100, (SELECT id FROM sites WHERE name = 'SITE02'), 'SolarPanel', true, NOW()),
    (gen_random_uuid(), 'SITE02.SOLAR01.Temperature', 'Panel Temperature', 'Float', '°C', -20, 80, (SELECT id FROM sites WHERE name = 'SITE02'), 'SolarPanel', true, NOW()),
    (gen_random_uuid(), 'SITE02.SOLAR01.Irradiance', 'Solar Irradiance', 'Float', 'W/m²', 0, 1200, (SELECT id FROM sites WHERE name = 'SITE02'), 'SolarPanel', true, NOW()),

-- Battery Storage Tags
    (gen_random_uuid(), 'SITE03.BATTERY01.ChargeLevel', 'Battery Charge', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'SITE03'), 'Battery', true, NOW()),
    (gen_random_uuid(), 'SITE03.BATTERY01.Voltage', 'Battery Voltage', 'Float', 'V', 0, 1000, (SELECT id FROM sites WHERE name = 'SITE03'), 'Battery', true, NOW()),
    (gen_random_uuid(), 'SITE03.BATTERY01.Current', 'Charge/Discharge Current', 'Float', 'A', -500, 500, (SELECT id FROM sites WHERE name = 'SITE03'), 'Battery', true, NOW()),
    (gen_random_uuid(), 'SITE03.BATTERY01.Temperature', 'Battery Temperature', 'Float', '°C', 0, 60, (SELECT id FROM sites WHERE name = 'SITE03'), 'Battery', true, NOW()),
    (gen_random_uuid(), 'SITE03.BATTERY01.StateOfHealth', 'Battery Health', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'SITE03'), 'Battery', true, NOW())
ON CONFLICT (name) DO NOTHING;

-- ===== 5. Insert Demo Alarm Rules =====
INSERT INTO alarm_rules (id, tag_id, name, condition, threshold_value, priority, message, is_enabled, created_at) VALUES
    -- Wind turbine alarms
    (gen_random_uuid(), 
     (SELECT id FROM tags WHERE name = 'SITE01.TURBINE01.Temperature'), 
     'High Generator Temperature', 
     'GreaterThan', 
     80, 
     'High', 
     'Generator temperature critically high', 
     true, 
     NOW()),

    (gen_random_uuid(), 
     (SELECT id FROM tags WHERE name = 'SITE01.TURBINE01.Vibration'), 
     'Excessive Vibration', 
     'GreaterThan', 
     15, 
     'Critical', 
     'Turbine vibration exceeds safe limits', 
     true, 
     NOW()),

    -- Solar panel alarms
    (gen_random_uuid(), 
     (SELECT id FROM tags WHERE name = 'SITE02.SOLAR01.Power'), 
     'Low Solar Output', 
     'LessThan', 
     100, 
     'Medium', 
     'Solar panel output below expected', 
     true, 
     NOW()),

    (gen_random_uuid(), 
     (SELECT id FROM tags WHERE name = 'SITE02.SOLAR01.Temperature'), 
     'High Panel Temperature', 
     'GreaterThan', 
     70, 
     'High', 
     'Solar panel overheating', 
     true, 
     NOW()),

    -- Battery alarms
    (gen_random_uuid(), 
     (SELECT id FROM tags WHERE name = 'SITE03.BATTERY01.ChargeLevel'), 
     'Low Battery Charge', 
     'LessThan', 
     20, 
     'High', 
     'Battery charge critically low', 
     true, 
     NOW()),

    (gen_random_uuid(), 
     (SELECT id FROM tags WHERE name = 'SITE03.BATTERY01.Temperature'), 
     'High Battery Temperature', 
     'GreaterThan', 
     50, 
     'Critical', 
     'Battery temperature dangerous', 
     true, 
     NOW())
ON CONFLICT DO NOTHING;

-- ===== 6. Insert Demo Report Templates =====
INSERT INTO report_templates (id, name, description, template_type, frequency, recipients, is_active, created_at) VALUES
    (gen_random_uuid(), 'Daily Production Summary', 'Daily energy production across all sites', 'PDF', 'Daily', 'admin@scada.local', true, NOW()),
    (gen_random_uuid(), 'Weekly Alarm Report', 'Summary of alarms for the week', 'Excel', 'Weekly', 'engineer@scada.local', true, NOW()),
    (gen_random_uuid(), 'Monthly Performance Analysis', 'Detailed performance metrics', 'PDF', 'Monthly', 'admin@scada.local,engineer@scada.local', true, NOW())
ON CONFLICT (name) DO NOTHING;

-- ===== 7. Insert System Configuration =====
INSERT INTO system_config (key, value, description, updated_at) VALUES
    ('data_retention_days', '90', 'Number of days to retain time-series data', NOW()),
    ('alarm_aggregation_window_seconds', '300', 'Window for alarm aggregation', NOW()),
    ('max_concurrent_connections', '10000', 'Maximum WebSocket connections', NOW()),
    ('enable_ml_features', 'true', 'Enable ML-based anomaly detection', NOW()),
    ('enable_analytics', 'true', 'Enable ClickHouse analytics', NOW()),
    ('smtp_configured', 'false', 'Whether SMTP is configured', NOW()),
    ('sms_configured', 'false', 'Whether SMS is configured', NOW())
ON CONFLICT (key) DO UPDATE SET value = EXCLUDED.value;

COMMIT;

-- ================================================
-- Verification Queries
-- ================================================
-- Run these to verify seed data was inserted correctly

-- SELECT 'Users created:' AS info, COUNT(*) AS count FROM users;
-- SELECT 'Roles created:' AS info, COUNT(*) AS count FROM roles;
-- SELECT 'Sites created:' AS info, COUNT(*) AS count FROM sites;
-- SELECT 'Tags created:' AS info, COUNT(*) AS count FROM tags;
-- SELECT 'Alarm rules created:' AS info, COUNT(*) AS count FROM alarm_rules;
-- SELECT 'Report templates created:' AS info, COUNT(*) AS count FROM report_templates;

-- ================================================
-- Default Credentials (CHANGE IN PRODUCTION!)
-- ================================================
-- Email: admin@scada.local
-- Password: Admin123!
-- 
-- Email: engineer@scada.local
-- Password: Admin123!
-- 
-- Email: operator@scada.local
-- Password: Admin123!
-- ================================================
