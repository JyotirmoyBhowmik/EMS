import { useState, useEffect } from 'react';
import { AlertTriangle, CheckCircle, Clock } from 'lucide-react';
import { mlService } from '../../services/ml';
import type { PredictiveMaintenanceResponse } from '../../services/ml';

interface MaintenanceWidgetProps {
    tagName: string;
    equipmentType: string;
}

export default function MaintenanceWidget({ tagName, equipmentType }: MaintenanceWidgetProps) {
    const [prediction, setPrediction] = useState<PredictiveMaintenanceResponse | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchPrediction = async () => {
            try {
                const result = await mlService.predictMaintenance(tagName, equipmentType);
                setPrediction(result);
            } catch (error) {
                console.error('Maintenance prediction error:', error);
            } finally {
                setLoading(false);
            }
        };

        fetchPrediction();
        const interval = setInterval(fetchPrediction, 300000); // Refresh every 5 minutes

        return () => clearInterval(interval);
    }, [tagName, equipmentType]);

    if (loading) {
        return (
            <div className="card">
                <div className="flex items-center justify-center p-4">
                    <div className="spinner"></div>
                </div>
            </div>
        );
    }

    if (!prediction) {
        return null;
    }

    const getSeverityColor = (probability: number) => {
        if (probability > 0.7) return 'text-scada-critical';
        if (probability > 0.4) return 'text-scada-warning';
        return 'text-scada-good';
    };

    const getSeverityIcon = (probability: number) => {
        if (probability > 0.4) return AlertTriangle;
        return CheckCircle;
    };

    const Icon = getSeverityIcon(prediction.failureProbability);

    return (
        <div className="card glass">
            <div className="flex items-start justify-between mb-3">
                <div>
                    <h4 className="font-semibold">Predictive Maintenance</h4>
                    <p className="text-xs text-gray-400">{equipmentType}</p>
                </div>
                <Icon className={`w-6 h-6 ${getSeverityColor(prediction.failureProbability)}`} />
            </div>

            <div className="space-y-3">
                <div>
                    <div className="flex justify-between items-center mb-1">
                        <span className="text-sm text-gray-400">Failure Probability</span>
                        <span className={`text-sm font-bold ${getSeverityColor(prediction.failureProbability)}`}>
                            {(prediction.failureProbability * 100).toFixed(1)}%
                        </span>
                    </div>
                    <div className="w-full bg-scada-background rounded-full h-2">
                        <div
                            className={`h-2 rounded-full ${prediction.failureProbability > 0.7 ? 'bg-scada-critical' :
                                    prediction.failureProbability > 0.4 ? 'bg-scada-warning' :
                                        'bg-scada-good'
                                }`}
                            style={{ width: `${prediction.failureProbability * 100}%` }}
                        ></div>
                    </div>
                </div>

                <div className="flex items-center gap-2 text-sm">
                    <Clock className="w-4 h-4 text-gray-400" />
                    <span className="text-gray-300">
                        Est. Time to Failure: <span className="font-medium">{prediction.estimatedTimeToFailure}</span>
                    </span>
                </div>

                <div className="p-3 bg-scada-background rounded-lg">
                    <p className="text-sm font-medium mb-1">Recommendation</p>
                    <p className="text-sm text-gray-300">{prediction.recommendedAction}</p>
                </div>

                <div className="text-xs text-gray-500 text-center">
                    Confidence: {(prediction.confidence * 100).toFixed(0)}%
                </div>
            </div>
        </div>
    );
}
