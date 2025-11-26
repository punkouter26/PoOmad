using PoOmad.Shared.DTOs;

namespace PoOmad.Client.Services;

public class AuthStateService
{
    private UserInfoDto? _currentUser;

    public event Action? OnChange;

    public UserInfoDto? CurrentUser => _currentUser;

    public bool IsAuthenticated => _currentUser?.IsAuthenticated ?? false;

    public bool HasProfile => _currentUser?.HasProfile ?? false;

    public void SetUser(UserInfoDto? user)
    {
        _currentUser = user;
        NotifyStateChanged();
    }

    public void ClearUser()
    {
        _currentUser = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
