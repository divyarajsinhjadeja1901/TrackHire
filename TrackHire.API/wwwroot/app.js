const API = '/api';
let token = localStorage.getItem('token');
let userName = localStorage.getItem('userName');

if (token) showApp();

// ─── AUTH ────────────────────────────────────────────

async function register() {
    clearErrors();
    const name = document.getElementById('name').value.trim();
    const email = document.getElementById('email').value.trim();
    const password = document.getElementById('password').value;

    if (!name || !email || !password) {
        showError('auth-error', 'Please fill all fields');
        return;
    }

    const res = await fetch(`${API}/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, email, password })
    });

    const data = await res.json();
    if (data.token) {
        saveSession(data.token, name);
        showApp();
    } else {
        showError('auth-error', data.message || 'Registration failed');
    }
}

async function login() {
    clearErrors();
    const email = document.getElementById('email').value.trim();
    const password = document.getElementById('password').value;

    if (!email || !password) {
        showError('auth-error', 'Please enter email and password');
        return;
    }

    const res = await fetch(`${API}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
    });

    const data = await res.json();
    if (data.token) {
        saveSession(data.token, email);
        showApp();
    } else {
        showError('auth-error', 'Invalid email or password');
    }
}

function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('userName');
    token = null;
    document.getElementById('auth-section').style.display = 'flex';
    document.getElementById('app-section').style.display = 'none';
}

function saveSession(t, name) {
    token = t;
    userName = name;
    localStorage.setItem('token', t);
    localStorage.setItem('userName', name);
}

function showApp() {
    document.getElementById('auth-section').style.display = 'none';
    document.getElementById('app-section').style.display = 'block';
    document.getElementById('user-name').textContent = userName || '';
    loadApplications();
}

// ─── API HELPER ──────────────────────────────────────

function authHeaders() {
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    };
}

// ─── APPLICATIONS ────────────────────────────────────

async function loadApplications() {
    const status = document.getElementById('statusFilter').value;
    const url = status ? `${API}/applications?status=${status}` : `${API}/applications`;

    const res = await fetch(url, { headers: authHeaders() });

    if (res.status === 401) { logout(); return; }

    const apps = await res.json();
    renderStats(apps);
    renderList(apps);
}

async function addApplication() {
    clearErrors();
    const company = document.getElementById('company').value.trim();
    const jobTitle = document.getElementById('jobTitle').value.trim();

    if (!company || !jobTitle) {
        showError('form-error', 'Company name and job title are required');
        return;
    }

    const body = {
        companyName: company,
        jobTitle: jobTitle,
        location: document.getElementById('location').value.trim(),
        jobUrl: document.getElementById('jobUrl').value.trim(),
        expectedSalary: document.getElementById('salary').value || null,
        interviewDate: document.getElementById('interviewDate').value || null
    };

    const res = await fetch(`${API}/applications`, {
        method: 'POST',
        headers: authHeaders(),
        body: JSON.stringify(body)
    });

    if (res.ok) {
        // Clear form
        ['company', 'jobTitle', 'location', 'jobUrl', 'salary', 'interviewDate']
            .forEach(id => document.getElementById(id).value = '');
        loadApplications();
    } else {
        showError('form-error', 'Failed to add application');
    }
}

async function updateStatus(id, newStatus) {
    await fetch(`${API}/applications/${id}/status`, {
        method: 'PATCH',
        headers: authHeaders(),
        body: JSON.stringify(newStatus)
    });
    loadApplications();
}

async function deleteApp(id) {
    if (!confirm('Delete this application?')) return;
    await fetch(`${API}/applications/${id}`, {
        method: 'DELETE',
        headers: authHeaders()
    });
    loadApplications();
}

// ─── RENDER ──────────────────────────────────────────

function renderStats(apps) {
    document.getElementById('stat-total').textContent = apps.length;
    document.getElementById('stat-applied').textContent =
        apps.filter(a => a.status === 'Applied').length;
    document.getElementById('stat-interview').textContent =
        apps.filter(a => a.status === 'Interview').length;
    document.getElementById('stat-offer').textContent =
        apps.filter(a => a.status === 'Offer').length;
    document.getElementById('stat-rejected').textContent =
        apps.filter(a => a.status === 'Rejected').length;
}

function renderList(apps) {
    const list = document.getElementById('applications-list');

    if (apps.length === 0) {
        list.innerHTML = '<p class="empty">No applications yet. Add your first one above!</p>';
        return;
    }

    list.innerHTML = apps.map(a => `
    <div class="app-card">
      <div class="app-info">
        <strong>${escapeHtml(a.companyName)}</strong>
        <span>${escapeHtml(a.jobTitle)} ${a.location ? '· ' + escapeHtml(a.location) : ''}</span>
        <span class="badge badge-${a.status.toLowerCase()}">${a.status}</span>
      </div>
      <div class="app-actions">
        <select onchange="updateStatus(${a.id}, this.value)">
          ${['Applied', 'Interview', 'Offer', 'Rejected', 'Withdrawn']
            .map(s => `<option ${s === a.status ? 'selected' : ''}>${s}</option>`)
            .join('')}
        </select>
        <button class="danger small" onclick="deleteApp(${a.id})">Delete</button>
      </div>
    </div>
  `).join('');
}

// ─── HELPERS ─────────────────────────────────────────

function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

function showError(id, msg) {
    document.getElementById(id).textContent = msg;
}

function clearErrors() {
    document.querySelectorAll('.error').forEach(el => el.textContent = '');
}