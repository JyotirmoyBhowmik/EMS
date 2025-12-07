import React, { useState } from 'react';
import './DashboardTab.css';

interface DashboardTabProps {
    filters: any;
    machineData: any[];
    loading: boolean;
}

export const DashboardTab: React.FC<DashboardTabProps> = ({ machineData }) => {
    const [viewMode, setViewMode] = useState<'grid' | 'list' | 'diagram'>('grid');
    const [sortBy, setSortBy] = useState<'status' | 'name' | 'alarms'>('status');

    const sortedMachines = [...machineData].sort((a, b) => {
        if (sortBy === 'status') {
            const statusOrder = { stopped: 0, warning: 1, running: 2 };
            return statusOrder[a.status] - statusOrder[b.status];
        } else if (sortBy === 'name') {
            return a.name.localeCompare(b.name);
        } else {
            return (b.activeAlarms || 0) - (a.activeAlarms || 0);
        }
    });

    const getStatusIcon = (status: string) => {
        switch (status) {
            case 'running':
                return '✅';
            case 'warning':
                return '⚠️';
            case 'stopped':
                return '❌';
            default:
                return '⚪';
        }
    };

    const getStatusClass = (status: string) => {
        switch (status) {
            case 'running':
                return 'status-running';
            case 'warning':
                return 'status-warning';
            case 'stopped':
                return 'status-stopped';
            default:
                return '';
        }
    };

    return (
        <div className="dashboard-tab">
            {/* View Controls */}
            <div className="dashboard-controls">
                <div className="view-mode">
                    <button
                        className={`view-btn ${viewMode === 'grid' ? 'active' : ''}`}
                        onClick={() => setViewMode('grid')}
                    >
                        ⊞ Grid
                    </button>
                    <button
                        className={`view-btn ${viewMode === 'list' ? 'active' : ''}`}
                        onClick={() => setViewMode('list')}
                    >
                        ☰ List
                    </button>
                    <button
                        className={`view-btn ${viewMode === 'diagram' ? 'active' : ''}`}
                        onClick={() => setViewMode('diagram')}
                    >
                        📐 Diagram
                    </button>
                </div>

                <div className="sort-control">
                    <label>Sort by:</label>
                    <select value={sortBy} onChange={(e) => setSortBy(e.target.value as any)}>
                        <option value="status">Status</option>
                        <option value="name">Name</option>
                        <option value="alarms">Alarms</option>
                    </select>
                </div>
            </div>

            {/* Machine Display */}
            {viewMode === 'grid' && (
                <div className="machine-grid">
                    {sortedMachines.map((machine) => (
                        <div key={machine.id} className={`machine-card ${getStatusClass(machine.status)}`}>
                            <div className="machine-header">
                                <span className="machine-icon">{getStatusIcon(machine.status)}</span>
                                <h3 className="machine-name">{machine.name}</h3>
                                {machine.activeAlarms > 0 && (
                                    <span className="alarm-badge">{machine.activeAlarms}</span>
                                )}
                            </div>

                            <div className="machine-metrics">
                                <div className="metric">
                                    <span className="metric-label">Temperature</span>
                                    <span className="metric-value">{machine.temperature}°C</span>
                                </div>
                                <div className="metric">
                                    <span className="metric-label">Speed</span>
                                    <span className="metric-value">{machine.speed} RPM</span>
                                </div>
                                <div className="metric">
                                    <span className="metric-label">Power</span>
                                    <span className="metric-value">{machine.power} kW</span>
                                </div>
                            </div>

                            <div className="machine-footer">
                                <span className="machine-location">{machine.location}</span>
                                <span className="machine-uptime">Uptime: {machine.uptime}h</span>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {viewMode === 'list' && (
                <div className="machine-list">
                    <table>
                        <thead>
                            <tr>
                                <th>Status</th>
                                <th>Machine</th>
                                <th>Location</th>
                                <th>Temperature</th>
                                <th>Speed</th>
                                <th>Power</th>
                                <th>Alarms</th>
                                <th>Uptime</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {sortedMachines.map((machine) => (
                                <tr key={machine.id} className={getStatusClass(machine.status)}>
                                    <td>
                                        <span className="status-badge">{getStatusIcon(machine.status)}</span>
                                    </td>
                                    <td className="machine-name-cell">{machine.name}</td>
                                    <td>{machine.location}</td>
                                    <td>{machine.temperature}°C</td>
                                    <td>{machine.speed} RPM</td>
                                    <td>{machine.power} kW</td>
                                    <td>
                                        {machine.activeAlarms > 0 ? (
                                            <span className="alarm-count">{machine.activeAlarms}</span>
                                        ) : (
                                            '-'
                                        )}
                                    </td>
                                    <td>{machine.uptime}h</td>
                                    <td>
                                        <button className="action-btn">Details</button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {viewMode === 'diagram' && (
                <div className="diagram-view">
                    <div className="diagram-placeholder">
                        <p>🏭 Site Layout Diagram</p>
                        <p className="diagram-note">
                            Interactive 2D/3D site layout showing machine positions
                        </p>
                        <p className="diagram-note">(Requires site layout configuration)</p>
                    </div>
                </div>
            )}

            {machineData.length === 0 && (
                <div className="empty-state">
                    <p>🔍 No machines found matching your filters</p>
                    <p>Try adjusting your filter criteria or selecting a different preset</p>
                </div>
            )}
        </div>
    );
};
