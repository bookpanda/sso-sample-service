using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class User
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string? ID { get; set; }
    public required string CASID { get; set; }
}
