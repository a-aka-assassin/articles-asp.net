using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Articles.Models;
using Articles.Models.ViewModels;
using Microsoft.Data.Sqlite;

namespace Articles.Controllers;

public class ArticleController : Controller
{
    private readonly ILogger<ArticleController> _logger;
    private readonly IConfiguration _configuration;

    public ArticleController(ILogger<ArticleController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        var ArticleList = GetAllArticles();
        return View(ArticleList);
    }

    public IActionResult NewArticle()
    {
        return View();
    }

    public IActionResult ViewArticle(int id)
    {
        var article = GetArticleByID(id);
        var articleViewModel = new ArticlesViewModel();
        articleViewModel.SingleArticle = article;
        return View(articleViewModel);

    }

    public IActionResult EditArticle(int id)
    {
        var article = GetArticleByID(id);
        var articlesViewModel = new ArticlesViewModel();
        articlesViewModel.SingleArticle = article;
        return View(articlesViewModel);
        
    }

//Methods with DB
    internal ArticlesViewModel GetAllArticles()
    {
        List<Article> articles= new();

        using (SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("ArticleDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = "SELECT * FROM Article";
                using (var reader = command.ExecuteReader())
                {
                    if(reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            articles.Add(
                                new Article
                                {
                                    Id = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Body = reader.GetString(2),
                                    CreatedAt = reader.GetDateTime(3),
                                    UpdatedAt = reader.GetDateTime(4)

                                }
                            );
                        }
                    }else{
                        return new ArticlesViewModel { Articles = articles};
                    }
                }
            }
        }
    return new ArticlesViewModel { Articles = articles};
    }


    //Inserting New Article
    public ActionResult Insert(Article SingleArticle)
    {
        SingleArticle.CreatedAt = DateTime.Now;
        SingleArticle.UpdatedAt = DateTime.Now;

        using (SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("ArticleDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"INSERT INTO article (title, body, createdat, updatedat) VALUES ('{SingleArticle.Title}', '{SingleArticle.Body}', '{SingleArticle.CreatedAt}','{SingleArticle.UpdatedAt}')";

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    
                    Console.WriteLine(ex.Message);
                }
            }
            
        }
        return RedirectToAction("Index");
    }

    //Getting Single article
    internal Article GetArticleByID(int id)
    {
        Article article = new();
        using (SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("ArticleDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT * FROM Article WHERE id = '{id}'";

                using (var reader = command.ExecuteReader())
                {
                    if(reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            article.Id = reader.GetInt32(0);
                            article.Title = reader.GetString(1);
                            article.Body =  reader.GetString(2);
                            article.CreatedAt = reader.GetDateTime(3);
                            article.UpdatedAt = reader.GetDateTime(4);
                        }
                    }
                    else{
                        return article;
                    }
                }
            }
        }
        return article;
    }

    //Update Article
    public ActionResult Update(Article SingleArticle)
    {
        SingleArticle.UpdatedAt = DateTime.Now;
         using (SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("ArticleDataContext")))
        {
            using ( var command = connection.CreateCommand() )
            {
                connection.Open();
                command.CommandText = $"UPDATE Article SET title = '{SingleArticle.Title}', body = '{SingleArticle.Body}', updatedat = '{SingleArticle.UpdatedAt}' WHERE Id = '{SingleArticle.Id}'";
               try
               {
                command.ExecuteNonQuery();
               }catch(Exception ex)
               {
                Console.WriteLine(ex.Message);
               }
            }
            return RedirectToAction("Index");
        }

    }

    //Delete Article
    [HttpPost]
    public JsonResult Delete(int id)
    {
        using (SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("ArticleDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"DELETE FROM Article WHERE id = '{id}'";

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    
                    Console.WriteLine(ex.Message);
                }
            }

        }
        return Json(new object());
    }

}
