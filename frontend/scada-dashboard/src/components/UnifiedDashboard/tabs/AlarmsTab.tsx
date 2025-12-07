import React, { useState, useEffect } from 'react';
import './AlarmsTab.css';

interface AlarmsTabProps {
    filters: any;
    machineData: any[];
    loading: boolean;
}

export const AlarmsTab: React.FC<AlarmsTabProps> = ({ machineData }) => {
    const [alarms, setAlarms] = useState<any[]>([]);
    const [selectedPriorities, setSelectedPriorities] = useState({
        critical: true,
        high: true,
        medium: false,
        low: false,
    });
    const [autoRefresh, setAutoRefresh] = useState(true);
    const [selectedAlarms, setSelectedAlarms] = useState<Set<string>>(new Set());

    useEffect(() => {
        // Fetch alarms for selected machines
        const allAlarms = machineData.flatMap((machine) =>
            (machine.alarms || []).map((alarm: any) => ({
                ...alarm,
                machineName: machine.name,
                machineId: machine.id,
            }))
        );
        setAlarms(allAlarms);
    }, [machineData]);

    // Auto-refresh every 5 seconds
    useEffect(() => {
        if (!autoRefresh) return;
        const interval = setInterval(() => {
            // Fetch latest alarms
            console.log('Refreshing alarms...');
        }, 5000);
        return () => clearInterval(interval);
    }, [autoRefresh]);

    const filteredAlarms = alarms.filter(
        (alarm) => selectedPriorities[alarm.priority as keyof typeof selectedPriorities]
    );

    const getPriorityIcon = (priority: string) => {
        switch (priority) {
            case 'critical':
                return '🔴';
            case 'high':
                return '🟠';
            case 'medium':
                return '🟡';
            case 'low':
                return '🟢';
            default:
                return '⚪';
        }
    };

    const getPriorityClass = (priority: string) => {
        return `priority-${priority}`;
    };

    const handleSelectAlarm = (alarmId: string) => {
        const newSelected = new Set(selectedAlarms);
        if (newSelected.has(alarmId)) {
            newSelected.delete(alarmId);
        } else {
            newSelected.add(alarmId);
        }
        setSelectedAlarms(newSelected);
    };

    const handleAcknowledgeSelected = () => {
        console.log('Acknowledging alarms:', Array.from(selectedAlarms));
        // API call to acknowledge alarms
        setSelectedAlarms(new Set());
    };

    const getTimeSince = (timestamp: Date) => {
        const seconds = Math.floor((Date.now() - new Date(timestamp).getTime()) / 1000);
        if (seconds < 60) return `${seconds}s ago`;
        if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
        if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`;
        return `${Math.floor(seconds / 86400)}d ago`;
    };

    const alarmsByPriority = {
        critical: filteredAlarms.filter((a) => a.priority === 'critical'),
        high: filteredAlarms.filter((a) => a.priority === 'high'),
        medium: filteredAlarms.filter((a) => a.priority === 'medium'),
        low: filteredAlarms.filter((a) => a.priority === 'low'),
    };

    return (
        <div className="alarms-tab">
            {/* Controls */}
            <div className="alarms-controls">
                <div className="priority-filters">
                    <label className="priority-checkbox">
                        <input
                            type="checkbox"
                            checked={selectedPriorities.critical}
                            onChange={(e) =>
                                setSelectedPriorities({ ...selectedPriorities, critical: e.target.checked })
                            }
                        />
                        🔴 Critical ({alarmsByPriority.critical.length})
                    </label>
                    <label className="priority-checkbox">
                        <input
                            type="checkbox"
                            checked={selectedPriorities.high}
                            onChange={(e) =>
                                setSelectedPriorities({ ...selectedPriorities, high: e.target.checked })
                            }
                        />
                        🟠 High ({alarmsByPriority.high.length})
                    </label>
                    <label className="priority-checkbox">
                        <input
                            type="checkbox"
                            checked={selectedPriorities.medium}
                            onChange={(e) =>
                                setSelectedPriorities({ ...selectedPriorities, medium: e.target.checked })
                            }
                        />
                        🟡 Medium ({alarmsByPriority.medium.length})
                    </label>
                    <label className="priority-checkbox">
                        <input
                            type="checkbox"
                            checked={selectedPriorities.low}
                            onChange={(e) =>
                                setSelectedPriorities({ ...selectedPriorities, low: e.target.checked })
                            }
                        />
                        🟢 Low ({alarmsByPriority.low.length})
                    </label>
                </div>

                <div className="alarm-actions">
                    <label className="auto-refresh-toggle">
                        <input
                            type="checkbox"
                            checked={autoRefresh}
                            onChange={(e) => setAutoRefresh(e.target.checked)}
                        />
                        Auto-refresh (5s)
                    </label>
                    {selectedAlarms.size > 0 && (
                        <button className="ack-btn" onClick={handleAcknowledgeSelected}>
                            ✓ Acknowledge ({selectedAlarms.size})
                        </button>
                    )}
                </div>
            </div>

            {/* Summary Stats */}
            <div className="alarms-summary">
                <div className="summary-card critical">
                    <div className="summary-icon">🔴</div>
                    <div className="summary-info">
                        <div className="summary-count">{alarmsByPriority.critical.length}</div>
                        <div className="summary-label">Critical</div>
                    </div>
                </div>
                <div className="summary-card high">
                    <div className="summary-icon">🟠</div>
                    <div className="summary-info">
                        <div className="summary-count">{alarmsByPriority.high.length}</div>
                        <div className="summary-label">High</div>
                    </div>
                </div>
                <div className="summary-card medium">
                    <div className="summary-icon">🟡</div>
                    <div className="summary-info">
                        <div className="summary-count">{alarmsByPriority.medium.length}</div>
                        <div className="summary-label">Medium</div>
                    </div>
                </div>
                <div className="summary-card low">
                    <div className="summary-icon">🟢</div>
                    <div className="summary-info">
                        <div className="summary-count">{alarmsByPriority.low.length}</div>
                        <div className="summary-label">Low</div>
                    </div>
                </div>
            </div>

            {/* Alarms List */}
            <div className="alarms-list">
                {filteredAlarms.map((alarm) => (
                    <div key={alarm.id} className={`alarm-item ${getPriorityClass(alarm.priority)}`}>
                        <input
                            type="checkbox"
                            className="alarm-checkbox"
                            checked={selectedAlarms.has(alarm.id)}
                            onChange={() => handleSelectAlarm(alarm.id)}
                        />
                        <div className="alarm-icon">{getPriorityIcon(alarm.priority)}</div>
                        <div className="alarm-content">
                            <div className="alarm-header">
                                <span className="alarm-machine">{alarm.machineName}</span>
                                <span className="alarm-time">{getTimeSince(alarm.timestamp)}</span>
                            </div>
                            <div className="alarm-message">{alarm.message}</div>
                            <div className="alarm-details">
                                <span className="alarm-tag">{alarm.tagName}</span>
                                <span className="alarm-value">
                                    Value: {alarm.value} {alarm.unit}
                                </span>
                                {alarm.threshold && (
                                    <span className="alarm-threshold">Threshold: {alarm.threshold}</span>
                                )}
                            </div>
                        </div>
                        <div className="alarm-actions-btn">
                            {!alarm.acknowledged && (
                                <button className="ack-single-btn" title="Acknowledge">
                                    ✓
                                </button>
                            )}
                            <button className="details-btn" title="Details">
                                ℹ️
                            </button>
                        </div>
                    </div>
                ))}
            </div>

            {filteredAlarms.length === 0 && (
                <div className="empty-state">
                    <p>✅ No active alarms</p>
                    <p>All systems operating normally</p>
                </div>
            )}
        </div>
    );
};
