-- ================================================
-- ENHANCEMENT: Meter Management with CT/PT and Status
-- ================================================
-- Add CT/PT ratios, meter status, and management features
-- Date: 2025-01-08
-- ================================================

BEGIN;

-- Add columns to energy_meters table
ALTER TABLE energy_meters 
ADD COLUMN IF NOT EXISTS ct_primary_amps DECIMAL(10,2),
ADD COLUMN IF NOT EXISTS ct_secondary_amps DECIMAL(10,2) DEFAULT 5,
ADD COLUMN IF NOT EXISTS ct_ratio VARCHAR(20), -- e.g., "1000/5"
ADD COLUMN IF NOT EXISTS pt_primary_volts DECIMAL(10,2),
ADD COLUMN IF NOT EXISTS pt_secondary_volts DECIMAL(10,2) DEFAULT 110,
ADD COLUMN IF NOT EXISTS pt_ratio VARCHAR(20), -- e.g., "11000/110"

-- Status and health
ADD COLUMN IF NOT EXISTS status VARCHAR(20) DEFAULT 'active', -- active, inactive, maintenance, faulty
ADD COLUMN IF NOT EXISTS health_status VARCHAR(20) DEFAULT 'good', -- good, warning, critical
ADD COLUMN IF NOT EXISTS last_communication TIMESTAMP,
ADD COLUMN IF NOT EXISTS communication_status VARCHAR(20) DEFAULT 'online', -- online, offline, timeout

-- Display properties
ADD COLUMN IF NOT EXISTS display_order INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS icon VARCHAR(50), -- For UI display
ADD COLUMN IF NOT EXISTS color VARCHAR(20), -- For visual coding

-- Configuration
ADD COLUMN IF NOT EXISTS modbus_address INT,
ADD COLUMN IF NOT EXISTS ip_address VARCHAR(45),
ADD COLUMN IF NOT EXISTS communication_protocol VARCHAR(50); -- Modbus, OPC UA, MQTT, etc.

-- Meter status history
CREATE TABLE IF NOT EXISTS meter_status_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    meter_id UUID REFERENCES energy_meters(id),
    
    -- Status change
    previous_status VARCHAR(20),
    new_status VARCHAR(20),
    change_reason TEXT,
    
    -- Health change
    previous_health VARCHAR(20),
    new_health VARCHAR(20),
    
    -- Metadata
    changed_by_user_id UUID REFERENCES users(id),
    changed_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_meter_status_history_meter ON meter_status_history(meter_id);
CREATE INDEX idx_meter_status_history_time ON meter_status_history(changed_at);

-- Meter configuration templates (for quick setup)
CREATE TABLE IF NOT EXISTS meter_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    template_name VARCHAR(200) NOT NULL,
    meter_type VARCHAR(50),
    manufacturer VARCHAR(100),
    model VARCHAR(100),
    
    -- Default CT/PT values
    default_ct_ratio VARCHAR(20),
    default_pt_ratio VARCHAR(20),
    
    -- Default communication
    default_protocol VARCHAR(50),
    default_port INT,
    
    -- Configuration JSON
    configuration_json JSONB,
    
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Update existing meters with CT/PT examples
UPDATE energy_meters 
SET 
    ct_primary_amps = 1000,
    ct_secondary_amps = 5,
    ct_ratio = '1000/5',
    pt_primary_volts = 11000,
    pt_secondary_volts = 110,
    pt_ratio = '11000/110',
    status = 'active',
    health_status = 'good',
    communication_status = 'online',
    last_communication = NOW()
WHERE meter_type IN ('Grid', 'Main', 'PMD', 'SMD');

-- Set appropriate CT/PT for different meter types
UPDATE energy_meters 
SET 
    ct_primary_amps = 100,
    ct_ratio = '100/5',
    pt_ratio = '415/110'
WHERE meter_type = 'Submeter';

UPDATE energy_meters 
SET 
    ct_primary_amps = 50,
    ct_ratio = '50/5',
    pt_ratio = '415/110'
WHERE meter_type = 'Machine';

-- Insert meter templates
INSERT INTO meter_templates (template_name, meter_type, default_ct_ratio, default_pt_ratio, default_protocol) VALUES
    ('Schneider PM8000 - Main Distribution', 'Main', '2000/5', '11000/110', 'Modbus TCP'),
    ('Schneider PM5000 - Submeter', 'Submeter', '400/5', '415/110', 'Modbus TCP'),
    ('ABB M2M - Feeder Meter', 'Submeter', '200/5', '415/110', 'Modbus RTU'),
    ('Siemens PAC3200 - Machine Meter', 'Machine', '100/5', '415/110', 'Modbus RTU');

COMMIT;

-- ================================================
-- VIEWS FOR UI
-- ================================================

-- Meter tree view with status
CREATE OR REPLACE VIEW meter_tree_view AS
WITH RECURSIVE meter_hierarchy AS (
    SELECT 
        m.id,
        m.meter_number,
        m.meter_name,
        m.meter_type,
        m.level,
        m.parent_meter_id,
        m.status,
        m.health_status,
        m.communication_status,
        m.ct_ratio,
        m.pt_ratio,
        m.display_order,
        m.meter_name as path,
        ARRAY[m.id] as id_path,
        0 as depth
    FROM energy_meters m
    WHERE m.parent_meter_id IS NULL
    
    UNION ALL
    
    SELECT 
        m.id,
        m.meter_number,
        m.meter_name,
        m.meter_type,
        m.level,
        m.parent_meter_id,
        m.status,
        m.health_status,
        m.communication_status,
        m.ct_ratio,
        m.pt_ratio,
        m.display_order,
        mh.path || ' > ' || m.meter_name,
        mh.id_path || m.id,
        mh.depth + 1
    FROM energy_meters m
    INNER JOIN meter_hierarchy mh ON m.parent_meter_id = mh.id
)
SELECT * FROM meter_hierarchy
ORDER BY path, display_order;

-- Meter status summary
CREATE OR REPLACE VIEW meter_status_summary AS
SELECT 
    COUNT(*) as total_meters,
    COUNT(*) FILTER (WHERE status = 'active') as active_meters,
    COUNT(*) FILTER (WHERE status = 'inactive') as inactive_meters,
    COUNT(*) FILTER (WHERE status = 'maintenance') as maintenance_meters,
    COUNT(*) FILTER (WHERE status = 'faulty') as faulty_meters,
    COUNT(*) FILTER (WHERE health_status = 'good') as healthy_meters,
    COUNT(*) FILTER (WHERE health_status = 'warning') as warning_meters,
    COUNT(*) FILTER (WHERE health_status = 'critical') as critical_meters,
    COUNT(*) FILTER (WHERE communication_status = 'online') as online_meters,
    COUNT(*) FILTER (WHERE communication_status = 'offline') as offline_meters
FROM energy_meters;
