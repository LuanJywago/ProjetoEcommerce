using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Persistence.Repositories
{
    public class PecaRepository : IPecaRepository
    {
        private readonly AppDbContext _context;
        // AppDbContext é a representação do BD em Memória;
        //Classe do Entity Framework que conversa com o SQL

        public PecaRepository(AppDbContext context)
        {
            _context = context; //Já possui login e senha do BD pelo Docker
        }

        public async Task AdicionarAsync(Peca peca)
        {
            //Adicionada apenas na memoria do EF
            // Se torna ADD, mas ainda não foi para o banco
            await _context.Pecas.AddAsync(peca);
            await _context.SaveChangesAsync();
            //Agora sim vai para o Banco com conexão com o Docker
            // Sem isso, nada salva
        }

        public async Task<IEnumerable<Peca>> ObterTodasAsync()
        {
            return await _context.Pecas.ToListAsync();
            // Lista no BD pelo SELECT * FROM Pecas...
            //Lista assincrona, executando o Select, transformando em objetos Peca do código
        }

        public async Task<Peca?> ObterPorIdAsync(Guid id)
        {
            return await _context.Pecas.FindAsync(id);
            // Busca feita pelo ID
            //Sistema inteligente que faz de forma filtrada
        }

        public async Task AtualizarAsync(Peca peca)
        {
            _context.Pecas.Update(peca);
            //Rastreamento pelo EF mostrando que ele foi alterado (através de monitoramento do EF)
            await _context.SaveChangesAsync();
            //SQL executa o UPDATE Pecas...
        }

        public async Task DeletarAsync(Peca peca)
        {
            _context.Pecas.Remove(peca);
            //Remove a peça como o DELETE
            await _context.SaveChangesAsync();
        }
    }
}