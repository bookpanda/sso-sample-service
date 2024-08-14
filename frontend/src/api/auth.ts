import { AxiosResponse } from "axios";
import { apiClient } from "./axios";
import { AuthenticateSSODTO, ValidateDTO } from "./dto/auth.dto";
import { parseAuthenticateSSO, parseCredentials } from "./parser/auth.parser";

export const authenticateSSO = async (serviceTicket: string) => {
  try {
    const res: AxiosResponse<AuthenticateSSODTO> = await apiClient.get(
      "/auth/auth-sso",
      {
        params: { ticket: serviceTicket },
      }
    );

    return parseAuthenticateSSO(res.data);
  } catch (error) {
    console.error("Failed to authenticate SSO: ", error);

    return Error("Failed to authenticate SSO");
  }
};

export const validate = async (accessToken: string) => {
  try {
    const res: AxiosResponse<ValidateDTO> = await apiClient.get(
      "/auth/validate",
      {
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
      }
    );

    return parseCredentials(res.data);
  } catch (error) {
    console.error("Failed to validate: ", error);

    return Error("Failed to validate");
  }
};

export const logout = async (accessToken: string) => {
  try {
    const res: AxiosResponse<string> = await apiClient.post("/auth/signout", {
      access_token: accessToken,
    });

    return res.data;
  } catch (error) {
    console.error("Failed to logout: ", error);

    return Error("Failed to logout");
  }
};
