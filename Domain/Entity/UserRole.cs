namespace Domain.Entity
{
    public class UserRole
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public Guid? AssignedById { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public User? User { get; set; }
        public Role? Role { get; set; }
        public User? AssignedBy { get; set; }
    }
}
