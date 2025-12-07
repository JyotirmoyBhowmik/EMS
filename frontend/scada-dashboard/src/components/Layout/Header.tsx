import { Menu, User, LogOut } from 'lucide-react'

interface HeaderProps {
    toggleSidebar: () => void
}

export default function Header({ toggleSidebar }: HeaderProps) {
    return (
        <header className="bg-scada-surface border-b border-scada-border px-6 py-4">
            <div className="flex items-center justify-between">
                {/* Left: Menu toggle */}
                <button
                    onClick={toggleSidebar}
                    className="p-2 hover:bg-scada-background rounded-lg transition-colors"
                >
                    <Menu className="w-6 h-6" />
                </button>

                {/* Center: System status */}
                <div className="flex items-center gap-2">
                    <div className="w-2 h-2 bg-scada-good rounded-full animate-pulse"></div>
                    <span className="text-sm text-gray-400">System Online</span>
                </div>

                {/* Right: User menu */}
                <div className="flex items-center gap-4">
                    <div className="text-right">
                        <p className="text-sm font-medium">Administrator</p>
                        <p className="text-xs text-gray-400">admin@scada.local</p>
                    </div>
                    <button className="p-2 hover:bg-scada-background rounded-lg transition-colors">
                        <User className="w-6 h-6" />
                    </button>
                    <button className="p-2 hover:bg-scada-background rounded-lg transition-colors text-scada-alarm">
                        <LogOut className="w-6 h-6" />
                    </button>
                </div>
            </div>
        </header>
    )
}
