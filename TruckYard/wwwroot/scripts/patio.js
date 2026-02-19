const api = {
    trucks: '/api/trucks',
    patio: '/api/trucks/patio',
    movements: '/api/movements',
    cargo: '/api/cargo',
    dashboard: '/api/dashboard'
};

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
if (!document.querySelector('style#spinner')) {
    const style = document.createElement('style');
    style.id = 'spinner';
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

async function fetchYard() {
    const refreshBtn = document.getElementById('refresh');
    setLoading(refreshBtn, true);
    try {
        const res = await fetch(api.patio);
        if (!res.ok) throw new Error('Falha ao carregar caminhões');
        const trucks = await res.json();
        renderYard(trucks);
    } catch (error) {
        showToast(error.message, 'error');
        console.error(error);
    } finally {
        setLoading(refreshBtn, false);
    }
}

function renderYard(trucks) {
    const root = document.getElementById('yard-list');
    if (!trucks.length) {
        document.getElementById('yard-empty').style.display = 'block';
        root.innerHTML = '';
        return;
    }
    document.getElementById('yard-empty').style.display = 'none';
    root.innerHTML = trucks.map(t => `
    <div class="truck" data-id="${t.id}">
      <div>
        <div><b>${t.plate}</b> — ${t.model}</div>
        <div class="meta">Motorista: ${t.driverName} • Status: ${t.status}</div>
        <div class="small">Última atualização: ${t.lastUpdate ? new Date(t.lastUpdate).toLocaleString() : '-'}</div>
      </div>
      <div class="actions">
        <button onclick="selectTruck(${t.id})">Ver detalhes</button>
        <button onclick="setStatus(${t.id}, 'Loading')">Carregar</button>
        <button onclick="registerExit(${t.id})" style="background:#db5245">Sair</button>
      </div>
    </div>
  `).join('');
}

window.selectTruck = async function (id) {
    try {
        const res = await fetch(`${api.trucks}/${id}`);
        if (!res.ok) throw new Error('Caminhão não encontrado');
        const t = await res.json();
        document.getElementById('selected-info').innerHTML = `
        <div><b>${t.plate}</b> — ${t.model}</div>
        <div class="small">Motorista: ${t.driverName}</div>
        <div class="small">Status: ${t.status}</div>
        <div style="margin-top:12px">
          <button onclick="openEdit(${t.id})" class="btn">Editar</button>
          <button onclick="deleteTruck(${t.id})" style="background:#db5245">Remover</button>
        </div>
        <div style="margin-top:16px">
          <h4>Cargas</h4>
          <div id="cargo-list">${t.cargos.map(c => `<div class="small">${c.description} — ${c.weightKg}kg</div>`).join('') || '<div class="small">Nenhuma carga</div>'}</div>
          <hr/>
          <div style="margin-top:12px">
            <input id="newCargoDesc" placeholder="Descrição" class="small-input"/>
            <input id="newCargoWeight" placeholder="Peso kg" type="number" class="small-input"/>
            <button onclick="addCargo(${t.id})" class="btn">Adicionar carga</button>
          </div>
        </div>
      `;
        document.getElementById('truckIdInput').value = t.id;
    } catch (error) {
        showToast(error.message, 'error');
    }
};

window.openEdit = async function (id) {
    try {
        const res = await fetch(`${api.trucks}/${id}`);
        if (!res.ok) throw new Error('Falha ao carregar dados do caminhão');
        const t = await res.json();
        const html = `
        <div>
          <div class="form-row">
            <input id="editPlate" value="${t.plate}" placeholder="Placa"/>
            <input id="editModel" value="${t.model}" placeholder="Modelo"/>
          </div>
          <div class="form-row">
            <input id="editDriver" value="${t.driverName}" placeholder="Motorista"/>
            <select id="editStatus">
              <option ${t.status === 'Available' ? 'selected' : ''}>Available</option>
              <option ${t.status === 'InYard' ? 'selected' : ''}>InYard</option>
              <option ${t.status === 'Loading' ? 'selected' : ''}>Loading</option>
              <option ${t.status === 'Unloading' ? 'selected' : ''}>Unloading</option>
              <option ${t.status === 'InTransit' ? 'selected' : ''}>InTransit</option>
              <option ${t.status === 'Maintenance' ? 'selected' : ''}>Maintenance</option>
            </select>
          </div>
          <div style="margin-top:12px">
            <button onclick="saveEdit(${id})" class="btn">Salvar alterações</button>
          </div>
        </div>
      `;
        document.getElementById('selected-info').innerHTML = html;
    } catch (error) {
        showToast(error.message, 'error');
    }
};

window.saveEdit = async function (id) {
    const plate = document.getElementById('editPlate').value;
    const model = document.getElementById('editModel').value;
    const driver = document.getElementById('editDriver').value;
    const status = document.getElementById('editStatus').value;

    if (!plate || !model || !driver) {
        return showToast('Preencha todos os campos', 'error');
    }

    const payload = { plate, model, driverName: driver, status, isInYard: true };

    const btn = document.querySelector('#selected-info button');
    setLoading(btn, true);

    try {
        const res = await fetch(`/api/trucks/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (res.ok) {
            showToast('Caminhão atualizado com sucesso');
            fetchYard();
            // Recarrega os detalhes do caminhão
            await selectTruck(id);
        } else {
            const error = await res.text();
            throw new Error(error || 'Erro ao salvar');
        }
    } catch (error) {
        showToast(error.message, 'error');
    } finally {
        setLoading(btn, false);
    }
};

window.deleteTruck = async function (id) {
    if (!confirm('Tem certeza que deseja remover este caminhão? Esta ação não pode ser desfeita.')) return;
    try {
        const res = await fetch(`/api/trucks/${id}`, { method: 'DELETE' });
        if (res.ok) {
            showToast('Caminhão removido com sucesso');
            fetchYard();
            document.getElementById('selected-info').innerHTML = '<div class="small">Selecione um caminhão para ver detalhes.</div>';
        } else {
            throw new Error('Erro ao remover caminhão');
        }
    } catch (error) {
        showToast(error.message, 'error');
    }
};

window.setStatus = async function (id, status) {
    try {
        const r = await fetch(`/api/trucks/${id}`);
        if (!r.ok) throw new Error('Falha ao carregar caminhão');
        const t = await r.json();
        const payload = {
            plate: t.plate,
            model: t.model,
            driverName: t.driverName,
            status,
            isInYard: t.isInYard
        };
        const res = await fetch(`/api/trucks/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (res.ok) {
            showToast('Status atualizado');
            fetchYard();
        } else {
            throw new Error('Erro ao atualizar status');
        }
    } catch (error) {
        showToast(error.message, 'error');
    }
};

window.registerEntry = async function (id) {
    const btn = document.getElementById('btn-entry');
    setLoading(btn, true);
    try {
        const payload = { truckId: id, notes: 'Entrada registrada via UI' };
        const res = await fetch(api.movements + '/entry', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (res.ok) {
            showToast('Entrada registrada');
            fetchYard();
        } else {
            throw new Error('Erro ao registrar entrada');
        }
    } catch (error) {
        showToast(error.message, 'error');
    } finally {
        setLoading(btn, false);
    }
};

window.registerExit = async function (id) {
    if (!confirm('Registrar saída deste caminhão?')) return;
    const btn = document.getElementById('btn-exit');
    setLoading(btn, true);
    try {
        const payload = { truckId: id, notes: 'Saída registrada via UI' };
        const res = await fetch(api.movements + '/exit', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (res.ok) {
            showToast('Saída registrada');
            fetchYard();
        } else {
            throw new Error('Erro ao registrar saída');
        }
    } catch (error) {
        showToast(error.message, 'error');
    } finally {
        setLoading(btn, false);
    }
};

window.addCargo = async function (truckId) {
    const desc = document.getElementById('newCargoDesc').value;
    const weight = parseFloat(document.getElementById('newCargoWeight').value || 0);
    if (!desc) return showToast('Descrição vazia', 'error');
    if (weight <= 0) return showToast('Peso inválido', 'error');

    const btn = document.querySelector('#selected-info button[onclick^="addCargo"]');
    setLoading(btn, true);

    try {
        const payload = { description: desc, weightKg: weight, truckId };
        const res = await fetch(api.cargo, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (res.ok) {
            showToast('Carga adicionada');
            selectTruck(truckId);
            document.getElementById('newCargoDesc').value = '';
            document.getElementById('newCargoWeight').value = '';
            fetchYard();
        } else {
            throw new Error('Erro ao adicionar carga');
        }
    } catch (error) {
        showToast(error.message, 'error');
    } finally {
        setLoading(btn, false);
    }
};

// Event Listeners
document.getElementById('refresh').addEventListener('click', fetchYard);

document.getElementById('createTruck').addEventListener('click', async () => {
    const plate = document.getElementById('newPlate').value;
    const model = document.getElementById('newModel').value;
    const driver = document.getElementById('newDriver').value;
    const status = document.getElementById('newStatus').value;

    if (!plate) return showToast('Informe a placa', 'error');
    if (!model) return showToast('Informe o modelo', 'error');
    if (!driver) return showToast('Informe o motorista', 'error');

    const btn = document.getElementById('createTruck');
    setLoading(btn, true);

    const payload = { plate, model, driverName: driver, status, isInYard: status === 'InYard' };
    try {
        const res = await fetch('/api/trucks', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (res.ok) {
            showToast('Caminhão criado com sucesso');
            fetchYard();
            document.getElementById('newPlate').value = '';
            document.getElementById('newModel').value = '';
            document.getElementById('newDriver').value = '';
        } else {
            const error = await res.text();
            throw new Error(error || 'Erro ao criar caminhão');
        }
    } catch (error) {
        showToast(error.message, 'error');
    } finally {
        setLoading(btn, false);
    }
});

document.getElementById('btn-entry').addEventListener('click', () => {
    const id = parseInt(document.getElementById('truckIdInput').value || '0');
    if (!id) return showToast('Informe o ID do caminhão', 'error');
    registerEntry(id);
});

document.getElementById('btn-exit').addEventListener('click', () => {
    const id = parseInt(document.getElementById('truckIdInput').value || '0');
    if (!id) return showToast('Informe o ID do caminhão', 'error');
    registerExit(id);
});

document.getElementById('open-dashboard').addEventListener('click', () => location.href = '/dashboard.html');

// Inicialização
fetchYard();