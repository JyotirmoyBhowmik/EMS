import { NavLink } from 'react-router-dom'
import {
    LayoutDashboard,
    Tags,
    Bell,
    TrendingUp,
    FileText,
    Settings,
    Activity
} from 'lucide-react'

interface SidebarProps {
    isOpen: boolean
    setIsOpen: (open: boolean) => void
}

export default function Sidebar({ isOpen }: SidebarProps) {
    const navItems = [
        { to: '/', icon: LayoutDashboard, label: 'Dashboard' },
        { to: '/tags', icon: Tags, label: 'Tags' },
        { to: '/alarms', icon: Bell, label: 'Alarms' },
        { to: '/trends', icon: TrendingUp, label: 'Trends' },
        { to: '/reports', icon: FileText, label: 'Reports' },
        { to: '/settings', icon: Settings, label: 'Settings' },
    ]

    return (
        <aside
            className={`bg-scada-surface border-r border-scada-border transition-all duration-300 ${isOpen ? 'w-64' : 'w-20'
                }`}
        >
            <div className="h-full flex flex-col">
                {/* Logo/Brand */}
                <div className="p-6 border-b border-scada-border">
                    <div className="flex items-center gap-3">
                        <Activity className="w-8 h-8 text-primary-500" />
                        {isOpen && (
                            <div>
                                <h1 className="text-xl font-bold gradient-text">SCADA</h1>
                                <p className="text-xs text-gray-400">Enterprise</p>
                            </div>
                        )}
                    </div>
                </div>

                {/* Navigation */}
                <nav className="flex-1 p-4 space-y-2">
                    {navItems.map((item) => (
                        <NavLink
                            key={item.to}
                            to={item.to}
                            className={({ isActive }) =>
                                `flex items-center gap-3 px-4 py-3 rounded-lg transition-colors ${isActive
                                    ? 'bg-primary-600 text-white'
                                    : 'text-gray-300 hover:bg-scada-background'
                                }`
                            }
                        >
                            <item.icon className="w-5 h-5" />
                            {isOpen && <span className="font-medium">{item.label}</span>}
                        </NavLink>
                    ))}
                </nav>

                {/* Footer */}
                {isOpen && (
                    <div className="p-4 border-t border-scada-border">
                        <p className="text-xs text-gray-500">
                            © 2025 SCADA System
                        </p>
                        <p className="text-xs text-gray-600">
                            Jyotirmoy Bhowmik
                        </p>
                    </div>
                )}
            </div>
        </aside>
    )
}
