using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using API.Models.Posts;
using API.Services.Interfaces;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using static System.Threading.Tasks.Task;

namespace API.Services;

[ExcludeFromCodeCoverage]
public class CryotoBackgroundService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly QueueClient _queueClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CryotoBackgroundService> _logger;


    public CryotoBackgroundService(QueueClient queueClient,
        IServiceProvider serviceProvider, IConfiguration configuration, ILogger<CryotoBackgroundService> logger)
    {
        _queueClient = queueClient;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var cryptoService =
                scope.ServiceProvider.GetRequiredService<ICryptoService>();
            var notificationService =
                scope.ServiceProvider.GetRequiredService<INotificationService>();
            var queueMessage = await _queueClient.ReceiveMessageAsync(cancellationToken: stoppingToken);
            
            if(queueMessage.Value == null) continue;
            
            await ProcessQueue(queueMessage, cryptoService, notificationService, scope);

            await _queueClient.DeleteMessageAsync(queueMessage.Value.MessageId, queueMessage.Value.PopReceipt,
                stoppingToken);
            

            await Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessQueue(Response<QueueMessage> queueMessage, ICryptoService cryptoService,
        INotificationService notificationService, IServiceScope scope)
    {
        try
        {
            var messageList =
                JsonSerializer.Deserialize<List<List<string>>>(queueMessage.Value.MessageText);
            switch (messageList![0][0])
            {
                case "checkAdminBalanceQueue":
                {
                    await CaseCheckAdminBalanceQueue(cryptoService, notificationService);
                    break;
                }
                case "monthlyTokenQueue":
                {
                    await CaseMonthlyTokenQueue(messageList, cryptoService);

                    break;
                }
                case "tokenUpdateQueue":
                {
                    await CaseTokenUpdateQueue(messageList, cryptoService);

                    break;
                }
                case "anniversaryBonusQueue":
                {
                    await CaseAnniversaryBonusQueue(scope, cryptoService);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Error in queue message: {Message}", e.Message);
        }
        await Delay(TimeSpan.FromSeconds(5));
    }

    private static async Task CaseAnniversaryBonusQueue(IServiceScope scope, ICryptoService cryptoService)
    {
        var userProfileService = scope.ServiceProvider.GetRequiredService<IUserProfileService>();
        var postService = scope.ServiceProvider.GetRequiredService<IPostService>();
        var anniversaryUsers = await userProfileService.GetAnniversaryUsersAsync();
        foreach (var userProfileModel in anniversaryUsers)
        {
            var anniversaryBonus =
                await cryptoService.GetAnniversaryBonusAmountOfRoleByOIdAsync(userProfileModel.OId);
            
            string[] anniversaryMessages =
            {
                "Congratulations on this big occasion and many wishes for future success.",
                "Wishing you many years of success and innovations. Happy anniversary!",
                "Congratulations on this special day and many wishes for more great days ahead.",
                "We are thinking of you on this important day and wishing good luck as you take on your new ventures!",
                "Keep up the good work! Happy anniversary and here’s to many more.",
                "Congratulations! I think all your hard work calls for a party!",
                "If there were an award for the worker of the year – you’d win it! Congratulations and best wishes on your anniversary.",
                "Thank you for being part of our company’s success over the years. We greatly appreciate and value your hard work and success. Happy Anniversary.",
                "Thank you for being an essential part of our success. Happy Anniversary!",
                "On your anniversary, we appreciate all your hard work and dedication. Best wishes for another successful year."
            };
            var anniversaryMessage =
                anniversaryMessages[new Random().NextInt64(anniversaryMessages.Length)];
            await postService.CreateAsync(new PostModel("da6e9ec4-6c5d-45b3-bf3c-22ad1c4b9800",
                anniversaryMessage, new[] { userProfileModel.OId }, new[] { "Anniversary" },
                DateTimeOffset.UtcNow, "Anniversary", true,
                (ulong)Math.Round(Math.Abs(anniversaryBonus))));
            await cryptoService.SendAnniversaryTokenByOId(userProfileModel.OId);
        }

        await cryptoService.QueueAnniversaryBonusAsync(new List<List<string>>
            { new() { "anniversaryBonusQueue" }, new() { "null" } });
    }

    private static async Task CaseTokenUpdateQueue(List<List<string>> messageList, ICryptoService cryptoService)
    {
        foreach (var oid in messageList[1].Select((value, index) => new { index, value }))
            if (oid.index == 0)
            {
                var senderTokenBalance =
                    await cryptoService.GetSolanaTokenBalanceAsync(oid.value, "toAward");
                await cryptoService.UpdateSolanaTokenBalance(senderTokenBalance, oid.value,
                    "toAward");
            }
            else
            {
                var tokenBalance =
                    await cryptoService.GetSolanaTokenBalanceAsync(oid.value, "toSpend");
                await cryptoService.UpdateSolanaTokenBalance(tokenBalance, oid.value, "toSpend");
            }
    }

    private static async Task CaseMonthlyTokenQueue(List<List<string>> messageList, ICryptoService cryptoService)
    {
        var oid = messageList[1][0];
        var weekNumber = int.Parse(messageList[1][1]);
        if (weekNumber == 4)
        {
            await cryptoService.SendMonthlyTokenBasedOnRole(oid);
            await cryptoService.QueueMonthlyTokensGiftAsync(new List<List<string>>
                { new() { "monthlyTokenQueue" }, new() { oid, "1" } });
        }
        else
        {
            weekNumber += 1;
            await cryptoService.QueueMonthlyTokensGiftAsync(new List<List<string>>
            {
                new() { "monthlyTokenQueue" },
                new() { oid, weekNumber.ToString() }
            });
        }
    }

    private async Task CaseCheckAdminBalanceQueue(ICryptoService cryptoService, INotificationService notificationService)
    {
        var solanaAdminBalance = cryptoService.GetSolanaAdminBalance();
        if (solanaAdminBalance < double.Parse(_configuration["SolanaBalanceLowLimit"]))
        {
            _logger.LogWarning("Solana admin balance is too low");
        }

        var solanaAdminTokenBalance = cryptoService.GetSolanaAdminTokenBalance();
        if (solanaAdminTokenBalance < double.Parse(_configuration["SolanaTokenBalanceLowLimit"]))
        {
            const string messageHtml =
                "<h1>Solana token balance is too low</h1> <p>Hi " + "Cryoto Admin" +
                ",</p> <p>Solana token balance is less than 10000</p>";
            await notificationService.SendEmailAsync(_configuration["LowBalanceAdminEmail"],
                "Solana token balance alert",
                messageHtml, true);
        }

        await cryptoService.QueueSolUpdateAsync(new List<List<string>>
            { new() { "checkAdminBalanceQueue" }, new() { "null" } });
    }
}