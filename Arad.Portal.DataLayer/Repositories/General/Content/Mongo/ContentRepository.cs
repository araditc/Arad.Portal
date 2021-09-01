using Arad.Portal.DataLayer.Contracts.General.Content;
using Arad.Portal.DataLayer.Repositories.General.Language.Mongo;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Repositories.General.ContentCategory.Mongo;

namespace Arad.Portal.DataLayer.Repositories.General.Content.Mongo
{
    public class ContentRepository : BaseRepository, IContentRepository
    {
        private readonly IMapper _mapper;
        private readonly ContentContext _contentContext;
        private readonly ContentCategoryContext _categoryContext;
        private readonly LanguageContext _languageContext;
        public ContentRepository(IHttpContextAccessor httpContextAccessor,
           IMapper mapper, ContentCategoryContext categoryContext, ContentContext contentContext, LanguageContext langContext)
            : base(httpContextAccessor)
        {
            _mapper = mapper;
            _categoryContext = categoryContext;
            _contentContext = contentContext;
            _languageContext = langContext;
        }
    }
}
