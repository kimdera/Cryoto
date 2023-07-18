using API.Models.Comments;
using API.Models.Posts;
using API.Models.Users;
using API.Utils;

namespace API.Services.Interfaces;

public interface IPostService
{
    public Task<PostModel?> GetByIdAsync(string guid);
    public Task<bool> CreateAsync(PostModel postModel);
    public Task<bool> UpdateAsync(PostModel postModel);
    public Task<bool> DeleteByIdAsync(string guid);
    public Task<PaginationWrapper<PostModel>> GetUserFeedPaginatedAsync(int page, int pageCount);
    public Task<PaginationWrapper<PostModel>> GetUserProfileFeedPaginatedAsync(string userId, int page, int pageCount);
    Task<bool> ReactAsync(int type, string guid, string actorId);
    Task<bool> CommentOnPostAsync(PostModel postModel, CommentModel commentModel);
    Task<bool> BoostAsync(string guid, UserProfileModel userProfile);
    Task<bool> UnboostAsync(string guid, UserProfileModel userProfile);
}