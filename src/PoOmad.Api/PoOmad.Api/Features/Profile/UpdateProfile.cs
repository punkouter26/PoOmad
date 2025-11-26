using Azure;
using MediatR;
using PoOmad.Api.Infrastructure.TableStorage;
using PoOmad.Shared.DTOs;

namespace PoOmad.Api.Features.Profile;

/// <summary>
/// Command to update existing user profile
/// </summary>
public record UpdateProfileCommand(string GoogleId, string Height, decimal StartingWeight) : IRequest<UserProfileDto>;

public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, UserProfileDto>
{
    private readonly TableStorageClient _tableStorage;
    private readonly ILogger<UpdateProfileHandler> _logger;

    public UpdateProfileHandler(TableStorageClient tableStorage, ILogger<UpdateProfileHandler> logger)
    {
        _tableStorage = tableStorage;
        _logger = logger;
    }

    public async Task<UserProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var tableClient = await _tableStorage.GetTableClientAsync("UserProfiles");

        try
        {
            var existingEntity = await tableClient.GetEntityAsync<UserProfile>(
                request.GoogleId,
                "profile",
                cancellationToken: cancellationToken);

            var entity = existingEntity.Value;
            entity.Height = request.Height;
            entity.StartingWeight = request.StartingWeight;

            await tableClient.UpdateEntityAsync(entity, entity.ETag, cancellationToken: cancellationToken);

            _logger.LogInformation("Updated profile for user {GoogleId}", request.GoogleId);

            return new UserProfileDto
            {
                GoogleId = entity.PartitionKey,
                Email = entity.Email,
                Height = entity.Height,
                StartingWeight = entity.StartingWeight,
                StartDate = entity.StartDate
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Profile not found for user {GoogleId}", request.GoogleId);
            throw new InvalidOperationException($"Profile not found for user {request.GoogleId}");
        }
    }
}
