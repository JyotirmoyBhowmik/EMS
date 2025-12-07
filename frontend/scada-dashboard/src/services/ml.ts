import axios from 'axios';

const ML_API_URL = import.meta.env.VITE_ML_API_URL || 'http://localhost:8000';

export interface AnomalyDetectionRequest {
    tagName: string;
    currentValue: number;
    historicalValues: number[];
}

export interface AnomalyDetectionResponse {
    isAnomaly: boolean;
    anomalyScore: number;
    threshold: number;
    explanation: string;
}

export interface ForecastRequest {
    tagName: string;
    historicalData: Array<{
        tagName: string;
        value: number;
        timestamp: string;
    }>;
    forecastHorizon?: number;
}

export interface ForecastResponse {
    tagName: string;
    predictions: Array<{
        timestamp: string;
        value: number;
        confidence_interval: {
            lower: number;
            upper: number;
        };
    }>;
    confidence: number;
    model: string;
}

export interface PredictiveMaintenanceResponse {
    tagName: string;
    equipmentType: string;
    failureProbability: number;
    recommendedAction: string;
    estimatedTimeToFailure: string;
    confidence: number;
}

export interface PatternRecognitionResponse {
    tagName: string;
    trend: string;
    trendStrength: number;
    hasCyclicPattern: boolean;
    dominantPeriod: string;
    volatility: number;
}

class MLService {
    private baseURL: string;

    constructor() {
        this.baseURL = ML_API_URL;
    }

    async detectAnomaly(request: AnomalyDetectionRequest): Promise<AnomalyDetectionResponse> {
        const response = await axios.post<AnomalyDetectionResponse>(
            `${this.baseURL}/api/ml/detect-anomaly`,
            request
        );
        return response.data;
    }

    async forecast(request: ForecastRequest): Promise<ForecastResponse> {
        const response = await axios.post<ForecastResponse>(
            `${this.baseURL}/api/ml/forecast`,
            request
        );
        return response.data;
    }

    async predictMaintenance(tagName: string, equipmentType: string): Promise<PredictiveMaintenanceResponse> {
        const response = await axios.post<PredictiveMaintenanceResponse>(
            `${this.baseURL}/api/ml/predictive-maintenance`,
            null,
            { params: { tagName, equipmentType } }
        );
        return response.data;
    }

    async recognizePatterns(tagName: string, values: number[]): Promise<PatternRecognitionResponse> {
        const response = await axios.post<PatternRecognitionResponse>(
            `${this.baseURL}/api/ml/pattern-recognition`,
            null,
            { params: { tagName, values } }
        );
        return response.data;
    }

    async healthCheck(): Promise<{ status: string; service: string }> {
        const response = await axios.get(`${this.baseURL}/health`);
        return response.data;
    }
}

export const mlService = new MLService();
export default mlService;
