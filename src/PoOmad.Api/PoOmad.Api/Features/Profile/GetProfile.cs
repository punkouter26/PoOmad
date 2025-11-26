using Azure;
using MediatR;
using PoOmad.Api.Infrastructure.TableStorage;
using PoOmad.Shared.DTOs;

namespace PoOmad.Api.Features.Profile;

/// <summary>
/// Query to get user profile by GoogleId
/// </summary>
public record GetProfileQuery(string GoogleId) : IRequest<UserProfileDto?>;

public class GetProfileHandler : IRequestHandler<GetProfileQuery, UserProfileDto?>
{
    private readonly TableStorageClient _tableStorage;
    private readonly ILogger<GetProfileHandler> _logger;

    public GetProfileHandler(TableStorageClient tableStorage, ILogger<GetProfileHandler> logger)
    {
        _tableStorage = tableStorage;
        _logger = logger;
    }

    public async Task<UserProfileDto?> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tableClient = await _tableStorage.GetTableClientAsync("UserProfiles");
            var response = await tableClient.GetEntityAsync<UserProfile>(
                request.GoogleId,
                "profile",
                cancellationToken: cancellationToken);

            var entity = response.Value;

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
            _logger.LogInformation("Profile not found for user {GoogleId}", request.GoogleId);
            return null;
        }
    }
}
