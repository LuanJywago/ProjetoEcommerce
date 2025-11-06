using System;
using System.Collections.Generic;

namespace Ecommerce.Application.Exceptions
{
    // Esta é a nossa exceção customizada para regras de negócio
    public class ValidacaoException : Exception
    {
        // Esta lista guardará os erros de validação
        public List<string> Erros { get; }

        public ValidacaoException(string mensagem) : base(mensagem)
        {
            Erros = new List<string> { mensagem };
        }

        public ValidacaoException(List<string> erros) : base("Múltiplos erros de validação ocorreram.")
        {
            Erros = erros;
        }
    }
}