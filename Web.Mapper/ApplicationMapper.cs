using Riok.Mapperly.Abstractions;
using Web.Data.Entities;
using Web.DTOs;
using Web.Models;

namespace Web.Mapper
{
    [Mapper]
    public static partial class ApplicationMapper
    {
        [MapProperty(nameof(Article.Source.Title), nameof(ArticleModel.SourceName))]
        public static partial ArticleModel ArticleToArticleModel(Article? article);

        [MapProperty(nameof(Article.Source.Title), nameof(ArticleModel.SourceName))]
        public static partial List<ArticleModel> ArticleListToArticleModelList(List<Article> articles);

        [MapProperty(nameof(Comment.User.Name), nameof(CommentModel.UserName))]
        public static partial List<CommentModel> CommentsModelsToCommentsList(List<Comment>? commentsList);

        [MapProperty(nameof(Article.Id), nameof(ArticleDto.Id))]
        [MapProperty([nameof(Article.Source), nameof(Article.Source.Title)],
            [nameof(ArticleDto.SourceName)])]
        public static partial ArticleDto? ArticleToArticleDto(Article? article);

        [MapProperty(nameof(ArticleDto.Id), nameof(ArticleModel.Id))]
        public static partial ArticleModel? ArticleDtoToArticleModel(ArticleDto? articleDto);

        public static partial Article? ArticleDtoToArticle(ArticleDto? articleDto);

    }
}
