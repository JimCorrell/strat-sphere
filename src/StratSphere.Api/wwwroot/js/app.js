// Main Application Module
const App = {
    currentLeague: null,
    currentView: 'home',
    
    // Initialize the app
    init() {
        Auth.init();
        this.renderNav();
        this.renderAuthNav();
        this.navigate('home');
    },
    
    // Render navigation links
    renderNav() {
        const navLinks = document.getElementById('nav-links');
        
        if (!Auth.isLoggedIn()) {
            navLinks.innerHTML = '';
            return;
        }
        
        navLinks.innerHTML = `
            <li class="nav-item">
                <a class="nav-link" href="#" data-nav="home">
                    <i class="bi bi-house me-1"></i>My Leagues
                </a>
            </li>
        `;
        
        // Add admin link if admin
        if (Auth.isAdmin) {
            navLinks.innerHTML += `
                <li class="nav-item">
                    <a class="nav-link text-warning" href="#" data-nav="admin">
                        <i class="bi bi-shield-check me-1"></i>Admin
                    </a>
                </li>
            `;
        }
        
        // Add event listeners
        navLinks.querySelectorAll('[data-nav]').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                this.navigate(link.dataset.nav);
            });
        });
    },
    
    // Render auth section of nav
    renderAuthNav() {
        const authNav = document.getElementById('auth-nav');
        
        if (Auth.isLoggedIn()) {
            authNav.innerHTML = `
                <div class="dropdown">
                    <button class="btn btn-outline-light dropdown-toggle d-flex align-items-center gap-2" 
                            type="button" data-bs-toggle="dropdown">
                        <span class="user-avatar">${Auth.getInitials()}</span>
                        <span class="d-none d-md-inline">${Auth.getDisplayName()}</span>
                        ${Auth.isAdmin ? '<span class="badge bg-warning text-dark">Admin</span>' : ''}
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end">
                        <li><span class="dropdown-item-text text-muted">${Auth.user.email}</span></li>
                        <li><hr class="dropdown-divider"></li>
                        <li><a class="dropdown-item" href="#" id="logout-btn">
                            <i class="bi bi-box-arrow-right me-2"></i>Sign Out
                        </a></li>
                    </ul>
                </div>
            `;
            
            document.getElementById('logout-btn').addEventListener('click', (e) => {
                e.preventDefault();
                Auth.logout();
            });
        } else {
            authNav.innerHTML = `
                <button class="btn btn-outline-light me-2" data-bs-toggle="modal" data-bs-target="#loginModal">
                    Sign In
                </button>
                <button class="btn btn-warning" data-bs-toggle="modal" data-bs-target="#registerModal">
                    Create Account
                </button>
            `;
        }
    },
    
    // Navigate to a view
    navigate(view, params = {}) {
        this.currentView = view;
        
        switch (view) {
            case 'home':
                this.renderHome();
                break;
            case 'league':
                this.renderLeague(params.id);
                break;
            case 'team':
                this.renderTeam(params.leagueId, params.teamId);
                break;
            case 'draft':
                this.renderDraft(params.leagueId, params.draftId);
                break;
            case 'admin':
                if (Auth.isAdmin) {
                    this.renderAdmin();
                } else {
                    this.navigate('home');
                }
                break;
            default:
                this.renderHome();
        }
    },
    
    // Render home view
    async renderHome() {
        const app = document.getElementById('app');
        
        if (!Auth.isLoggedIn()) {
            app.innerHTML = this.renderLanding();
            return;
        }
        
        app.innerHTML = this.renderLoading();
        
        try {
            const leagues = await API.auth.myLeagues();
            app.innerHTML = this.renderLeaguesList(leagues);
            this.bindLeagueEvents();
        } catch (error) {
            app.innerHTML = this.renderError(error.message);
        }
    },
    
    // Render landing page for non-authenticated users
    renderLanding() {
        return `
            <div class="hero text-center">
                <h1><i class="bi bi-globe me-3"></i>Welcome to StratSphere</h1>
                <p class="lead">The ultimate platform for Strat-o-matic baseball league management</p>
                <div class="d-flex gap-3 justify-content-center">
                    <button class="btn btn-warning btn-lg" data-bs-toggle="modal" data-bs-target="#registerModal">
                        Get Started
                    </button>
                    <button class="btn btn-outline-light btn-lg" data-bs-toggle="modal" data-bs-target="#loginModal">
                        Sign In
                    </button>
                </div>
            </div>
            
            <div class="row g-4 mt-4">
                <div class="col-md-4">
                    <div class="card h-100">
                        <div class="card-body text-center p-4">
                            <i class="bi bi-trophy text-warning" style="font-size: 3rem;"></i>
                            <h5 class="mt-3">League Management</h5>
                            <p class="text-muted">Track standings, rosters, and stats for your Strat-o-matic league</p>
                        </div>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="card h-100">
                        <div class="card-body text-center p-4">
                            <i class="bi bi-lightning text-primary" style="font-size: 3rem;"></i>
                            <h5 class="mt-3">Live Drafts</h5>
                            <p class="text-muted">Run real-time drafts with pick timers and instant updates</p>
                        </div>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="card h-100">
                        <div class="card-body text-center p-4">
                            <i class="bi bi-search text-success" style="font-size: 3rem;"></i>
                            <h5 class="mt-3">Prospect Scouting</h5>
                            <p class="text-muted">Scout minor leaguers with 20-80 scale ratings</p>
                        </div>
                    </div>
                </div>
            </div>
        `;
    },
    
    // Render leagues list
    renderLeaguesList(leagues) {
        const adminBadge = Auth.isAdmin ? 
            '<span class="badge bg-warning text-dark ms-2">Viewing All Leagues</span>' : '';
        
        let html = `
            <div class="d-flex justify-content-between align-items-center mb-4">
                <h2>My Leagues ${adminBadge}</h2>
                <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#createLeagueModal">
                    <i class="bi bi-plus-lg me-2"></i>Create League
                </button>
            </div>
        `;
        
        if (leagues.length === 0) {
            html += `
                <div class="empty-state">
                    <i class="bi bi-collection"></i>
                    <h4>No Leagues Yet</h4>
                    <p>Create your first league to get started!</p>
                    <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#createLeagueModal">
                        Create League
                    </button>
                </div>
            `;
        } else {
            html += '<div class="row g-4">';
            leagues.forEach(league => {
                html += `
                    <div class="col-md-6 col-lg-4">
                        <div class="card league-card" data-league-id="${league.id}">
                            <div class="card-body">
                                <div class="d-flex justify-content-between align-items-start">
                                    <div>
                                        <div class="league-name">${this.escapeHtml(league.name)}</div>
                                        <div class="league-meta">
                                            <span class="badge badge-${league.status.toLowerCase()}">${league.status}</span>
                                        </div>
                                    </div>
                                    <div class="text-end">
                                        <div class="fs-4 fw-bold text-primary">${league.teamCount}</div>
                                        <div class="text-muted small">teams</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
            });
            html += '</div>';
        }
        
        return html;
    },
    
    // Bind league card click events
    bindLeagueEvents() {
        document.querySelectorAll('.league-card').forEach(card => {
            card.addEventListener('click', () => {
                this.navigate('league', { id: card.dataset.leagueId });
            });
        });
        
        // Create league form
        const createBtn = document.getElementById('create-league-submit');
        if (createBtn) {
            createBtn.onclick = async () => {
                await this.createLeague();
            };
        }
    },
    
    // Create a new league
    async createLeague() {
        const name = document.getElementById('league-name').value;
        const description = document.getElementById('league-description').value;
        const maxTeams = parseInt(document.getElementById('league-max-teams').value);
        const rosterSize = parseInt(document.getElementById('league-roster-size').value);
        const useDH = document.getElementById('league-use-dh').checked;
        const errorDiv = document.getElementById('create-league-error');
        
        try {
            errorDiv.classList.add('d-none');
            await API.leagues.create({
                name,
                description,
                maxTeams,
                rosterSize,
                activeRosterSize: 25,
                useDH
            }, Auth.getUserId());
            
            bootstrap.Modal.getInstance(document.getElementById('createLeagueModal')).hide();
            document.getElementById('create-league-form').reset();
            this.showToast('League created successfully!', 'success');
            this.navigate('home');
        } catch (error) {
            errorDiv.textContent = error.message;
            errorDiv.classList.remove('d-none');
        }
    },
    
    // Render league detail view
    async renderLeague(leagueId) {
        const app = document.getElementById('app');
        app.innerHTML = this.renderLoading();
        
        try {
            const [league, teams, drafts] = await Promise.all([
                API.leagues.get(leagueId),
                API.teams.list(leagueId),
                API.drafts.list(leagueId)
            ]);
            
            this.currentLeague = league;
            
            app.innerHTML = `
                <nav aria-label="breadcrumb" class="mb-4">
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item"><a href="#" data-nav="home">My Leagues</a></li>
                        <li class="breadcrumb-item active">${this.escapeHtml(league.name)}</li>
                    </ol>
                </nav>
                
                <div class="d-flex justify-content-between align-items-start mb-4">
                    <div>
                        <h2>${this.escapeHtml(league.name)}</h2>
                        <p class="text-muted mb-0">${this.escapeHtml(league.description || '')}</p>
                        <span class="badge badge-${league.status.toLowerCase()} mt-2">${league.status}</span>
                    </div>
                    <div class="text-end">
                        <div class="text-muted small">Season ${league.currentSeason}</div>
                        <div class="text-muted small">${league.currentPhase}</div>
                    </div>
                </div>
                
                <ul class="nav nav-tabs mb-4" id="leagueTabs" role="tablist">
                    <li class="nav-item">
                        <button class="nav-link active" data-bs-toggle="tab" data-bs-target="#teams-tab">
                            <i class="bi bi-people me-1"></i>Teams (${teams.length})
                        </button>
                    </li>
                    <li class="nav-item">
                        <button class="nav-link" data-bs-toggle="tab" data-bs-target="#drafts-tab">
                            <i class="bi bi-list-ol me-1"></i>Drafts (${drafts.length})
                        </button>
                    </li>
                    <li class="nav-item">
                        <button class="nav-link" data-bs-toggle="tab" data-bs-target="#settings-tab">
                            <i class="bi bi-gear me-1"></i>Settings
                        </button>
                    </li>
                </ul>
                
                <div class="tab-content">
                    <div class="tab-pane fade show active" id="teams-tab">
                        ${this.renderTeamsTab(league, teams)}
                    </div>
                    <div class="tab-pane fade" id="drafts-tab">
                        ${this.renderDraftsTab(league, drafts)}
                    </div>
                    <div class="tab-pane fade" id="settings-tab">
                        ${this.renderSettingsTab(league)}
                    </div>
                </div>
            `;
            
            this.bindLeagueDetailEvents(leagueId);
        } catch (error) {
            app.innerHTML = this.renderError(error.message);
        }
    },
    
    // Render teams tab
    renderTeamsTab(league, teams) {
        let html = `
            <div class="d-flex justify-content-between align-items-center mb-3">
                <span class="text-muted">${teams.length} of ${league.maxTeams} teams</span>
                <button class="btn btn-primary btn-sm" id="add-team-btn">
                    <i class="bi bi-plus me-1"></i>Add Team
                </button>
            </div>
        `;
        
        if (teams.length === 0) {
            html += `
                <div class="empty-state">
                    <i class="bi bi-people"></i>
                    <h5>No Teams Yet</h5>
                    <p>Add teams to your league to get started</p>
                </div>
            `;
        } else {
            html += '<div class="row g-3">';
            teams.forEach(team => {
                html += `
                    <div class="col-md-6 col-lg-4">
                        <div class="card team-card">
                            <div class="card-body">
                                <div class="d-flex justify-content-between align-items-start">
                                    <div>
                                        <span class="team-abbr">${this.escapeHtml(team.abbreviation)}</span>
                                        <h6 class="mb-1">${this.escapeHtml(team.name)}</h6>
                                        <small class="text-muted">${this.escapeHtml(team.city || '')}</small>
                                    </div>
                                    <div class="text-end">
                                        <div class="small text-muted">${team.rosterCount} players</div>
                                    </div>
                                </div>
                                <div class="mt-2 small text-muted">
                                    <i class="bi bi-person me-1"></i>${this.escapeHtml(team.ownerName)}
                                </div>
                            </div>
                        </div>
                    </div>
                `;
            });
            html += '</div>';
        }
        
        return html;
    },
    
    // Render drafts tab
    renderDraftsTab(league, drafts) {
        let html = `
            <div class="d-flex justify-content-end mb-3">
                <button class="btn btn-primary btn-sm" id="create-draft-btn">
                    <i class="bi bi-plus me-1"></i>Create Draft
                </button>
            </div>
        `;
        
        if (drafts.length === 0) {
            html += `
                <div class="empty-state">
                    <i class="bi bi-list-ol"></i>
                    <h5>No Drafts Yet</h5>
                    <p>Create a draft when you're ready</p>
                </div>
            `;
        } else {
            html += '<div class="list-group">';
            drafts.forEach(draft => {
                const statusClass = draft.status === 'InProgress' ? 'list-group-item-warning' : '';
                html += `
                    <a href="#" class="list-group-item list-group-item-action ${statusClass}" 
                       data-draft-id="${draft.id}">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <h6 class="mb-1">${this.escapeHtml(draft.name)}</h6>
                                <small class="text-muted">
                                    ${draft.totalRounds} rounds • ${draft.mode}
                                </small>
                            </div>
                            <span class="badge bg-${this.getDraftStatusColor(draft.status)}">
                                ${draft.status}
                            </span>
                        </div>
                    </a>
                `;
            });
            html += '</div>';
        }
        
        return html;
    },
    
    // Render settings tab
    renderSettingsTab(league) {
        return `
            <div class="card">
                <div class="card-body">
                    <h6 class="card-title">League Settings</h6>
                    <div class="row">
                        <div class="col-md-6">
                            <table class="table table-sm">
                                <tr>
                                    <th>Max Teams</th>
                                    <td>${league.maxTeams}</td>
                                </tr>
                                <tr>
                                    <th>Roster Size</th>
                                    <td>${league.rosterSize}</td>
                                </tr>
                                <tr>
                                    <th>Active Roster</th>
                                    <td>${league.activeRosterSize}</td>
                                </tr>
                                <tr>
                                    <th>Designated Hitter</th>
                                    <td>${league.useDH ? 'Yes' : 'No'}</td>
                                </tr>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
        `;
    },
    
    // Bind events for league detail page
    bindLeagueDetailEvents(leagueId) {
        // Breadcrumb navigation
        document.querySelectorAll('[data-nav]').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                this.navigate(link.dataset.nav);
            });
        });
        
        // Draft links
        document.querySelectorAll('[data-draft-id]').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                this.navigate('draft', { leagueId, draftId: link.dataset.draftId });
            });
        });
    },
    
    // Render admin view
    async renderAdmin() {
        const app = document.getElementById('app');
        app.innerHTML = this.renderLoading();
        
        try {
            const leagues = await API.leagues.list();
            
            app.innerHTML = `
                <h2><i class="bi bi-shield-check me-2 text-warning"></i>Admin Dashboard</h2>
                <p class="text-muted mb-4">Manage all leagues in the system</p>
                
                <div class="card">
                    <div class="card-header">
                        All Leagues (${leagues.length})
                    </div>
                    <div class="card-body p-0">
                        <table class="table table-hover mb-0">
                            <thead>
                                <tr>
                                    <th>Name</th>
                                    <th>Slug</th>
                                    <th>Status</th>
                                    <th>Teams</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                ${leagues.map(l => `
                                    <tr>
                                        <td><strong>${this.escapeHtml(l.name)}</strong></td>
                                        <td><code>${l.slug}</code></td>
                                        <td><span class="badge badge-${l.status.toLowerCase()}">${l.status}</span></td>
                                        <td>${l.teamCount}</td>
                                        <td>
                                            <button class="btn btn-sm btn-outline-primary" 
                                                    onclick="App.navigate('league', { id: '${l.id}' })">
                                                View
                                            </button>
                                        </td>
                                    </tr>
                                `).join('')}
                            </tbody>
                        </table>
                    </div>
                </div>
            `;
        } catch (error) {
            app.innerHTML = this.renderError(error.message);
        }
    },
    
    // Helper: Get draft status color
    getDraftStatusColor(status) {
        const colors = {
            'Scheduled': 'secondary',
            'InProgress': 'warning',
            'Paused': 'info',
            'Completed': 'success',
            'Cancelled': 'danger'
        };
        return colors[status] || 'secondary';
    },
    
    // Helper: Render loading state
    renderLoading() {
        return `
            <div class="loading">
                <div class="spinner-border" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
            </div>
        `;
    },
    
    // Helper: Render error state
    renderError(message) {
        return `
            <div class="alert alert-danger">
                <i class="bi bi-exclamation-triangle me-2"></i>
                ${this.escapeHtml(message)}
            </div>
        `;
    },
    
    // Helper: Escape HTML
    escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },
    
    // Show toast notification
    showToast(message, type = 'info') {
        const container = document.getElementById('toast-container');
        const id = 'toast-' + Date.now();
        
        const bgClass = {
            'success': 'bg-success',
            'error': 'bg-danger',
            'warning': 'bg-warning',
            'info': 'bg-primary'
        }[type] || 'bg-primary';
        
        const html = `
            <div id="${id}" class="toast ${bgClass} text-white" role="alert">
                <div class="d-flex">
                    <div class="toast-body">${this.escapeHtml(message)}</div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `;
        
        container.insertAdjacentHTML('beforeend', html);
        const toast = new bootstrap.Toast(document.getElementById(id));
        toast.show();
        
        // Remove from DOM after hidden
        document.getElementById(id).addEventListener('hidden.bs.toast', function() {
            this.remove();
        });
    }
};

// Initialize app on load
document.addEventListener('DOMContentLoaded', () => {
    App.init();
});
