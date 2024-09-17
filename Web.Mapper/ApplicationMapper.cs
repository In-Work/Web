using Riok.Mapperly.Abstractions;
using Web.Data.Entities;
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
    }
}
