import { Database, Server, Activity, Wifi } from 'lucide-react'

export default function SystemStatus() {
    const services = [
        { name: 'ScadaCore', status: 'online', icon: Server },
        { name: 'DataAcquisition', status: 'online', icon: Activity },
        { name: 'InfluxDB', status: 'online', icon: Database },
        { name: 'RabbitMQ', status: 'online', icon: Wifi },
    ]

    return (
        <div className="card">
            <h3 className="text-xl font-semibold mb-4">System Status</h3>

            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                {services.map((service) => (
                    <div key={service.name} className="flex items-center gap-3 p-3 bg-scada-background rounded-lg">
                        <service.icon className="w-5 h-5 text-scada-good" />
                        <div>
                            <p className="text-sm font-medium">{service.name}</p>
                            <p className="text-xs text-scada-good">Online</p>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    )
}
