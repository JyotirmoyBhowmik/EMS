import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
})

// Tag Service
export const tagService = {
    getTags: async (filters?: { site?: string, device?: string, isEnabled?: boolean }) => {
        const { data } = await api.get('/tags', { params: filters })
        return data
    },

    getTagById: async (id: number) => {
        const { data } = await api.get(`/tags/${id}`)
        return data
    },

    getTagByName: async (tagName: string) => {
        const { data } = await api.get(`/tags/by-name/${tagName}`)
        return data
    },

    createTag: async (tag: any) => {
        const { data } = await api.post('/tags', tag)
        return data
    },

    updateTag: async (id: number, tag: any) => {
        const { data } = await api.put(`/tags/${id}`, tag)
        return data
    },

    deleteTag: async (id: number) => {
        await api.delete(`/tags/${id}`)
    },

    searchTags: async (query: string) => {
        const { data } = await api.get('/tags/search', { params: { query } })
        return data
    },

    getTagCountBySite: async () => {
        const { data } = await api.get('/tags/stats/count-by-site')
        return data
    },

    getLatestValue: async (tagName: string) => {
        const { data } = await api.get(`/tags/${tagName}/value/latest`)
        return data
    },

    getHistory: async (tagName: string, start: Date, end: Date) => {
        const { data } = await api.get(`/tags/${tagName}/hist ory`, {
            params: { start: start.toISOString(), end: end.toISOString() }
        })
        return data
    },

    getAggregates: async (tagName: string, start: Date, end: Date, window: string = '1m') => {
        const { data } = await api.get(`/tags/${tagName}/aggregates`, {
            params: { start: start.toISOString(), end: end.toISOString(), window }
        })
        return data
    },
}

// WebSocket service for real-time updates
export const createWebSocketConnection = (onMessage: (data: any) => void) => {
    const wsUrl = import.meta.env.VITE_WS_URL || 'ws://localhost:5000'
    const ws = new WebSocket(wsUrl)

    ws.onopen = () => {
        console.log('WebSocket connected')
    }

    ws.onmessage = (event) => {
        const data = JSON.parse(event.data)
        onMessage(data)
    }

    ws.onerror = (error) => {
        console.error('WebSocket error:', error)
    }

    ws.onclose = () => {
        console.log('WebSocket disconnected')
        // Reconnect after 5 seconds
        setTimeout(() => createWebSocketConnection(onMessage), 5000)
    }

    return ws
}

export default api
