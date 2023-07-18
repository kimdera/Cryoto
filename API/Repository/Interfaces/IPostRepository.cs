using API.Models.Comments;
using API.Models.Posts;
using API.Utils;

namespace API.Repository.Interfaces;

public interface IPostRepository
{
    public Task<PostModel?> GetByIdAsync(string guid);
    public Task<bool> CreateAsync(PostModel postModel);
    public Task<bool> UpdateAsync(PostModel postModel);
    public Task<bool> DeleteAsyncById(string guid);
    Task<PaginationWrapper<PostModel>> GetAllByDatePaginatedAsync(int page, int pageCount, string oid = "oid");
    Task<bool> ReactAsync(int type, string guid, string actorId);
    Task<bool> CommentOnPostAsync(PostModel postModel, CommentModel commentModel);
    Task<bool> BoostAsync(string guid, string actorId);
    Task<bool> UnboostAsync(string guid, string actorId);
}