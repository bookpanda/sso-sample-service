using System.Text.Json.Serialization;

namespace backend.DTO;

public class UserDTO
{
    [JsonPropertyName("id")]
    public required string ID { get; set; }
    [JsonPropertyName("cas_id")]
    public required string CASID { get; set; }
}

public class CreateUserDTO
{
    [JsonPropertyName("cas_id")]
    public required string CASID { get; set; }
}
