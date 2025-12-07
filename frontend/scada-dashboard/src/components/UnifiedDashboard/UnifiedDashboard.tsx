import React, { useState, useEffect } from 'react';
import { FilterBar, FilterState } from './FilterBar';
import { DashboardTab } from './tabs/DashboardTab';
import { TagsTab } from './tabs/TagsTab';
import { AlarmsTab } from './tabs/AlarmsTab';
import { TrendsTab } from './tabs/TrendsTab';
import { ReportsTab } from './tabs/ReportsTab';
import { SettingsTab } from './tabs/SettingsTab';
import './UnifiedDashboard.css';

type TabType = 'dashboard' | 'tags' | 'alarms' | 'trends' | 'reports' | 'settings';

export const UnifiedDashboard: React.FC = () => {
    const [activeTab, setActiveTab] = useState<TabType>('dashboard');
    const [filters, setFilters] = useState<FilterState>({
        siteId: 'all',
        machineType: 'all',
        machineId: 'all',
        preset: 'critical',
    });
    const [machineData, setMachineData] = useState<any[]>([]);
    const [loading, setLoading] = useState(false);

    // Fetch machine data when filters change
    useEffect(() => {
        fetchMachineData();
    }, [filters]);

    const fetchMachineData = async () => {
        setLoading(true);
        try {
            const params = new URLSearchParams();

            if (filters.siteId !== 'all') params.append('siteId', filters.siteId);
            if (filters.machineType !== 'all') params.append('type', filters.machineType);
            if (filters.machineId !== 'all') params.append('machineId', filters.machineId);
            params.append('preset', filters.preset);

            const response = await fetch(`/api/machines/filtered?${params}`);
            const data = await response.json();
            setMachineData(data);
        } catch (error) {
            console.error('Failed to fetch machine data:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleFilterChange = (newFilters: FilterState) => {
        setFilters(newFilters);
        // Save to localStorage for persistence
        localStorage.setItem('dashboard-filters', JSON.stringify(newFilters));
    };

    // Load saved filters on mount
    useEffect(() => {
        const saved = localStorage.getItem('dashboard-filters');
        if (saved) {
            setFilters(JSON.parse(saved));
        }
    }, []);

    const renderActiveTab = () => {
        const tabProps = { filters, machineData, loading };

        switch (activeTab) {
            case 'dashboard':
                return <DashboardTab {...tabProps} />;
            case 'tags':
                return <TagsTab {...tabProps} />;
            case 'alarms':
                return <AlarmsTab {...tabProps} />;
            case 'trends':
                return <TrendsTab {...tabProps} />;
            case 'reports':
                return <ReportsTab {...tabProps} />;
            case 'settings':
                return <SettingsTab {...tabProps} />;
            default:
                return <DashboardTab {...tabProps} />;
        }
    };

    const getTabStats = () => {
        return {
            dashboard: machineData.length,
            tags: machineData.reduce((sum, m) => sum + (m.tagCount || 0), 0),
            alarms: machineData.reduce((sum, m) => sum + (m.activeAlarms || 0), 0),
            trends: machineData.length,
            reports: 0,
            settings: 0,
        };
    };

    const stats = getTabStats();

    return (
        <div className="unified-dashboard">
            {/* Header */}
            <div className="dashboard-header">
                <h1>🏭 Fleet Management Dashboard</h1>
                <div className="header-stats">
                    <div className="stat">
                        <span className="stat-label">Total Machines</span>
                        <span className="stat-value">{machineData.length}</span>
                    </div>
                    <div className="stat">
                        <span className="stat-label">Running</span>
                        <span className="stat-value stat-success">
                            {machineData.filter((m) => m.status === 'running').length}
                        </span>
                    </div>
                    <div className="stat">
                        <span className="stat-label">Warnings</span>
                        <span className="stat-value stat-warning">
                            {machineData.filter((m) => m.status === 'warning').length}
                        </span>
                    </div>
                    <div className="stat">
                        <span className="stat-label">Down</span>
                        <span className="stat-value stat-danger">
                            {machineData.filter((m) => m.status === 'stopped').length}
                        </span>
                    </div>
                </div>
            </div>

            {/* Filter Bar (Consistent across all tabs) */}
            <FilterBar onFilterChange={handleFilterChange} initialFilters={filters} />

            {/* Tab Navigation */}
            <div className="tab-navigation">
                <button
                    className={`tab-btn ${activeTab === 'dashboard' ? 'active' : ''}`}
                    onClick={() => setActiveTab('dashboard')}
                >
                    <span className="tab-icon">📊</span>
                    <span className="tab-label">Dashboard</span>
                    <span className="tab-badge">{stats.dashboard}</span>
                </button>

                <button
                    className={`tab-btn ${activeTab === 'tags' ? 'active' : ''}`}
                    onClick={() => setActiveTab('tags')}
                >
                    <span className="tab-icon">📋</span>
                    <span className="tab-label">Tags</span>
                    <span className="tab-badge">{stats.tags}</span>
                </button>

                <button
                    className={`tab-btn ${activeTab === 'alarms' ? 'active' : ''}`}
                    onClick={() => setActiveTab('alarms')}
                >
                    <span className="tab-icon">🚨</span>
                    <span className="tab-label">Alarms</span>
                    {stats.alarms > 0 && (
                        <span className="tab-badge badge-alarm">{stats.alarms}</span>
                    )}
                </button>

                <button
                    className={`tab-btn ${activeTab === 'trends' ? 'active' : ''}`}
                    onClick={() => setActiveTab('trends')}
                >
                    <span className="tab-icon">📈</span>
                    <span className="tab-label">Trends</span>
                </button>

                <button
                    className={`tab-btn ${activeTab === 'reports' ? 'active' : ''}`}
                    onClick={() => setActiveTab('reports')}
                >
                    <span className="tab-icon">📄</span>
                    <span className="tab-label">Reports</span>
                </button>

                <button
                    className={`tab-btn ${activeTab === 'settings' ? 'active' : ''}`}
                    onClick={() => setActiveTab('settings')}
                >
                    <span className="tab-icon">⚙️</span>
                    <span className="tab-label">Settings</span>
                </button>
            </div>

            {/* Tab Content */}
            <div className="tab-content">
                {loading ? (
                    <div className="loading-state">
                        <div className="spinner"></div>
                        <p>Loading machine data...</p>
                    </div>
                ) : (
                    renderActiveTab()
                )}
            </div>

            {/* Footer */}
            <div className="dashboard-footer">
                <span>Last updated: {new Date().toLocaleTimeString()}</span>
                <span>
                    {machineData.filter((m) => m.status === 'running').length}/{machineData.length}{' '}
                    machines operational
                </span>
            </div>
        </div>
    );
};
