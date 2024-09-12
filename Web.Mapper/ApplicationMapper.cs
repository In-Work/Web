using Riok.Mapperly.Abstractions;
using Web.Data.Entities;
using Web.Models;

namespace Web.Mapper
{
    [Mapper]
    public static partial class ApplicationMapper
    {
        [MapProperty(nameof(Article.Source), nameof(ArticleModel.SourceModel))]
        [MapProperty(nameof(Article.Comments), nameof(ArticleModel.CommentsModels))]
        public static partial ArticleModel ArticleToArticleModel(Article article);

        [MapProperty(nameof(Article.Source), nameof(ArticleModel.SourceModel))]
        [MapProperty(nameof(Article.Comments), nameof(ArticleModel.CommentsModels))]
        public static partial List<ArticleModel> ArticleListToArticleModelList(List<Article> articles);

        [MapProperty(nameof(ArticleModel.SourceModel), nameof(Article.Source))]
        [MapProperty(nameof(ArticleModel.CommentsModels), nameof(Article.Comments))]
        public static partial Article ArticleModelToArticle(ArticleModel articleModel);

        [MapProperty(nameof(ArticleModel.SourceModel), nameof(Article.Source))]
        [MapProperty(nameof(ArticleModel.CommentsModels), nameof(Article.Comments))]
        public static partial List<Article> ArticleModelListToArticleList(List<ArticleModel> articleModels);

        [MapProperty(nameof(Comment.User), nameof(CommentModel.UserModel))]
        public static partial CommentModel CommentToCommentModel(Comment comment);
        [MapProperty(nameof(CommentModel.UserModel), nameof(Comment.User))]
        public static partial Comment CommentModelToComment(CommentModel commentModel);

        public static partial SourceModel SourceToSourceModel(Source source);
        public static partial Source SourceModelToSource(SourceModel sourceModel);

        public static partial UserModel UserModelToUser(User user);
        public static partial User UserToUserModel(UserModel userModel);
    }
}
