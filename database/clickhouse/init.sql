-- ClickHouse Analytics Database Schema

CREATE DATABASE IF NOT EXISTS scada_analytics;

USE scada_analytics;

-- Tag analytics table (optimized for time-series analytics)
CREATE TABLE IF NOT EXISTS tag_analytics
(
    tag_name String,
    timestamp DateTime,
    value Float64,
    quality String,
    site String,
    device String,
    data_type String
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(timestamp)
ORDER BY (tag_name, timestamp)
TTL timestamp + INTERVAL 90 DAY;

-- Aggregated statistics (materialized view)
CREATE MATERIALIZED VIEW IF NOT EXISTS tag_hourly_stats
ENGINE = SummingMergeTree()
PARTITION BY toYYYYMM(timestamp)
ORDER BY (tag_name, timestamp)
AS SELECT
    tag_name,
    toStartOfHour(timestamp) as timestamp,
    avg(value) as avg_value,
    min(value) as min_value,
    max(value) as max_value,
    count() as sample_count
FROM tag_analytics
GROUP BY tag_name, toStartOfHour(timestamp);

-- Alarm events table
CREATE TABLE IF NOT EXISTS alarm_events
(
    alarm_id UInt64,
    tag_name String,
    alarm_type String,
    priority String,
    message String,
    triggered_at DateTime,
    acknowledged_at Nullable(DateTime),
    resolved_at Nullable(DateTime),
    acknowledged_by Nullable(String)
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(triggered_at)
ORDER BY (triggered_at, alarm_id);

-- Site-level KPIs
CREATE MATERIALIZED VIEW IF NOT EXISTS site_kpis
ENGINE = AggregatingMergeTree()
PARTITION BY toYYYYMM(timestamp)
ORDER BY (site, timestamp)
AS SELECT
    site,
    toStartOfDay(timestamp) as timestamp,
    uniqState(tag_name) as unique_tags,
    avgState(value) as avg_value,
    sumState(CASE WHEN quality = 'Good' THEN 1 ELSE 0 END) as good_quality_count,
    countState() as total_samples
FROM tag_analytics
GROUP BY site, toStartOfDay(timestamp);

-- Anomaly detection results
CREATE TABLE IF NOT EXISTS anomaly_detections
(
    tag_name String,
    timestamp DateTime,
    value Float64,
    anomaly_score Float64,
    is_anomaly UInt8,
    explanation String
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(timestamp)
ORDER BY (timestamp, tag_name);

-- Performance optimization: Create dictionaries for fast lookups
CREATE DICTIONARY IF NOT EXISTS tag_metadata
(
    tag_name String,
    site String,
    device String,
    data_type String,
    units String
)
PRIMARY KEY tag_name
SOURCE(HTTP(
    url 'http://scada-core:5000/api/tags/dictionary'
    format 'JSONEachRow'
))
LIFETIME(MIN 300 MAX 600)
LAYOUT(FLAT());
