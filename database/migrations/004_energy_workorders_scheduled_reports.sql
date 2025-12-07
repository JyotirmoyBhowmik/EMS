-- ================================================
-- MIGRATION 004: Energy Management, Work Orders, Scheduled Reports
-- ================================================
-- Date: 2025-01-08
-- Description: Add tables for energy tracking, work order management, and scheduled reports
-- ================================================

BEGIN;

-- ===== ENERGY MANAGEMENT TABLES =====

-- Energy consumption records (real-time and historical)
CREATE TABLE IF NOT EXISTS energy_consumption (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    timestamp TIMESTAMP NOT NULL,
    site_id UUID REFERENCES sites(id),
    equipment_tag_id UUID REFERENCES tags(id),
    
    -- Power measurements
    active_power_kw DECIMAL(10,2),
    reactive_power_kvar DECIMAL(10,2),
    apparent_power_kva DECIMAL(10,2),
    power_factor DECIMAL(5,4),
    
    -- Energy totals
    energy_kwh DECIMAL(15,3),
    energy_cost DECIMAL(10,2),
    
    -- Carbon footprint
    carbon_kg_co2 DECIMAL(10,2),
    
    -- Metadata
    created_at TIMESTAMP DEFAULT NOW()
);

-- Create index for fast time-range queries
CREATE INDEX idx_energy_timestamp ON energy_consumption(timestamp);
CREATE INDEX idx_energy_site ON energy_consumption(site_id);

-- Energy targets and baselines (ISO 50001 compliance)
CREATE TABLE IF NOT EXISTS energy_targets (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID REFERENCES sites(id),
    
    -- Target definition
    name VARCHAR(200),
    target_type VARCHAR(50), -- Daily, Weekly, Monthly, Yearly
    
    -- Energy values
    baseline_kwh DECIMAL(15,2),
    target_kwh DECIMAL(15,2),
    target_reduction_percent DECIMAL(5,2),
    
    -- Date range
    start_date DATE,
    end_date DATE,
    
    -- Status
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Load profiles (peak demand tracking)
CREATE TABLE IF NOT EXISTS load_profiles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID REFERENCES sites(id),
    profile_date DATE,
    hour_of_day INT, -- 0-23
    
    -- Demand values
    peak_demand_kw DECIMAL(10,2),
    average_demand_kw DECIMAL(10,2),
    load_factor DECIMAL(5,4),
    
    created_at TIMESTAMP DEFAULT NOW(),
    
    UNIQUE(site_id, profile_date, hour_of_day)
);

-- ===== WORK ORDER MANAGEMENT TABLES =====

-- Work orders
CREATE TABLE IF NOT EXISTS work_orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    work_order_number VARCHAR(50) UNIQUE NOT NULL,
    
    -- Basic info
    title VARCHAR(200) NOT NULL,
    description TEXT,
    
    -- Classification
    priority VARCHAR(20) DEFAULT 'Medium', -- Low, Medium, High, Critical
    status VARCHAR(50) DEFAULT 'New', -- New, Assigned, InProgress, OnHold, Completed, Cancelled
    type VARCHAR(50), -- Corrective, Preventive, Predictive, Inspection, Calibration
    
    -- Assignment
    assigned_to_user_id UUID REFERENCES users(id),
    created_by_user_id UUID REFERENCES users(id),
    
    -- Equipment reference
    site_id UUID REFERENCES sites(id),
    equipment_tag_id UUID REFERENCES tags(id),
    alarm_id UUID, -- Reference to alarm that triggered this WO
    
    -- Scheduling
    created_at TIMESTAMP DEFAULT NOW(),
    scheduled_start TIMESTAMP,
    scheduled_end TIMESTAMP,
    actual_start TIMESTAMP,
    actual_end TIMESTAMP,
    
    -- Time tracking
    estimated_hours DECIMAL(8,2),
    actual_hours DECIMAL(8,2),
    
    -- Completion
    completion_notes TEXT,
    signature_data TEXT, -- Base64 encoded signature image
    completed_by_user_id UUID REFERENCES users(id),
    
    -- Costs
    estimated_cost DECIMAL(10,2),
    actual_cost DECIMAL(10,2)
);

-- Create indexes for work orders
CREATE INDEX idx_wo_status ON work_orders(status);
CREATE INDEX idx_wo_assigned ON work_orders(assigned_to_user_id);
CREATE INDEX idx_wo_equipment ON work_orders(equipment_tag_id);
CREATE INDEX idx_wo_created ON work_orders(created_at);

-- Work order tasks (checklist items)
CREATE TABLE IF NOT EXISTS work_order_tasks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    work_order_id UUID REFERENCES work_orders(id) ON DELETE CASCADE,
    
    sequence_number INT,
    task_description TEXT NOT NULL,
    
    -- Completion tracking
    is_completed BOOLEAN DEFAULT false,
    completed_by_user_id UUID REFERENCES users(id),
    completed_at TIMESTAMP,
    
    notes TEXT
);

CREATE INDEX idx_wo_tasks_order ON work_order_tasks(work_order_id, sequence_number);

-- Work order materials and parts
CREATE TABLE IF NOT EXISTS work_order_materials (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    work_order_id UUID REFERENCES work_orders(id) ON DELETE CASCADE,
    
    -- Material info
    material_name VARCHAR(200) NOT NULL,
    part_number VARCHAR(100),
    quantity DECIMAL(10,2),
    unit VARCHAR(50), -- ea, kg, m, L, etc.
    
    -- Costs
    unit_cost DECIMAL(10,2),
    total_cost DECIMAL(10,2),
    
    -- Tracking
    added_at TIMESTAMP DEFAULT NOW(),
    added_by_user_id UUID REFERENCES users(id)
);

-- Work order attachments
CREATE TABLE IF NOT EXISTS work_order_attachments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    work_order_id UUID REFERENCES work_orders(id) ON DELETE CASCADE,
    
    file_name VARCHAR(255),
    file_path TEXT,
    file_type VARCHAR(50),
    file_size_bytes BIGINT,
    
    uploaded_by_user_id UUID REFERENCES users(id),
    uploaded_at TIMESTAMP DEFAULT NOW()
);

-- ===== SCHEDULED REPORTS TABLES =====

-- Scheduled report definitions
CREATE TABLE IF NOT EXISTS scheduled_reports (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- Report definition
    name VARCHAR(200) NOT NULL,
    description TEXT,
    report_type VARCHAR(50), -- Production, Energy, Alarms, WorkOrders, Custom
    
    -- Schedule (cron format)
    schedule_cron VARCHAR(100), -- e.g., "0 6 * * *" = 6 AM daily
    time_zone VARCHAR(50) DEFAULT 'UTC',
    
    -- Recipients
    recipients TEXT, -- Comma-separated email addresses
    cc_recipients TEXT,
    
    -- Format and options
    output_format VARCHAR(20) DEFAULT 'PDF', -- PDF, Excel, CSV
    parameters JSONB, -- Report-specific parameters (date ranges, filters, etc.)
    
    -- Status
    is_enabled BOOLEAN DEFAULT true,
    last_run TIMESTAMP,
    next_run TIMESTAMP,
    
    -- Metadata
    created_by_user_id UUID REFERENCES users(id),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Report execution history
CREATE TABLE IF NOT EXISTS report_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    scheduled_report_id UUID REFERENCES scheduled_reports(id),
    
    -- Execution info
    executed_at TIMESTAMP DEFAULT NOW(),
    execution_duration_ms INT,
    
    -- Status
    status VARCHAR(50), -- Success, Failed, Partial
    error_message TEXT,
    
    -- Output
    file_path TEXT,
    file_size_bytes BIGINT,
    
    -- Recipients
    recipients_sent TEXT, -- Actual emails sent to
    email_sent BOOLEAN DEFAULT false
);

CREATE INDEX idx_report_history_scheduled ON report_history(scheduled_report_id);
CREATE INDEX idx_report_history_executed ON report_history(executed_at);

-- ===== SEED DATA =====

-- Insert sample energy targets
INSERT INTO energy_targets (site_id, name, target_type, baseline_kwh, target_kwh, target_reduction_percent, start_date, end_date) VALUES
    ((SELECT id FROM sites WHERE name = 'FACTORY_01'), '2025 Annual Energy Reduction', 'Yearly', 5000000, 4500000, 10, '2025-01-01', '2025-12-31'),
    ((SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Q1 Energy Optimization', 'Monthly', 400000, 360000, 10, '2025-01-01', '2025-03-31');

-- Insert sample scheduled reports
INSERT INTO scheduled_reports (name, description, report_type, schedule_cron, recipients, output_format, is_enabled) VALUES
    ('Daily Production Summary', 'Daily production metrics and efficiency report', 'Production', '0 6 * * *', 'manager@company.com,supervisor@company.com', 'PDF', true),
    ('Weekly Energy Report', 'Weekly energy consumption and cost analysis', 'Energy', '0 7 * * 1', 'energy-team@company.com', 'Excel', true),
    ('Daily Alarm Summary', 'Summary of all alarms from previous day', 'Alarms', '0 8 * * *', 'operations@company.com', 'PDF', true);

COMMIT;

-- ================================================
-- VERIFICATION
-- ================================================

-- Check table creation
SELECT 'energy_consumption' as table_name, COUNT(*) as row_count FROM energy_consumption
UNION ALL
SELECT 'energy_targets', COUNT(*) FROM energy_targets
UNION ALL
SELECT 'work_orders', COUNT(*) FROM work_orders
UNION ALL
SELECT 'scheduled_reports', COUNT(*) FROM scheduled_reports;

-- Show sample data
SELECT * FROM energy_targets;
SELECT * FROM scheduled_reports;
