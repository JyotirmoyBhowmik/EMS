import React, { useState, useEffect } from 'react';
import './MeterSetup.css';

interface Meter {
    id: string;
    meterNumber: string;
    meterName: string;
    meterType: string;
    level: number;
    parentMeterId: string | null;
    status: 'active' | 'inactive' | 'maintenance' | 'faulty';
    healthStatus: 'good' | 'warning' | 'critical';
    communicationStatus: 'online' | 'offline' | 'timeout';
    ctPrimaryAmps: number;
    ctSecondaryAmps: number;
    ctRatio: string;
    ptPrimaryVolts: number;
    ptSecondaryVolts: number;
    ptRatio: string;
    location?: string;
    ipAddress?: string;
    modbusAddress?: number;
}

const MeterSetup: React.FC = () => {
    const [meters, setMeters] = useState<Meter[]>([]);
    const [selectedMeter, setSelectedMeter] = useState<Meter | null>(null);
    const [isEditing, setIsEditing] = useState(false);
    const [showAddForm, setShowAddForm] = useState(false);

    // Fetch all meters
    useEffect(() => {
        fetch('http://localhost:5010/api/Energy/meters')
            .then(res => res.json())
            .then(data => setMeters(data))
            .catch(err => console.error('Error loading meters:', err));
    }, []);

    // Get status color
    const getStatusColor = (status: string): string => {
        switch (status) {
            case 'active': return '#4CAF50'; // Green
            case 'inactive': return '#F44336'; // Red
            case 'maintenance': return '#FFC107'; // Yellow
            case 'faulty': return '#FF5722'; // Orange-Red
            default: return '#9E9E9E'; // Gray
        }
    };

    // Get health icon
    const getHealthIcon = (health: string): string => {
        switch (health) {
            case 'good': return '✓';
            case 'warning': return '⚠';
            case 'critical': return '✗';
            default: return '?';
        }
    };

    // Handle meter selection
    const handleSelectMeter = (meter: Meter) => {
        setSelectedMeter(meter);
        setIsEditing(false);
    };

    // Handle save meter
    const handleSaveMeter = async (meter: Meter) => {
        try {
            const method = meter.id ? 'PUT' : 'POST';
            const url = meter.id
                ? `http://localhost:5010/api/Energy/meters/${meter.id}`
                : 'http://localhost:5010/api/Energy/meters';

            await fetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(meter)
            });

            // Refresh meters list
            const response = await fetch('http://localhost:5010/api/Energy/meters');
            const updated = await response.json();
            setMeters(updated);
            setShowAddForm(false);
            setIsEditing(false);
        } catch (err) {
            console.error('Error saving meter:', err);
            alert('Failed to save meter');
        }
    };

    // Handle delete meter
    const handleDeleteMeter = async (meterId: string) => {
        if (!confirm('Are you sure you want to delete this meter?')) return;

        try {
            await fetch(`http://localhost:5010/api/Energy/meters/${meterId}`, {
                method: 'DELETE'
            });

            setMeters(meters.filter(m => m.id !== meterId));
            setSelectedMeter(null);
        } catch (err) {
            console.error('Error deleting meter:', err);
        }
    };

    // Render meter tree (hierarchical)
    const renderMeterTree = (parentId: string | null = null, level: number = 0) => {
        const childMeters = meters.filter(m => m.parentMeterId === parentId);

        return childMeters.map(meter => (
            <div key={meter.id} style={{ marginLeft: `${level * 20}px` }}>
                <div
                    className={`meter-item ${selectedMeter?.id === meter.id ? 'selected' : ''}`}
                    onClick={() => handleSelectMeter(meter)}
                >
                    <span
                        className="status-indicator"
                        style={{ backgroundColor: getStatusColor(meter.status) }}
                    />
                    <span className={`health-icon ${meter.healthStatus}`}>
                        {getHealthIcon(meter.healthStatus)}
                    </span>
                    <span className="meter-name">{meter.meterName}</span>
                    <span className="meter-type">({meter.meterType})</span>
                    {meter.communicationStatus === 'offline' && (
                        <span className="offline-badge">OFFLINE</span>
                    )}
                </div>
                {renderMeterTree(meter.id, level + 1)}
            </div>
        ));
    };

    return (
        <div className="meter-setup-container">
            <h1>⚡ Energy Meter Setup</h1>

            <div className="meter-stats">
                <div className="stat-card">
                    <div className="stat-value">{meters.length}</div>
                    <div className="stat-label">Total Meters</div>
                </div>
                <div className="stat-card green">
                    <div className="stat-value">{meters.filter(m => m.status === 'active').length}</div>
                    <div className="stat-label">Active</div>
                </div>
                <div className="stat-card yellow">
                    <div className="stat-value">{meters.filter(m => m.healthStatus === 'warning').length}</div>
                    <div className="stat-label">Warnings</div>
                </div>
                <div className="stat-card red">
                    <div className="stat-value">{meters.filter(m => m.status === 'inactive').length}</div>
                    <div className="stat-label">Inactive</div>
                </div>
            </div>

            <div className="meter-content">
                {/* Left Panel - Meter Tree */}
                <div className="meter-tree-panel">
                    <div className="panel-header">
                        <h2>Meter Hierarchy</h2>
                        <button className="btn-add" onClick={() => setShowAddForm(true)}>
                            + Add Meter
                        </button>
                    </div>
                    <div className="meter-tree">
                        {renderMeterTree()}
                    </div>
                    <div className="legend">
                        <h4>Status Legend:</h4>
                        <div><span className="dot green"></span> Active</div>
                        <div><span className="dot red"></span> Inactive</div>
                        <div><span className="dot yellow"></span> Maintenance</div>
                        <div><span className="dot orange"></span> Faulty</div>
                    </div>
                </div>

                {/* Right Panel - Meter Details */}
                <div className="meter-details-panel">
                    {selectedMeter ? (
                        <div className="meter-details">
                            <div className="details-header">
                                <h2>{selectedMeter.meterName}</h2>
                                <div className="action-buttons">
                                    <button
                                        className="btn-edit"
                                        onClick={() => setIsEditing(!isEditing)}
                                    >
                                        {isEditing ? 'Cancel' : 'Edit'}
                                    </button>
                                    <button
                                        className="btn-delete"
                                        onClick={() => handleDeleteMeter(selectedMeter.id)}
                                    >
                                        Delete
                                    </button>
                                </div>
                            </div>

                            {isEditing ? (
                                <MeterForm
                                    meter={selectedMeter}
                                    onSave={handleSaveMeter}
                                    meters={meters}
                                />
                            ) : (
                                <MeterView meter={selectedMeter} />
                            )}
                        </div>
                    ) : (
                        <div className="no-selection">
                            <p>Select a meter from the tree to view details</p>
                        </div>
                    )}
                </div>
            </div>

            {/* Add Meter Modal */}
            {showAddForm && (
                <div className="modal-overlay" onClick={() => setShowAddForm(false)}>
                    <div className="modal-content" onClick={(e) => e.stopPropagation()}>
                        <h2>Add New Meter</h2>
                        <MeterForm
                            meter={{
                                id: '',
                                meterNumber: '',
                                meterName: '',
                                meterType: 'Submeter',
                                level: 1,
                                parentMeterId: null,
                                status: 'active',
                                healthStatus: 'good',
                                communicationStatus: 'online',
                                ctPrimaryAmps: 100,
                                ctSecondaryAmps: 5,
                                ctRatio: '100/5',
                                ptPrimaryVolts: 415,
                                ptSecondaryVolts: 110,
                                ptRatio: '415/110'
                            }}
                            onSave={handleSaveMeter}
                            meters={meters}
                        />
                    </div>
                </div>
            )}
        </div>
    );
};

// Meter View Component (Read-only display)
const MeterView: React.FC<{ meter: Meter }> = ({ meter }) => (
    <div className="meter-view">
        <section>
            <h3>Basic Information</h3>
            <div className="info-grid">
                <div><strong>Meter Number:</strong> {meter.meterNumber}</div>
                <div><strong>Type:</strong> {meter.meterType}</div>
                <div><strong>Level:</strong> {meter.level}</div>
                <div><strong>Location:</strong> {meter.location || 'N/A'}</div>
            </div>
        </section>

        <section>
            <h3>CT/PT Configuration</h3>
            <div className="ct-pt-grid">
                <div className="ct-section">
                    <h4>Current Transformer (CT)</h4>
                    <div><strong>Primary:</strong> {meter.ctPrimaryAmps} A</div>
                    <div><strong>Secondary:</strong> {meter.ctSecondaryAmps} A</div>
                    <div><strong>Ratio:</strong> {meter.ctRatio}</div>
                </div>
                <div className="pt-section">
                    <h4>Potential Transformer (PT)</h4>
                    <div><strong>Primary:</strong> {meter.ptPrimaryVolts} V</div>
                    <div><strong>Secondary:</strong> {meter.ptSecondaryVolts} V</div>
                    <div><strong>Ratio:</strong> {meter.ptRatio}</div>
                </div>
            </div>
        </section>

        <section>
            <h3>Communication</h3>
            <div className="info-grid">
                <div><strong>IP Address:</strong> {meter.ipAddress || 'N/A'}</div>
                <div><strong>Modbus Address:</strong> {meter.modbusAddress || 'N/A'}</div>
                <div><strong>Status:</strong>
                    <span className={`badge ${meter.communicationStatus}`}>
                        {meter.communicationStatus}
                    </span>
                </div>
            </div>
        </section>

        <section>
            <h3>Status</h3>
            <div className="status-grid">
                <div>
                    <strong>Meter Status:</strong>
                    <span className={`badge ${meter.status}`}>{meter.status}</span>
                </div>
                <div>
                    <strong>Health:</strong>
                    <span className={`badge ${meter.healthStatus}`}>{meter.healthStatus}</span>
                </div>
            </div>
        </section>
    </div>
);

// Meter Form Component (Editable)
const MeterForm: React.FC<{
    meter: Meter;
    onSave: (meter: Meter) => void;
    meters: Meter[];
}> = ({ meter: initialMeter, onSave, meters }) => {
    const [meter, setMeter] = useState(initialMeter);

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        // Calculate ratios
        meter.ctRatio = `${meter.ctPrimaryAmps}/${meter.ctSecondaryAmps}`;
        meter.ptRatio = `${meter.ptPrimaryVolts}/${meter.ptSecondaryVolts}`;
        onSave(meter);
    };

    return (
        <form className="meter-form" onSubmit={handleSubmit}>
            <div className="form-group">
                <label>Meter Number *</label>
                <input
                    type="text"
                    value={meter.meterNumber}
                    onChange={(e) => setMeter({ ...meter, meterNumber: e.target.value })}
                    required
                />
            </div>

            <div className="form-group">
                <label>Meter Name *</label>
                <input
                    type="text"
                    value={meter.meterName}
                    onChange={(e) => setMeter({ ...meter, meterName: e.target.value })}
                    required
                />
            </div>

            <div className="form-row">
                <div className="form-group">
                    <label>Type</label>
                    <select
                        value={meter.meterType}
                        onChange={(e) => setMeter({ ...meter, meterType: e.target.value })}
                    >
                        <option value="Grid">Grid</option>
                        <option value="Main">Main</option>
                        <option value="DG">DG</option>
                        <option value="Solar">Solar</option>
                        <option value="PMD">PMD</option>
                        <option value="SMD">SMD</option>
                        <option value="Submeter">Submeter</option>
                        <option value="Machine">Machine</option>
                    </select>
                </div>

                <div className="form-group">
                    <label>Parent Meter</label>
                    <select
                        value={meter.parentMeterId || ''}
                        onChange={(e) => setMeter({ ...meter, parentMeterId: e.target.value || null })}
                    >
                        <option value="">None (Root Level)</option>
                        {meters.map(m => (
                            <option key={m.id} value={m.id}>{m.meterName}</option>
                        ))}
                    </select>
                </div>
            </div>

            <h4>CT Configuration</h4>
            <div className="form-row">
                <div className="form-group">
                    <label>CT Primary (A)</label>
                    <input
                        type="number"
                        value={meter.ctPrimaryAmps}
                        onChange={(e) => setMeter({ ...meter, ctPrimaryAmps: Number(e.target.value) })}
                    />
                </div>
                <div className="form-group">
                    <label>CT Secondary (A)</label>
                    <input
                        type="number"
                        value={meter.ctSecondaryAmps}
                        onChange={(e) => setMeter({ ...meter, ctSecondaryAmps: Number(e.target.value) })}
                    />
                </div>
            </div>

            <h4>PT Configuration</h4>
            <div className="form-row">
                <div className="form-group">
                    <label>PT Primary (V)</label>
                    <input
                        type="number"
                        value={meter.ptPrimaryVolts}
                        onChange={(e) => setMeter({ ...meter, ptPrimaryVolts: Number(e.target.value) })}
                    />
                </div>
                <div className="form-group">
                    <label>PT Secondary (V)</label>
                    <input
                        type="number"
                        value={meter.ptSecondaryVolts}
                        onChange={(e) => setMeter({ ...meter, ptSecondaryVolts: Number(e.target.value) })}
                    />
                </div>
            </div>

            <div className="form-row">
                <div className="form-group">
                    <label>IP Address</label>
                    <input
                        type="text"
                        value={meter.ipAddress || ''}
                        onChange={(e) => setMeter({ ...meter, ipAddress: e.target.value })}
                        placeholder="192.168.1.100"
                    />
                </div>
                <div className="form-group">
                    <label>Modbus Address</label>
                    <input
                        type="number"
                        value={meter.modbusAddress || ''}
                        onChange={(e) => setMeter({ ...meter, modbusAddress: Number(e.target.value) })}
                    />
                </div>
            </div>

            <div className="form-actions">
                <button type="submit" className="btn-save">Save Meter</button>
            </div>
        </form>
    );
};

export default MeterSetup;
