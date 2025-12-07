import { Activity } from 'lucide-react'

interface TagValueCardProps {
    tag: any
}

export default function TagValueCard({ tag }: TagValueCardProps) {
    // Simulate live value
    const value = (Math.random() * 100).toFixed(2)

    return (
        <div className="card glass hover:border-primary-500 transition-colors cursor-pointer">
            <div className="flex items-start justify-between mb-2">
                <p className="tag-label text-xs">{tag.name}</p>
                <Activity className="w-4 h-4 text-scada-good animate-pulse" />
            </div>

            <div className="flex items-baseline gap-2">
                <span className="text-xl font-bold text-primary-400">{value}</span>
                <span className="text-sm text-gray-400">{tag.units || ''}</span>
            </div>

            <p className="text-xs text-gray-500 mt-1">{tag.description || tag.site}</p>
        </div>
    )
}
