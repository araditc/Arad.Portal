using Arad.Portal.DataLayer.Entities.General.Content;
using Arad.Portal.DataLayer.Entities.Shop.Product;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using Lucene.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Util;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Arad.Portal.DataLayer.Contracts.General.Language;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Arad.Portal.DataLayer.Services
{
    //public interface ILuceneService
    //{
    //    Result AddItemToExistingIndex(string indexFullPath, LuceneSearchIndexModel model, bool isProduct);
    //    void BuildContentIndexesPerLanguage(IList<Content> contents, string indexFullPath);
    //    void BuildProductIndexesPerLanguage(IList<Product> products, string mainPath);
    //    Result DeleteItemFromExistingIndex(string indexFullPath, string id);
    //    /// <summary>
    //    /// this part search on all lucene indexes
    //    /// </summary>
    //    /// <param name="searchword"></param>
    //    /// <returns></returns>
    //    List<LuceneSearchIndexModel> Search(string searchword, List<string> directories);

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="indexFullPath"></param>
    //    /// <param name="id"></param>
    //    /// <param name="model"></param>
    //    /// <param name="isProduct">if it isnt product it is content</param>
    //    /// <returns></returns>
    //    Result UpdateItemInIndex(string indexFullPath, string id, LuceneSearchIndexModel model, bool isProduct);
    //}
    public class LuceneService 
    {
        public IndexWriter Writer { get; set; }
        public  List<LuceneSearchIndexModel> Data { get; set; }
        const LuceneVersion Lv = LuceneVersion.LUCENE_48;
        static Analyzer Analyzer = new StandardAnalyzer(Lv);
        private readonly string[] SupportedCultures = new string[] { "fa-IR", "en-US" };
        private readonly ILanguageRepository _lanRepository;
        IndexWriterConfig config;
        public LuceneService(ILanguageRepository lanRepository)
        {
            _lanRepository = lanRepository;
        }


        public Result AddItemToExistingIndex(string indexFullPath, LuceneSearchIndexModel model, bool isProduct)
        {
            var res = new Result();
            FSDirectory luceneContentIndexDirectory = FSDirectory.Open(indexFullPath);
            config  = new IndexWriterConfig(Lv, Analyzer)
            {
                OpenMode = OpenMode.APPEND,
                WriteLockTimeout = Lock.LOCK_POLL_INTERVAL * 2
            };
            Writer = new IndexWriter(luceneContentIndexDirectory, config);
            try
            {
               

                var d = new Document()
                {
                    new StringField("ID", model.ID, Field.Store.YES),
                    new TextField("EntityName", model.EntityName, Field.Store.YES),
                    new StringField("Code", model.Code, Field.Store.YES)
                };
                foreach (var grp in model.GroupIds)
                {
                    d.Add(new StringField("GroupId", grp, Field.Store.YES));
                }
                foreach (var item in model.GroupNames)
                {
                    d.Add(new TextField("GroupName", item, Field.Store.YES));
                }
                foreach (var tag in model.TagKeywordList)
                {
                    d.Add(new TextField("TagKeyword", tag, Field.Store.YES));
                }
                if(isProduct)
                {
                    d.Add(new StringField("UniqueCode", model.UniqueCode, Field.Store.YES));
                }

                Writer.AddDocument(d);
                Writer.Commit();
                res.Succeeded = true;
            }
            catch (Exception)
            {
                res.Message = ConstMessages.ErrorInSaving;
            }
            Writer.Dispose();
            return res;
        }

        public void BuildContentIndexesPerLanguage(IList<Content> contents, string indexFullPath)
        {
            FSDirectory luceneContentIndexDirectory = FSDirectory.Open(indexFullPath);
            config = new IndexWriterConfig(Lv, Analyzer)
            {
                OpenMode = OpenMode.CREATE,
                WriteLockTimeout = Lock.LOCK_POLL_INTERVAL * 2
            };
            Writer = new IndexWriter(luceneContentIndexDirectory, config);
            Document d = new();
            foreach (Content item in contents)
            {
                d.Add(new StringField("ID", item.ContentId, Field.Store.YES));
                d.Add(new TextField("EntityName", item.Title, Field.Store.YES));
                d.Add(new StringField("Code", item.ContentCode.ToString(), Field.Store.YES));
                d.Add(new StringField("GroupId", item.ContentCategoryId, Field.Store.YES));
                d.Add(new TextField("GroupName", item.ContentCategoryName, Field.Store.YES));
                foreach (var tag in item.TagKeywords)
                {
                    d.Add(new TextField("TagKeyword", tag, Field.Store.YES));
                }

                Writer.AddDocument(d);
            }
            Writer.Commit();

            Writer.Dispose();
        }

        public void BuildProductIndexesPerLanguage(IList<Product> products, string mainPath)
        {
            for (int i = 0; i < SupportedCultures.Length; i++)
            {
                config = new IndexWriterConfig(Lv, Analyzer)
                {
                    OpenMode = OpenMode.CREATE,
                    WriteLockTimeout = Lock.LOCK_POLL_INTERVAL * 2
                };
                var lanId = _lanRepository.FetchBySymbol(SupportedCultures[i]);
                var indexPath = Path.Combine(mainPath, SupportedCultures[i]);
                FSDirectory luceneIndexDirectory = FSDirectory.Open(indexPath);
                Writer = new IndexWriter(luceneIndexDirectory, config);
               
                    
                Document d = new();

                foreach (Product item in products)
                {
                    d.Add(new StringField("ID", item.ProductId, Field.Store.YES));
                    d.Add(new StringField("UniqueCode", item.UniqueCode, Field.Store.YES));
                    d.Add(new StringField("Code", item.ProductCode.ToString(), Field.Store.YES));
                    d.Add(new TextField("EntityName", (item.MultiLingualProperties.Any(_ => _.LanguageId == lanId) ?
                            item.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == lanId).Name : ""), Field.Store.YES));
                    foreach (var grp in item.GroupIds)
                    {
                        d.Add(new StringField("GroupId", grp, Field.Store.YES));
                    }
                    foreach (var name in item.GroupNames)
                    {
                        d.Add(new TextField("GroupName", name, Field.Store.YES));
                    }
                    var tagKeywords = item.MultiLingualProperties.Any(_ => _.LanguageId == lanId) ?
                       item.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == lanId).TagKeywords : new List<string>();
                    foreach (var tag in tagKeywords)
                    {
                        d.Add(new TextField("TagKeyword", tag, Field.Store.YES));
                    }

                    Writer.AddDocument(d);
                }
               Writer.Commit();
               Writer.Dispose();
            }
           
        }

        public Result DeleteItemFromExistingIndex(string indexFullPath, string id)
        {
            var res = new Result();
            FSDirectory luceneIndexFullPath = FSDirectory.Open(indexFullPath);
            config = new IndexWriterConfig(Lv, Analyzer)
            {
                OpenMode = OpenMode.APPEND,
                WriteLockTimeout = Lock.LOCK_POLL_INTERVAL * 2
            };
           Writer = new IndexWriter(luceneIndexFullPath, config);
            try
            {
               
                Writer = new IndexWriter(luceneIndexFullPath, config);
                Writer.DeleteDocuments(new Term("ID", id));
                Writer.Commit();
                res.Succeeded = true;
            }
            catch (Exception)
            {
                res.Message = ConstMessages.InternalServerErrorMessage;
            }
            Writer.Dispose();
            return res;
        }

        public List<LuceneSearchIndexModel> Search(string searchword, List<string> directories)
        {
            var finalRes = new List<LuceneSearchIndexModel>();
            string[] searchFields = { "EntityName", "Code", "UniqueCode", "GroupName", "TagKeyword" };
            IndexSearcher searcher;
            foreach (var dir in directories)
            {
                var isProduct = dir.Replace("\\", "/").Contains("/Product");
                FSDirectory luceneIndexDirectory = FSDirectory.Open(dir);
                var dirReader = DirectoryReader.Open(luceneIndexDirectory);
                searcher = new IndexSearcher(dirReader);
                var multiFieldQP = new MultiFieldQueryParser(Lv, searchFields, Analyzer);
                Query query = multiFieldQP.Parse(searchword);
                ScoreDoc[] docs = searcher.Search(query, null, 1000).ScoreDocs;
                for (int i = 0; i < docs.Length; i++)
                {
                    var obj = new LuceneSearchIndexModel();
                    Document d = searcher.Doc(docs[i].Doc);
                    obj.IsProduct = isProduct;
                    obj.ID = d.Get("ID");
                    obj.GroupIds = d.GetValues("GroupId").ToList();
                    obj.GroupNames = d.GetValues("GroupName").ToList();
                    obj.UniqueCode = d.Get("UniqueCode");
                    obj.EntityName = d.Get("EntityName");
                    obj.TagKeywordList = d.GetValues("TagKeyword").ToList();
                    obj.Code = d.Get("Code");

                    finalRes.Add(obj);
                }
               
            }
            return finalRes;
        }

        public Result UpdateItemInIndex(string indexFullPath, string id, LuceneSearchIndexModel model, bool isProduct)
        {
            var res = new Result();
            FSDirectory luceneIndexDirectory = FSDirectory.Open(indexFullPath);
            config = new IndexWriterConfig(Lv, Analyzer)
            {
                OpenMode = OpenMode.CREATE,
                WriteLockTimeout = Lock.LOCK_POLL_INTERVAL * 2
            };
            Writer = new IndexWriter(luceneIndexDirectory, config);

            //code couldnt change in update 
            try
            {
              
                var d = new Document()
                {
                    new StringField("ID", id, Field.Store.YES),
                    new TextField("EntityName", model.EntityName, Field.Store.YES)
                };
                foreach (var grp in model.GroupIds)
                {
                    d.Add(new StringField("GroupId", grp, Field.Store.YES));
                }
                foreach (var name in model.GroupNames)
                {
                    d.Add(new TextField("GroupName", name, Field.Store.YES));
                }

                foreach (var tag in model.TagKeywordList)
                {
                    d.Add(new TextField("TagKeyword", tag, Field.Store.YES));
                }
                if(isProduct)
                {
                    d.Add(new StringField("UniqueCode", model.UniqueCode, Field.Store.YES));
                }
                Writer.UpdateDocument(new Term("ID", id), d);
                Writer.Commit();
                res.Succeeded = true;
            }
            catch (Exception)
            {
                res.Message = ConstMessages.ExceptionOccured;
            }
            return res;
        }
      
    }
}
