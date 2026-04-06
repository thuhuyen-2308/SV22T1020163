namespace SV22T1020163.Models.Security
{
    public class UserAccount
    {
        public string UserID { get; set; } = "";
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Photo { get; set; }
        public string RoleNames { get; set; } = "";
    }
}
