document.addEventListener("DOMContentLoaded", () => {

    // --- VARIÁVEIS GLOBAIS ---
    const mainCard = document.getElementById("main-card");
    let jwtToken = localStorage.getItem("jwt_token");
    let usuarioNome = localStorage.getItem("usuario_nome") || "Usuário";
    let usuarioCargo = localStorage.getItem("usuario_cargo") || "Cliente";

    let listaPecasCache = [];
    let pecaParaEditarId = null;
    let carrinho = [];
    let produtosDisponiveisVenda = [];

    // ==================================================================
    // 1. LOGIN E REGISTRO
    // ==================================================================
    const loginForm = document.getElementById("login-form");
    const registerForm = document.getElementById("register-form");

    if (loginForm && registerForm) {

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

            const email = document.getElementById("email").value;
            const senha = document.getElementById("senha").value;
            const btn = document.getElementById("login-button");
            const err = document.getElementById("login-error");

            err.style.display = "none";
            btn.disabled = true;

            try {
                const res = await fetch('/api/auth/login', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email, senha }),
                });

                if (res.ok) {
                    const data = await res.json();

                    jwtToken = data.token || data.Token;
                    usuarioNome = data.nome || data.Nome;

                    let tipo = data.tipo || data.Tipo;

                    if (tipo === 2 || tipo === "Admin") usuarioCargo = "Admin";
                    else if (tipo === 1 || tipo === "Funcionario") usuarioCargo = "Funcionario";
                    else usuarioCargo = "Cliente";

                    localStorage.setItem("jwt_token", jwtToken);
                    localStorage.setItem("usuario_nome", usuarioNome);
                    localStorage.setItem("usuario_cargo", usuarioCargo);

                    mostrarDashboard();
                } else {
                    err.textContent = "E-mail ou senha incorretos.";
                    err.style.display = "block";
                }
            } catch (e) {
                err.textContent = "Erro de conexão.";
                err.style.display = "block";
            } finally {
                btn.disabled = false;
            }
        });

        // REGISTRO
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

            try {
                const res = await fetch('/api/auth/registrar', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        Nome: nome,
                        Email: email,
                        Senha: senha,
                        Tipo: cargo
                    }),
                });

                if (res.ok) {
                    suc.textContent = "Conta criada com sucesso! Faça login.";
                    suc.style.display = "block";

                    setTimeout(() => window.location.reload(), 1500);
                } else {
                    err.textContent = "Erro ao cadastrar.";
                    err.style.display = "block";
                }
            } catch (e) {
                err.textContent = "Erro de conexão.";
                err.style.display = "block";
            } finally {
                btn.disabled = false;
            }
        });
    }

    // ==================================================================
    // 2. DASHBOARD DINÂMICO
    // ==================================================================
    function mostrarDashboard() {

        carrinho = [];
        mainCard.style.maxWidth = "850px";

        let botoesHtml = "";

        if (usuarioCargo === "Cliente") {

            botoesHtml = `
                <div class="menu-item" onclick="window.mostrarTelaVendas()">
                    <span style="font-size: 2em;">🛒</span><span>Nova Compra</span>
                </div>
                <div class="menu-item" onclick="window.mostrarTelaHistorico()">
                    <span style="font-size: 2em;">📜</span><span>Meus Pedidos</span>
                </div>
            `;

        } else if (usuarioCargo === "Funcionario") {

            botoesHtml = `
                <div class="menu-item" onclick="window.mostrarTelaCadastro()">
                    <span style="font-size: 2em;">➕</span><span>Cadastrar Peça</span>
                </div>
                <div class="menu-item" onclick="window.mostrarTelaListagem()">
                    <span style="font-size: 2em;">📦</span><span>Estoque</span>
                </div>
                <div class="menu-item" onclick="window.mostrarTelaVendas()">
                    <span style="font-size: 2em;">🛒</span><span>Vender</span>
                </div>
                <div class="menu-item" onclick="window.mostrarTelaHistorico()">
                    <span style="font-size: 2em;">📜</span><span>Histórico</span>
                </div>
                <div class="menu-item" onclick="window.mostrarPainelPonto()">
                    <span style="font-size: 2em;">⏰</span><span>Ponto</span>
                </div>
            `;

        } else {

            botoesHtml = `
                <div class="menu-item" onclick="window.mostrarTelaRelatorioAdmin()" style="background-color:#e3f2fd;">
                    <span style="font-size: 2em;">📊</span><span>Relatório Geral</span>
                </div>
                <div class="menu-item" onclick="window.mostrarTelaCadastro()">
                    <span style="font-size: 2em;">➕</span><span>Cadastrar Peça</span>
                </div>
                <div class="menu-item" onclick="window.mostrarTelaListagem()">
                    <span style="font-size: 2em;">📦</span><span>Estoque</span>
                </div>
                <div class="menu-item" onclick="window.mostrarTelaVendas()">
                    <span style="font-size: 2em;">🛒</span><span>Vender</span>
                </div>
                <div class="menu-item" onclick="window.mostrarTelaHistorico()">
                    <span style="font-size: 2em;">📜</span><span>Histórico</span>
                </div>
                <div class="menu-item" onclick="window.mostrarPainelPonto()">
                    <span style="font-size: 2em;">⏰</span><span>Ponto</span>
                </div>
            `;
        }

        mainCard.innerHTML = `
            <button class="top-right-button" onclick="window.logout()">Sair</button>
            <h2 style="margin-bottom: 5px;">Olá, ${usuarioNome}!</h2>
            <p style="text-align:center; color:#666; margin-bottom:20px;">Perfil: <strong>${usuarioCargo}</strong></p>

            <div class="dashboard-menu">
                ${botoesHtml}
            </div>
        `;
    }

    window.mostrarDashboard = mostrarDashboard;

    // ==================================================================
    // 3. TELAS E FUNÇÕES
    // ==================================================================

    // =======================
    // RELATÓRIO ADMIN
    // =======================
    window.mostrarTelaRelatorioAdmin = async function () {

        mainCard.style.maxWidth = "600px";

        mainCard.innerHTML = `
            <button class="top-right-button" onclick="window.mostrarDashboard()">Voltar</button>
            <h2>📊 Relatório</h2>
            <div id="relatorio-conteudo" style="text-align:center; padding:20px;">Carregando...</div>
        `;

        try {
            const res = await fetch(`/api/pedidos/relatorio-admin`, {
                headers: { "Authorization": `Bearer ${jwtToken}` }
            });

            if (res.ok) {
                const dados = await res.json();

                document.getElementById("relatorio-conteudo").innerHTML = `
                    <div style="display:grid; grid-template-columns:1fr 1fr; gap:20px; margin-top:20px;">
                        <div style="background:#e8f5e9; padding:20px; border-radius:8px;">
                            <h3>Faturamento</h3>
                            <p style="font-size:1.5em; color:green;">R$ ${dados.faturamentoTotal.toFixed(2)}</p>
                        </div>

                        <div style="background:#e3f2fd; padding:20px; border-radius:8px;">
                            <h3>Total Pedidos</h3>
                            <p style="font-size:1.5em; color:blue;">${dados.totalPedidos}</p>
                        </div>
                    </div>

                    <div style="margin-top:20px; padding:15px; background:#fff3e0; border-radius:8px;">
                        <strong>Ticket Médio:</strong> R$ ${dados.ticketMedio.toFixed(2)}
                    </div>
                `;
            } else {
                document.getElementById("relatorio-conteudo").innerHTML = `
                    <p style="color:red;">Acesso negado.</p>
                `;
            }

        } catch (error) {
            console.error(error);
        }
    };

    // =======================
    // TELA DE VENDAS
    // =======================
    window.mostrarTelaVendas = async function () {

        mainCard.style.maxWidth = "900px";

        mainCard.innerHTML = `
            <button class="top-right-button" onclick="window.mostrarDashboard()">Voltar</button>

            <h2>🛒 Vendas</h2>
            <div class="divisor"></div>

            <div class="vendas-container">

                <div class="coluna-produtos">
                    <h3>Disponível</h3>
                    <div id="lista-venda-produtos">Carregando...</div>
                </div>

                <div class="coluna-carrinho">
                    <h3>Carrinho</h3>
                    <ul id="lista-carrinho"></ul>
                    <hr>

                    <div>Total: <span id="valor-total">R$ 0.00</span></div>

                    <button onclick="window.finalizarVendaAPI()" style="width:100%; background:green; color:white; padding:10px; margin-top:10px;">
                        Finalizar
                    </button>
                </div>

            </div>
        `;

        const res = await fetch(`/api/pecas`, {
            headers: { "Authorization": `Bearer ${jwtToken}` }
        });

        if (res.ok) {
            const pecas = await res.json();
            produtosDisponiveisVenda = pecas.filter(p => (p.estoque || p.Estoque) > 0);

            renderizarListaProdutosVenda();
        }
    };

    function renderizarListaProdutosVenda() {
        const div = document.getElementById("lista-venda-produtos");

        div.innerHTML = "";

        if (produtosDisponiveisVenda.length === 0) {
            div.innerHTML = "<p>Sem estoque.</p>";
            return;
        }

        produtosDisponiveisVenda.forEach((p) => {

            const item = document.createElement("div");
            item.className = "item-venda";

            item.innerHTML = `
                <div>
                    <b>${p.nome}</b><br>
                    <small>R$ ${p.preco}</small>
                </div>

                <button class="btn-add-carrinho" onclick="window.addCarrinho('${p.id}')">
                    Add
                </button>
            `;

            div.appendChild(item);
        });
    }

    window.addCarrinho = function (id) {

        const p = produtosDisponiveisVenda.find(x => x.id == id);
        const exist = carrinho.find(x => x.pecaId == id);

        if (exist) {
            if (exist.quantidade < p.estoque) exist.quantidade++;
        } else {
            carrinho.push({
                pecaId: id,
                nome: p.nome,
                preco: p.preco,
                quantidade: 1
            });
        }

        atualizarCarrinho();
    };

    function atualizarCarrinho() {

        const ul = document.getElementById("lista-carrinho");

        ul.innerHTML = "";

        let total = 0;

        if (carrinho.length === 0) {
            ul.innerHTML = "<li style='color:#666;'>Vazio</li>";
        }

        carrinho.forEach((item, idx) => {

            total += item.preco * item.quantidade;

            ul.innerHTML += `
                <li>
                    ${item.nome} (x${item.quantidade})

                    <button class="btn-rm-carrinho" onclick="window.rmCarrinho(${idx})">
                        X
                    </button>
                </li>
            `;
        });

        document.getElementById("valor-total").innerText = "R$ " + total.toFixed(2);
    }

    window.rmCarrinho = function (idx) {
        carrinho.splice(idx, 1);
        atualizarCarrinho();
    };

    window.finalizarVendaAPI = async function () {
        if (carrinho.length === 0) {
            return alert("Carrinho vazio!");
        }

        const res = await fetch(`/api/pedidos/realizar-venda`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${jwtToken}`
            },
            body: JSON.stringify({ itens: carrinho })
        });

        if (res.ok) {
            alert("Venda concluída!");
            carrinho = [];
            atualizarCarrinho();
            window.mostrarTelaVendas();
        } else {
            alert("Erro ao finalizar venda.");
        }
    };

    // =======================
    // HISTÓRICO
    // =======================
    window.mostrarTelaHistorico = async function () {

        mainCard.style.maxWidth = "800px";

        mainCard.innerHTML = `
            <button class="top-right-button" onclick="window.mostrarDashboard()">Voltar</button>
            <h2>📜 Histórico</h2>
            <div id="lista-historico">Carregando...</div>
        `;

        const res = await fetch(`/api/pedidos/meus-pedidos`, {
            headers: { "Authorization": `Bearer ${jwtToken}` }
        });

        if (res.ok) {

            const pedidos = await res.json();
            const div = document.getElementById("lista-historico");

            div.innerHTML = pedidos.length
                ? pedidos.map(p => `
                    <div style="border:1px solid #ccc; margin:10px; padding:10px;">
                        <b>Pedido #${p.id.substring(0, 8)}</b>
                        <br>
                        Total: R$ ${p.valorTotal.toFixed(2)}
                    </div>
                `).join("")
                : "Nenhum pedido encontrado.";
        }
    };

    // =======================
    // CADASTRO DE PEÇAS
    // =======================
    window.mostrarTelaCadastro = function (peca = null) {

        mainCard.style.maxWidth = "600px";
        pecaParaEditarId = peca ? peca.id : null;

        mainCard.innerHTML = `
            <button class="top-right-button" onclick="window.mostrarDashboard()">Voltar</button>

            <h2>${peca ? "Editar" : "Nova"} Peça</h2>

            <div class="pecas-form">
                <label>Nome</label>
                <input type="text" id="pn" value="${peca ? peca.nome : ""}">

                <label>Preço</label>
                <input type="number" id="pp" value="${peca ? peca.preco : ""}">

                <label>Estoque</label>
                <input type="number" id="pe" value="${peca ? peca.estoque : ""}">

                <label>Categoria</label>
                <input type="text" id="pc" value="${peca ? peca.categoria : ""}">

                <button onclick="window.salvarPeca()" style="width:100%; margin-top:10px;">Salvar</button>
            </div>
        `;
    };

    window.salvarPeca = async function () {

        const payload = {
            Nome: document.getElementById("pn").value,
            Preco: parseFloat(document.getElementById("pp").value),
            Estoque: parseInt(document.getElementById("pe").value),
            Categoria: document.getElementById("pc").value,
        };

        let url = "/api/pecas";
        let method = "POST";

        if (pecaParaEditarId) {
            url = `/api/pecas/${pecaParaEditarId}`;
            method = "PUT";
        }

        const res = await fetch(url, {
            method,
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${jwtToken}`,
            },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            window.mostrarTelaListagem();
        } else {
            alert("Erro ao salvar peça.");
        }
    };

    // =======================
    // LISTAGEM DE PEÇAS
    // =======================
    window.mostrarTelaListagem = async function () {

        mainCard.style.maxWidth = "900px";

        mainCard.innerHTML = `
            <button class="top-right-button" onclick="window.mostrarDashboard()">Voltar</button>

            <h2>Estoque</h2>

            <table class="pecas-table" id="tb">
                <thead>
                    <tr>
                        <th>Nome</th>
                        <th>Qtd</th>
                        <th>Ações</th>
                    </tr>
                </thead>

                <tbody id="tbb"></tbody>
            </table>
        `;

        const res = await fetch(`/api/pecas`, {
            headers: { "Authorization": `Bearer ${jwtToken}` }
        });

        if (res.ok) {
            const pecas = await res.json();

            listaPecasCache = pecas;

            document.getElementById("tbb").innerHTML = pecas.map(p => `
                <tr>
                    <td>${p.nome}</td>
                    <td>${p.estoque}</td>
                    <td>
                        <button onclick="window.prepararEdicao('${p.id}')">Editar</button>
                        <button onclick="window.deletarPeca('${p.id}')">Excluir</button>
                    </td>
                </tr>
            `).join("");

            document.getElementById("tb").style.display = "table";
        }
    };

    window.prepararEdicao = function (id) {
        const peca = listaPecasCache.find(p => p.id == id);
        window.mostrarTelaCadastro(peca);
    };

    window.deletarPeca = async function (id) {

        if (!confirm("Deseja realmente excluir?")) return;

        await fetch(`/api/pecas/${id}`, {
            method: "DELETE",
            headers: { "Authorization": `Bearer ${jwtToken}` }
        });

        window.mostrarTelaListagem();
    };

    // =======================
    // PAINEL DE PONTO
    // =======================
    window.mostrarPainelPonto = function () {

        mainCard.style.maxWidth = "500px";

        mainCard.innerHTML = `
            <button class="top-right-button" onclick="window.mostrarDashboard()">Voltar</button>

            <h2>Ponto</h2>

            <button onclick="window.regPonto('entrada')">Entrada</button>
            <button onclick="window.regPonto('saida')">Saída</button>
        `;
    };

    window.regPonto = async function (tipo) {

        const res = await fetch(`/api/ponto/${tipo}`, {
            method: "POST",
            headers: { "Authorization": `Bearer ${jwtToken}` }
        });

        if (res.ok) alert("Registrado com sucesso!");
        else alert("Erro ao registrar ponto.");
    };

    // =======================
    // LOGOUT
    // =======================
    window.logout = function () {
        localStorage.clear();
        window.location.reload();
    };

});
