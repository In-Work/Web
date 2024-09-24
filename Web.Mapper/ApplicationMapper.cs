using Riok.Mapperly.Abstractions;
using Web.Data.Entities;
using Web.DTOs;
using Web.Models;

namespace Web.Mapper
{
    [Mapper]
    public static partial class ApplicationMapper
    {
        public static  List<ArticleModel> ArticleListToArticleModelList(List<Article> articles)
        {
            var models = new List<ArticleModel>(articles.Count);
            foreach (var article in articles)
            {
                var model = ArticleToArticleModel(article);
                models.Add(model);
            }
            return models;
        }

        [MapProperty([nameof(Article.Source), nameof(Article.Source.Title)],
            [nameof(ArticleModel.SourceName)])]
        public static partial ArticleModel ArticleToArticleModel(Article? article);

        [MapProperty(nameof(Comment.User.Name), nameof(CommentModel.UserName))]
        public static partial List<CommentModel> CommentsModelsToCommentsList(List<Comment>? commentsList);

        [MapProperty(nameof(Article.Id), nameof(ArticleDto.Id))]
        [MapProperty([nameof(Article.Source), nameof(Article.Source.Title)],
            [nameof(ArticleDto.SourceName)])]
        public static partial ArticleDto? ArticleToArticleDto(Article? article);

        [MapProperty(nameof(Article.Id), nameof(ArticleDto.Id))]
        [MapProperty([nameof(Article.Source), nameof(Article.Source.Title)],
            [nameof(ArticleDto.SourceName)])]
        public static partial List<ArticleDto?> ArticlesToArticlesDto(List<Article?> article);
        public static partial ArticleModel? ArticleDtoToArticleModel(ArticleDto? articleDto);

        public static partial Article? ArticleDtoToArticle(ArticleDto? articleDto);


        [MapProperty(nameof(User.Name), nameof(UserSettingsModel.UserName))]
        public static partial UserSettingsModel UserToUserSettingsModel(User user);
        
        public static partial List<UserRolesModel> UsersToUserRolesModel(List<User> users);
        
    }
}
