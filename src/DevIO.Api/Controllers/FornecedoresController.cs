using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DevIO.Api.Extensions;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.Controllers
{
    [Authorize]
    [Route("api/fornecedores")]
    public class FornecedoresController : MainAPIController
    {

        private readonly IEnderecoRepository _enderecoRepository;
        private readonly IFornecedorService _fornecedorService;
        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly IMapper _mapper;
        private readonly INotificador _notificador;

        public FornecedoresController(IEnderecoRepository enderecoRepository, IFornecedorService fornecedorService, IFornecedorRepository fornecedorRepository, IMapper mapper, INotificador notificador)
            :base(notificador)
        {
            _enderecoRepository = enderecoRepository ?? throw new ArgumentNullException(nameof(enderecoRepository));
            _fornecedorService = fornecedorService ?? throw new ArgumentNullException(nameof(fornecedorService));
            _fornecedorRepository = fornecedorRepository ?? throw new ArgumentNullException(nameof(fornecedorRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        #region ===== GET Fornecedor ====================================================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FornecedorViewModel>>> ObterTodos()
        {

            IEnumerable<Fornecedor> fornecedores = await _fornecedorRepository.ObterTodos();

            IEnumerable<FornecedorViewModel> fornecedoresVM = _mapper.Map<IEnumerable<FornecedorViewModel>>(fornecedores);

            return Ok(fornecedoresVM);

        } //ObterTodos

        [HttpGet("{id:guid}")]
        //[Route("{id:guid}")]
        public async Task<ActionResult<FornecedorViewModel>> ObterPorId(Guid id)
        {

            FornecedorViewModel fornecedorVM = await ObterFornecedorProdutosEndereco(id);

            if (fornecedorVM == null)
                return NotFound();

            return Ok(fornecedorVM);

        } //ObterTodos

        #endregion

        #region ===== POST/PUT/DELETE Fornecedor ======================================== 

        [TestAuthorize]
        [ClaimsAuthorize("Fornecedor", "Adicionar")]
        [HttpPost]
        public async Task<ActionResult<FornecedorViewModel>> Adicionar(FornecedorViewModel model)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            Fornecedor fornecedor = _mapper.Map<Fornecedor>(model);

            await _fornecedorService.Adicionar(fornecedor);

            return CustomResponse(model);

        } //Adicionar

        [TestAuthorize]
        [ClaimsAuthorize("Fornecedor", "Atualizar")]
        [HttpPut("{id:guid}")]
        //[Route("{id:guid}")]
        public async Task<ActionResult<FornecedorViewModel>> Atualizar(Guid id, FornecedorViewModel model)
        {

            if (id != model.Id)
            {
                //Aqui estou dando apenas um exemplo de erro que posso notificar. Neste caso, e' interessante
                //informar o erro?
                NotificarErro("O id informado nao e' o mesmo que foi passado no query");
                return CustomResponse(model);
            }
                
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            Fornecedor fornecedor = _mapper.Map<Fornecedor>(model);
            await _fornecedorService.Atualizar(fornecedor);

           return CustomResponse(model);

        } //Atualizar

        [ClaimsAuthorize("Fornecedor", "Excluir")]
        [HttpDelete("{id:guid}")]
        //[Route("{id:guid}")]
        public async Task<ActionResult<FornecedorViewModel>> Excluir(Guid id)
        {
            FornecedorViewModel fornecedorVM = await ObterFornecedorEndereco(id);

            if (fornecedorVM == null)
                return NotFound();

            await _fornecedorService.Remover(id);

            return CustomResponse(fornecedorVM);
        
        } //Excluir

        #endregion


        #region ===== GET Endereco ======================================================

        [HttpGet]
        [Route("obter-endereco/{id:guid}")]
        public async Task<ActionResult<EnderecoViewModel>> ObterEnderecoPorId(Guid id)
        {
            var fornecedor = await _fornecedorRepository.ObterFornecedorEndereco(id);

            var endereco = fornecedor.Endereco;

            var enderecoVM = _mapper.Map<EnderecoViewModel>(endereco);

            return enderecoVM;

        } //ObterEnderecoPorId

        #endregion

        #region ===== POST Endereco ===================================================== 

        [ClaimsAuthorize("Fornecedor", "Atualizar")]
        [HttpPut("atualizar-endereco/{id:guid}")]
        //[Route("atualizar-endereco/{id:guid}")]
        public async Task<ActionResult<EnderecoViewModel>> AtualizarEndereco(Guid id, EnderecoViewModel model)
        {

            if (id != model.Id)
            {
                //Aqui estou dando apenas um exemplo de erro que posso notificar. Neste caso, e' interessante
                //informar o erro?
                NotificarErro("O id informado nao e' o mesmo que foi passado no query");
                return CustomResponse(model);
            }

            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            Endereco endereco = _mapper.Map<Endereco>(model);
            await _fornecedorService.AtualizarEndereco(endereco);

            return CustomResponse(model);

        } //Atualizar


        #endregion

        /// <summary>
        /// Obtendo Fornecedor com seus Produtos e Endereco
        /// </summary>
        /// <param name="id">id do Fornecedor</param>
        /// <returns>FornecedorViewModel</returns>
        private async Task<FornecedorViewModel> ObterFornecedorProdutosEndereco(Guid id)
        {

            Fornecedor fornecedor = await _fornecedorRepository.ObterFornecedorProdutosEndereco(id);

            FornecedorViewModel fornecedorVM = _mapper.Map<FornecedorViewModel>(fornecedor);

            return fornecedorVM;

        } //ObterFornecedorProdutosEndereco

        /// <summary>
        /// Obtendo Fornecedor e seu Endereco
        /// </summary>
        /// <param name="id">id do Fornecedor</param>
        /// <returns>FornecedorViewModel</returns>
        private async Task<FornecedorViewModel> ObterFornecedorEndereco(Guid id)
        {

            Fornecedor fornecedor = await _fornecedorRepository.ObterFornecedorEndereco(id);

            FornecedorViewModel fornecedorVM = _mapper.Map<FornecedorViewModel>(fornecedor);

            return fornecedorVM;

        } //ObterFornecedorProdutosEndereco

    } //class

} //namespace
