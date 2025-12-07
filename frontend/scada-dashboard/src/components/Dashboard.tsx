import React, { useEffect, useState } from 'react';

interface Tag {
    name: string;
    value: number;
    unit: string;
}

const Dashboard: React.FC = () => {
    const [tags, setTags] = useState<Tag[]>([]);

    useEffect(() => {
        // Poll for tag data (could be WebSocket in production)
        const interval = setInterval(async () => {
            try {
                const response = await fetch('http://localhost:5000/api/Tags');
                if (response.ok) {
                    const data = await response.json();
                    setTags(data);
                }
            } catch (e) {
                console.error(e);
            }
        }, 2000);

        return () => clearInterval(interval);
    }, []);

    return (
        <div className="dashboard">
            <h1>Dashboard</h1>
            <div className="metrics-grid">
                {tags.map(tag => (
                    <div key={tag.name} className="metric-card">
                        <h3>{tag.name}</h3>
                        <p>{tag.value} {tag.unit}</p>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default Dashboard;
