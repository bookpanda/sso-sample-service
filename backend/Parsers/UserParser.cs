using backend.DTO;
using backend.Models;

namespace backend.Parsers;

public class UserParser
{
    public static UserDTO ModelToDTO(User user) =>
        new UserDTO
        {
            ID = user.ID ?? string.Empty,
            CASID = user.CASID
        };
}