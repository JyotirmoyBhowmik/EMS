import { useEffect, useState } from 'react'
import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer } from 'recharts'
import { tagService } from '../../services/api'

interface RealTimeChartProps {
    tagName: string
    unit?: string
    color?: string
}

export default function RealTimeChart({ tagName, unit = '', color = '#3b82f6' }: RealTimeChartProps) {
    const [data, setData] = useState<any[]>([])

    useEffect(() => {
        // Simulate real-time data updates
        const interval = setInterval(() => {
            const newPoint = {
                time: new Date().toLocaleTimeString(),
                value: Math.random() * 100 + 50
            }

            setData(prev => [...prev.slice(-19), newPoint])
        }, 2000)

        return () => clearInterval(interval)
    }, [tagName])

    return (
        <ResponsiveContainer width="100%" height={250}>
            <LineChart data={data}>
                <XAxis
                    dataKey="time"
                    stroke="#64748b"
                    style={{ fontSize: '12px' }}
                />
                <YAxis
                    stroke="#64748b"
                    style={{ fontSize: '12px' }}
                />
                <Tooltip
                    contentStyle={{
                        backgroundColor: '#1e293b',
                        border: '1px solid #334155',
                        borderRadius: '8px'
                    }}
                />
                <Line
                    type="monotone"
                    dataKey="value"
                    stroke={color}
                    strokeWidth={2}
                    dot={false}
                    animationDuration={300}
                />
            </LineChart>
        </ResponsiveContainer>
    )
}
