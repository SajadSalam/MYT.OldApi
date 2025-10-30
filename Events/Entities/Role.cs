namespace Events.Entities
{
    public class Role : BaseEntity<int>
    {
            public string Name { get; set; }
            public List<RolePermission> RolePermissions { get; set; }

            // fix 
    }
}