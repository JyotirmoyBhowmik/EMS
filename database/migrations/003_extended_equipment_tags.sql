-- ================================================
-- SCADA System - Extended Industrial Equipment Seed Data
-- ================================================
-- Comprehensive tag library for major industrial equipment
-- Including: Siemens PLCs, Schneider Energy Meters, Compressors, HVAC, Flow Meters, Manufacturing

BEGIN;

-- ===== Insert Extended Sites =====
INSERT INTO sites (id, name, location, description, timezone, is_active, created_at) VALUES
    (gen_random_uuid(), 'FACTORY_01', 'Manufacturing Plant A', 'Main production facility with Siemens automation', 'UTC', true, NOW()),
    (gen_random_uuid(), 'UTILITY_01', 'Utility Building', 'Schneider energy monitoring and HVAC systems', 'UTC', true, NOW()),
    (gen_random_uuid(), 'PROCESS_01', 'Process Area', 'Compressors, pumps, and flow measurement', 'UTC', true, NOW())
ON CONFLICT (name) DO NOTHING;

-- ===== SIEMENS PLC TAGS =====

-- Siemens S7-1500 PLC Tags
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    -- CPU Status
    (gen_random_uuid(), 'SIEMENS.S7_1500.PLC01.CPU.Status', 'CPU operational status', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Siemens_S7-1500', true, NOW()),
    (gen_random_uuid(), 'SIEMENS.S7_1500.PLC01.CPU.ScanTime', 'PLC scan cycle time', 'Float', 'ms', 0, 100, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Siemens_S7-1500', true, NOW()),
    (gen_random_uuid(), 'SIEMENS.S7_1500.PLC01.CPU.Load', 'CPU load percentage', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Siemens_S7-1500', true, NOW()),
    (gen_random_uuid(), 'SIEMENS.S7_1500.PLC01.CPU.Memory.Used', 'Memory usage', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Siemens_S7-1500', true, NOW()),
    
    -- Digital Inputs/Outputs
    (gen_random_uuid(), 'SIEMENS.S7_1500.PLC01.DI.EmergencyStop', 'Emergency stop button', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Siemens_S7-1500', true, NOW()),
    (gen_random_uuid(), 'SIEMENS.S7_1500.PLC01.DI.SafetyGate', 'Safety gate status', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Siemens_S7-1500', true, NOW()),
    (gen_random_uuid(), 'SIEMENS.S7_1500.PLC01.DO.MotorStarter', 'Main motor starter output', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Siemens_S7-1500', true, NOW()),
    
  -- Analog Inputs
    (gen_random_uuid(), 'SIEMENS.S7_1500.PLC01.AI.Temperature_01', 'Process temperature sensor 1', 'Float', '°C', -50, 200, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Siemens_S7-1500', true, NOW()),
    (gen_random_uuid(), 'SIEMENS.S7_1500.PLC01.AI.Pressure_01', 'Process pressure sensor 1', 'Float', 'bar', 0, 10, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Siemens_S7-1500', true, NOW()),
    (gen_random_uuid(), 'SIEMENS.S7_1500.PLC01.AI.Level_Tank01', 'Tank 1 level sensor', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Siemens_S7-1500', true, NOW()),

-- Siemens S7-1200 PLC Tags
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'SIEMENS.S7_1200.PLC02.Status', 'PLC operational status', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Siemens_S7-1200', true, NOW()),
    (gen_random_uuid(), 'SIEMENS.S7_1200.PLC02.Conveyor.Speed', 'Conveyor belt speed', 'Float', 'm/min', 0, 60, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Siemens_S7-1200', true, NOW()),
    (gen_random_uuid(), 'SIEMENS.S7_1200.PLC02.Conveyor.Running', 'Conveyor running status', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Siemens_S7-1200', true, NOW()),
    (gen_random_uuid(), 'SIEMENS.S7_1200.PLC02.PartCounter', 'Production part counter', 'Integer', 'pcs', 0, 999999, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Siemens_S7-1200', true, NOW()),

-- Siemens LOGO! PLC Tags
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'SIEMENS.LOGO.PLC03.LightControl', 'Facility lighting control', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Siemens_LOGO', true, NOW()),
    (gen_random_uuid(), 'SIEMENS.LOGO.PLC03.DoorLock', 'Automated door lock', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Siemens_LOGO', true, NOW());

-- ===== SCHNEIDER ELECTRIC ENERGY METERS =====

-- Schneider PowerLogic PM8000 Series
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'SCHNEIDER.PM8000.METER01.Voltage.L1', 'Phase 1 voltage', 'Float', 'V', 0, 500, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Schneider_PM8000', true, NOW()),
    (gen_random_uuid(), 'SCHNEIDER.PM8000.METER01.Voltage.L2', 'Phase 2 voltage', 'Float', 'V', 0, 500, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Schneider_PM8000', true, NOW()),
    (gen_random_uuid(), 'SCHNEIDER.PM8000.METER01.Voltage.L3', 'Phase 3 voltage', 'Float', 'V', 0, 500, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Schneider_PM8000', true, NOW()),
    (gen_random_uuid(), 'SCHNEIDER.PM8000.METER01.Current.L1', 'Phase 1 current', 'Float', 'A', 0, 5000, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Schneider_PM8000', true, NOW()),
    (gen_random_uuid(), 'SCHNEIDER.PM8000.METER01.Current.L2', 'Phase 2 current', 'Float', 'A', 0, 5000, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Schneider_PM8000', true, NOW()),
    (gen_random_uuid(), 'SCHNEIDER.PM8000.METER01.Current.L3', 'Phase 3 current', 'Float', 'A', 0, 5000, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Schneider_PM8000', true, NOW()),
    (gen_random_uuid(), 'SCHNEIDER.PM8000.METER01.Power.Active', 'Active power (kW)', 'Float', 'kW', 0, 10000, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Schneider_PM8000', true, NOW()),
    (gen_random_uuid(), 'SCHNEIDER.PM8000.METER01.Power.Reactive', 'Reactive power (kVAR)', 'Float', 'kVAR', -5000, 5000, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Schneider_PM8000', true, NOW()),
    (gen_random_uuid(), 'SCHNEIDER.PM8000.METER01.Power.Apparent', 'Apparent power (kVA)', 'Float', 'kVA', 0, 10000, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Schneider_PM8000', true, NOW()),
    (gen_random_uuid(), 'SCHNEIDER.PM8000.METER01.PowerFactor', 'Power factor', 'Float', '', -1, 1, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Schneider_PM8000', true, NOW()),
    (gen_random_uuid(), 'SCHNEIDER.PM8000.METER01.Frequency', 'System frequency', 'Float', 'Hz', 45, 65, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Schneider_PM8000', true, NOW()),
    (gen_random_uuid(), 'SCHNEIDER.PM8000.METER01.Energy.Active', 'Cumulative active energy', 'Float', 'kWh', 0, 999999999, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Schneider_PM8000', true, NOW()),
    (gen_random_uuid(), 'SCHNEIDER.PM8000.METER01.Energy.Reactive', 'Cumulative reactive energy', 'Float', 'kVARh', 0, 999999999, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Schneider_PM8000', true, NOW()),
    (gen_random_uuid(), 'SCHNEIDER.PM8000.METER01.THD.Voltage', 'Voltage total harmonic distortion', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Schneider_PM8000', true, NOW()),
    (gen_random_uuid(), 'SCHNEIDER.PM8000.METER01.THD.Current', 'Current total harmonic distortion', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Schneider_PM8000', true, NOW()),

-- Schneider iEM3000 Series
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'SCHNEIDER.iEM3000.METER02.Power.Total', 'Total power consumption', 'Float', 'kW', 0, 5000, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Schneider_iEM3000', true, NOW()),
    (gen_random_uuid(), 'SCHNEIDER.iEM3000.METER02.Energy.Total', 'Total energy consumed', 'Float', 'kWh', 0, 999999999, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Schneider_iEM3000', true, NOW()),
    (gen_random_uuid(), 'SCHNEIDER.iEM3000.METER02.Demand.Peak', 'Peak demand', 'Float', 'kW', 0, 5000, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Schneider_iEM3000', true, NOW());

-- ===== COMPRESSOR TAGS =====

-- Atlas Copco Screw Compressor
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'COMPRESSOR.ATLAS_COPCO.COMP01.Status', 'Compressor running status', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'AtlasCopco_GA75', true, NOW()),
    (gen_random_uuid(), 'COMPRESSOR.ATLAS_COPCO.COMP01.Pressure.Discharge', 'Discharge pressure', 'Float', 'bar', 0, 13, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'AtlasCopco_GA75', true, NOW()),
    (gen_random_uuid(), 'COMPRESSOR.ATLAS_COPCO.COMP01.Temperature.Discharge', 'Discharge air temperature', 'Float', '°C', 0, 120, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'AtlasCopco_GA75', true, NOW()),
    (gen_random_uuid(), 'COMPRESSOR.ATLAS_COPCO.COMP01.Temperature.Oil', 'Oil temperature', 'Float', '°C', 0, 110, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'AtlasCopco_GA75', true, NOW()),
    (gen_random_uuid(), 'COMPRESSOR.ATLAS_COPCO.COMP01.Motor.Current', 'Motor current', 'Float', 'A', 0, 150, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'AtlasCopco_GA75', true, NOW()),
    (gen_random_uuid(), 'COMPRESSOR.ATLAS_COPCO.COMP01.Motor.Power', 'Motor power', 'Float', 'kW', 0, 75, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'AtlasCopco_GA75', true, NOW()),
    (gen_random_uuid(), 'COMPRESSOR.ATLAS_COPCO.COMP01.RunHours', 'Total running hours', 'Integer', 'hrs', 0, 999999, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'AtlasCopco_GA75', true, NOW()),
    (gen_random_uuid(), 'COMPRESSOR.ATLAS_COPCO.COMP01.Alarm.Active', 'Active alarm status', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'AtlasCopco_GA75', true, NOW()),
    (gen_random_uuid(), 'COMPRESSOR.ATLAS_COPCO.COMP01.Load.Percentage', 'Compressor load', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'AtlasCopco_GA75', true, NOW()),
    (gen_random_uuid(), 'COMPRESSOR.ATLAS_COPCO.COMP01.Dewpoint', 'Pressure dewpoint', 'Float', '°C', -70, 50, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'AtlasCopco_GA75', true, NOW());

-- ===== HVAC SYSTEM TAGS =====

-- Air Handling Unit (AHU)
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'HVAC.AHU01.Status', 'AHU running status', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'AHU', true, NOW()),
    (gen_random_uuid(), 'HVAC.AHU01.Fan.Supply.Speed', 'Supply fan speed', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'AHU', true, NOW()),
    (gen_random_uuid(), 'HVAC.AHU01.Fan.Return.Speed', 'Return fan speed', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'AHU', true, NOW()),
    (gen_random_uuid(), 'HVAC.AHU01.Temperature.Supply', 'Supply air temperature', 'Float', '°C', 0, 50, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'AHU', true, NOW()),
    (gen_random_uuid(), 'HVAC.AHU01.Temperature.Return', 'Return air temperature', 'Float', '°C', 0, 50, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'AHU', true, NOW()),
    (gen_random_uuid(), 'HVAC.AHU01.Temperature.Outside', 'Outside air temperature', 'Float', '°C', -30, 50, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'AHU', true, NOW()),
    (gen_random_uuid(), 'HVAC.AHU01.Humidity.Supply', 'Supply air humidity', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'AHU', true, NOW()),
    (gen_random_uuid(), 'HVAC.AHU01.Pressure.Static', 'Static pressure', 'Float', 'Pa', -500, 500, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'AHU', true, NOW()),
    (gen_random_uuid(), 'HVAC.AHU01.Filter.DifferentialPressure', 'Filter differential pressure', 'Float', 'Pa', 0, 500, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'AHU', true, NOW()),
    (gen_random_uuid(), 'HVAC.AHU01.Damper.Position', 'Mixing damper position', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'AHU', true, NOW()),
    (gen_random_uuid(), 'HVAC.AHU01.Valve.Cooling', 'Cooling valve position', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'AHU', true, NOW()),
    (gen_random_uuid(), 'HVAC.AHU01.Valve.Heating', 'Heating valve position', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'AHU', true, NOW()),

-- Chiller
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'HVAC.CHILLER01.Status', 'Chiller running status', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Chiller', true, NOW()),
    (gen_random_uuid(), 'HVAC.CHILLER01.Temperature.ChilledWater.Supply', 'Chilled water supply temp', 'Float', '°C', 0, 20, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Chiller', true, NOW()),
    (gen_random_uuid(), 'HVAC.CHILLER01.Temperature.ChilledWater.Return', 'Chilled water return temp', 'Float', '°C', 0, 25, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Chiller', true, NOW()),
    (gen_random_uuid(), 'HVAC.CHILLER01.Temperature.Condenser.Water', 'Condenser water temp', 'Float', '°C', 0, 40, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Chiller', true, NOW()),
    (gen_random_uuid(), 'HVAC.CHILLER01.Capacity.Current', 'Current cooling capacity', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Chiller', true, NOW()),
    (gen_random_uuid(), 'HVAC.CHILLER01.Power.Total', 'Total power consumption', 'Float', 'kW', 0, 500, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Chiller', true, NOW()),
    (gen_random_uuid(), 'HVAC.CHILLER01.COP', 'Coefficient of performance', 'Float', '', 0, 10, (SELECT id FROM sites WHERE name = 'UTILITY_01'), 'Chiller', true, NOW());

-- ===== FLOW METER TAGS =====

-- Electromagnetic Flow Meter (Siemens SITRANS FM)
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'FLOWMETER.SIEMENS.FM01.FlowRate', 'Volumetric flow rate', 'Float', 'm³/h', 0, 500, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'Siemens_SITRANS_FM', true, NOW()),
    (gen_random_uuid(), 'FLOWMETER.SIEMENS.FM01.FlowRate.Mass', 'Mass flow rate', 'Float', 'kg/h', 0, 5000, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'Siemens_SITRANS_FM', true, NOW()),
    (gen_random_uuid(), 'FLOWMETER.SIEMENS.FM01.Totalizer', 'Total volume accumulated', 'Float', 'm³', 0, 999999999, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'Siemens_SITRANS_FM', true, NOW()),
    (gen_random_uuid(), 'FLOWMETER.SIEMENS.FM01.Velocity', 'Flow velocity', 'Float', 'm/s', 0, 10, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'Siemens_SITRANS_FM', true, NOW()),
    (gen_random_uuid(), 'FLOWMETER.SIEMENS.FM01.Conductivity', 'Fluid conductivity', 'Float', 'μS/cm', 0, 10000, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'Siemens_SITRANS_FM', true, NOW()),

-- Coriolis Mass Flow Meter
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'FLOWMETER.CORIOLIS.FM02.MassFlow', 'Mass flow rate', 'Float', 'kg/h', 0, 10000, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'Coriolis', true, NOW()),
    (gen_random_uuid(), 'FLOWMETER.CORIOLIS.FM02.Density', 'Fluid density', 'Float', 'kg/m³', 0, 2000, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'Coriolis', true, NOW()),
    (gen_random_uuid(), 'FLOWMETER.CORIOLIS.FM02.Temperature', 'Process temperature', 'Float', '°C', -50, 200, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'Coriolis', true, NOW()),
    (gen_random_uuid(), 'FLOWMETER.CORIOLIS.FM02.Totalizer.Mass', 'Total mass accumulated', 'Float', 'kg', 0, 999999999, (SELECT id FROM sites WHERE name = 'PROCESS_01'), 'Coriolis', true, NOW());

-- ===== MANUFACTURING MACHINES =====

-- CNC Machine
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'MANUFACTURING.CNC01.Status', 'Machine operational status', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'CNC_Mill', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.CNC01.Mode', 'Operation mode (0=Off, 1=Manual, 2=Auto)', 'Integer', '', 0, 2, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'CNC_Mill', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.CNC01.Spindle.Speed', 'Spindle RPM', 'Float', 'RPM', 0, 12000, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'CNC_Mill', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.CNC01.Spindle.Load', 'Spindle load percentage', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'CNC_Mill', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.CNC01.Feed.Rate', 'Feed rate', 'Float', 'mm/min', 0, 5000, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'CNC_Mill', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.CNC01.Position.X', 'X-axis position', 'Float', 'mm', -500, 500, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'CNC_Mill', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.CNC01.Position.Y', 'Y-axis position', 'Float', 'mm', -400, 400, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'CNC_Mill', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.CNC01.Position.Z', 'Z-axis position', 'Float', 'mm', -300, 300, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'CNC_Mill', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.CNC01.PartCounter', 'Parts completed', 'Integer', 'pcs', 0, 999999, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'CNC_Mill', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.CNC01.CycleTime', 'Current cycle time', 'Float', 'sec', 0, 3600, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'CNC_Mill', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.CNC01.Tool.Number', 'Active tool number', 'Integer', '', 1, 60, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'CNC_Mill', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.CNC01.Coolant.Level', 'Coolant tank level', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'CNC_Mill', true, NOW()),

-- Injection Molding Machine
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'MANUFACTURING.INJECTION01.Status', 'Machine status', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Injection_Molding', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.INJECTION01.Temperature.Barrel.Zone1', 'Barrel zone 1 temperature', 'Float', '°C', 0, 350, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Injection_Molding', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.INJECTION01.Temperature.Barrel.Zone2', 'Barrel zone 2 temperature', 'Float', '°C', 0, 350, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Injection_Molding', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.INJECTION01.Temperature.Barrel.Zone3', 'Barrel zone 3 temperature', 'Float', '°C', 0, 350, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Injection_Molding', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.INJECTION01.Temperature.Mold', 'Mold temperature', 'Float', '°C', 0, 200, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Injection_Molding', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.INJECTION01.Pressure.Injection', 'Injection pressure', 'Float', 'bar', 0, 2000, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Injection_Molding', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.INJECTION01.Pressure.Holding', 'Holding pressure', 'Float', 'bar', 0, 1500, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Injection_Molding', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.INJECTION01.Cycle.Time', 'Cycle time', 'Float', 'sec', 0, 300, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Injection_Molding', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.INJECTION01.ShotCounter', 'Total shots', 'Integer', '', 0, 999999999, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Injection_Molding', true, NOW()),

-- Industrial Robot (ABB)
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'MANUFACTURING.ROBOT01.Status', 'Robot operational status', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'ABB_Robot', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.ROBOT01.Mode', 'Operating mode', 'Integer', '', 0, 3, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'ABB_Robot', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.ROBOT01.Speed', 'Robot speed override', 'Float', '%', 0, 100, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'ABB_Robot', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.ROBOT01.CycleCount', 'Cycle counter', 'Integer', '', 0, 999999999, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'ABB_Robot', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.ROBOT01.Motor.Temperature', 'Motor temperature', 'Float', '°C', 0, 100, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'ABB_Robot', true, NOW()),

-- Packaging Machine
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'MANUFACTURING.PACKAGING01.Status', 'Machine running', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Packaging_Machine', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.PACKAGING01.Speed', 'Packaging speed', 'Float', 'pcs/min', 0, 200, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Packaging_Machine', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.PACKAGING01.ProductCounter', 'Products packaged', 'Integer', 'pcs', 0, 999999999, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Packaging_Machine', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.PACKAGING01.RejectCounter', 'Rejected products', 'Integer', 'pcs', 0, 999999, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Packaging_Machine', true, NOW()),

-- Welding Station
INSERT INTO tags (id, name, description, data_type, unit, min_value, max_value, site_id, device_type, is_enabled, created_at) VALUES
    (gen_random_uuid(), 'MANUFACTURING.WELDING01.Status', 'Welding active', 'Boolean', '', 0, 1, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Welding_Station', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.WELDING01.Current', 'Welding current', 'Float', 'A', 0, 500, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Welding_Station', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.WELDING01.Voltage', 'Welding voltage', 'Float', 'V', 0, 50, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Welding_Station', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.WELDING01.WireSpeed', 'Wire feed speed', 'Float', 'm/min', 0, 20, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Welding_Station', true, NOW()),
    (gen_random_uuid(), 'MANUFACTURING.WELDING01.WeldCounter', 'Welds completed', 'Integer', '', 0, 999999, (SELECT id FROM sites WHERE name = 'FACTORY_01'), 'Welding_Station', true, NOW());

COMMIT;

-- ================================================
-- VERIFICATION
-- ================================================
-- Total tags by device type:
SELECT device_type, COUNT(*) as tag_count 
FROM tags 
GROUP BY device_type 
ORDER BY tag_count DESC;

-- Total tags: Should be 100+ tags
SELECT COUNT(*) as total_tags FROM tags;
