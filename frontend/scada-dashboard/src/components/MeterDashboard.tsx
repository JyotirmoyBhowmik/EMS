import React, { useState, useEffect } from 'react';
import { ResponsiveSankey } from '@nivo/sankey';
import { ResponsivePie } from '@nivo/pie';
import { ResponsiveLine } from '@nivo/line';
import './MeterDashboard.css';

interface Meter {
    id: string;
    meterName: string;
    meterType: string;
    status: string;
    healthStatus: string;
    communicationStatus: string;
    parentMeterId: string | null;
    level: number;
}

interface MeterReading {
    timestamp: string;
    activePowerKw: number;
    totalEnergyKwh: number;
}

const MeterDashboard: React.FC = () => {
    const [meters, setMeters] = useState<Meter[]>([]);
    const [selectedMeterId, setSelectedMeterId] = useState<string | null>(null);
    const [powerFlowData, setPowerFlowData] = useState<any>(null);
    const [consumptionPattern, setConsumptionPattern] = useState<any[]>([]);

    useEffect(() => {
        // Load meters
        fetch('http://localhost:5010/api/Energy/meters')
            .then(res => res.json())
            .then(data => {
                setMeters(data);
                generatePowerFlowData(data);
            });

        // Load consumption pattern
        loadConsumptionPattern();
    }, []);

    // Generate Sankey diagram data for power flow
    const generatePowerFlowData = (meters: Meter[]) => {
        const nodes: any[] = [];
        const links: any[] = [];

        // Create nodes for each meter
        meters.forEach(meter => {
            nodes.push({
                id: meter.id,
                label: meter.meterName
            });
        });

        // Create links based on parent-child relationships
        meters.forEach(meter => {
            if (meter.parentMeterId) {
                links.push({
                    source: meter.parentMeterId,
                    target: meter.id,
                    value: Math.random() * 1000 + 100 // Replace with actual power flow
                });
            }
        });

        setPowerFlowData({ nodes, links });
    };

    // Load consumption pattern data
    const loadConsumptionPattern = async () => {
        try {
            const response = await fetch('http://localhost:5010/api/Energy/consumption/pattern?days=7');
            const data = await response.json();

            const formattedData = [{
                id: 'consumption',
                data: data.map((item: any) => ({
                    x: item.date,
                    y: item.totalKwh
                }))
            }];

            setConsumptionPattern(formattedData);
        } catch (err) {
            console.error('Error loading consumption pattern:', err);
        }
    };

    // Calculate meter statistics
    const stats = {
        total: meters.length,
        active: meters.filter(m => m.status === 'active').length,
        inactive: meters.filter(m => m.status === 'inactive').length,
        maintenance: meters.filter(m => m.status === 'maintenance').length,
        faulty: meters.filter(m => m.status === 'faulty').length,
        online: meters.filter(m => m.communicationStatus === 'online').length,
        offline: meters.filter(m => m.communicationStatus === 'offline').length,
        healthy: meters.filter(m => m.healthStatus === 'good').length,
        warning: meters.filter(m => m.healthStatus === 'warning').length,
        critical: meters.filter(m => m.healthStatus === 'critical').length
    };

    // Pie chart data for status distribution
    const statusPieData = [
        { id: 'Active', label: 'Active', value: stats.active, color: '#4CAF50' },
        { id: 'Inactive', label: 'Inactive', value: stats.inactive, color: '#F44336' },
        { id: 'Maintenance', label: 'Maintenance', value: stats.maintenance, color: '#FFC107' },
        { id: 'Faulty', label: 'Faulty', value: stats.faulty, color: '#FF5722' }
    ].filter(item => item.value > 0);

    // Pie chart data for health distribution
    const healthPieData = [
        { id: 'Healthy', label: 'Healthy', value: stats.healthy, color: '#4CAF50' },
        { id: 'Warning', label: 'Warning', value: stats.warning, color: '#FFC107' },
        { id: 'Critical', label: 'Critical', value: stats.critical, color: '#FF5722' }
    ].filter(item => item.value > 0);

    return (
        <div className="meter-dashboard">
            <h1>🗺️ Energy Meter Distribution Dashboard</h1>

            {/* Summary Cards */}
            <div className="dashboard-stats">
                <div className="stat-card">
                    <div className="stat-icon">📊</div>
                    <div className="stat-content">
                        <div className="stat-value">{stats.total}</div>
                        <div className="stat-label">Total Meters</div>
                    </div>
                </div>

                <div className="stat-card green">
                    <div className="stat-icon">✓</div>
                    <div className="stat-content">
                        <div className="stat-value">{stats.active}</div>
                        <div className="stat-label">Active</div>
                    </div>
                </div>

                <div className="stat-card blue">
                    <div className="stat-icon">🌐</div>
                    <div className="stat-content">
                        <div className="stat-value">{stats.online}</div>
                        <div className="stat-label">Online</div>
                    </div>
                </div>

                <div className="stat-card yellow">
                    <div className="stat-icon">⚠</div>
                    <div className="stat-content">
                        <div className="stat-value">{stats.warning}</div>
                        <div className="stat-label">Warnings</div>
                    </div>
                </div>

                <div className="stat-card red">
                    <div className="stat-icon">✗</div>
                    <div className="stat-content">
                        <div className="stat-value">{stats.critical}</div>
                        <div className="stat-label">Critical</div>
                    </div>
                </div>
            </div>

            {/* Main Dashboard Grid */}
            <div className="dashboard-grid">
                {/* Power Flow Sankey Diagram */}
                <div className="dashboard-card full-width">
                    <h2>⚡ Power Flow Distribution</h2>
                    {powerFlowData && (
                        <div style={{ height: '400px' }}>
                            <ResponsiveSankey
                                data={powerFlowData}
                                margin={{ top: 40, right: 160, bottom: 40, left: 50 }}
                                align="justify"
                                colors={{ scheme: 'category10' }}
                                nodeOpacity={1}
                                nodeThickness={18}
                                nodeInnerPadding={3}
                                nodeBorderWidth={0}
                                nodeBorderColor={{ from: 'color', modifiers: [['darker', 0.8]] }}
                                linkOpacity={0.5}
                                linkHoverOthersOpacity={0.1}
                                enableLinkGradient={true}
                                label={(node: any) => `${node.label}`}
                                labelOrientation="horizontal"
                                labelPadding={16}
                                labelTextColor={{ from: 'color', modifiers: [['darker', 1]] }}
                            />
                        </div>
                    )}
                </div>

                {/* Status Distribution Pie */}
                <div className="dashboard-card">
                    <h2>📊 Status Distribution</h2>
                    <div style={{ height: '300px' }}>
                        <ResponsivePie
                            data={statusPieData}
                            margin={{ top: 40, right: 80, bottom: 80, left: 80 }}
                            innerRadius={0.5}
                            padAngle={0.7}
                            cornerRadius={3}
                            activeOuterRadiusOffset={8}
                            colors={{ datum: 'data.color' }}
                            borderWidth={1}
                            borderColor={{ from: 'color', modifiers: [['darker', 0.2]] }}
                            arcLinkLabelsSkipAngle={10}
                            arcLinkLabelsTextColor="#333333"
                            arcLinkLabelsThickness={2}
                            arcLabelsSkipAngle={10}
                            arcLabelsTextColor={{ from: 'color', modifiers: [['darker', 2]] }}
                            legends={[
                                {
                                    anchor: 'bottom',
                                    direction: 'row',
                                    justify: false,
                                    translateX: 0,
                                    translateY: 56,
                                    itemsSpacing: 0,
                                    itemWidth: 100,
                                    itemHeight: 18,
                                    itemTextColor: '#999',
                                    itemDirection: 'left-to-right',
                                    itemOpacity: 1,
                                    symbolSize: 18,
                                    symbolShape: 'circle'
                                }
                            ]}
                        />
                    </div>
                </div>

                {/* Health Distribution Pie */}
                <div className="dashboard-card">
                    <h2>🏥 Health Status</h2>
                    <div style={{ height: '300px' }}>
                        <ResponsivePie
                            data={healthPieData}
                            margin={{ top: 40, right: 80, bottom: 80, left: 80 }}
                            innerRadius={0.5}
                            padAngle={0.7}
                            cornerRadius={3}
                            activeOuterRadiusOffset={8}
                            colors={{ datum: 'data.color' }}
                            borderWidth={1}
                            borderColor={{ from: 'color', modifiers: [['darker', 0.2]] }}
                            arcLinkLabelsSkipAngle={10}
                            arcLinkLabelsTextColor="#333333"
                            arcLinkLabelsThickness={2}
                            arcLabelsSkipAngle={10}
                            arcLabelsTextColor={{ from: 'color', modifiers: [['darker', 2]] }}
                            legends={[
                                {
                                    anchor: 'bottom',
                                    direction: 'row',
                                    justify: false,
                                    translateX: 0,
                                    translateY: 56,
                                    itemsSpacing: 0,
                                    itemWidth: 100,
                                    itemHeight: 18,
                                    itemTextColor: '#999',
                                    itemDirection: 'left-to-right',
                                    itemOpacity: 1,
                                    symbolSize: 18,
                                    symbolShape: 'circle'
                                }
                            ]}
                        />
                    </div>
                </div>

                {/* Consumption Pattern Line Chart */}
                <div className="dashboard-card full-width">
                    <h2>📈 Power Consumption Pattern (Last 7 Days)</h2>
                    <div style={{ height: '300px' }}>
                        {consumptionPattern.length > 0 && (
                            <ResponsiveLine
                                data={consumptionPattern}
                                margin={{ top: 50, right: 110, bottom: 50, left: 60 }}
                                xScale={{ type: 'point' }}
                                yScale={{ type: 'linear', min: 'auto', max: 'auto', stacked: false, reverse: false }}
                                curve="monotoneX"
                                axisTop={null}
                                axisRight={null}
                                axisBottom={{
                                    tickSize: 5,
                                    tickPadding: 5,
                                    tickRotation: 0,
                                    legend: 'Date',
                                    legendOffset: 36,
                                    legendPosition: 'middle'
                                }}
                                axisLeft={{
                                    tickSize: 5,
                                    tickPadding: 5,
                                    tickRotation: 0,
                                    legend: 'Energy (kWh)',
                                    legendOffset: -40,
                                    legendPosition: 'middle'
                                }}
                                pointSize={10}
                                pointColor={{ theme: 'background' }}
                                pointBorderWidth={2}
                                pointBorderColor={{ from: 'serieColor' }}
                                pointLabelYOffset={-12}
                                useMesh={true}
                                legends={[
                                    {
                                        anchor: 'bottom-right',
                                        direction: 'column',
                                        justify: false,
                                        translateX: 100,
                                        translateY: 0,
                                        itemsSpacing: 0,
                                        itemDirection: 'left-to-right',
                                        itemWidth: 80,
                                        itemHeight: 20,
                                        itemOpacity: 0.75,
                                        symbolSize: 12,
                                        symbolShape: 'circle',
                                        symbolBorderColor: 'rgba(0, 0, 0, .5)'
                                    }
                                ]}
                            />
                        )}
                    </div>
                </div>

                {/* Meter Map */}
                <div className="dashboard-card full-width">
                    <h2>🗺️ Meter Distribution Map</h2>
                    <div className="meter-map">
                        {renderMeterMap(meters, null, 0)}
                    </div>
                </div>
            </div>
        </div>
    );

    // Render hierarchical meter map
    function renderMeterMap(meters: Meter[], parentId: string | null, level: number) {
        const children = meters.filter(m => m.parentMeterId === parentId);

        if (children.length === 0) return null;

        return (
            <div className={`map-level level-${level}`}>
                {children.map(meter => {
                    const statusColor =
                        meter.status === 'active' ? '#4CAF50' :
                            meter.status === 'inactive' ? '#F44336' :
                                meter.status === 'maintenance' ? '#FFC107' : '#FF5722';

                    return (
                        <div key={meter.id} className="meter-node-container">
                            <div
                                className="meter-node"
                                style={{ borderColor: statusColor }}
                                title={`${meter.meterName}\nStatus: ${meter.status}\nHealth: ${meter.healthStatus}`}
                            >
                                <div className="meter-node-header">
                                    <span
                                        className="status-dot"
                                        style={{ backgroundColor: statusColor }}
                                    />
                                    <span className="meter-node-name">{meter.meterName}</span>
                                </div>
                                <div className="meter-node-type">{meter.meterType}</div>
                                <div className="meter-node-status">
                                    {meter.communicationStatus === 'online' ? '🟢' : '🔴'}
                                    {meter.communicationStatus}
                                </div>
                            </div>
                            {renderMeterMap(meters, meter.id, level + 1)}
                        </div>
                    );
                })}
            </div>
        );
    }
};

export default MeterDashboard;
