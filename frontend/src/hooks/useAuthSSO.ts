import { useEffect, useState } from "react";
import { authenticateSSO, validate } from "../api/auth";
import { AuthToken, Credentials } from "../api/dto/auth.dto";

interface AuthSSOPayload {
  setAuthToken: React.Dispatch<React.SetStateAction<AuthToken>>;
  authToken: AuthToken;
  credentials: Credentials;
  loading: boolean;
  error: Error | null;
}

export const useAuthSSO = (serviceTicket: string | null): AuthSSOPayload => {
  const expiresInString = localStorage.getItem("expires_in") || "";
  const accessToken = localStorage.getItem("access_token") || "";
  const initState: AuthToken = {
    accessToken: accessToken,
    refreshToken: localStorage.getItem("refresh_token") || "",
    expiresIn: new Date(expiresInString),
  };

  const [authToken, setAuthToken] = useState<AuthToken>(initState);
  const [credentials, setCredentials] = useState<Credentials>({
    userId: "",
    email: "",
    role: "",
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    setLoading(true);
    if (serviceTicket)
      (async () => {
        const res = await authenticateSSO(serviceTicket);

        if (res instanceof Error) {
          return setError(res);
        }

        localStorage.setItem("access_token", res.accessToken);
        localStorage.setItem("refresh_token", res.refreshToken);
        localStorage.setItem("expires_in", res.expiresIn.toString());
        setAuthToken(res);
      })();
    else
      (async () => {
        const res = await validate(accessToken);

        if (res instanceof Error) {
          localStorage.removeItem("access_token");
          localStorage.removeItem("refresh_token");
          localStorage.removeItem("expires_in");
          setAuthToken({
            accessToken: "",
            refreshToken: "",
            expiresIn: new Date(),
          });
          return;
        }

        setCredentials(res);
      })();
    setLoading(false);
  }, [serviceTicket, accessToken]);

  return { setAuthToken, authToken, credentials, loading, error };
};
