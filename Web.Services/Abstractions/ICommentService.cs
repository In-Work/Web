using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Data.Entities;
using Web.Services.Implementations;

namespace Web.Services.Abstractions
{
    public interface ICommentService
    {
        Task AddCommentByArticleIdAsync(Guid articleId, Guid userId, string commentText, CancellationToken token);
        Task<Comment?> GetCommentById(Guid commentId, CancellationToken token);
        Task<List<Comment>?> GetCommentsByArticleId(Guid articleId, CancellationToken token);
        Task DeleteCommentById(Guid commentId, CancellationToken token);
    }
}
