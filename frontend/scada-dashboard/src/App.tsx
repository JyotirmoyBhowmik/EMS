import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import { useState } from 'react'
import Sidebar from './components/Layout/Sidebar'
import Header from './components/Layout/Header'
import Dashboard from './pages/Dashboard'
import TagsPage from './pages/TagsPage'
import AlarmsPage from './pages/AlarmsPage'
import TrendsPage from './pages/TrendsPage'
import ReportsPage from './pages/ReportsPage'
import SettingsPage from './pages/SettingsPage'

function App() {
    const [sidebarOpen, setSidebarOpen] = useState(true)

    return (
        <Router>
            <div className="flex h-screen bg-scada-background">
                <Sidebar isOpen={sidebarOpen} setIsOpen={setSidebarOpen} />

                <div className="flex-1 flex flex-col overflow-hidden">
                    <Header toggleSidebar={() => setSidebarOpen(!sidebarOpen)} />

                    <main className="flex-1 overflow-y-auto p-6">
                        <Routes>
                            <Route path="/" element={<Dashboard />} />
                            <Route path="/tags" element={<TagsPage />} />
                            <Route path="/alarms" element={<AlarmsPage />} />
                            <Route path="/trends" element={<TrendsPage />} />
                            <Route path="/reports" element={<ReportsPage />} />
                            <Route path="/settings" element={<SettingsPage />} />
                        </Routes>
                    </main>
                </div>
            </div>
        </Router>
    )
}

export default App
