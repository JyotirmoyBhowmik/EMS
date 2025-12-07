import { useState, useEffect } from 'react';
import { Line } from 'react-chartjs-2';
import { mlService } from '../../services/ml';
import type { ForecastResponse } from '../../services/ml';

interface PredictiveAnalyticsProps {
    tagName: string;
}

export default function PredictiveAnalytics({ tagName }: PredictiveAnalyticsProps) {
    const [forecast, setForecast] = useState<ForecastResponse | null>(null);
    const [loading, setLoading] = useState(false);

    const generateForecast = async () => {
        setLoading(true);
        try {
            // In production, fetch actual historical data
            const historicalData = Array.from({ length: 100 }, (_, i) => ({
                tagName,
                value: 50 + Math.sin(i / 10) * 20 + Math.random() * 10,
                timestamp: new Date(Date.now() - (100 - i) * 3600000).toISOString()
            }));

            const result = await mlService.forecast({
                tagName,
                historicalData,
                forecastHorizon: 24
            });

            setForecast(result);
        } catch (error) {
            console.error('Forecast error:', error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        generateForecast();
    }, [tagName]);

    if (loading) {
        return (
            <div className="card">
                <div className="flex items-center justify-center p-8">
                    <div className="spinner"></div>
                    <span className="ml-3">Generating forecast...</span>
                </div>
            </div>
        );
    }

    if (!forecast) {
        return null;
    }

    const chartData = {
        labels: forecast.predictions.map(p => new Date(p.timestamp).toLocaleTimeString()),
        datasets: [
            {
                label: 'Predicted Value',
                data: forecast.predictions.map(p => p.value),
                borderColor: '#3b82f6',
                backgroundColor: 'rgba(59, 130, 246, 0.1)',
                fill: true,
            },
            {
                label: 'Upper Bound',
                data: forecast.predictions.map(p => p.confidence_interval.upper),
                borderColor: 'rgba(59, 130, 246, 0.3)',
                borderDash: [5, 5],
                fill: false,
            },
            {
                label: 'Lower Bound',
                data: forecast.predictions.map(p => p.confidence_interval.lower),
                borderColor: 'rgba(59, 130, 246, 0.3)',
                borderDash: [5, 5],
                fill: false,
            },
        ],
    };

    return (
        <div className="card">
            <div className="flex items-center justify-between mb-4">
                <div>
                    <h3 className="text-xl font-semibold">Predictive Analytics</h3>
                    <p className="text-sm text-gray-400">{tagName}</p>
                </div>
                <div className="text-right">
                    <p className="text-sm text-gray-400">Model</p>
                    <p className="font-medium">{forecast.model}</p>
                    <p className="text-sm text-scada-good">
                        {(forecast.confidence * 100).toFixed(0)}% confidence
                    </p>
                </div>
            </div>

            <div className="h-64">
                <Line
                    data={chartData}
                    options={{
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                position: 'top',
                            },
                            title: {
                                display: true,
                                text: '24-Hour Forecast',
                            },
                        },
                    }}
                />
            </div>

            <div className="mt-4 grid grid-cols-3 gap-4">
                <div className="text-center">
                    <p className="text-sm text-gray-400">Avg Predicted</p>
                    <p className="text-lg font-bold text-primary-400">
                        {(forecast.predictions.reduce((sum, p) => sum + p.value, 0) / forecast.predictions.length).toFixed(2)}
                    </p>
                </div>
                <div className="text-center">
                    <p className="text-sm text-gray-400">Max Predicted</p>
                    <p className="text-lg font-bold text-scada-warning">
                        {Math.max(...forecast.predictions.map(p => p.value)).toFixed(2)}
                    </p>
                </div>
                <div className="text-center">
                    <p className="text-sm text-gray-400">Min Predicted</p>
                    <p className="text-lg font-bold text-scada-good">
                        {Math.min(...forecast.predictions.map(p => p.value)).toFixed(2)}
                    </p>
                </div>
            </div>

            <button
                onClick={generateForecast}
                className="btn-primary mt-4 w-full"
            >
                Regenerate Forecast
            </button>
        </div>
    );
}
