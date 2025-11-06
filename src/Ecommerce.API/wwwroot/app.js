// Espera o HTML carregar antes de rodar o script
document.addEventListener("DOMContentLoaded", () => {
    
    // --- 1. Mapeamento dos Elementos do HTML ---
    const loginForm = document.getElementById("login-form");
    const emailInput = document.getElementById("email");
    const senhaInput = document.getElementById("senha");
    
    // Containers (os <div> que escondemos/mostramos)
    const emailContainer = document.getElementById("email-container");
    const senhaContainer = document.getElementById("senha-container");
    const a2fContainer = document.getElementById("a2f-container");
    const a2fInput = document.getElementById("codigo-a2f");
    
    // Botão e Mensagem
    const loginButton = document.getElementById("login-button");
    const messageDisplay = document.getElementById("message-display");

    // Variável para guardar o estado (se estamos no passo 1 ou 2)
    let estadoLogin = 'passo1_senha'; // 'passo1_senha' ou 'passo2_a2f'

    // --- 2. Função para mostrar mensagens ---
    // (tipo: 'error' (vermelho) ou 'info' (azul))
    function mostrarMensagem(tipo, mensagem) {
        messageDisplay.textContent = mensagem;
        // Remove classes antigas e adiciona a nova
        messageDisplay.classList.remove('error-message', 'info-message');
        messageDisplay.classList.add(tipo + '-message');
        messageDisplay.style.display = 'block';
    }

    // --- 3. Função para mudar a tela para o Passo 2 (A2F) ---
    function mudarParaPassoA2F(codigoSimulado) {
        // Esconde os campos de email e senha
        // (Deixamos o email visível, mas desabilitado, para o usuário lembrar)
        emailInput.disabled = true; 
        senhaContainer.style.display = 'none';

        // Mostra o campo A2F
        a2fContainer.style.display = 'block';
        a2fInput.focus(); // Foca no campo de código

        // Muda o texto do botão
        loginButton.textContent = 'Validar Código';

        // Muda o estado
        estadoLogin = 'passo2_a2f';

        // Mostra a mensagem de informação (azul)
        mostrarMensagem('info', 'Senha correta. Insira o código de 6 dígitos.');

        // DEBUG: Mostra o código no console do navegador (F12)
        // Em produção, isso NUNCA seria feito.
        console.log("=== LOGIN PASSO 1 (DEBUG) ===");
        console.log("Código A2F Simulado (Debug): ", codigoSimulado);
        console.log("=============================");
    }

    // --- 4. Função Principal (Ouvinte do Formulário) ---
    loginForm.addEventListener("submit", async (event) => {
        // Previne o recarregamento da página
        event.preventDefault();
        
        // Esconde mensagens antigas
        messageDisplay.style.display = "none";
        
        // Pega os valores atuais
        const email = emailInput.value;
        const senha = senhaInput.value;
        const codigoA2F = a2fInput.value;

        // Desabilita o botão para evitar cliques duplos
        loginButton.disabled = true;

        try {
            if (estadoLogin === 'passo1_senha') {
                // --- LÓGICA DO PASSO 1: Enviar E-mail/Senha ---
                
                const response = await fetch('/api/auth/login', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email: email, senha: senha })
                });

                if (response.ok) {
                    // SUCESSO NO PASSO 1
                    const data = await response.json();
                    
                    // =========================================================
                    // --- CORREÇÃO (PASSO 38.0) ---
                    // Usamos 'PascalCase' (Maiúsculo) para bater com o C#
                    if (data.A2FRequerido) {
                        // Usamos 'data.A2FInfo.CodigoA2FSimulado' (Maiúsculo)
                        mudarParaPassoA2F(data.A2FInfo.CodigoA2FSimulado);
                    } else {
                        // Usamos 'data.LoginSucesso.Nome' (Maiúsculo)
                        loginForm.innerHTML = `<h3 style='color: green; text-align: center;'>Login bem-sucedido!</h3><p style='text-align: center;'>Bem-vindo, ${data.LoginSucesso.Nome}!</p>`;
                    }
                    // =========================================================

                } else {
                    // FALHA NO PASSO 1 (Senha errada ou usuário não existe)
                    mostrarMensagem('error', 'Falha no login. E-mail ou senha incorretos.');
                }

            } else {
                // --- LÓGICA DO PASSO 2: Enviar E-mail/Código A2F ---
                
                const response = await fetch('/api/auth/validar-a2f', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email: email, codigoA2F: codigoA2F })
                });

                if (response.ok) {
                    // SUCESSO NO PASSO 2 (Token JWT recebido!)
                    const data = await response.json();
                    
                    // Em um app real, salvaríamos o token:
                    // localStorage.setItem("jwt_token", data.token);

                    // =========================================================
                    // --- CORREÇÃO (PASSO 38.0) ---
                    // Usamos 'data.Nome' (Maiúsculo)
                    loginForm.innerHTML = `<h3 style='color: green; text-align: center;'>Login bem-sucedido!</h3><p style='text-align: center;'>Bem-vindo, ${data.Nome}!</p>`;
                    // =========================================================
                
                } else {
                    // FALHA NO PASSO 2 (Código A2F errado ou expirado)
                    mostrarMensagem('error', 'Código A2F inválido ou expirado. Tente o login novamente.');
                }
            }

        } catch (error) {
            // Erro de rede ou a API está fora do ar
            console.error("Erro na requisição:", error);
            mostrarMensagem('error', 'Erro de conexão. Tente novamente mais tarde.');
        } finally {
            // Reabilita o botão após a tentativa
            loginButton.disabled = false;
        }
    });
});