const api = {
    dashboard: '/api/dashboard',
    patio: '/api/trucks/patio',
    trucks: '/api/trucks',
    movements: '/api/movements',
    nfe: '/api/nfe',
    cargo: '/api/cargo'
};

function $id(id) { return document.getElementById(id); }

// Toast notification
function showToast(message, type = 'success') {
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.textContent = message;
    document.body.appendChild(toast);
    setTimeout(() => {
        toast.remove();
    }, 3000);
}

// Toggle loading state
function setLoading(button, isLoading) {
    if (isLoading) {
        button.setAttribute('data-original-text', button.textContent);
        button.innerHTML = '<span class="spinner"></span> Carregando...';
        button.classList.add('loading');
        button.disabled = true;
    } else {
        const originalText = button.getAttribute('data-original-text');
        if (originalText) {
            button.textContent = originalText;
        }
        button.classList.remove('loading');
        button.disabled = false;
    }
}

// Adicionar spinner CSS se não existir
if (!document.querySelector('style#spinner-dash')) {
    const style = document.createElement('style');
    style.id = 'spinner-dash';
    style.textContent = `
        .spinner {
            display: inline-block;
            width: 16px;
            height: 16px;
            border: 2px solid rgba(0,0,0,0.3);
            border-radius: 50%;
            border-top-color: #000;
            animation: spin 1s ease-in-out infinite;
        }
        @keyframes spin {
            to { transform: rotate(360deg); }
        }
    `;
    document.head.appendChild(style);
}

async function loadOverview() {
    try {
        const res = await fetch(api.dashboard);
        if (!res.ok) throw new Error('Falha ao carregar dados do dashboard');
        const data = await res.json();
        const stats = document.getElementById('stats');
        stats.innerHTML = `
        <div class="stat">
          <div class="small">Total</div>
          <div class="n">${data.totalTrucks}</div>
        </div>
        <div class="stat">
          <div class="small">No Pátio</div>
          <div class="n">${data.inYard}</div>
        </div>
        <div class="stat">
          <div class="small">Carregando</div>
          <div class="n">${data.loading}</div>
        </div>
        <div class="stat">
          <div class="small">Em trânsito</div>
          <div class="n">${data.inTransit}</div>
        </div>
        <div class="stat">
          <div class="small">Manutenção</div>
          <div class="n">${data.maintenance}</div>
        </div>
      `;
    } catch (error) {
        showToast(error.message, 'error');
    }
}

async function loadTrucks() {
    try {
        const res = await fetch(api.patio);
        if (!res.ok) throw new Error('Falha ao carregar caminhões');
        const trucks = await res.json();
        const list = document.getElementById('truckList');
        list.innerHTML = '';
        if (!trucks.length) {
            list.innerHTML = `<div class="hint">Nenhum caminhão no pátio</div>`;
            return;
        }
        for (const t of trucks) {
            const el = document.createElement('div');
            el.className = 'truck-item';
            el.innerHTML = `
          <div>
            <div class="plate">${t.plate} <span class="badge">${t.driverName || '—'}</span></div>
            <div class="meta">${t.model} • ${t.status} • Últ: ${t.lastUpdate ? new Date(t.lastUpdate).toLocaleString() : '-'}</div>
          </div>
          <div class="mini-actions">
            <button class="btn small view" data-id="${t.id}">Ver</button>
          </div>
        `;
            list.appendChild(el);
        }
        // bind view buttons
        list.querySelectorAll('button.view').forEach(btn => {
            btn.addEventListener('click', () => selectTruck(btn.dataset.id));
        });
    } catch (error) {
        showToast(error.message, 'error');
    }
}

async function loadMovements() {
    try {
        const res = await fetch(api.movements);
        if (!res.ok) throw new Error('Falha ao carregar movimentos');
        const mv = await res.json();
        const container = document.getElementById('movements');
        const recent = mv.slice(-10).reverse();
        container.innerHTML = recent.map(m => `
        <div class="nfe-item">
          <div><b>${m.type}</b> — Truck #${m.truckId}</div>
          <div class="small">${new Date(m.timestamp).toLocaleString()}</div>
        </div>
      `).join('');
    } catch (error) {
        showToast(error.message, 'error');
    }
}

async function selectTruck(id) {
    try {
        const res = await fetch(`${api.trucks}/${id}`);
        if (!res.ok) throw new Error('Falha ao carregar detalhes do caminhão');
        const truck = await res.json();
        const d = document.getElementById('truckDetails');
        d.innerHTML = `
        <div class="plate">${truck.plate} <span class="small">#${truck.id}</span></div>
        <div class="meta">${truck.model} • ${truck.driverName}</div>
        <div class="small" style="margin-top:8px">Status: ${truck.status} • IsInYard: ${truck.isInYard}</div>

        <h4 style="margin-top:16px">Cargas</h4>
        <div id="cargos">${truck.cargos.length ? truck.cargos.map(c => `<div class="small">${c.description} — ${c.weightKg}kg</div>`).join('') : '<div class="hint">Nenhuma carga</div>'}</div>

        <h4 style="margin-top:16px">NFe / Documentos</h4>
        <div id="nfeArea" class="nfe-list"><div class="hint">carregando...</div></div>

        <div class="upload-row">
          <input type="file" id="nfeFile" accept=".pdf,.png,.jpg,.jpeg"/>
          <button id="btnUpload" class="btn small">Enviar NFe</button>
          <button id="btnEntry" class="btn small" style="background:var(--accent-2)">Registrar Entrada</button>
          <button id="btnExit" class="btn small" style="background:var(--danger)">Registrar Saída</button>
        </div>
      `;

        // carregar NFes
        await loadNFeList(id);

        // bind upload
        document.getElementById('btnUpload').addEventListener('click', async () => {
            const fileInput = document.getElementById('nfeFile');
            if (!fileInput.files.length) return showToast('Selecione um arquivo', 'error');
            const file = fileInput.files[0];
            const fd = new FormData();
            fd.append('file', file);
            const btn = document.getElementById('btnUpload');
            setLoading(btn, true);
            try {
                const r = await fetch(`${api.nfe}/upload/${id}`, { method: 'POST', body: fd });
                if (r.ok) {
                    showToast('NFe enviada com sucesso');
                    await loadNFeList(id);
                } else {
                    const txt = await r.text();
                    throw new Error(txt || 'Erro ao enviar NFe');
                }
            } catch (error) {
                showToast(error.message, 'error');
            } finally {
                setLoading(btn, false);
            }
        });

        document.getElementById('btnEntry').addEventListener('click', async () => {
            const btn = document.getElementById('btnEntry');
            setLoading(btn, true);
            try {
                const r = await fetch(api.movements + '/entry', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ truckId: parseInt(id), notes: 'Entrada via dashboard' })
                });
                if (r.ok) {
                    showToast('Entrada registrada com sucesso');
                    loadTrucks();
                    loadOverview();
                    loadMovements();
                } else {
                    throw new Error('Erro ao registrar entrada');
                }
            } catch (error) {
                showToast(error.message, 'error');
            } finally {
                setLoading(btn, false);
            }
        });

        document.getElementById('btnExit').addEventListener('click', async () => {
            if (!confirm('Registrar saída deste caminhão?')) return;
            const btn = document.getElementById('btnExit');
            setLoading(btn, true);
            try {
                const r = await fetch(api.movements + '/exit', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ truckId: parseInt(id), notes: 'Saída via dashboard' })
                });
                if (r.ok) {
                    showToast('Saída registrada com sucesso');
                    loadTrucks();
                    loadOverview();
                    loadMovements();
                } else {
                    throw new Error('Erro ao registrar saída');
                }
            } catch (error) {
                showToast(error.message, 'error');
            } finally {
                setLoading(btn, false);
            }
        });
    } catch (error) {
        showToast(error.message, 'error');
    }
}

async function loadNFeList(truckId) {
    try {
        const r = await fetch(`${api.nfe}/truck/${truckId}`);
        if (!r.ok) throw new Error('Falha ao carregar NFes');
        const list = await r.json();
        const container = document.getElementById('nfeArea');
        if (!list.length) {
            container.innerHTML = '<div class="hint">Nenhuma NFe enviada</div>';
            return;
        }
        container.innerHTML = list.map(n => `
        <div class="nfe-item">
          <div>${n.fileName}</div>
          <div>
            <a class="btn small" href="${n.downloadUrl}" target="_blank">Baixar</a>
            <button class="btn small" data-id="${n.id}" style="background:transparent;border:1px solid rgba(255,255,255,0.06)">Excluir</button>
          </div>
        </div>
      `).join('');
        // bind delete buttons
        container.querySelectorAll('button[data-id]').forEach(b => {
            b.addEventListener('click', async () => {
                const id = b.dataset.id;
                if (!confirm('Tem certeza que deseja excluir esta NFe?')) return;
                setLoading(b, true);
                try {
                    const r = await fetch(`${api.nfe}/${id}`, { method: 'DELETE' });
                    if (r.ok) {
                        showToast('NFe excluída com sucesso');
                        await loadNFeList(truckId);
                    } else {
                        throw new Error('Erro ao excluir NFe');
                    }
                } catch (error) {
                    showToast(error.message, 'error');
                } finally {
                    setLoading(b, false);
                }
            });
        });
    } catch (error) {
        showToast(error.message, 'error');
    }
}

// Event Listeners
document.getElementById('refreshBtn').addEventListener('click', async () => {
    const btn = document.getElementById('refreshBtn');
    setLoading(btn, true);
    await Promise.all([loadOverview(), loadTrucks(), loadMovements()]);
    setLoading(btn, false);
});

document.getElementById('backBtn').addEventListener('click', () => location.href = '/patio.html');

// Inicialização
(async function () {
    await loadOverview();
    await loadTrucks();
    await loadMovements();
})();