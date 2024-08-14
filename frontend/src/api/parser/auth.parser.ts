import {
  AuthenticateSSODTO,
  AuthToken,
  Credentials,
  ValidateDTO,
} from "../dto/auth.dto";

export const parseAuthenticateSSO = (dto: AuthenticateSSODTO): AuthToken => {
  return {
    accessToken: dto.access_token,
    refreshToken: dto.refresh_token,
    expiresIn: dto.expires_in,
  };
};

export const parseCredentials = (dto: ValidateDTO): Credentials => {
  return {
    userId: dto.user_id,
    email: dto.email,
    role: dto.role,
  };
};
