using AutoMapper;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Api.Controllers
{

    [Route("api/produtos")]
    public class ProdutosController : MainAPIController
    {

        private readonly IProdutoRepository _produtoRepository;
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;

        public ProdutosController(IProdutoRepository produtoRepository, IProdutoService produtoService, IMapper mapper, INotificador notificador) : base(notificador)
        {
            _produtoRepository = produtoRepository ?? throw new ArgumentNullException(nameof(produtoRepository));
            _produtoService = produtoService ?? throw new ArgumentNullException(nameof(produtoService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        } //constructor



        #region ===== GET Produto =======================================================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoViewModel>>> ObterTodos()
        {

            IEnumerable<Produto> Produtos = await _produtoRepository.ObterProdutosFornecedores();

            IEnumerable<ProdutoViewModel> ProdutosVM = _mapper.Map<IEnumerable<ProdutoViewModel>>(Produtos);

            return Ok(ProdutosVM);

        } //ObterTodos

        [HttpGet("{id:guid}")]
        //[Route("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ObterPorId(Guid id)
        {

            ProdutoViewModel ProdutoVM = await ObterProduto(id);

            if (ProdutoVM == null)
                return NotFound();

            return Ok(ProdutoVM);

        } //ObterTodos

        #endregion

        #region ===== POST/PUT/DELETE Produto V1 ===================================== 

        [HttpPost]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar(ProdutoViewModel model)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var imagemNome = Guid.NewGuid() + "_" + model.Imagem;
            if (!UploadArquivo(model.ImagemUpload, imagemNome))
                return CustomResponse(model);

            model.Imagem = imagemNome;
            Produto Produto = _mapper.Map<Produto>(model);

            await _produtoService.Adicionar(Produto);

            return CustomResponse(model);

        } //Adicionar

        [HttpPut("{id:guid}")]
        //[Route("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> Atualizar(Guid id, ProdutoViewModel model)
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

            Produto Produto = _mapper.Map<Produto>(model);
            await _produtoService.Atualizar(Produto);

            return CustomResponse(model);

        } //Atualizar


        [HttpDelete("{id:guid}")]
        //[Route("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> Excluir(Guid id)
        {
            ProdutoViewModel ProdutoVM = await ObterProduto(id);

            if (ProdutoVM == null)
                return NotFound();

            await _produtoService.Remover(id);

            return CustomResponse(ProdutoVM);

        } //Excluir

        #endregion

        #region ===== POST/PUT/DELETE Produto V2 (Large Files) ====================== 

        [HttpPost("Adicionar")]
        public async Task<ActionResult<ProdutoImagemGrandeViewModel>> AdicionarAlternativo(ProdutoImagemGrandeViewModel model)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var imgPrefixo = Guid.NewGuid() + "_";
            if (!await UploadArquivoAlternativo(model.ImagemUpload, imgPrefixo))
                return CustomResponse(model);

            model.Imagem = imgPrefixo + model.ImagemUpload.FileName;
            Produto Produto = _mapper.Map<Produto>(model);

            await _produtoService.Adicionar(Produto);

            return CustomResponse(model);

        } //Adicionar


        [HttpPut("Atualizar/{id:guid}")]
        public async Task<IActionResult> AtualizarAlternativo(Guid id, ProdutoViewModel produtoViewModel)
        {
            if (id != produtoViewModel.Id)
            {
                NotificarErro("Os ids informados não são iguais!");
                return CustomResponse();
            }

            var produtoAtualizacao = await ObterProduto(id);

            if (string.IsNullOrEmpty(produtoViewModel.Imagem))
                produtoViewModel.Imagem = produtoAtualizacao.Imagem;

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if (produtoViewModel.ImagemUpload != null)
            {
                var imagemNome = Guid.NewGuid() + "_" + produtoViewModel.Imagem;
                if (!UploadArquivo(produtoViewModel.ImagemUpload, imagemNome))
                {
                    return CustomResponse(ModelState);
                }

                produtoAtualizacao.Imagem = imagemNome;
            }

            produtoAtualizacao.FornecedorId = produtoViewModel.FornecedorId;
            produtoAtualizacao.Nome = produtoViewModel.Nome;
            produtoAtualizacao.Descricao = produtoViewModel.Descricao;
            produtoAtualizacao.Valor = produtoViewModel.Valor;
            produtoAtualizacao.Ativo = produtoViewModel.Ativo;

            await _produtoService.Atualizar(_mapper.Map<Produto>(produtoAtualizacao));

            return CustomResponse(produtoViewModel);
        } //Atualizar


        #endregion


        #region === Helper Methods ======================================================

        private async Task<ProdutoViewModel> ObterProduto(Guid id)
        {

            Produto produto = await _produtoRepository.ObterProdutoFornecedor(id);

            ProdutoViewModel produtoVM = _mapper.Map<ProdutoViewModel>(produto);

            return produtoVM;


        } //ObterProduto

        private bool UploadArquivo(string arquivo, string imgNome)
        {
            if (string.IsNullOrEmpty(arquivo))
            {
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }

            var imageDataByteArray = Convert.FromBase64String(arquivo);

            //Diretorio onde a aplicacao esta rodando
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imgNome);

            if (System.IO.File.Exists(filePath))
            {
                NotificarErro("Já existe um arquivo com este nome!");
                return false;
            }

            System.IO.File.WriteAllBytes(filePath, imageDataByteArray);

            return true;
        }


        #region UploadAlternativo

        [RequestSizeLimit(40000000)]
        //[DisableRequestSizeLimit]
        [HttpPost("imagem")]
        public ActionResult AdicionarImagem(IFormFile file)
        {
            return Ok(file);
        }

        [RequestSizeLimit(40000000)]
        private async Task<bool> UploadArquivoAlternativo(IFormFile arquivo, string imgPrefixo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imgPrefixo + arquivo.FileName);

            if (System.IO.File.Exists(path))
            {
                NotificarErro("Já existe um arquivo com este nome!");
                return false;
            }


            using (var stream = new FileStream(path, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            return true;
        }
        
        #endregion

        #endregion

    } //class
} //namespace
