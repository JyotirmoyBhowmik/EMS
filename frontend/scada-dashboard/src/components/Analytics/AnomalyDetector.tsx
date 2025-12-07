import { useState, useEffect } from 'react';
import { AlertCircle } from 'lucide-react';
import { mlService } from '../../services/ml';
import type { AnomalyDetectionResponse } from '../../services/ml';

interface AnomalyDetectorProps {
    tagName: string;
    currentValue: number;
}

export default function AnomalyDetector({ tagName, currentValue }: AnomalyDetectorProps) {
    const [anomaly, setAnomaly] = useState<AnomalyDetectionResponse | null>(null);
    const [historicalValues, setHistoricalValues] = useState<number[]>([]);

    useEffect(() => {
        // Add current value to history
        setHistoricalValues(prev => [...prev.slice(-99), currentValue]);
    }, [currentValue]);

    useEffect(() => {
        if (historicalValues.length < 30) return; // Need enough history

        const checkAnomaly = async () => {
            try {
                const result = await mlService.detectAnomaly({
                    tagName,
                    currentValue,
                    historicalValues
                });
                setAnomaly(result);
            } catch (error) {
                console.error('Anomaly detection error:', error);
            }
        };

        checkAnomaly();
    }, [currentValue, historicalValues, tagName]);

    if (!anomaly || !anomaly.isAnomaly) {
        return null; // Only show when anomaly detected
    }

    return (
        <div className="card glass border-l-4 border-scada-alarm animate-glow">
            <div className="flex items-start gap-3">
                <AlertCircle className="w-6 h-6 text-scada-alarm flex-shrink-0 mt-1" />
                <div className="flex-1">
                    <h4 className="font-semibold text-scada-alarm mb-2">Anomaly Detected</h4>
                    <p className="text-sm text-gray-300 mb-2">{anomaly.explanation}</p>

                    <div className="grid grid-cols-2 gap-3 text-sm">
                        <div>
                            <p className="text-gray-400">Anomaly Score</p>
                            <p className="font-bold text-scada-alarm">{anomaly.anomalyScore.toFixed(2)}</p>
                        </div>
                        <div>
                            <p className="text-gray-400">Threshold</p>
                            <p className="font-bold">{anomaly.threshold.toFixed(2)}</p>
                        </div>
                    </div>

                    <div className="mt-3 p-2 bg-scada-alarm/20 rounded text-xs">
                        <p className="font-bold">⚠️ Action Required</p>
                        <p className="text-gray-300 mt-1">Investigate unusual behavior - Value deviates significantly from normal pattern</p>
                    </div>
                </div>
            </div>
        </div>
    );
}
