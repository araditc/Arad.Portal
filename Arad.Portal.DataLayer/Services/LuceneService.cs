using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Arad.Portal.DataLayer.Entities.General.Content;
using Arad.Portal.DataLayer.Entities.Shop.Product;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

using Serilog;

namespace Arad.Portal.DataLayer.Services;
//public interface ILuceneService
//{
//    Result AddItemToExistingIndex(string indexFullPath, LuceneSearchIndexModel model, bool isProduct);
//    void BuildContentIndexesPerLanguage(IList<Content> contents, string indexFullPath);
//    void BuildProductIndexesPerLanguage(IList<Product> products, string mainPath);
//    Result DeleteItemFromExistingIndex(string indexFullPath, string id);
//    /// <summary>
//    /// this part search on all lucene indexes
//    /// </summary>
//    /// <param name="search word"></param>
//    /// <returns></returns>
//    List<LuceneSearchIndexModel> Search(string search word, List<string> directories);

//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="indexFullPath"></param>
//    /// <param name="id"></param>
//    /// <param name="model"></param>
//    /// <param name="isProduct">if it isn't product it is content</param>
//    /// <returns></returns>
//    Result UpdateItemInIndex(string indexFullPath, string id, LuceneSearchIndexModel model, bool isProduct);
//}
public class LuceneService(ILanguageRepository lanRepository)
{
    private const LuceneVersion Lv = LuceneVersion.LUCENE_48;
    private static readonly Analyzer _analyzer = new StandardAnalyzer(Lv);
    private readonly string[] _supportedCultures = ["fa-IR", "en-US"];
    private IndexWriterConfig _config;

    private IndexWriter Writer { get; set; }

    public List<LuceneSearchIndexModel> Data { get; set; }

    public Result AddItemToExistingIndex(string indexFullPath, LuceneSearchIndexModel model, bool isProduct)
    {
        Result res = new();
        FSDirectory luceneContentIndexDirectory = FSDirectory.Open(indexFullPath);

        // Initialize the IndexWriterConfig with APPEND mode
        _config = new(Lv, _analyzer)
        {
            OpenMode = OpenMode.APPEND, // Append to existing index
            WriteLockTimeout = Lock.LOCK_POLL_INTERVAL * 2 // Timeout for acquiring lock
        };

        try
        {
            // Use `using` statement for automatic disposal of the IndexWriter
            using (Writer = new(luceneContentIndexDirectory, _config))
            {
                // Create a new Lucene Document
                Document d =
                [
                    new StringField("ID", model.Id, Field.Store.YES),
                    new TextField("EntityName", model.EntityName, Field.Store.YES),
                    new StringField("Code", model.Code, Field.Store.YES)

                    // Add group IDs
                ];

                // Add group IDs
                foreach (string grp in model.GroupIds)
                {
                    d.Add(new StringField("GroupId", grp, Field.Store.YES));
                }

                // Add group names
                foreach (string item in model.GroupNames)
                {
                    d.Add(new TextField("GroupName", item, Field.Store.YES));
                }

                // Add tag keywords
                foreach (string tag in model.TagKeywordList)
                {
                    d.Add(new TextField("TagKeyword", tag, Field.Store.YES));
                }

                // If it's a product, add the UniqueCode field
                if (isProduct)
                {
                    d.Add(new StringField("UniqueCode", model.UniqueCode, Field.Store.YES));
                }

                // Add the document to the index
                Writer.AddDocument(d);
                Writer.Commit();

                res.Succeeded = true; // Indicate success
            }
        }
        catch (IOException ioEx)
        {
            // Handle I/O exceptions related to file operations
            Log.Fatal("I/O Error in AddItemToExistingIndex: " + ioEx.Message);
            res.Message = "Error in accessing the index file.";
        }
        catch (Exception ex)
        {
            // Handle any other exceptions
            Log.Fatal("Error in LuceneService AddItemToExistingIndex: " + ex.Message + "******" + ex.InnerException);
            res.Message = ConstMessages.ErrorInSaving;
        }

        return res;
    }

    public void BuildContentIndexesPerLanguage(IList<Content> contents, string indexFullPath)
    {
        FSDirectory luceneContentIndexDirectory = FSDirectory.Open(indexFullPath);
        _config = new(Lv, _analyzer) { OpenMode = OpenMode.CREATE, WriteLockTimeout = Lock.LOCK_POLL_INTERVAL * 2 };
        Writer = new(luceneContentIndexDirectory, _config);

        try
        {
            foreach (Content item in contents)
            {
                Document d =
                [
                    new StringField("ID", item.Id, Field.Store.YES),
                    new TextField("EntityName", item.Title, Field.Store.YES),
                    new StringField("Code", item.ContentCode.ToString(), Field.Store.YES),
                    new StringField("GroupId", item.ContentCategoryId, Field.Store.YES),
                    new TextField("GroupName", item.ContentCategoryName, Field.Store.YES)
                ];

                foreach (string tag in item.TagKeywords)
                {
                    d.Add(new TextField("TagKeyword", tag, Field.Store.YES));
                }

                Writer.AddDocument(d);
            }

            Writer.Commit();

            Writer.Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            throw;
        }
     
    }

    public void BuildProductIndexesPerLanguage(IList<Product> products, string mainPath)
    {
        for (int i = 0; i < _supportedCultures.Length; i++)
        {
            _config = new(Lv, _analyzer) { OpenMode = OpenMode.CREATE, WriteLockTimeout = Lock.LOCK_POLL_INTERVAL * 2 };
            int i1 = i;
            string lanId = lanRepository.First(c => c.Symbol == _supportedCultures[i1]).Id;
            string indexPath = Path.Combine(mainPath, _supportedCultures[i]);
            FSDirectory luceneIndexDirectory = FSDirectory.Open(indexPath);
            Writer = new(luceneIndexDirectory, _config);

            foreach (Product item in products)
            {
                Document d =
                [
                    new StringField("ID", item.Id, Field.Store.YES),
                    new StringField("UniqueCode", item.UniqueCode, Field.Store.YES),
                    new StringField("Code", item.ProductCode.ToString(), Field.Store.YES),
                    new TextField("EntityName", item.MultiLingualProperties.Any(p => p.LanguageId == lanId) ? item.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId)?.Name : "", Field.Store.YES)
                ];

                foreach (string grp in item.GroupIds)
                {
                    d.Add(new StringField("GroupId", grp, Field.Store.YES));
                }

                foreach (string name in item.GroupNames)
                {
                    d.Add(new TextField("GroupName", name, Field.Store.YES));
                }

                List<string> tagKeywords = item.MultiLingualProperties.Any(p => p.LanguageId == lanId) ? item.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId)?.TagKeywords : [];

                if (tagKeywords != null)
                {
                    foreach (string tag in tagKeywords)
                    {
                        d.Add(new TextField("TagKeyword", tag, Field.Store.YES));
                    }
                }

                Writer.AddDocument(d);
            }

            Writer.Commit();
            Writer.Dispose();
        }
    }

    public void DeleteItemFromExistingIndex(string indexFullPath, string id)
    {
        Result res = new();
        FSDirectory luceneIndexFullPath = FSDirectory.Open(indexFullPath);
        _config = new(Lv, _analyzer) { OpenMode = OpenMode.APPEND, WriteLockTimeout = Lock.LOCK_POLL_INTERVAL * 2 };
        Writer = new(luceneIndexFullPath, _config);

        try
        {
            Writer = new(luceneIndexFullPath, _config);
            Writer.DeleteDocuments(new Term("ID", id));
            Writer.Commit();
            res.Succeeded = true;
        }
        catch (Exception ex)
        {
            Log.Fatal("Error in luceneService DeleteItemFromExistingIndex" + ex + "******" + ex.InnerException);
            res.Message = ConstMessages.InternalServerErrorMessage;
        }

        Writer.Dispose();
    }

    public List<LuceneSearchIndexModel> Search(string searchWord, List<string> directories)
    {
        List<LuceneSearchIndexModel> finalRes = [];
        string[] searchFields = ["EntityName", "Code", "UniqueCode", "GroupName", "TagKeyword"];

        foreach (string dir in directories)
        {
            bool isProduct = dir.Replace("\\", "/").Contains("/Product");
            FSDirectory luceneIndexDirectory = FSDirectory.Open(dir);
            DirectoryReader dirReader = DirectoryReader.Open(luceneIndexDirectory);
            IndexSearcher searcher = new(dirReader);
            MultiFieldQueryParser multiFieldQp = new(Lv, searchFields, _analyzer);
            Query query = multiFieldQp.Parse(searchWord);
            ScoreDoc[] docs = searcher.Search(query, null, 1000).ScoreDocs;

            foreach (ScoreDoc t in docs)
            {
                LuceneSearchIndexModel obj = new();
                Document d = searcher.Doc(t.Doc);
                obj.IsProduct = isProduct;
                obj.Id = d.Get("ID");
                obj.GroupIds.Add(d.GetValues("GroupId").ToString());
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
        Result res = new();
        FSDirectory luceneIndexDirectory = FSDirectory.Open(indexFullPath);
        _config = new(Lv, _analyzer) { OpenMode = OpenMode.CREATE, WriteLockTimeout = Lock.LOCK_POLL_INTERVAL * 2 };
        Writer = new(luceneIndexDirectory, _config);

        //code couldn't change in update 
        try
        {
            Document d = [new StringField("ID", id, Field.Store.YES), new TextField("EntityName", model.EntityName, Field.Store.YES)];

            foreach (string grp in model.GroupIds)
            {
                d.Add(new StringField("GroupId", grp, Field.Store.YES));
            }

            foreach (string name in model.GroupNames)
            {
                d.Add(new TextField("GroupName", name, Field.Store.YES));
            }

            foreach (string tag in model.TagKeywordList)
            {
                d.Add(new TextField("TagKeyword", tag, Field.Store.YES));
            }

            if (isProduct)
            {
                d.Add(new StringField("UniqueCode", model.UniqueCode, Field.Store.YES));
            }

            Writer.UpdateDocument(new("ID", id), d);
            Writer.Commit();
            Writer.Dispose();
            res.Succeeded = true;
        }
        catch (Exception ex)
        {
            Log.Fatal("Error in luceneService UpdateItemInIndex" + ex + "******" + ex.InnerException);
            res.Message = ConstMessages.ExceptionOccured;
        }

        return res;
    }
}