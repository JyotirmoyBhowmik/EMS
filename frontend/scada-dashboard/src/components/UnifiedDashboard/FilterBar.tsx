import React, { useState, useEffect } from 'react';
import './FilterBar.css';

interface FilterBarProps {
    onFilterChange: (filters: FilterState) => void;
    initialFilters?: FilterState;
}

export interface FilterState {
    siteId: string;
    machineType: string;
    machineId: string;
    preset: 'critical' | 'favorites' | 'all';
}

export const FilterBar: React.FC<FilterBarProps> = ({ onFilterChange, initialFilters }) => {
    const [filters, setFilters] = useState<FilterState>(
        initialFilters || {
            siteId: 'all',
            machineType: 'all',
            machineId: 'all',
            preset: 'critical',
        }
    );

    const [sites, setSites] = useState<any[]>([]);
    const [types, setTypes] = useState<any[]>([]);
    const [machines, setMachines] = useState<any[]>([]);

    // Fetch sites
    useEffect(() => {
        fetch('/api/sites')
            .then((res) => res.json())
            .then((data) => setSites(data));
    }, []);

    // Fetch types when site changes
    useEffect(() => {
        if (filters.siteId && filters.siteId !== 'all') {
            fetch(`/api/sites/${filters.siteId}/machine-types`)
                .then((res) => res.json())
                .then((data) => setTypes(data));
        } else {
            fetch('/api/machine-types')
                .then((res) => res.json())
                .then((data) => setTypes(data));
        }
    }, [filters.siteId]);

    // Fetch machines when site or type changes
    useEffect(() => {
        const params = new URLSearchParams();
        if (filters.siteId !== 'all') params.append('siteId', filters.siteId);
        if (filters.machineType !== 'all') params.append('type', filters.machineType);

        fetch(`/api/machines?${params}`)
            .then((res) => res.json())
            .then((data) => setMachines(data));
    }, [filters.siteId, filters.machineType]);

    // Notify parent when filters change
    useEffect(() => {
        onFilterChange(filters);
    }, [filters, onFilterChange]);

    const handleSiteChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        setFilters((prev) => ({
            ...prev,
            siteId: e.target.value,
            machineType: 'all',
            machineId: 'all',
        }));
    };

    const handleTypeChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        setFilters((prev) => ({
            ...prev,
            machineType: e.target.value,
            machineId: 'all',
        }));
    };

    const handleMachineChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        setFilters((prev) => ({
            ...prev,
            machineId: e.target.value,
        }));
    };

    const handlePresetChange = (preset: 'critical' | 'favorites' | 'all') => {
        setFilters((prev) => ({ ...prev, preset }));
    };

    const resetFilters = () => {
        setFilters({
            siteId: 'all',
            machineType: 'all',
            machineId: 'all',
            preset: 'critical',
        });
    };

    return (
        <div className="filter-bar">
            {/* Breadcrumb Navigation */}
            <div className="filter-breadcrumb">
                <span className="breadcrumb-icon">🏭</span>
                <select
                    value={filters.siteId}
                    onChange={handleSiteChange}
                    className="filter-select filter-site"
                >
                    <option value="all">All Sites ({sites.length})</option>
                    {sites.map((site) => (
                        <option key={site.id} value={site.id}>
                            {site.name} ({site.machineCount})
                        </option>
                    ))}
                </select>

                <span className="breadcrumb-separator">›</span>

                <span className="breadcrumb-icon">⚙️</span>
                <select
                    value={filters.machineType}
                    onChange={handleTypeChange}
                    className="filter-select filter-type"
                    disabled={filters.siteId === 'all'}
                >
                    <option value="all">All Types ({types.length})</option>
                    {types.map((type) => (
                        <option key={type.name} value={type.name}>
                            {type.name} ({type.count})
                        </option>
                    ))}
                </select>

                <span className="breadcrumb-separator">›</span>

                <span className="breadcrumb-icon">📍</span>
                <select
                    value={filters.machineId}
                    onChange={handleMachineChange}
                    className="filter-select filter-machine"
                    disabled={filters.machineType === 'all'}
                >
                    <option value="all">All Machines ({machines.length})</option>
                    {machines.map((machine) => (
                        <option key={machine.id} value={machine.id}>
                            {machine.name}
                        </option>
                    ))}
                </select>
            </div>

            {/* Quick Presets */}
            <div className="filter-presets">
                <button
                    className={`preset-btn ${filters.preset === 'critical' ? 'active' : ''}`}
                    onClick={() => handlePresetChange('critical')}
                >
                    🔴 20 Critical
                </button>
                <button
                    className={`preset-btn ${filters.preset === 'favorites' ? 'active' : ''}`}
                    onClick={() => handlePresetChange('favorites')}
                >
                    ⭐ My Favorites
                </button>
                <button
                    className={`preset-btn ${filters.preset === 'all' ? 'active' : ''}`}
                    onClick={() => handlePresetChange('all')}
                >
                    📊 Show All
                </button>
                <button className="reset-btn" onClick={resetFilters}>
                    🔄 Reset
                </button>
            </div>

            {/* Search Bar */}
            <div className="filter-search">
                <input
                    type="text"
                    placeholder="🔍 Search machines, tags, alarms..."
                    className="search-input"
                />
            </div>
        </div>
    );
};
