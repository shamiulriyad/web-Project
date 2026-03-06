using Microsoft.AspNetCore.Authorization;

namespace Backend.API.Security
{
    public sealed class AdminOnlyAttribute : AuthorizeAttribute
    {
        public AdminOnlyAttribute() => Roles = "Admin";
    }

    public sealed class SellerOnlyAttribute : AuthorizeAttribute
    {
        public SellerOnlyAttribute() => Roles = "Seller";
    }

    public sealed class CustomerOnlyAttribute : AuthorizeAttribute
    {
        public CustomerOnlyAttribute() => Roles = "User";
    }
}