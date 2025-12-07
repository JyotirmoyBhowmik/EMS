-- ================================================
-- ENHANCEMENT: Hierarchical Energy Metering
-- ================================================
-- Support for multi-level meter hierarchy and power loss tracking
-- Date: 2025-01-08
-- ================================================

BEGIN;

-- ===== METER HIERARCHY TABLES =====

-- Meter definitions (physical meters in the system)
CREATE TABLE IF NOT EXISTS energy_meters (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- Meter identification
    meter_number VARCHAR(100) UNIQUE NOT NULL,
    meter_name VARCHAR(200) NOT NULL,
    meter_type VARCHAR(50), -- Grid, DG, Solar, PMD, SMD, Submeter, Machine
    manufacturer VARCHAR(100),
    model VARCHAR(100),
    
    -- Hierarchy
    parent_meter_id UUID REFERENCES energy_meters(id), -- For hierarchical structure
    level INT DEFAULT 0, -- 0=Grid, 1=Main, 2=Substation, 3=Unit, 4=Machine
    
    -- Location
    site_id UUID REFERENCES sites(id),
    location VARCHAR(200),
    building VARCHAR(100),
    floor VARCHAR(50),
    
    -- Capacity
    rated_capacity_kw DECIMAL(10,2),
    rated_voltage DECIMAL(10,2),
    rated_current DECIMAL(10,2),
    
    -- Status
    is_active BOOLEAN DEFAULT true,
    installation_date DATE,
    last_calibration_date DATE,
    next_calibration_date DATE,
    
    -- Metadata
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_meter_parent ON energy_meters(parent_meter_id);
CREATE INDEX idx_meter_type ON energy_meters(meter_type);
CREATE INDEX idx_meter_level ON energy_meters(level);

-- Meter readings (replaces/enhances energy_consumption)
CREATE TABLE IF NOT EXISTS meter_readings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    meter_id UUID REFERENCES energy_meters(id) NOT NULL,
    timestamp TIMESTAMP NOT NULL,
    
    -- Electrical measurements
    voltage_l1 DECIMAL(10,2),
    voltage_l2 DECIMAL(10,2),
    voltage_l3 DECIMAL(10,2),
    current_l1 DECIMAL(10,2),
    current_l2 DECIMAL(10,2),
    current_l3 DECIMAL(10,2),
    
    -- Power
    active_power_kw DECIMAL(10,2),
    reactive_power_kvar DECIMAL(10,2),
    apparent_power_kva DECIMAL(10,2),
    power_factor DECIMAL(5,4),
    frequency_hz DECIMAL(5,2),
    
    -- Energy (cumulative)
    total_energy_kwh DECIMAL(15,3), -- Cumulative total
    energy_import_kwh DECIMAL(15,3), -- From grid
    energy_export_kwh DECIMAL(15,3), -- To grid (solar)
    
    -- For DG meters
    diesel_consumption_liters DECIMAL(10,2),
    runtime_hours DECIMAL(10,2),
    
    -- Calculated
    energy_cost DECIMAL(10,2),
    carbon_kg_co2 DECIMAL(10,2),
    
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_meter_reading_meter ON meter_readings(meter_id);
CREATE INDEX idx_meter_reading_timestamp ON meter_readings(timestamp);

-- Power loss tracking
CREATE TABLE IF NOT EXISTS power_loss_analysis (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- Time period
    analysis_date DATE NOT NULL,
    hour_of_day INT,
    
    -- Meters being compared
    source_meter_id UUID REFERENCES energy_meters(id), -- Parent meter
    destination_meters UUID[], -- Array of child meter IDs
    
    -- Energy comparison
    source_energy_kwh DECIMAL(15,3),
    total_destination_energy_kwh DECIMAL(15,3),
    energy_loss_kwh DECIMAL(15,3),
    loss_percentage DECIMAL(5,2),
    
    -- Cost impact
    loss_cost DECIMAL(10,2),
    
    -- Classification
    loss_type VARCHAR(50), -- Technical, Non-Technical, Acceptable
    
    created_at TIMESTAMP DEFAULT NOW()
);

-- Renewable energy sources
CREATE TABLE IF NOT EXISTS renewable_sources (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    meter_id UUID REFERENCES energy_meters(id),
    
    -- Source info
    source_type VARCHAR(50), -- Solar, Wind, Hydro, Biomass
    capacity_kw DECIMAL(10,2),
    
    -- Carbon offset
    carbon_offset_factor DECIMAL(5,4), -- kg CO2 saved per kWh
    
    -- Performance
    efficiency_percent DECIMAL(5,2),
    availability_percent DECIMAL(5,2),
    
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Diesel generator tracking
CREATE TABLE IF NOT EXISTS diesel_generators (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    meter_id UUID REFERENCES energy_meters(id),
    
    -- Generator info
    generator_name VARCHAR(200),
    rated_capacity_kva DECIMAL(10,2),
    fuel_tank_capacity_liters DECIMAL(10,2),
    
    -- Efficiency
    specific_fuel_consumption_lkwh DECIMAL(5,3), -- Liters per kWh
    
    -- Carbon emissions
    carbon_emission_factor DECIMAL(5,3), -- kg CO2 per liter diesel (default: 2.68)
    
    -- Maintenance
    total_runtime_hours DECIMAL(10,2),
    last_service_date DATE,
    next_service_hours DECIMAL(10,2),
    
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW()
);

-- ===== SEED DATA FOR HIERARCHICAL STRUCTURE =====

-- Level 0: Grid Connection (Source)
INSERT INTO energy_meters (meter_number, meter_name, meter_type, level, site_id) VALUES
    ('GRID-001', 'Main Grid Connection', 'Grid', 0, (SELECT id FROM sites WHERE name = 'FACTORY_01'));

-- Level 1: Power Station Advanced Meter
INSERT INTO energy_meters (meter_number, meter_name, meter_type, level, parent_meter_id, site_id) VALUES
    ('PS-MAIN-001', 'Power Station Main Meter', 'Main', 1, 
     (SELECT id FROM energy_meters WHERE meter_number = 'GRID-001'),
     (SELECT id FROM sites WHERE name = 'FACTORY_01'));

-- Level 2: Major Distribution Meters
WITH main_meter AS (SELECT id FROM energy_meters WHERE meter_number = 'PS-MAIN-001')
INSERT INTO energy_meters (meter_number, meter_name, meter_type, level, parent_meter_id, site_id) VALUES
    ('DG-001', 'Diesel Generator Meter', 'DG', 2, (SELECT id FROM main_meter), (SELECT id FROM sites WHERE name = 'FACTORY_01')),
    ('SOLAR-001', 'Solar Panel Array Meter', 'Solar', 2, (SELECT id FROM main_meter), (SELECT id FROM sites WHERE name = 'FACTORY_01')),
    ('PMD-001', 'Power Management Device Unit', 'PMD', 2, (SELECT id FROM main_meter), (SELECT id FROM sites WHERE name = 'FACTORY_01')),
    ('SMD-001', 'Sub Main Distribution Unit', 'SMD', 2, (SELECT id FROM main_meter), (SELECT id FROM sites WHERE name = 'FACTORY_01')),
    ('LIGHT-001', 'Lighting Circuit Meter', 'Submeter', 2, (SELECT id FROM main_meter), (SELECT id FROM sites WHERE name = 'FACTORY_01')),
    ('ADMIN-001', 'Admin Building Meter', 'Submeter', 2, (SELECT id FROM main_meter), (SELECT id FROM sites WHERE name = 'FACTORY_01'));

-- Level 3: Unit-level meters (example under PMD)
WITH pmd_meter AS (SELECT id FROM energy_meters WHERE meter_number = 'PMD-001')
INSERT INTO energy_meters (meter_number, meter_name, meter_type, level, parent_meter_id, site_id) VALUES
    ('PMD-UNIT-A', 'Production Unit A', 'Submeter', 3, (SELECT id FROM pmd_meter), (SELECT id FROM sites WHERE name = 'FACTORY_01')),
    ('PMD-UNIT-B', 'Production Unit B', 'Submeter', 3, (SELECT id FROM pmd_meter), (SELECT id FROM sites WHERE name = 'FACTORY_01')),
    ('PMD-UNIT-C', 'Production Unit C', 'Submeter', 3, (SELECT id FROM pmd_meter), (SELECT id FROM sites WHERE name = 'FACTORY_01'));

-- Level 4: Machine-level meters (example under Unit A)
WITH unit_a AS (SELECT id FROM energy_meters WHERE meter_number = 'PMD-UNIT-A')
INSERT INTO energy_meters (meter_number, meter_name, meter_type, level, parent_meter_id, site_id, location) VALUES
    ('UNIT-A-CNC-01', 'CNC Machine 01', 'Machine', 4, (SELECT id FROM unit_a), (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Unit A - Bay 1'),
    ('UNIT-A-CNC-02', 'CNC Machine 02', 'Machine', 4, (SELECT id FROM unit_a), (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Unit A - Bay 2'),
    ('UNIT-A-COMP-01', 'Air Compressor 01', 'Machine', 4, (SELECT id FROM unit_a), (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Unit A - Utility'),
    ('UNIT-A-HVAC-01', 'HVAC System', 'Machine', 4, (SELECT id FROM unit_a), (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Unit A - Roof');

-- Configure DG meter
INSERT INTO diesel_generators (meter_id, generator_name, rated_capacity_kva, specific_fuel_consumption_lkwh, carbon_emission_factor) VALUES
    ((SELECT id FROM energy_meters WHERE meter_number = 'DG-001'), 
     'Cummins 500 KVA DG', 
     500, 
     0.28, -- 0.28 liters per kWh
     2.68); -- 2.68 kg CO2 per liter diesel

-- Configure Solar meter
INSERT INTO renewable_sources (meter_id, source_type, capacity_kw, carbon_offset_factor) VALUES
    ((SELECT id FROM energy_meters WHERE meter_number = 'SOLAR-001'),
     'Solar',
     250, -- 250 kW solar capacity
     0.5); -- 0.5 kg CO2 saved per kWh (grid offset)

COMMIT;

-- ================================================
-- UTILITY QUERIES
-- ================================================

-- Get meter hierarchy (tree structure)
WITH RECURSIVE meter_tree AS (
    -- Root meters (Grid level)
    SELECT 
        id, meter_number, meter_name, meter_type, level, parent_meter_id,
        meter_name as path,
        0 as depth
    FROM energy_meters
    WHERE parent_meter_id IS NULL
    
    UNION ALL
    
    -- Child meters
    SELECT 
        m.id, m.meter_number, m.meter_name, m.meter_type, m.level, m.parent_meter_id,
        mt.path || ' > ' || m.meter_name as path,
        mt.depth + 1
    FROM energy_meters m
    INNER JOIN meter_tree mt ON m.parent_meter_id = mt.id
)
SELECT 
    REPEAT('  ', depth) || meter_name as hierarchy,
    meter_number,
    meter_type,
    level
FROM meter_tree
ORDER BY path;

-- Calculate power loss between main meter and sub-meters
SELECT 
    date_trunc('hour', mr.timestamp) as hour,
    main.meter_name as source_meter,
    SUM(main_reading.total_energy_kwh) as source_energy,
    SUM(mr.total_energy_kwh) as total_sub_energy,
    SUM(main_reading.total_energy_kwh) - SUM(mr.total_energy_kwh) as energy_loss,
    ((SUM(main_reading.total_energy_kwh) - SUM(mr.total_energy_kwh)) / NULLIF(SUM(main_reading.total_energy_kwh), 0) * 100) as loss_percent
FROM meter_readings mr
JOIN energy_meters sub ON mr.meter_id = sub.id
JOIN energy_meters main ON sub.parent_meter_id = main.id
JOIN meter_readings main_reading ON main_reading.meter_id = main.id 
    AND date_trunc('hour', main_reading.timestamp) = date_trunc('hour', mr.timestamp)
WHERE mr.timestamp >= NOW() - INTERVAL '24 hours'
GROUP BY date_trunc('hour', mr.timestamp), main.meter_name
ORDER BY hour DESC;
