using Azure.Data.Tables;
using MediatR;
using PoOmad.Api.Infrastructure.TableStorage;
using PoOmad.Shared.DTOs;

namespace PoOmad.Api.Features.Profile;

/// <summary>
/// Command to create a new user profile
/// </summary>
public record CreateProfileCommand(string GoogleId, string Email, string Height, decimal StartingWeight) : IRequest<UserProfileDto>;

public class CreateProfileHandler : IRequestHandler<CreateProfileCommand, UserProfileDto>
{
    private readonly TableStorageClient _tableStorage;
    private readonly ILogger<CreateProfileHandler> _logger;

    public CreateProfileHandler(TableStorageClient tableStorage, ILogger<CreateProfileHandler> logger)
    {
        _tableStorage = tableStorage;
        _logger = logger;
    }

    public async Task<UserProfileDto> Handle(CreateProfileCommand request, CancellationToken cancellationToken)
    {
        var tableClient = await _tableStorage.GetTableClientAsync("UserProfiles");

        var entity = new UserProfile
        {
            PartitionKey = request.GoogleId,
            RowKey = "profile",
            Email = request.Email,
            Height = request.Height,
            StartingWeight = request.StartingWeight,
            StartDate = DateTime.UtcNow
        };

        await tableClient.AddEntityAsync(entity, cancellationToken);

        _logger.LogInformation("Created profile for user {GoogleId}", request.GoogleId);

        return new UserProfileDto
        {
            GoogleId = request.GoogleId,
            Email = entity.Email,
            Height = entity.Height,
            StartingWeight = entity.StartingWeight,
            StartDate = entity.StartDate
        };
    }
}
