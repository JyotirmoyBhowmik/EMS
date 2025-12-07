import React, { useState, useEffect } from 'react';
import './TagsTab.css';

interface TagsTabProps {
    filters: any;
    machineData: any[];
    loading: boolean;
}

export const TagsTab: React.FC<TagsTabProps> = ({ machineData }) => {
    const [tags, setTags] = useState<any[]>([]);
    const [searchQuery, setSearchQuery] = useState('');
    const [selectedTag, setSelectedTag] = useState<any>(null);
    const [groupBy, setGroupBy] = useState<'category' | 'machine'>('category');

    useEffect(() => {
        // Fetch tags for selected machines
        const allTags = machineData.flatMap((machine) =>
            (machine.tags || []).map((tag: any) => ({
                ...tag,
                machineName: machine.name,
                machineId: machine.id,
            }))
        );
        setTags(allTags);
    }, [machineData]);

    const filteredTags = tags.filter(
        (tag) =>
            tag.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
            tag.machineName.toLowerCase().includes(searchQuery.toLowerCase())
    );

    const groupedTags = filteredTags.reduce((acc: any, tag) => {
        const key = groupBy === 'category' ? tag.category : tag.machineName;
        if (!acc[key]) acc[key] = [];
        acc[key].push(tag);
        return acc;
    }, {});

    const getTagStatusIcon = (quality: string) => {
        switch (quality) {
            case 'good':
                return '✅';
            case 'bad':
                return '❌';
            case 'uncertain':
                return '⚠️';
            default:
                return '⚪';
        }
    };

    return (
        <div className="tags-tab">
            {/* Controls */}
            <div className="tags-controls">
                <div className="tags-search">
                    <input
                        type="text"
                        placeholder="🔍 Search tags by name or machine..."
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        className="tag-search-input"
                    />
                </div>

                <div className="tags-actions">
                    <select value={groupBy} onChange={(e) => setGroupBy(e.target.value as any)}>
                        <option value="category">Group by Category</option>
                        <option value="machine">Group by Machine</option>
                    </select>
                    <button className="export-btn">📥 Export CSV</button>
                </div>
            </div>

            {/* Stats */}
            <div className="tags-stats">
                <div className="tag-stat">
                    <span className="stat-label">Total Tags</span>
                    <span className="stat-value">{tags.length}</span>
                </div>
                <div className="tag-stat">
                    <span className="stat-label">Good Quality</span>
                    <span className="stat-value stat-good">
                        {tags.filter((t) => t.quality === 'good').length}
                    </span>
                </div>
                <div className="tag-stat">
                    <span className="stat-label">Bad Quality</span>
                    <span className="stat-value stat-bad">
                        {tags.filter((t) => t.quality === 'bad').length}
                    </span>
                </div>
                <div className="tag-stat">
                    <span className="stat-label">Machines</span>
                    <span className="stat-value">{machineData.length}</span>
                </div>
            </div>

            {/* Tag Groups */}
            <div className="tags-content">
                {Object.keys(groupedTags).map((groupName) => (
                    <div key={groupName} className="tag-group">
                        <div className="tag-group-header">
                            <h3>
                                {groupBy === 'category' ? '📁' : '⚙️'} {groupName}
                            </h3>
                            <span className="tag-count">{groupedTags[groupName].length} tags</span>
                        </div>

                        <div className="tag-list">
                            {groupedTags[groupName].map((tag: any) => (
                                <div
                                    key={tag.id}
                                    className="tag-item"
                                    onClick={() => setSelectedTag(tag)}
                                >
                                    <div className="tag-info">
                                        <span className="tag-status">{getTagStatusIcon(tag.quality)}</span>
                                        <div className="tag-details">
                                            <span className="tag-name">{tag.name}</span>
                                            {groupBy === 'category' && (
                                                <span className="tag-machine">{tag.machineName}</span>
                                            )}
                                        </div>
                                    </div>
                                    <div className="tag-value">
                                        <span className="tag-current-value">{tag.value}</span>
                                        <span className="tag-unit">{tag.unit}</span>
                                    </div>
                                    <button className="tag-trend-btn" title="View Trend">
                                        📊
                                    </button>
                                </div>
                            ))}
                        </div>
                    </div>
                ))}
            </div>

            {/* Tag Detail Modal */}
            {selectedTag && (
                <div className="tag-modal-overlay" onClick={() => setSelectedTag(null)}>
                    <div className="tag-modal" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h2>{selectedTag.name}</h2>
                            <button className="modal-close" onClick={() => setSelectedTag(null)}>
                                ✕
                            </button>
                        </div>
                        <div className="modal-body">
                            <div className="modal-info-grid">
                                <div><strong>Machine:</strong> {selectedTag.machineName}</div>
                                <div><strong>Category:</strong> {selectedTag.category}</div>
                                <div><strong>Current Value:</strong> {selectedTag.value} {selectedTag.unit}</div>
                                <div><strong>Quality:</strong> {selectedTag.quality}</div>
                                <div><strong>Data Type:</strong> {selectedTag.dataType}</div>
                                <div><strong>Update Rate:</strong> {selectedTag.scanRate}ms</div>
                            </div>
                            <div className="modal-actions">
                                <button className="modal-btn">📊 View Trend</button>
                                <button className="modal-btn">📝 Edit Config</button>
                                <button className="modal-btn">📥 Export History</button>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {filteredTags.length === 0 && (
                <div className="empty-state">
                    <p>🔍 No tags found</p>
                    <p>Try adjusting your search or filter criteria</p>
                </div>
            )}
        </div>
    );
};
