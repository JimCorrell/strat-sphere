// API Helper Module
const API = {
    baseUrl: '',  // Same origin, no prefix needed
    
    // Get auth token from storage
    getToken() {
        return localStorage.getItem('token');
    },
    
    // Set auth header if token exists
    getHeaders(includeAuth = true) {
        const headers = {
            'Content-Type': 'application/json'
        };
        
        if (includeAuth) {
            const token = this.getToken();
            if (token) {
                headers['Authorization'] = `Bearer ${token}`;
            }
        }
        
        return headers;
    },
    
    // Generic fetch wrapper
    async request(endpoint, options = {}) {
        const url = `${this.baseUrl}${endpoint}`;
        const config = {
            headers: this.getHeaders(options.auth !== false),
            ...options
        };
        
        if (config.body && typeof config.body === 'object') {
            config.body = JSON.stringify(config.body);
        }
        
        try {
            const response = await fetch(url, config);
            
            // Handle 401 Unauthorized
            if (response.status === 401) {
                Auth.logout();
                throw new Error('Session expired. Please log in again.');
            }
            
            // Handle no content
            if (response.status === 204) {
                return null;
            }
            
            const data = await response.json();
            
            if (!response.ok) {
                throw new Error(data.message || `Request failed: ${response.status}`);
            }
            
            return data;
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    },
    
    // Convenience methods
    get(endpoint, options = {}) {
        return this.request(endpoint, { method: 'GET', ...options });
    },
    
    post(endpoint, data, options = {}) {
        return this.request(endpoint, { method: 'POST', body: data, ...options });
    },
    
    put(endpoint, data, options = {}) {
        return this.request(endpoint, { method: 'PUT', body: data, ...options });
    },
    
    delete(endpoint, options = {}) {
        return this.request(endpoint, { method: 'DELETE', ...options });
    },
    
    // Auth endpoints
    auth: {
        login(emailOrUsername, password) {
            return API.post('/api/auth/login', { emailOrUsername, password }, { auth: false });
        },
        
        register(email, username, password, displayName) {
            return API.post('/api/auth/register', { email, username, password, displayName }, { auth: false });
        },
        
        me() {
            return API.get('/api/auth/me');
        },
        
        myLeagues() {
            return API.get('/api/auth/me/leagues');
        }
    },
    
    // League endpoints
    leagues: {
        list(userId = null) {
            const query = userId ? `?userId=${userId}` : '';
            return API.get(`/api/leagues${query}`);
        },
        
        get(id) {
            return API.get(`/api/leagues/${id}`);
        },
        
        create(data, creatorUserId) {
            return API.post(`/api/leagues?creatorUserId=${creatorUserId}`, data);
        },
        
        update(id, data) {
            return API.put(`/api/leagues/${id}`, data);
        },
        
        getMembers(leagueId) {
            return API.get(`/api/leagues/${leagueId}/members`);
        },
        
        addMember(leagueId, userId, role = 'Member') {
            return API.post(`/api/leagues/${leagueId}/members`, { userId, role });
        }
    },
    
    // Team endpoints
    teams: {
        list(leagueId) {
            return API.get(`/api/leagues/${leagueId}/teams`);
        },
        
        get(leagueId, teamId) {
            return API.get(`/api/leagues/${leagueId}/teams/${teamId}`);
        },
        
        create(leagueId, data, ownerId) {
            return API.post(`/api/leagues/${leagueId}/teams?ownerId=${ownerId}`, data);
        },
        
        update(leagueId, teamId, data) {
            return API.put(`/api/leagues/${leagueId}/teams/${teamId}`, data);
        },
        
        delete(leagueId, teamId) {
            return API.delete(`/api/leagues/${leagueId}/teams/${teamId}`);
        },
        
        getRoster(leagueId, teamId) {
            return API.get(`/api/leagues/${leagueId}/teams/${teamId}/roster`);
        }
    },
    
    // Draft endpoints
    drafts: {
        list(leagueId) {
            return API.get(`/api/leagues/${leagueId}/drafts`);
        },
        
        get(leagueId, draftId) {
            return API.get(`/api/leagues/${leagueId}/drafts/${draftId}`);
        },
        
        create(leagueId, data) {
            return API.post(`/api/leagues/${leagueId}/drafts`, data);
        },
        
        setOrder(leagueId, draftId, order) {
            return API.post(`/api/leagues/${leagueId}/drafts/${draftId}/order`, { order });
        },
        
        start(leagueId, draftId) {
            return API.post(`/api/leagues/${leagueId}/drafts/${draftId}/start`);
        },
        
        makePick(leagueId, draftId, playerId, teamId) {
            return API.post(`/api/leagues/${leagueId}/drafts/${draftId}/pick?teamId=${teamId}`, { playerId });
        },
        
        getPicks(leagueId, draftId) {
            return API.get(`/api/leagues/${leagueId}/drafts/${draftId}/picks`);
        }
    },
    
    // Player endpoints
    players: {
        search(params = {}) {
            const query = new URLSearchParams(params).toString();
            return API.get(`/api/players/search?${query}`);
        },
        
        get(playerId) {
            return API.get(`/api/players/${playerId}`);
        },
        
        getStats(playerId) {
            return API.get(`/api/players/${playerId}/stats`);
        }
    },
    
    // Scouting endpoints
    scouting: {
        getReports(leagueId, playerId) {
            return API.get(`/api/leagues/${leagueId}/scouting/player/${playerId}`);
        },
        
        create(leagueId, data, scoutUserId) {
            return API.post(`/api/leagues/${leagueId}/scouting?scoutUserId=${scoutUserId}`, data);
        }
    }
};
