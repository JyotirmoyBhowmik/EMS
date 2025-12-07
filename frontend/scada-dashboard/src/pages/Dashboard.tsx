import { useEffect, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Activity, Zap, TrendingUp, AlertTriangle } from 'lucide-react'
import RealTimeChart from '../components/Dashboard/RealTimeChart'
import TagValueCard from '../components/Dashboard/TagValueCard'
import SystemStatus from '../components/Dashboard/SystemStatus'
import { tagService } from '../services/api'

export default function Dashboard() {
    const [liveData, setLiveData] = useState<any[]>([])

    // Fetch tags
    const { data: tags } = useQuery({
        queryKey: ['tags'],
        queryFn: () => tagService.getTags()
    })

    // Fetch tag stats
    const { data: stats } = useQuery({
        queryKey: ['tag-stats'],
        queryFn: () => tagService.getTagCountBySite(),
        refetchInterval: 10000,
    })

    // Mock active alarms (would be real in production)
    const activeAlarms = 3

    return (
        <div className="space-y-6">
            {/* Header */}
            <div>
                <h1 className="text-3xl font-bold gradient-text mb-2">
                    Enterprise SCADA Dashboard
                </h1>
                <p className="text-gray-400">Real-time monitoring and control system</p>
            </div>

            {/* KPI Cards */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                <div className="card glass">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="tag-label">Active Tags</p>
                            <p className="tag-value text-primary-400">{tags?.length || 0}</p>
                        </div>
                        <Activity className="w-12 h-12 text-primary-500" />
                    </div>
                </div>

                <div className="card glass">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="tag-label">Processing Rate</p>
                            <p className="tag-value text-scada-good">98.5k/s</p>
                        </div>
                        <Zap className="w-12 h-12 text-scada-good" />
                    </div>
                </div>

                <div className="card glass">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="tag-label">Active Sites</p>
                            <p className="tag-value text-purple-400">
                                {Object.keys(stats || {}).length}
                            </p>
                        </div>
                        <TrendingUp className="w-12 h-12 text-purple-400" />
                    </div>
                </div>

                <div className="card glass">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="tag-label">Active Alarms</p>
                            <p className={`tag-value ${activeAlarms > 0 ? 'status-alarm' : 'text-scada-good'}`}>
                                {activeAlarms}
                            </p>
                        </div>
                        <AlertTriangle className={`w-12 h-12 ${activeAlarms > 0 ? 'text-scada-alarm' : 'text-scada-good'}`} />
                    </div>
                </div>
            </div>

            {/* System Status */}
            <SystemStatus />

            {/* Real-time Charts */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <div className="card">
                    <h3 className="text-xl font-semibold mb-4">Wind Turbine Power Output</h3>
                    <RealTimeChart
                        tagName="SITE01.TURBINE01.PowerOutput"
                        unit="kW"
                        color="#10b981"
                    />
                </div>

                <div className="card">
                    <h3 className="text-xl font-semibold mb-4">Solar Panel Voltage</h3>
                    <RealTimeChart
                        tagName="SITE02.SOLAR01.Voltage"
                        unit="V"
                        color="#f59e0b"
                    />
                </div>
            </div>

            {/* Live Tag Values */}
            <div className="card">
                <h3 className="text-xl font-semibold mb-4">Live Tag Values</h3>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
                    {tags?.slice(0, 8).map((tag: any) => (
                        <TagValueCard key={tag.id} tag={tag} />
                    ))}
                </div>
            </div>
        </div>
    )
}
