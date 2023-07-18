using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Controllers;
using API.Crypto.Solana.SolanaObjects;
using API.Models.Comments;
using API.Models.Posts;
using API.Models.Users;
using API.Services.Interfaces;
using API.Tests.Utils;
using API.Utils;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Xunit;

namespace API.Tests.ControllersTests;

public class PostsControllerTests
{
    private readonly ICommentService _commentService;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly PostsController _controller;
    private readonly ICryptoService _cryptoService;
    private readonly ILogger<PostsController> _logger;
    private readonly INotificationService _notificationService;
    private readonly IPostService _postService;
    private readonly ITransactionService _transactionService;
    private readonly IUserProfileService _userProfileService;


    public PostsControllerTests()
    {
        _logger = A.Fake<ILogger<PostsController>>();
        _cryptoService = A.Fake<ICryptoService>();
        _postService = A.Fake<IPostService>();
        _transactionService = A.Fake<ITransactionService>();
        _contextAccessor = A.Fake<IHttpContextAccessor>();
        _notificationService = A.Fake<INotificationService>();
        _userProfileService = A.Fake<IUserProfileService>();
        _commentService = A.Fake<ICommentService>();
        _controller = new PostsController(_postService, _cryptoService, _transactionService, _contextAccessor,
            _notificationService, _userProfileService, _commentService, _logger);
    }

    private List<PostModel> GetFakePosts()
    {
        var post1 = new PostModel(
            "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            "Test Message",
            new[] { "6fa88f64-5717-4562-b3fc-2c963f66afa6" },
            new[] { "teamwork", "leadership" },
            DateTimeOffset.Now,
            "General",
            true,
            100
        );
        var post2 = new PostModel(
            "6fa88f64-5717-4562-b3fc-2c963f66afa6",
            "Some Message",
            new[] { "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
            new[] { "work", "loyalty" },
            DateTimeOffset.Now,
            "General",
            true,
            50
        );
        return new List<PostModel>
        {
            post1,
            post2
        };
    }

    private PostModel GetFakePost()
    {
        return new PostModelBuilder().BuildDefaultFakePost();
    }

    private PostsController GetControllerWithIodContext(string iod)
    {
        var mockController = new PostsController(_postService, _cryptoService, _transactionService, _contextAccessor,
            _notificationService, _userProfileService, _commentService, _logger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            HttpContext =
            {
                User = new ClaimsPrincipal()
            }
        };

        // adding claims
        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim("oid", iod));
        mockController.HttpContext.User.AddIdentity(claims);

        return mockController;
    }

    [Fact]
    public async Task PostsController_GetUserFeedPaginated_ReturnsOk()
    {
        // Arrange
        var posts = GetFakePosts();
        var postsPaginated = new PaginationWrapper<PostModel>(posts, 1, posts.Count, 1);

        A.CallTo(() => _postService
                .GetUserFeedPaginatedAsync(A<int>.Ignored, A<int>.Ignored))
            .Returns(postsPaginated);
        // Act
        var actionResult = await _controller.GetUserFeedPaginated();

        var objectResult = actionResult.Result as ObjectResult;
        var objectResultValue = objectResult?.Value as PaginationWrapper<PostModel>;

        // Assert
        objectResult.Should().NotBeNull();
        objectResult.Should().BeOfType(typeof(OkObjectResult));
        Assert.Equal(posts, objectResultValue?.Data.ToList());
    }

    [Fact]
    public async Task PostsController_GetPostById_ReturnsOk()
    {
        // Arrange
        var post = GetFakePost();
        var postId = post.Id;

        A.CallTo(() => _postService.GetByIdAsync(postId)).Returns(post);

        // Act
        var actionResult = await _controller.GetById(postId);

        var objectResult = actionResult.Result as ObjectResult;
        var objectResultValue = objectResult?.Value as PostModel;

        // Assert
        objectResult.Should().NotBeNull();
        objectResult.Should().BeOfType(typeof(OkObjectResult));
        Assert.Equal(post, objectResultValue);
    }

    [Fact]
    public async Task PostsController_GetPostById_ReturnsNotFound()
    {
        // Arrange
        PostModel? post = null;

        A.CallTo(() => _postService.GetByIdAsync(A<string>._)).Returns(post);

        // Act
        var actionResult = await _controller.GetById(A.Dummy<string>());

        var objectResult = actionResult.Result as NotFoundResult;

        // Assert
        objectResult.Should().NotBeNull();
        objectResult.Should().BeOfType(typeof(NotFoundResult));
    }

    [Fact]
    public async Task PostController_Create_ReturnsOk()
    {
        // Arrange
        var post = GetFakePost();
        var balance = post.Coins * 10000;
        var rpcTransactionResult = GetRpcTransactionResultSuccessful();

        A.CallTo(() => _postService.CreateAsync(A<PostModel>.Ignored)).Returns(true);
        A.CallTo(() => _postService.GetByIdAsync(A<string>._)).Returns(post);
        A.CallTo(() => _cryptoService.GetTokenBalanceAsync(A<string>._, A<string>._)).Returns(balance);
        A.CallTo(() => _cryptoService.SendTokens(A<double>._, A<string>._, A<string>._))!.Returns(rpcTransactionResult);
        var postCreateModel = new PostCreateModel(post.Message, post.Recipients, post.Tags, post.CreatedDate,
            post.PostType, post.IsTransactable, post.Coins);

        var mockController = GetControllerWithIodContext(post.Author);

        // Act
        var actionResult = await mockController.Create(postCreateModel);

        var objectResult = actionResult.Result as ObjectResult;
        var objectResultValue = objectResult?.Value as PostModel;

        // Assert
        objectResult.Should().NotBeNull();
        objectResult.Should().BeOfType(typeof(OkObjectResult));

        Assert.Equal(post.Message, objectResultValue?.Message);
        Assert.Equal(post.Author, objectResultValue?.Author);
        Assert.Equal(post.Coins, objectResultValue?.Coins);
        Assert.Equal(post.Tags, objectResultValue?.Tags);
        Assert.Equal(post.Recipients, objectResultValue?.Recipients);
    }

    [Fact]
    public async Task PostController_React_ReturnsConflict()
    {
        // Arrange
        PostModel? existingPost = null;
        A.CallTo(() => _postService.GetByIdAsync(A<string>._)).Returns(existingPost);

        var mockController = GetControllerWithIodContext(A.Dummy<string>());

        // Act
        var actionResult = await mockController.React(A.Dummy<int>(), A.Dummy<string>());

        var objectResult = actionResult.Result as ConflictObjectResult;

        // Assert
        objectResult.Should().NotBeNull();
        objectResult.Should().BeOfType(typeof(ConflictObjectResult));
    }

    [Fact]
    public async Task PostController_React_ReturnsBadRequest()
    {
        // Arrange
        var post = GetFakePost();
        A.CallTo(() => _postService.GetByIdAsync(A<string>._)).Returns(post);

        A.CallTo(() => _postService.ReactAsync(0, A<string>._, A<string>._)).Returns(false);

        var mockController = GetControllerWithIodContext(A.Dummy<string>());

        // Act
        var actionResult = await mockController.React(A.Dummy<int>(), A.Dummy<string>());

        var objectResult = actionResult.Result as BadRequestObjectResult;

        // Assert
        objectResult.Should().NotBeNull();
        objectResult.Should().BeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task PostController_React_ReturnsOK()
    {
        // Arrange
        var post = GetFakePost();
        A.CallTo(() => _postService.GetByIdAsync(A<string>._)).Returns(post);

        A.CallTo(() => _postService.ReactAsync(0, A<string>._, A<string>._)).Returns(true);

        var mockController = GetControllerWithIodContext(A.Dummy<string>());

        // Act
        var actionResult = await mockController.React(A.Dummy<int>(), A.Dummy<string>());

        var objectResult = actionResult.Result as OkObjectResult;

        // Assert
        objectResult.Should().NotBeNull();
        objectResult.Should().BeOfType(typeof(OkObjectResult));
        objectResult?.Value.Should().BeOfType(typeof(PostModel));
        Assert.Equal(objectResult?.Value, post);
    }

    [Fact]
    public async Task Boost_SuccessfulBoost()
    {
        // Arrange
        var existingPost = new PostModel("authorId", "message", new[] { "rec1", "rec2" }, new[] { "tag" },
            DateTimeOffset.UtcNow);
        var actorProfile = new UserProfileModel("actorId", "name", "email", "en", new[] { "role" });

        A.CallTo(() => _postService.GetByIdAsync(A<string>._)).Returns(existingPost);
        A.CallTo(() => _userProfileService.GetUserByIdAsync(A<string>._)).Returns(actorProfile);
        A.CallTo(() => _cryptoService.BoostRecognition(A<string>._, A<List<string>>._)).Returns(true);
        A.CallTo(() => _postService.BoostAsync(A<string>._, A<UserProfileModel>._)).Returns(true);
        A.CallTo(() => _notificationService.SendBoostNotification(A<string>._, A<PostModel>._))
            .Returns(true);
        var mockController = GetControllerWithIodContext(A.Dummy<string>());

        // Act
        var actionResult = await mockController.Boost("guid");
        var objectResult = actionResult.Result as OkObjectResult;

        // Assert
        objectResult.Should().NotBeNull();
        objectResult.Should().BeOfType(typeof(OkObjectResult));
        objectResult?.Value.Should().BeOfType(typeof(PostModel));
        Assert.Equal(objectResult?.Value, existingPost);
    }

    [Fact]
    public async Task Boost_NonExistingPost_ReturnsBadRequest()
    {
        // Arrange
        PostModel? existingPost = null;
        var actorProfile = new UserProfileModel("actorId", "name", "email", "en", new[] { "role" });

        A.CallTo(() => _postService.GetByIdAsync(A<string>._)).Returns(existingPost);
        A.CallTo(() => _userProfileService.GetUserByIdAsync(A<string>._)).Returns(actorProfile);
        A.CallTo(() => _cryptoService.BoostRecognition(A<string>._, A<List<string>>._)).Returns(true);
        A.CallTo(() => _postService.BoostAsync(A<string>._, A<UserProfileModel>._)).Returns(true);
        A.CallTo(() => _notificationService.SendBoostNotification(A<string>._, A<PostModel>._))
            .Returns(true);
        var mockController = GetControllerWithIodContext(A.Dummy<string>());

        // Act
        var actionResult = await mockController.Boost("guid");
        var objectResult = actionResult.Result as BadRequestObjectResult;

        // Assert
        objectResult.Should().NotBeNull();
        objectResult.Should().BeOfType(typeof(BadRequestObjectResult));
        Assert.Equal("Cannot boost to the post because it does not exist.", objectResult!.Value);
    }

    [Fact]
    public async Task Boost_NonExistingUser_ReturnsBadRequest()
    {
        // Arrange
        var existingPost = new PostModel("authorId", "message", new[] { "rec1", "rec2" }, new[] { "tag" },
            DateTimeOffset.UtcNow);
        UserProfileModel? actorProfile = null;

        A.CallTo(() => _postService.GetByIdAsync(A<string>._)).Returns(existingPost);
        A.CallTo(() => _userProfileService.GetUserByIdAsync(A<string>._)).Returns(actorProfile);
        A.CallTo(() => _cryptoService.BoostRecognition(A<string>._, A<List<string>>._)).Returns(true);
        A.CallTo(() => _postService.BoostAsync(A<string>._, A<UserProfileModel>._)).Returns(true);
        A.CallTo(() => _notificationService.SendBoostNotification(A<string>._, A<PostModel>._))
            .Returns(true);
        var mockController = GetControllerWithIodContext(A.Dummy<string>());

        // Act
        var actionResult = await mockController.Boost("guid");
        var objectResult = actionResult.Result as BadRequestObjectResult;

        // Assert
        objectResult.Should().NotBeNull();
        objectResult.Should().BeOfType(typeof(BadRequestObjectResult));
        Assert.Equal("Cannot boost the post because user does not exist.", objectResult!.Value);
    }

    [Fact]
    public async Task Boost_FailedTransaction_ReturnsBadRequest()
    {
        // Arrange
        var existingPost = new PostModel("authorId", "message", new[] { "rec1", "rec2" }, new[] { "tag" },
            DateTimeOffset.UtcNow);
        var actorProfile = new UserProfileModel("actorId", "name", "email", "en", new[] { "role" });

        A.CallTo(() => _postService.GetByIdAsync(A<string>._)).Returns(existingPost);
        A.CallTo(() => _userProfileService.GetUserByIdAsync(A<string>._)).Returns(actorProfile);
        A.CallTo(() => _cryptoService.BoostRecognition(A<string>._, A<List<string>>._)).Returns(false);
        A.CallTo(() => _postService.BoostAsync(A<string>._, A<UserProfileModel>._)).Returns(true);
        A.CallTo(() => _notificationService.SendBoostNotification(A<string>._, A<PostModel>._))
            .Returns(true);
        var mockController = GetControllerWithIodContext(A.Dummy<string>());

        // Act
        var actionResult = await mockController.Boost("guid");
        var objectResult = actionResult.Result as BadRequestObjectResult;

        // Assert
        objectResult.Should().NotBeNull();
        objectResult.Should().BeOfType(typeof(BadRequestObjectResult));
        Assert.Equal("Could not complete boost transaction.", objectResult!.Value);
    }

    [Fact]
    public async Task Boost_ReturnsBadRequest()
    {
        // Arrange
        var existingPost = new PostModel("authorId", "message", new[] { "rec1", "rec2" }, new[] { "tag" },
            DateTimeOffset.UtcNow);
        var actorProfile = new UserProfileModel("actorId", "name", "email", "en", new[] { "role" });

        A.CallTo(() => _postService.GetByIdAsync(A<string>._)).Returns(existingPost);
        A.CallTo(() => _userProfileService.GetUserByIdAsync(A<string>._)).Returns(actorProfile);
        A.CallTo(() => _cryptoService.BoostRecognition(A<string>._, A<List<string>>._)).Returns(true);
        A.CallTo(() => _postService.BoostAsync(A<string>._, A<UserProfileModel>._)).Returns(false);
        A.CallTo(() => _notificationService.SendBoostNotification(A<string>._, A<PostModel>._))
            .Returns(true);
        var mockController = GetControllerWithIodContext(A.Dummy<string>());

        // Act
        var actionResult = await mockController.Boost("guid");
        var objectResult = actionResult.Result as BadRequestObjectResult;

        // Assert
        objectResult.Should().NotBeNull();
        objectResult.Should().BeOfType(typeof(BadRequestObjectResult));
        Assert.Equal("Could not boost the post.", objectResult!.Value);
    }

    [Fact]
    public async Task CommentController_Delete_ReturnsOk()
    {
        //Arrange
        const string id = "oidValue";

        var contextAccessor = A.Fake<IHttpContextAccessor>();
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimConstants.ObjectId, id) });

        var controller = new PostsController(_postService, _cryptoService, _transactionService, _contextAccessor,
            _notificationService, _userProfileService, _commentService, _logger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            HttpContext = { User = new ClaimsPrincipal() }
        };

        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim(ClaimConstants.ObjectId, id));
        claims.AddClaim(new Claim("oid", id));
        claims.AddClaim(new Claim("name", "namValue"));
        claims.AddClaim(new Claim("preferred_username", "email"));
        claims.AddClaim(new Claim("roles", "Admin"));
        controller.HttpContext.User.AddIdentity(claims);

        var commentModel = A.Fake<CommentModel>();

        var guid = Guid.NewGuid();
        commentModel.Id = guid.ToString();

        commentModel.Author = id;
        A.CallTo(() => contextAccessor.HttpContext).Returns(A.Fake<HttpContext>());
        A.CallTo(() => _commentService.GetCommentById(A<string>.That.Matches(x => x == commentModel.Id)))
            .Returns(commentModel);
        A.CallTo(() => _commentService.DeleteComment(A<CommentModel>.That.Matches(x => x == commentModel)))
            .Returns(Task.FromResult(true));
        A.CallTo(() => contextAccessor.HttpContext!.User.Identity).Returns(identity);

        //Act
        var actionResult = await controller.DeleteComment(guid);

        //Assert
        actionResult.Should().NotBeNull();
        A.CallTo(() => _commentService.GetCommentById(A<string>.That.Matches(x => x == commentModel.Id)))
            .MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task GetUserFeedPaginated_Returns_PaginationWrapper_Of_PostModels()
    {
        // Arrange
        var page = 1;
        var pageCount = 10;
        var postService = A.Fake<IPostService>();
        var expectedPosts = Enumerable.Range(1, 20).Select(i => new PostModel($"authorId{i}", "message", new[] { "rec1", "rec2" }, new[] { "tag" },
            DateTimeOffset.UtcNow));
        
        A.CallTo(() => postService.GetUserFeedPaginatedAsync(page, pageCount)).Returns(
            new PaginationWrapper<PostModel>(expectedPosts, 1, pageCount, 2));
        
        var controller = new PostsController(
            postService, A.Fake<ICryptoService>(), A.Fake<ITransactionService>(), A.Fake<IHttpContextAccessor>(),
            A.Fake<INotificationService>(), A.Fake<IUserProfileService>(), A.Fake<ICommentService>(), A.Fake<ILogger<PostsController>>());

        // Act
        var result = await controller.GetUserFeedPaginated(page, pageCount);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var paginationWrapper = result.Result.As<OkObjectResult>().Value as PaginationWrapper<PostModel>;
        
        Assert.NotNull(paginationWrapper);
        Assert.Equal(2, paginationWrapper.TotalPages);
        Assert.Equal(1, paginationWrapper.Page);
        Assert.Equal(10, paginationWrapper.ItemsPerPage);
    }

    [Fact]
    public async Task CommentOnPost_Sends_Comment_Notification_And_Returns_Comment_Model()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var actorId = "actor-id";
        var commentCreateModel = new CommentCreateModel("message", "");
        var postService = A.Fake<IPostService>();
        var notificationService = A.Fake<INotificationService>();
        var postModel = new PostModel("authorId", "message", new[] { "rec1", "rec2" }, new[] { "tag" },
            DateTimeOffset.UtcNow);
        A.CallTo(() => postService.GetByIdAsync(postId.ToString())).Returns(postModel);
        A.CallTo(() => postService.CommentOnPostAsync(postModel, A<CommentModel>._)).Returns(true);
        A.CallTo(() => notificationService.SendCommentNotification(actorId, postId.ToString(), postModel)).Returns(true);
        var controller = new PostsController(
            postService, A.Fake<ICryptoService>(), A.Fake<ITransactionService>(), A.Fake<IHttpContextAccessor>(),
            notificationService, A.Fake<IUserProfileService>(), A.Fake<ICommentService>(), A.Fake<ILogger<PostsController>>());
        
        controller.ActorId = actorId;

        // Act
        var result = await controller.CommentOnPost(commentCreateModel, postId);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var commentModel = (result.Result as OkObjectResult)?.Value as CommentModel;
        Assert.NotNull(commentModel);
        A.CallTo(() => postService.CommentOnPostAsync(postModel, commentModel)).MustHaveHappenedOnceExactly();
        A.CallTo(() => notificationService.SendCommentNotification(actorId, postId.ToString(), postModel)).MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task Create_WithRecipientContainingActorId_ReturnsBadRequest()
    {
        // Arrange
        var postCreateModel = new PostCreateModel("message", new []{ "rec1", "rec2" }, new []{ "tag" }, DateTimeOffset.Now, "Kudos", true, 100);
        _controller.ActorId = "rec1";
        
        
        // Act
        var result = await _controller.Create(postCreateModel);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not create the post", (result.Result as BadRequestObjectResult)?.Value);
    }
    
    [Fact]
    public async Task Create_PostServiceCreateAsyncReturnsFalse_ReturnsBadRequest()
    {
        // Arrange
        var postCreateModel = new PostCreateModel("message", new []{ "rec1", "rec2" }, new []{ "tag" }, DateTimeOffset.Now, "Kudos", true, 100);

        A.CallTo(() => _postService.CreateAsync(A<PostModel>._)).Returns(false);
        
        _controller.ActorId = "rec3";

        // Act
        var result = await _controller.Create(postCreateModel);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not create the post", (result.Result as BadRequestObjectResult)?.Value);
    }
    


    private Task<RpcTransactionResult> GetRpcTransactionResultSuccessful()
    {
        var rpcTransactionResult = new RpcTransactionResult
        {
            result = A.Dummy<string>()
        };
        return Task.FromResult(rpcTransactionResult);
    }
}