namespace HospitalApi.Dtos
{
    public class LoginRequestDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime AccessTokenExpiry { get; set; }
        public string Username { get; set; } = null!;
        public string Role { get; set; } = null!;
    }

    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; } = null!;
    }

    public class RevokeTokenRequestDto
    {
        public string RefreshToken { get; set; } = null!;
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string ConfirmNewPassword { get; set; } = null!;
    }

    public class ResetPasswordDto
    {
        public string Username { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
