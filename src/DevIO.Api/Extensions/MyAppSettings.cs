using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Api.Extensions
{
    public class MyAppSettings
    {
        /// <summary>
        /// Chave de criptografia do Token
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// Quantas horas o token vai levar para perder a validade
        /// </summary>
        public int ExpiracaoHora { get; set; }

        /// <summary>
        /// Emissor do token (no caso a propria aplicacao)
        /// </summary>
        public string Emissor { get; set; }

        /// <summary>
        /// Quais urls este token eh valido
        /// </summary>
        public string ValidoEm { get; set; }

    }
}
