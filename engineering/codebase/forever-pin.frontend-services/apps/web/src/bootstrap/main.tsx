import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { GoogleOAuthProvider } from "@react-oauth/google";
import { BrowserRouter } from "react-router-dom";
import { ColorModeProvider } from "@wow-two-beta/ui/foundation/primitives";
import "@fontsource-variable/geist";
import "@fontsource-variable/geist-mono";
import { GOOGLE_CLIENT_ID, loadRuntimeConfig } from "@/integration/common";
import "./index.css";
import { App } from "./App";

loadRuntimeConfig()
  .then(() => createRoot(document.getElementById("root")!).render(
    <StrictMode>
      <ColorModeProvider>
        <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
          <BrowserRouter>
            <App />
          </BrowserRouter>
        </GoogleOAuthProvider>
      </ColorModeProvider>
    </StrictMode>,
  ))
  .catch(() => {
    document.getElementById("root")!.textContent = "Unable to load ForeverPin. Please reload the page.";
  });
