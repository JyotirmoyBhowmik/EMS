import React, { useState } from 'react';
import './SettingsTab.css';

interface SettingsTabProps {
    filters: any;
    machineData: any[];
    loading: boolean;
}

export const SettingsTab: React.FC<SettingsTabProps> = ({ filters, machineData }) => {
    const [preferences, setPreferences] = useState({
        theme: 'light',
        timezone: 'UTC',
        units: 'metric',
        refreshRate: '5',
        defaultView: 'critical',
    });

    const [favoriteMachines, setFavoriteMachines] = useState<string[]>([]);
    const [notifications, setNotifications] = useState({
        email: true,
        sms: false,
        push: true,
        alarmLevels: ['critical', 'high'],
    });

    const handleSavePreferences = () => {
        localStorage.setItem('user-preferences', JSON.stringify(preferences));
        alert('Preferences saved successfully!');
    };

    const toggleFavorite = (machineId: string) => {
        if (favoriteMachines.includes(machineId)) {
            setFavoriteMachines(favoriteMachines.filter((id) => id !== machineId));
        } else {
            setFavoriteMachines([...favoriteMachines, machineId]);
        }
    };

    return (
        <div className="settings-tab">
            {/* User Preferences */}
            <div className="settings-section">
                <h2>👤 User Preferences</h2>

                <div className="settings-grid">
                    <div className="setting-item">
                        <label>Theme:</label>
                        <select
                            value={preferences.theme}
                            onChange={(e) => setPreferences({ ...preferences, theme: e.target.value })}
                        >
                            <option value="light">Light</option>
                            <option value="dark">Dark</option>
                            <option value="auto">Auto (System)</option>
                        </select>
                    </div>

                    <div className="setting-item">
                        <label>Timezone:</label>
                        <select
                            value={preferences.timezone}
                            onChange={(e) => setPreferences({ ...preferences, timezone: e.target.value })}
                        >
                            <option value="UTC">UTC</option>
                            <option value="America/New_York">Eastern Time</option>
                            <option value="Asia/Kolkata">India (IST)</option>
                            <option value="Europe/London">London (GMT)</option>
                        </select>
                    </div>

                    <div className="setting-item">
                        <label>Units:</label>
                        <select
                            value={preferences.units}
                            onChange={(e) => setPreferences({ ...preferences, units: e.target.value })}
                        >
                            <option value="metric">Metric (°C, kW, bar)</option>
                            <option value="imperial">Imperial (°F, HP, psi)</option>
                        </select>
                    </div>

                    <div className="setting-item">
                        <label>Auto-Refresh Rate:</label>
                        <select
                            value={preferences.refreshRate}
                            onChange={(e) => setPreferences({ ...preferences, refreshRate: e.target.value })}
                        >
                            <option value="1">1 second</option>
                            <option value="5">5 seconds</option>
                            <option value="10">10 seconds</option>
                            <option value="30">30 seconds</option>
                            <option value="0">Manual</option>
                        </select>
                    </div>

                    <div className="setting-item">
                        <label>Default Dashboard View:</label>
                        <select
                            value={preferences.defaultView}
                            onChange={(e) => setPreferences({ ...preferences, defaultView: e.target.value })}
                        >
                            <option value="critical">20 Critical Machines</option>
                            <option value="favorites">My Favorites</option>
                            <option value="all">Show All</option>
                            <option value="last">Last Selection</option>
                        </select>
                    </div>
                </div>

                <button className="save-btn" onClick={handleSavePreferences}>
                    💾 Save Preferences
                </button>
            </div>

            {/* Favorite Machines */}
            <div className="settings-section">
                <h2>⭐ Favorite Machines</h2>
                <p className="section-description">Mark machines as favorites for quick access</p>

                <div className="favorites-grid">
                    {machineData.slice(0, 20).map((machine) => (
                        <div
                            key={machine.id}
                            className={`favorite-item ${favoriteMachines.includes(machine.id) ? 'favorited' : ''}`}
                            onClick={() => toggleFavorite(machine.id)}
                        >
                            <span className="favorite-icon">
                                {favoriteMachines.includes(machine.id) ? '⭐' : '☆'}
                            </span>
                            <span className="favorite-name">{machine.name}</span>
                        </div>
                    ))}
                </div>
            </div>

            {/* Notification Settings */}
            <div className="settings-section">
                <h2>🔔 Notification Settings</h2>

                <div className="notification-options">
                    <label className="notification-checkbox">
                        <input
                            type="checkbox"
                            checked={notifications.email}
                            onChange={(e) => setNotifications({ ...notifications, email: e.target.checked })}
                        />
                        📧 Email Notifications
                    </label>

                    <label className="notification-checkbox">
                        <input
                            type="checkbox"
                            checked={notifications.sms}
                            onChange={(e) => setNotifications({ ...notifications, sms: e.target.checked })}
                        />
                        📱 SMS Notifications
                    </label>

                    <label className="notification-checkbox">
                        <input
                            type="checkbox"
                            checked={notifications.push}
                            onChange={(e) => setNotifications({ ...notifications, push: e.target.checked })}
                        />
                        🔔 Push Notifications
                    </label>
                </div>

                <div className="alarm-level-settings">
                    <label>Notify me for alarm levels:</label>
                    <div className="alarm-checkboxes">
                        <label>
                            <input type="checkbox" defaultChecked /> 🔴 Critical
                        </label>
                        <label>
                            <input type="checkbox" defaultChecked /> 🟠 High
                        </label>
                        <label>
                            <input type="checkbox" /> 🟡 Medium
                        </label>
                        <label>
                            <input type="checkbox" /> 🟢 Low
                        </label>
                    </div>
                </div>
            </div>

            {/* Account Settings */}
            <div className="settings-section">
                <h2>🔐 Account Settings</h2>

                <div className="account-actions">
                    <button className="account-btn">🔑 Change Password</button>
                    <button className="account-btn">🔒 Enable MFA</button>
                    <button className="account-btn">📥 Export My Data</button>
                    <button className="account-btn danger">🚪 Logout</button>
                </div>
            </div>
        </div>
    );
};
