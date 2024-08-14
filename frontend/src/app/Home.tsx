import { useLocation, useNavigate } from "react-router-dom";
import { logout } from "../api/auth";
import { Button } from "../components/Button";
import { BG_COLOR, SERVICE, SSO_URL, WEB_URL } from "../constant/constant";
import { useAuthSSO } from "../hooks/useAuthSSO";

function Home() {
  const location = useLocation();
  const navigate = useNavigate();
  const queryParams = new URLSearchParams(location.search);
  const serviceTicket = queryParams.get("ticket") ?? "";

  const { setAuthToken, authToken, credentials, loading, error } =
    useAuthSSO(serviceTicket);

  if (serviceTicket) {
    navigate("/");
  }

  const handleClick = () => {
    window.location.href = `${SSO_URL}?service=${WEB_URL}`;
  };

  const handleLogout = async () => {
    if (!authToken.accessToken) return;

    try {
      await logout(authToken.accessToken);
      localStorage.removeItem("access_token");
      localStorage.removeItem("refresh_token");
      localStorage.removeItem("expires_in");
      setAuthToken((prev) => ({
        ...prev,
        accessToken: "",
      }));
    } catch (error) {
      console.error("Failed to logout: ", error);
    }
  };

  const SSOLoginStatus = () => {
    if (authToken.accessToken)
      return (
        <>
          <h3 className="mt-4 text-2xl font-medium">Logged in as</h3>
          <p className="mt-1">{credentials.email}</p>
          <Button onClick={handleLogout}>Logout</Button>
        </>
      );

    return <Button onClick={handleClick}>Login via SSO</Button>;
  };

  return (
    <div
      style={{ backgroundColor: BG_COLOR }}
      className="flex h-screen w-screen items-center justify-center"
    >
      <div className="flex h-[40vh] w-[60vw] flex-col items-center justify-center rounded-xl bg-white px-8 py-[10vh] drop-shadow-xl md:w-[40vw] xl:w-[30vw] 2xl:w-[20vw]">
        <h1 className="text-4xl font-bold">{SERVICE}</h1>
        {SSOLoginStatus()}

        {loading && <p className="mt-4 text-gray-500">Loading...</p>}
        {error && <p className="mt-4 text-red-500">{error.message}</p>}
      </div>
    </div>
  );
}

export default Home;
