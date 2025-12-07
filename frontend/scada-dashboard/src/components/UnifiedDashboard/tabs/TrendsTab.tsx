import React, { useState } from 'react';
import './TrendsTab.css';

interface TrendsTabProps {
    filters: any;
    machineData: any[];
    loading: boolean;
}

export const TrendsTab: React.FC<TrendsTabProps> = ({ machineData }) => {
    const [selectedMachines, setSelectedMachines] = useState<string[]>([]);
    const [selectedTag, setSelectedTag] = useState('temperature');
    const [timeRange, setTimeRange] = useState('24h');

    const handleMachineToggle = (machineId: string) => {
        if (selectedMachines.includes(machineId)) {
            setSelectedMachines(selectedMachines.filter((id) => id !== machineId));
        } else {
            if (selectedMachines.length < 10) {
                setSelectedMachines([...selectedMachines, machineId]);
            } else {
                alert('Maximum 10 machines can be compared');
            }
        }
    };

    return (
        <div className="trends-tab">
            {/* Selection Controls */}
            <div className="trends-controls">
                <div className="machine-selector">
                    <label>
                        <strong>Select Machines to Compare</strong> (max 10):
                    </label>
                    <div className="machine-chips">
                        {machineData.slice(0, 20).map((machine) => (
                            <button
                                key={machine.id}
                                className={`machine-chip ${selectedMachines.includes(machine.id) ? 'selected' : ''
                                    }`}
                                onClick={() => handleMachineToggle(machine.id)}
                            >
                                {machine.name}
                                {selectedMachines.includes(machine.id) && ' ✓'}
                            </button>
                        ))}
                    </div>
                    {selectedMachines.length > 0 && (
                        <button className="clear-selection" onClick={() => setSelectedMachines([])}>
                            Clear Selection
                        </button>
                    )}
                </div>

                <div className="trend-filters">
                    <div className="filter-group">
                        <label>Tag to Plot:</label>
                        <select value={selectedTag} onChange={(e) => setSelectedTag(e.target.value)}>
                            <option value="temperature">Temperature</option>
                            <option value="speed">Speed</option>
                            <option value="power">Power</option>
                            <option value="pressure">Pressure</option>
                            <option value="vibration">Vibration</option>
                            <option value="efficiency">Efficiency</option>
                        </select>
                    </div>

                    <div className="filter-group">
                        <label>Time Range:</label>
                        <select value={timeRange} onChange={(e) => setTimeRange(e.target.value)}>
                            <option value="1h">Last Hour</option>
                            <option value="6h">Last 6 Hours</option>
                            <option value="24h">Last 24 Hours</option>
                            <option value="7d">Last 7 Days</option>
                            <option value="30d">Last 30 Days</option>
                            <option value="custom">Custom Range</option>
                        </select>
                    </div>
                </div>
            </div>

            {/* Trend Chart */}
            {selectedMachines.length > 0 ? (
                <div className="trend-chart-container">
                    <div className="chart-header">
                        <h3>
                            {selectedTag.charAt(0).toUpperCase() + selectedTag.slice(1)} Trend - {timeRange}
                        </h3>
                        <div className="chart-actions">
                            <button className="chart-btn">📊 Export PNG</button>
                            <button className="chart-btn">📥 Export CSV</button>
                            <button className="chart-btn">⛶ Fullscreen</button>
                        </div>
                    </div>

                    <div className="chart-placeholder">
                        <svg viewBox="0 0 800 400" className="trend-svg">
                            {/* Y-axis */}
                            <line x1="50" y1="20" x2="50" y2="360" stroke="#94a3b8" strokeWidth="2" />
                            {/* X-axis */}
                            <line x1="50" y1="360" x2="780" y2="360" stroke="#94a3b8" strokeWidth="2" />

                            {/* Grid lines */}
                            {[0, 1, 2, 3, 4].map((i) => (
                                <g key={i}>
                                    <line
                                        x1="50"
                                        y1={20 + i * 85}
                                        x2="780"
                                        y2={20 + i * 85}
                                        stroke="#e2e8f0"
                                        strokeWidth="1"
                                        strokeDasharray="5,5"
                                    />
                                    <text x="20" y={25 + i * 85} fill="#64748b" fontSize="12">
                                        {100 - i * 25}
                                    </text>
                                </g>
                            ))}

                            {/* Sample trend lines */}
                            {selectedMachines.slice(0, 5).map((_, idx) => {
                                const colors = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6'];
                                const points = Array.from({ length: 20 }, (_, i) => ({
                                    x: 50 + i * 36,
                                    y: 180 + Math.sin(i * 0.5 + idx) * 80 + Math.random() * 40,
                                }));
                                const pathD = points.map((p, i) => `${i === 0 ? 'M' : 'L'} ${p.x} ${p.y}`).join(' ');
                                return (
                                    <path
                                        key={idx}
                                        d={pathD}
                                        fill="none"
                                        stroke={colors[idx]}
                                        strokeWidth="3"
                                    />
                                );
                            })}

                            {/* Time labels */}
                            {['0h', '6h', '12h', '18h', '24h'].map((label, i) => (
                                <text key={i} x={45 + i * 180} y="380" fill="#64748b" fontSize="12">
                                    {label}
                                </text>
                            ))}
                        </svg>

                        <div className="chart-legend">
                            {selectedMachines.slice(0, 10).map((machineId, idx) => {
                                const colors = [
                                    '#3b82f6',
                                    '#10b981',
                                    '#f59e0b',
                                    '#ef4444',
                                    '#8b5cf6',
                                    '#ec4899',
                                    '#06b6d4',
                                    '#84cc16',
                                    '#f97316',
                                    '#6366f1',
                                ];
                                const machine = machineData.find((m) => m.id === machineId);
                                return (
                                    <div key={machineId} className="legend-item">
                                        <span
                                            className="legend-color"
                                            style={{ backgroundColor: colors[idx] }}
                                        ></span>
                                        <span className="legend-label">{machine?.name}</span>
                                    </div>
                                );
                            })}
                        </div>
                    </div>

                    {/* Statistics */}
                    <div className="trend-stats">
                        <div className="stat-card">
                            <div className="stat-label">Average</div>
                            <div className="stat-value">82.5°C</div>
                        </div>
                        <div className="stat-card">
                            <div className="stat-label">Minimum</div>
                            <div className="stat-value">65.2°C</div>
                        </div>
                        <div className="stat-card">
                            <div className="stat-label">Maximum</div>
                            <div className="stat-value">98.7°C</div>
                        </div>
                        <div className="stat-card">
                            <div className="stat-label">Std Dev</div>
                            <div className="stat-value">±8.3°C</div>
                        </div>
                    </div>
                </div>
            ) : (
                <div className="empty-state-trends">
                    <p>📊 Select machines to view trends</p>
                    <p>Choose up to 10 machines from the list above to compare their {selectedTag} trends</p>
                </div>
            )}
        </div>
    );
};
