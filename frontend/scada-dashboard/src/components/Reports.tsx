import React from 'react';

const Reports: React.FC = () => {
    const generateReport = async (type: 'pdf' | 'excel') => {
        const response = await fetch(`http://localhost:5000/api/Reports/generate/${type}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                reportType: 'Daily Summary',
                startDate: new Date(new Date().setDate(new Date().getDate() - 1)).toISOString(),
                endDate: new Date().toISOString()
            })
        });

        if (response.ok) {
            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `report.${type === 'excel' ? 'xlsx' : 'pdf'}`;
            document.body.appendChild(a);
            a.click();
            a.remove();
        }
    };

    return (
        <div className="reports">
            <h1>Reports</h1>
            <button onClick={() => generateReport('pdf')}>Download PDF</button>
            <button onClick={() => generateReport('excel')}>Download Excel</button>
        </div>
    );
};

export default Reports;
