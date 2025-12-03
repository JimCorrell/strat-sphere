// Authentication Module
const Auth = {
    user: null,
    isAdmin: false,
    
    // Initialize auth state from storage
    init() {
        const token = localStorage.getItem('token');
        const userData = localStorage.getItem('user');
        
        if (token && userData) {
            try {
                this.user = JSON.parse(userData);
                this.isAdmin = this.checkAdminFromToken(token);
                return true;
            } catch (e) {
                this.logout();
                return false;
            }
        }
        return false;
    },
    
    // Check if admin from JWT claims
    checkAdminFromToken(token) {
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            const roles = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
            return roles === 'Admin' || (Array.isArray(roles) && roles.includes('Admin'));
        } catch {
            return false;
        }
    },
    
    // Check if logged in
    isLoggedIn() {
        return !!this.user && !!localStorage.getItem('token');
    },
    
    // Login
    async login(emailOrUsername, password) {
        const response = await API.auth.login(emailOrUsername, password);
        
        localStorage.setItem('token', response.token);
        localStorage.setItem('user', JSON.stringify(response.user));
        
        this.user = response.user;
        this.isAdmin = this.checkAdminFromToken(response.token);
        
        return response;
    },
    
    // Register
    async register(email, username, password, displayName) {
        const response = await API.auth.register(email, username, password, displayName);
        
        localStorage.setItem('token', response.token);
        localStorage.setItem('user', JSON.stringify(response.user));
        
        this.user = response.user;
        this.isAdmin = false;
        
        return response;
    },
    
    // Logout
    logout() {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        this.user = null;
        this.isAdmin = false;
        
        // Reload to reset state
        window.location.reload();
    },
    
    // Get current user
    getUser() {
        return this.user;
    },
    
    // Get user ID
    getUserId() {
        return this.user?.id;
    },
    
    // Get display name
    getDisplayName() {
        return this.user?.displayName || this.user?.username || 'User';
    },
    
    // Get initials for avatar
    getInitials() {
        const name = this.getDisplayName();
        return name.split(' ')
            .map(part => part[0])
            .join('')
            .toUpperCase()
            .slice(0, 2);
    }
};

// Modal handlers
document.addEventListener('DOMContentLoaded', () => {
    const loginModal = new bootstrap.Modal(document.getElementById('loginModal'));
    const registerModal = new bootstrap.Modal(document.getElementById('registerModal'));
    
    // Switch between modals
    document.getElementById('show-register').addEventListener('click', () => {
        loginModal.hide();
        registerModal.show();
    });
    
    document.getElementById('show-login').addEventListener('click', () => {
        registerModal.hide();
        loginModal.show();
    });
    
    // Login form submit
    document.getElementById('login-submit').addEventListener('click', async () => {
        const email = document.getElementById('login-email').value;
        const password = document.getElementById('login-password').value;
        const errorDiv = document.getElementById('login-error');
        
        try {
            errorDiv.classList.add('d-none');
            await Auth.login(email, password);
            loginModal.hide();
            App.init(); // Reload app
            App.showToast('Welcome back!', 'success');
        } catch (error) {
            errorDiv.textContent = error.message;
            errorDiv.classList.remove('d-none');
        }
    });
    
    // Register form submit
    document.getElementById('register-submit').addEventListener('click', async () => {
        const email = document.getElementById('register-email').value;
        const username = document.getElementById('register-username').value;
        const displayName = document.getElementById('register-displayname').value;
        const password = document.getElementById('register-password').value;
        const confirm = document.getElementById('register-confirm').value;
        const errorDiv = document.getElementById('register-error');
        
        // Validate passwords match
        if (password !== confirm) {
            errorDiv.textContent = 'Passwords do not match';
            errorDiv.classList.remove('d-none');
            return;
        }
        
        try {
            errorDiv.classList.add('d-none');
            await Auth.register(email, username, password, displayName);
            registerModal.hide();
            App.init(); // Reload app
            App.showToast('Account created! Welcome to StratSphere.', 'success');
        } catch (error) {
            errorDiv.textContent = error.message;
            errorDiv.classList.remove('d-none');
        }
    });
    
    // Allow Enter key to submit
    document.getElementById('login-form').addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            document.getElementById('login-submit').click();
        }
    });
    
    document.getElementById('register-form').addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            document.getElementById('register-submit').click();
        }
    });
});
