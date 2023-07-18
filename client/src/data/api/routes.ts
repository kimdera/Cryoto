export const apiEndpoint =
  process.env.VITE_API_BASE_URL || 'https://localhost:7175/';

export const clientEndpoint =
  process.env.VITE_CLIENT_BASE_URL || 'http://localhost:5173/';

// Authentication
export const clientEndpointAuth = `${clientEndpoint}authentication`;

// Posts
export const apiRoutePosts = `${apiEndpoint}posts`;
export const apiRoutePostsGetUserFeed = `${apiRoutePosts}/GetUserFeedPaginated`;
export const apiRoutePostsGetUserProfileFeed = `${apiRoutePosts}/GetUserProfileFeedPaginated`;
export const apiRoutePostsGetPostById = `${apiRoutePosts}/GetPostById`;
export const apiRoutePostsCreatePost = `${apiRoutePosts}/Create`;
export const apiRoutePostsReactPost = `${apiRoutePosts}/React`;
export const apiRoutePostsCommentOnPost = `${apiRoutePosts}/CommentOnPost`;
export const apiRouteCommentsDelete = `${apiRoutePosts}/DeleteComment`;
export const apiRoutePostsBoostPost = `${apiRoutePosts}/Boost`;

// UserProfile
export const apiRouteUserProfile = `${apiEndpoint}userProfile`;
export const apiRouteUserProfileGetUserProfile = `${apiRouteUserProfile}/GetUserProfile`;
export const apiRouteUserProfileGetUserProfilePhoto = `${apiRouteUserProfile}/GetUserProfilePhoto`;
export const apiRouteUserSearch = `${apiRouteUserProfile}/GetSearchResult`;
export const apiRouteUserProfileUpdate = `${apiRouteUserProfile}/Update`;
export const apiRouteUserProfileGetUpcomingAnniversaries = `${apiRouteUserProfile}/GetUpcomingAnniversaries`;
export const apiRouteUserProfileGetTopRecognizers = `${apiRouteUserProfile}/GetTopRecognizers`;

// Crypto
export const apiRouteCrypto = `${apiEndpoint}Crypto`;
export const apiRouteCryptoGetTokenBalance = `${apiRouteCrypto}/GetTokenBalance`;
export const apiRouteCryptoSelfTransferTokens = `${apiRouteCrypto}/SelfTransferTokens`;

// Transactions
export const apiRouteTransactions = `${apiEndpoint}Transaction`;
export const apiRouteTransactionsGetTransactionsBySenderOId = `${apiRouteTransactions}/GetTransactionsBySenderOId`;
export const apiRouteTransactionsGetTransactionsByReceiverOId = `${apiRouteTransactions}/GetTransactionsByReceiverOId`;

// Notifications
export const apiRouteNotifications = `${apiEndpoint}Notification`;
export const apiRouteNotificationsReadNotification = `${apiRouteNotifications}/ReadNotification`;
export const apiRouteNotificationsGetNotificationsPaginated = `${apiRouteNotifications}/GetNotificationsPaginated`;

// Address
export const apiRouteAddress = `${apiEndpoint}Address`;
export const apiRouteAddressGetDefaultAddressOrCreate = `${apiRouteAddress}/GetDefaultAddressOrCreate`;
export const apiRouteAddressGetDefaultAddress = `${apiRouteAddress}/GetDefaultAddress`;
export const apiRouteAddressAdd = `${apiRouteAddress}/Add`;
export const apiRouteAddressUpdate = `${apiRouteAddress}/Update`;

// MarketPlace
export const apiRouteMarketPlace = `${apiEndpoint}MarketPlace`;
export const apiRouteMarketPlaceGetAllItems = `${apiRouteMarketPlace}/GetAllItems`;
export const apiRouteMarketPlaceCompletePurchase = `${apiRouteMarketPlace}/CompletePurchase`;

// Admin
export const apiRouteAdmin = `${apiEndpoint}Admin`;
export const apiRouteAdminUserProfile = `${apiRouteAdmin}/UserProfile`;
export const apiRouteAdminUserProfileGetAllUsersRoles = `${apiRouteAdminUserProfile}/GetAllUsersRoles`;
export const apiRouteAdminUserProfileUpdateUserRoles = `${apiRouteAdminUserProfile}/UpdateUserRoles`;
export const apiRouteAdminUserProfileGetUserByID = `${apiRouteAdminUserProfile}/GetUserById`;
