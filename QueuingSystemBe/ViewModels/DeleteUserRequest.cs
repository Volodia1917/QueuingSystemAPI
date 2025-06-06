using System.ComponentModel.DataAnnotations;

namespace QueuingSystemBe.ViewModels
{
    public class DeleteUserRequest
    {


        public string? DeletedUser { get; set; }
        public DateTimeOffset? DeletedDate { get; set; }

    }
}
