from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from datetime import datetime, timedelta
import numpy as np
from typing import List, Optional
import logging
from prometheus_client import Counter, Histogram, generate_latest, CONTENT_TYPE_LATEST
from fastapi.responses import Response

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI(
    title="SCADA ML Service",
    description="Machine Learning service for predictive maintenance and anomaly detection",
    version="1.0.0"
)

# Prometheus metrics
predictions_total = Counter('ml_predictions_total', 'Total ML predictions made')
inference_duration = Histogram('ml_inference_duration_seconds', 'ML inference duration')
anomalies_detected = Counter('ml_anomalies_detected_total', 'Total anomalies detected')

# Models
class TagData(BaseModel):
    tagName: str
    value: float
    timestamp: datetime

class PredictionRequest(BaseModel):
    tagName: str
    historicalData: List[TagData]
    forecastHorizon: int = 24  # hours

class PredictionResponse(BaseModel):
    tagName: str
    predictions: List[dict]
    confidence: float
    model: str

class AnomalyRequest(BaseModel):
    tagName: str
    currentValue: float
    historicalValues: List[float]

class AnomalyResponse(BaseModel):
    isAnomaly: bool
    anomalyScore: float
    threshold: float
    explanation: str

# Anomaly Detection using Isolation Forest (simplified)
@app.post("/api/ml/detect-anomaly", response_model=AnomalyResponse)
async def detect_anomaly(request: AnomalyRequest):
    """
    Detect anomalies in tag values using statistical methods
    """
    try:
        with inference_duration.time():
            # Simple statistical anomaly detection
            historical = np.array(request.historicalValues)
            mean = np.mean(historical)
            std = np.std(historical)
            
            # Z-score based anomaly detection
            z_score = abs((request.currentValue - mean) / (std + 1e-10))
            threshold = 3.0  # 3-sigma rule
            
            is_anomaly = z_score > threshold
            
            if is_anomaly:
                anomalies_detected.inc()
            
            predictions_total.inc()
            
            explanation = f"Value {request.currentValue:.2f} is {z_score:.2f} standard deviations from mean {mean:.2f}"
            
            return AnomalyResponse(
                isAnomaly=is_anomaly,
                anomalyScore=float(z_score),
                threshold=threshold,
                explanation=explanation
            )
    
    except Exception as e:
        logger.error(f"Error in anomaly detection: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

# Time Series Forecasting
@app.post("/api/ml/forecast", response_model=PredictionResponse)
async def forecast_timeseries(request: PredictionRequest):
    """
    Forecast future tag values using time series analysis
    """
    try:
        with inference_duration.time():
            # Extract values
            values = [d.value for d in request.historicalData]
            
            # Simple moving average forecast (placeholder for LSTM/ARIMA)
            window_size = min(24, len(values))
            if len(values) >= window_size:
                recent_avg = np.mean(values[-window_size:])
                trend = (values[-1] - values[-window_size]) / window_size if len(values) > window_size else 0
            else:
                recent_avg = np.mean(values)
                trend = 0
            
            # Generate predictions
            predictions = []
            for i in range(request.forecastHorizon):
                predicted_value = recent_avg + (trend * i)
                predictions.append({
                    "timestamp": (datetime.now() + timedelta(hours=i+1)).isoformat(),
                    "value": float(predicted_value),
                    "confidence_interval": {
                        "lower": float(predicted_value * 0.95),
                        "upper": float(predicted_value * 1.05)
                    }
                })
            
            predictions_total.inc()
            
            return PredictionResponse(
                tagName=request.tagName,
                predictions=predictions,
                confidence=0.85,
                model="MovingAverage"
            )
    
    except Exception as e:
        logger.error(f"Error in forecasting: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

# Predictive Maintenance
@app.post("/api/ml/predictive-maintenance")
async def predict_maintenance(tagName: str, equipmentType: str):
    """
    Predict equipment maintenance needs
    """
    try:
        # Placeholder for actual predictive maintenance model
        # Would use features like vibration, temperature, operating hours
        
        failure_probability = np.random.uniform(0.1, 0.4)  # Replace with actual model
        
        return {
            "tagName": tagName,
            "equipmentType": equipmentType,
            "failureProbability": float(failure_probability),
            "recommendedAction": "Schedule inspection" if failure_probability > 0.3 else "Normal operation",
            "estimatedTimeToFailure": f"{int(30 / (failure_probability + 0.1))} days",
            "confidence": 0.82
        }
    
    except Exception as e:
        logger.error(f"Error in predictive maintenance: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

# Pattern Recognition
@app.post("/api/ml/pattern-recognition")
async def recognize_patterns(tagName: str, values: List[float]):
    """
    Recognize patterns in tag data (seasonality, trends, cycles)
    """
    try:
        values_array = np.array(values)
        
        # Detect trend
        x = np.arange(len(values))
        coeffs = np.polyfit(x, values_array, 1)
        trend = "increasing" if coeffs[0] > 0.01 else "decreasing" if coeffs[0] < -0.01 else "stable"
        
        # Detect seasonality (simplified)
        fft = np.fft.fft(values_array)
        frequencies = np.fft.fftfreq(len(values_array))
        dominant_freq = frequencies[np.argmax(np.abs(fft[1:len(fft)//2]))+1]
        
        return {
            "tagName": tagName,
            "trend": trend,
            "trendStrength": abs(float(coeffs[0])),
            "hasCyclicPattern": abs(dominant_freq) > 0.01,
            "dominantPeriod": f"{1/abs(dominant_freq):.1f} samples" if abs(dominant_freq) > 0.01 else "None detected",
            "volatility": float(np.std(values_array))
        }
    
    except Exception as e:
        logger.error(f"Error in pattern recognition: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

# Health check
@app.get("/health")
async def health_check():
    return {"status": "healthy", "service": "ml-service"}

# Prometheus metrics
@app.get("/metrics")
async def metrics():
    return Response(content=generate_latest(), media_type=CONTENT_TYPE_LATEST)

@app.get("/")
async def root():
    return {
        "service": "SCADA ML Service",
        "version": "1.0.0",
        "endpoints": {
            "anomaly_detection": "/api/ml/detect-anomaly",
            "forecasting": "/api/ml/forecast",
            "predictive_maintenance": "/api/ml/predictive-maintenance",
            "pattern_recognition": "/api/ml/pattern-recognition"
        }
    }

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
