namespace HospitalApi.Dtos
{
    public class RegisterUserDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int RoleId { get; set; }  // 2=Admin,3=Doctor,4=User
    }
}
