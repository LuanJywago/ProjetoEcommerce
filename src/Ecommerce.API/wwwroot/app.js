document.addEventListener("DOMContentLoaded", () => {

    // --- Elementos Globais ---
    const mainCard = document.getElementById("main-card");
    let jwtToken = localStorage.getItem("jwt_token");
    let usuarioNome = localStorage.getItem("usuario_nome") || "Usuário";

    let listaPecasCache = [];
    let pecaParaEditarId = null;

    // --- Carrinho de Compras ---
    let carrinho = [];
    let produtosDisponiveisVenda = [];

    // ==================================================================
    // 1. LÓGICA DE LOGIN E CADASTRO
    // ==================================================================

    const loginForm = document.getElementById("login-form");
    const registerForm = document.getElementById("register-form");

    if (loginForm && registerForm) {

        // Alternar entre Telas
        document.getElementById("link-ir-cadastro").addEventListener("click", (e) => {
            e.preventDefault();
            loginForm.style.display = "none";
            registerForm.style.display = "block";
        });

        document.getElementById("link-ir-login").addEventListener("click", (e) => {
            e.preventDefault();
            registerForm.style.display = "none";
            loginForm.style.display = "block";
        });

        // LOGIN
        loginForm.addEventListener("submit", async (event) => {
            event.preventDefault();
            const emailInput = document.getElementById("email");
            const senhaInput = document.getElementById("senha");
            const errorMessage = document.getElementById("login-error");
            const loginButton = document.getElementById("login-button");

            errorMessage.style.display = "none";
            loginButton.disabled = true;

            try {
                const response = await fetch('/api/auth/login', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        email: emailInput.value,
                        senha: senhaInput.value
                    })
                });

                if (response.ok) {
                    const data = await response.json();
                    jwtToken = data.token || data.Token;
                    usuarioNome = data.nome || data.Nome || "Usuário";

                    localStorage.setItem("jwt_token", jwtToken);
                    localStorage.setItem("usuario_nome", usuarioNome);

                    mostrarDashboard();
                } else {
                    errorMessage.textContent = "E-mail ou senha incorretos.";
                    errorMessage.style.display = "block";
                }
            } catch (error) {
                errorMessage.textContent = "Erro de conexão.";
                errorMessage.style.display = "block";
            } finally {
                loginButton.disabled = false;
            }
        });

        // CADASTRO
        registerForm.addEventListener("submit", async (event) => {
            event.preventDefault();

            const nome = document.getElementById("reg-nome").value;
            const email = document.getElementById("reg-email").value;
            const senha = document.getElementById("reg-senha").value;
            const cargo = document.getElementById("reg-cargo").value;

            const btn = document.getElementById("reg-button");
            const err = document.getElementById("reg-error");
            const suc = document.getElementById("reg-success");

            err.style.display = "none";
            suc.style.display = "none";

            btn.disabled = true;
            btn.textContent = "Cadastrando...";

            try {
                const response = await fetch('/api/auth/registrar', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        Nome: nome,
                        Email: email,
                        Senha: senha,
                        Tipo: cargo
                    })
                });

                if (response.ok) {
                    suc.textContent = "Conta criada com sucesso! Faça login.";
                    suc.style.display = "block";
                    registerForm.reset();

                    setTimeout(() => {
                        registerForm.style.display = "none";
                        loginForm.style.display = "block";
                        suc.style.display = "none";
                    }, 2000);
                } else {
                    const msg = await response.text();
                    err.textContent = msg || "Erro ao cadastrar.";
                    err.style.display = "block";
                }
            } catch (e) {
                err.textContent = "Erro de conexão.";
                err.style.display = "block";
            } finally {
                btn.disabled = false;
                btn.textContent = "Cadastrar";
            }
        });
    }

    // ==================================================================
    // 2. DASHBOARD
    // ==================================================================

    function mostrarDashboard() {
        carrinho = []; 

        mainCard.style.maxWidth = "700px"; // Aumentei para caber mais botões

        mainCard.innerHTML = `
            <button class="top-right-button" id="btn-logout">Sair</button>

            <h2 style="margin-bottom: 10px;">Olá, ${usuarioNome}!</h2>
            <p style="text-align: center; color: #666;">Escolha uma opção:</p>

            <div class="dashboard-menu">
                <div class="menu-item" id="btn-menu-cadastro">
                    <span style="font-size: 2em;">➕</span>
                    <span>Cadastrar Peça</span>
                </div>
                <div class="menu-item" id="btn-menu-listar">
                    <span style="font-size: 2em;">📦</span>
                    <span>Gerenciar Estoque</span>
                </div>
                <div class="menu-item" id="btn-menu-venda">
                    <span style="font-size: 2em;">🛒</span>
                    <span>Nova Venda</span>
                </div>
                <div class="menu-item" id="btn-menu-historico">
                    <span style="font-size: 2em;">📜</span>
                    <span>Meus Pedidos</span>
                </div>
                <div class="menu-item" id="btn-menu-ponto">
                    <span style="font-size: 2em;">⏰</span>
                    <span>Registrar Ponto</span>
                </div>
            </div>
        `;

        document.getElementById("btn-logout").addEventListener("click", logout);
        document.getElementById("btn-menu-cadastro").addEventListener("click", () => mostrarTelaCadastro());
        document.getElementById("btn-menu-listar").addEventListener("click", mostrarTelaListagem);
        document.getElementById("btn-menu-ponto").addEventListener("click", mostrarPainelPonto);
        document.getElementById("btn-menu-venda").addEventListener("click", mostrarTelaVendas);
        document.getElementById("btn-menu-historico").addEventListener("click", mostrarTelaHistorico);
    }

    // ==================================================================
    // 3. TELA DE VENDAS
    // ==================================================================

    function mostrarTelaVendas() {
        mainCard.style.maxWidth = "900px";

        mainCard.innerHTML = `
            <button class="top-right-button" id="btn-voltar">Voltar</button>
            <h2 class="pecas-header">🛒 Nova Venda</h2>
            <div class="divisor"></div>

            <div class="vendas-container">
                <div class="coluna-produtos">
                    <h3>Produtos Disponíveis</h3>
                    <div id="lista-venda-produtos">
                        <p>Carregando...</p>
                    </div>
                </div>

                <div class="coluna-carrinho">
                    <h3>🛍️ Carrinho</h3>
                    <ul id="lista-carrinho" style="list-style: none; padding: 0; margin-top: 10px;">
                        <li style="color: #666; font-size: 0.9em;">Seu carrinho está vazio.</li>
                    </ul>
                    <hr>
                    <div class="total-venda">
                        Total: <span id="valor-total">R$ 0,00</span>
                    </div>
                    <button id="btn-finalizar-venda" style="width: 100%; padding: 10px; background-color: #28a745; color: white; border: none; cursor: pointer; border-radius: 4px; font-size: 1em;">
                        ✅ Finalizar Venda
                    </button>
                </div>
            </div>
        `;

        document.getElementById("btn-voltar").addEventListener("click", mostrarDashboard);
        document.getElementById("btn-finalizar-venda").addEventListener("click", finalizarVendaAPI);

        carregarProdutosParaVenda();
    }

    async function carregarProdutosParaVenda() {
        try {
            const res = await fetch('/api/pecas', {
                headers: { 'Authorization': `Bearer ${jwtToken}` }
            });
            
            if(res.ok) {
                const pecas = await res.json();
                produtosDisponiveisVenda = pecas.filter(p => (p.estoque || p.Estoque) > 0);
                renderizarListaProdutosVenda();
            } else {
                document.getElementById("lista-venda-produtos").innerHTML = "<p style='color:red'>Erro ao carregar peças.</p>";
            }
        } catch (e) {
            console.error(e);
        }
    }

    function renderizarListaProdutosVenda() {
        const divLista = document.getElementById("lista-venda-produtos");
        divLista.innerHTML = "";

        if(produtosDisponiveisVenda.length === 0) {
            divLista.innerHTML = "<p>Nenhum produto com estoque disponível.</p>";
            return;
        }

        produtosDisponiveisVenda.forEach(p => {
            const id = p.id || p.Id;
            const nome = p.nome || p.Nome;
            const preco = p.preco || p.Preco;
            const estoque = p.estoque || p.Estoque;

            const divItem = document.createElement("div");
            divItem.className = "item-venda";
            divItem.innerHTML = `
                <div>
                    <strong>${nome}</strong><br>
                    <small>R$ ${preco.toFixed(2)} | Disp: ${estoque}</small>
                </div>
                <button class="btn-add-carrinho" onclick="window.adicionarAoCarrinhoGlobal('${id}')">Adicionar</button>
            `;
            divLista.appendChild(divItem);
        });
    }

    window.adicionarAoCarrinhoGlobal = function(id) {
        const produto = produtosDisponiveisVenda.find(p => (p.id || p.Id) == id);
        if(!produto) return;

        const itemExistente = carrinho.find(item => item.pecaId === id);
        const estoqueAtual = produto.estoque || produto.Estoque;

        if (itemExistente) {
            if (itemExistente.quantidade + 1 > estoqueAtual) {
                alert("Estoque máximo atingido para este item!");
                return;
            }
            itemExistente.quantidade++;
        } else {
            carrinho.push({
                pecaId: id,
                nome: produto.nome || produto.Nome,
                preco: produto.preco || produto.Preco,
                quantidade: 1
            });
        }
        atualizarInterfaceCarrinho();
    };

    window.removerDoCarrinhoGlobal = function(index) {
        carrinho.splice(index, 1);
        atualizarInterfaceCarrinho();
    }

    function atualizarInterfaceCarrinho() {
        const ul = document.getElementById("lista-carrinho");
        const spanTotal = document.getElementById("valor-total");
        ul.innerHTML = "";
        
        let total = 0;

        if (carrinho.length === 0) {
            ul.innerHTML = "<li style='color: #666; font-size: 0.9em;'>Seu carrinho está vazio.</li>";
        } else {
            carrinho.forEach((item, index) => {
                const subtotal = item.quantidade * item.preco;
                total += subtotal;

                const li = document.createElement("li");
                li.style.marginBottom = "10px";
                li.style.borderBottom = "1px solid #eee";
                li.style.paddingBottom = "5px";
                li.innerHTML = `
                    <div style="display:flex; justify-content:space-between;">
                        <span>${item.nome} (x${item.quantidade})</span>
                        <button class="btn-rm-carrinho" onclick="window.removerDoCarrinhoGlobal(${index})">X</button>
                    </div>
                    <div style="text-align:right; font-size:0.9em; color:#555;">R$ ${subtotal.toFixed(2)}</div>
                `;
                ul.appendChild(li);
            });
        }

        spanTotal.innerText = `R$ ${total.toFixed(2)}`;
    }

    async function finalizarVendaAPI() {
        if(carrinho.length === 0) {
            alert("O carrinho está vazio!");
            return;
        }

        const btn = document.getElementById("btn-finalizar-venda");
        btn.disabled = true;
        btn.innerText = "Processando...";

        const payload = {
            itens: carrinho.map(item => ({
                pecaId: item.pecaId,
                quantidade: item.quantidade
            }))
        };

        try {
            const res = await fetch('/api/pedidos/realizar-venda', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${jwtToken}`
                },
                body: JSON.stringify(payload)
            });

            const data = await res.json();

            if(res.ok) {
                alert(`Venda realizada com sucesso!\nValor Total: R$ ${data.total}`);
                carrinho = [];
                atualizarInterfaceCarrinho();
                carregarProdutosParaVenda(); 
            } else {
                alert(`Erro: ${data.erro || 'Falha na venda'}`);
            }
        } catch (e) {
            console.error(e);
            alert("Erro de conexão com o servidor.");
        } finally {
            btn.disabled = false;
            btn.innerText = "✅ Finalizar Venda";
        }
    }

    // ==================================================================
    // 4. TELA DE HISTÓRICO (NOVO!)
    // ==================================================================

    function mostrarTelaHistorico() {
        mainCard.style.maxWidth = "800px";

        mainCard.innerHTML = `
            <button class="top-right-button" id="btn-voltar">Voltar</button>
            <h2 class="pecas-header">📜 Meus Pedidos</h2>
            <div class="divisor"></div>
            
            <div id="lista-historico">
                <p style="text-align:center;">Carregando...</p>
            </div>
        `;

        document.getElementById("btn-voltar").addEventListener("click", mostrarDashboard);
        carregarHistoricoAPI();
    }

    async function carregarHistoricoAPI() {
        try {
            const res = await fetch('/api/pedidos/meus-pedidos', {
                headers: { 'Authorization': `Bearer ${jwtToken}` }
            });

            const divLista = document.getElementById("lista-historico");

            if (res.ok) {
                const pedidos = await res.json();
                divLista.innerHTML = "";

                if (pedidos.length === 0) {
                    divLista.innerHTML = "<p style='text-align:center; color:#666;'>Você ainda não fez nenhum pedido.</p>";
                    return;
                }

                pedidos.forEach(p => {
                    const data = new Date(p.dataPedido || p.DataPedido).toLocaleString();
                    const total = p.valorTotal || p.ValorTotal;
                    const itens = p.itens || p.Itens || [];

                    const card = document.createElement("div");
                    card.style.border = "1px solid #ddd";
                    card.style.borderRadius = "8px";
                    card.style.padding = "15px";
                    card.style.marginBottom = "15px";
                    card.style.backgroundColor = "#f9f9f9";

                    let itensHtml = itens.map(i => {
                        // Tenta pegar o nome de diferentes formas caso o Include tenha falhado ou o JSON venha diferente
                        const nomePeca = i.peca ? (i.peca.nome || i.peca.Nome) : "Peça ID: " + (i.pecaId || i.PecaId);
                        const qtd = i.quantidade || i.Quantidade;
                        const prc = i.precoUnitario || i.PrecoUnitario;
                        return `<li>${nomePeca} (x${qtd}) - R$ ${prc.toFixed(2)}</li>`;
                    }).join("");

                    card.innerHTML = `
                        <div style="display:flex; justify-content:space-between; border-bottom:1px solid #eee; padding-bottom:5px; margin-bottom:10px;">
                            <strong>Pedido #${p.id ? p.id.substring(0,8) : "..."}</strong>
                            <span style="color:#666;">${data}</span>
                        </div>
                        <ul style="font-size:0.9em; color:#555; padding-left: 20px;">
                            ${itensHtml}
                        </ul>
                        <div style="text-align:right; font-weight:bold; margin-top:10px; color:green;">
                            Total: R$ ${total.toFixed(2)}
                        </div>
                    `;
                    divLista.appendChild(card);
                });

            } else {
                divLista.innerHTML = "<p style='color:red'>Erro ao carregar histórico.</p>";
            }
        } catch (e) {
            console.error(e);
            divLista.innerHTML = "<p style='color:red'>Erro de conexão.</p>";
        }
    }

    // ==================================================================
    // 5. TELA DE CADASTRO / EDIÇÃO
    // ==================================================================

    function mostrarTelaCadastro(pecaParaEditar = null) {
        mainCard.style.maxWidth = "600px";

        const titulo = pecaParaEditar ? "Editar Peça" : "Nova Peça";
        const textoBotao = pecaParaEditar ? "Salvar Alterações" : "Cadastrar Peça";
        const classeBotao = pecaParaEditar ? "btn-save" : "btn-add";

        pecaParaEditarId = pecaParaEditar ? (pecaParaEditar.id || pecaParaEditar.Id) : null;

        mainCard.innerHTML = `
            <button class="top-right-button" id="btn-voltar">Voltar</button>

            <h2 class="pecas-header">${titulo}</h2>
            <div class="divisor"></div>

            <div class="pecas-form">
                <div class="input-group">
                    <label>Nome</label>
                    <input type="text" id="peca-nome">
                </div>
                <div class="input-group">
                    <label>Categoria</label>
                    <input type="text" id="peca-categoria">
                </div>
                <div class="input-group">
                    <label>Preço (R$)</label>
                    <input type="number" id="peca-preco" step="0.01">
                </div>
                <div class="input-group">
                    <label>Estoque (Qtd)</label>
                    <input type="number" id="peca-estoque">
                </div>
                <button id="btn-salvar-peca" class="btn-full-width ${classeBotao}">
                    ${textoBotao}
                </button>
            </div>
        `;

        document.getElementById("btn-voltar").addEventListener("click", mostrarDashboard);
        document.getElementById("btn-salvar-peca").addEventListener("click", salvarPeca);

        if (pecaParaEditar) {
            document.getElementById("peca-nome").value = pecaParaEditar.nome || pecaParaEditar.Nome;
            document.getElementById("peca-categoria").value = pecaParaEditar.categoria || pecaParaEditar.Categoria;
            document.getElementById("peca-preco").value = pecaParaEditar.preco || pecaParaEditar.Preco;
            document.getElementById("peca-estoque").value = pecaParaEditar.estoque || pecaParaEditar.Estoque;
        }
    }

    async function salvarPeca() {
        const nome = document.getElementById("peca-nome").value;
        const cat = document.getElementById("peca-categoria").value;
        const preco = document.getElementById("peca-preco").value;
        const est = document.getElementById("peca-estoque").value;
        const btn = document.getElementById("btn-salvar-peca");

        if (!nome || !preco) {
            alert("Preencha os dados.");
            return;
        }

        btn.disabled = true;
        btn.textContent = "Salvando...";

        const payload = {
            Nome: nome,
            Categoria: cat,
            Preco: parseFloat(preco),
            Estoque: parseInt(est)
        };

        let url = `/api/pecas`;
        let method = 'POST';

        if (pecaParaEditarId) {
            url = `/api/pecas/${pecaParaEditarId}`;
            method = 'PUT';
        }

        try {
            const res = await fetch(url, {
                method,
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${jwtToken}`
                },
                body: JSON.stringify(payload)
            });

            if (res.ok) {
                alert(pecaParaEditarId ? "Atualizado!" : "Cadastrado!");
                pecaParaEditarId = null;
                mostrarTelaListagem();
            } else {
                alert("Erro ao salvar.");
            }
        } catch (e) {
            alert("Erro de conexão.");
        } finally {
            btn.disabled = false;
            btn.textContent = pecaParaEditarId ? "Salvar" : "Cadastrar";
        }
    }

    // ==================================================================
    // 6. LISTAGEM
    // ==================================================================

    function mostrarTelaListagem() {
        mainCard.style.maxWidth = "900px";
        
        mainCard.innerHTML = `
            <button class="top-right-button" id="btn-voltar">Voltar</button>
            <h2 class="pecas-header">Estoque Atual</h2>
            <div class="divisor"></div>

            <p id="loading-msg" style="text-align:center;">Carregando...</p>

            <div class="table-container">
                <table class="pecas-table" id="tabela-pecas" style="display:none;">
                    <thead>
                        <tr>
                            <th>Nome</th>
                            <th>Cat.</th>
                            <th>Preço</th>
                            <th>Qtd.</th>
                            <th style="text-align: center;">Ações</th>
                        </tr>
                    </thead>
                    <tbody id="lista-pecas-body"></tbody>
                </table>
            </div>
        `;

        document.getElementById("btn-voltar").addEventListener("click", mostrarDashboard);

        carregarPecas();
    }

    async function carregarPecas() {
        try {
            const res = await fetch('/api/pecas', {
                headers: { 'Authorization': `Bearer ${jwtToken}` }
            });

            const tbody = document.getElementById("lista-pecas-body");
            const table = document.getElementById("tabela-pecas");
            const loading = document.getElementById("loading-msg");

            if (res.ok) {
                const pecas = await res.json();
                listaPecasCache = pecas;

                tbody.innerHTML = "";
                loading.style.display = "none";
                table.style.display = "table";

                if (pecas.length === 0) {
                    loading.textContent = "Nenhuma peça cadastrada.";
                    loading.style.display = "block";
                    table.style.display = "none";
                } else {
                    pecas.forEach(p => {
                        const id = p.id || p.Id;
                        const nome = p.nome || p.Nome;
                        const cat = p.categoria || p.Categoria;
                        const preco = p.preco || p.Preco;
                        const est = p.estoque || p.Estoque;

                        const tr = document.createElement("tr");

                        tr.innerHTML = `
                            <td><strong>${nome}</strong></td>
                            <td>${cat}</td>
                            <td>R$ ${preco}</td>
                            <td>${est}</td>
                            <td style="text-align: center;">
                                <button class="btn-editar" onclick="window.prepararEdicaoGlobal('${id}')">Editar</button>
                                <button class="btn-deletar" onclick="window.deletarPecaGlobal('${id}')">Excluir</button>
                            </td>
                        `;

                        tbody.appendChild(tr);
                    });
                }
            } else {
                loading.textContent = "Erro ao carregar dados.";
            }
        } catch (e) {
            console.error(e);
        }
    }

    // ==================================================================
    // 7. TELA DE PONTO
    // ==================================================================

    function mostrarPainelPonto() {
        mainCard.style.maxWidth = "500px";
        mainCard.innerHTML = `
            <button class="top-right-button" id="btn-voltar">Voltar</button>
            <h2>Registro de Ponto</h2>
            <p style="text-align:center; margin-bottom:30px;">Registre sua entrada ou saída.</p>
            <div class="ponto-buttons">
                <button class="btn-ponto btn-entrada" id="btn-entrada">Registrar Entrada</button>
                <button class="btn-ponto btn-saida" id="btn-saida">Registrar Saída</button>
            </div>
            <p id="ponto-message" class="success-message" style="display:none;"></p>
            <p id="ponto-error" class="error-message" style="display:none;"></p>
        `;

        document.getElementById("btn-voltar").addEventListener("click", mostrarDashboard);
        document.getElementById("btn-entrada").addEventListener("click", () => registrarPonto('entrada'));
        document.getElementById("btn-saida").addEventListener("click", () => registrarPonto('saida'));
    }

    async function registrarPonto(tipo) {
        const msg = document.getElementById("ponto-message");
        const err = document.getElementById("ponto-error");
        msg.style.display = "none";
        err.style.display = "none";

        const url = tipo === 'entrada' ? '/api/ponto/entrada' : '/api/ponto/saida';

        try {
            const res = await fetch(url, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${jwtToken}` }
            });
            const data = await res.json();

            if (res.ok) {
                const hora = new Date(data.Hora || data.hora).toLocaleTimeString();
                const tipoReg = data.Tipo || data.tipo;
                msg.textContent = `${tipoReg} registrada às ${hora}!`;
                msg.style.display = "block";
            } else {
                err.textContent = data.message || "Erro.";
                err.style.display = "block";
            }
        } catch (e) {
            err.textContent = "Erro de conexão.";
            err.style.display = "block";
        }
    }

    function logout() {
        localStorage.removeItem("jwt_token");
        localStorage.removeItem("usuario_nome");
        window.location.reload();
    }

    // FUNÇÕES GLOBAIS
    window.prepararEdicaoGlobal = function (id) {
        const peca = listaPecasCache.find(p => (p.id || p.Id) == id);
        if (peca) mostrarTelaCadastro(peca);
    };

    window.deletarPecaGlobal = async function (id) {
        if (!confirm("Tem certeza?")) return;
        const res = await fetch(`/api/pecas/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${jwtToken}` }
        });
        if (res.ok) carregarPecas();
    };
});