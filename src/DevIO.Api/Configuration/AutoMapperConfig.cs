using AutoMapper;
using DevIO.Api.ViewModels;
using DevIO.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Api.Configuration
{
    public class AutoMapperConfig : Profile
    {

        public AutoMapperConfig()
        {

            //Mapping Endereco
            CreateMap<Endereco, EnderecoViewModel>()
                .ReverseMap();

            //Mapping Fornecedor
            CreateMap<Fornecedor, FornecedorViewModel>()
                .ReverseMap();

            //Mapping Produto
            CreateMap<ProdutoViewModel, Produto>();

            CreateMap<Produto, ProdutoViewModel>()
                .ForMember(dest => dest.NomeFornecedor, opt => opt.MapFrom(src => src.Fornecedor.Nome));


        } //constructor

    } //class
} //namespace
