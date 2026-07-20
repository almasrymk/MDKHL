// Native WebSocket ES6 wrapper module for Blazor JSInterop.
// Interacts with Geidea Web ECR Service (local Windows service on port 5000).
//
// MIXED-CONTENT NOTE:
// Browsers block ws:// (plain WebSocket) when the page is served over https://.
// The Geidea Web ECR service does NOT support TLS (wss://).
// Solution: when the page is on https://, we attempt the connection anyway on localhost
// because Chrome/Edge grant a mixed-content exemption for ws://localhost and ws://127.0.0.1
// per the "localhost is a secure context" rule (W3C Secure Contexts spec).
// If the browser does block it, onerror fires immediately and we report a clear message.

let socket = null;
let dotNetHelper = null;

export function initialize(helper) {
    dotNetHelper = helper;
}

export function connect(url) {
    if (socket && (socket.readyState === WebSocket.CONNECTING || socket.readyState === WebSocket.OPEN)) {
        console.warn("WebSocket already open or connecting to " + url);
        return;
    }

    // Detect mixed-content scenario: page on https://, service on ws://.
    // Per spec, ws://localhost and ws://127.0.0.1 are exempt from mixed-content blocking
    // because localhost is a "potentially trustworthy origin". Log a warning but proceed.
    const pageIsHttps = window.location.protocol === "https:";
    const urlObj = new URL(url);
    const isPlainWs = urlObj.protocol === "ws:";
    const isLocalhost = urlObj.hostname === "localhost" || urlObj.hostname === "127.0.0.1" || urlObj.hostname === "::1";

    if (pageIsHttps && isPlainWs && !isLocalhost) {
        // Non-localhost ws:// on https:// WILL be blocked. Report immediately.
        const msg = "MIXED_CONTENT: Page is on HTTPS but ECR service URL uses ws:// on a non-localhost host (" +
            urlObj.hostname + "). Change ServiceUrl to use wss://, or point it to localhost/127.0.0.1.";
        console.error("[Geidea ECR] " + msg);
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync("OnSocketError", msg);
        }
        return;
    }

    if (pageIsHttps && isPlainWs && isLocalhost) {
        console.warn("[Geidea ECR] Page is HTTPS but ECR uses ws:// on localhost. " +
            "Browsers should allow this per secure-context localhost exemption, but some browser configs may block it.");
    }

    try {
        console.log("[Geidea ECR] Connecting WebSocket to: " + url);
        socket = new WebSocket(url);

        socket.onopen = () => {
            console.log("[Geidea ECR] WebSocket connection established to " + url);
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync("OnSocketOpen");
            }
        };

        socket.onmessage = (event) => {
            console.log("[Geidea ECR] Message received:", event.data);
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync("OnMessageReceived", event.data);
            }
        };

        socket.onerror = (error) => {
            // onerror gives no detail in the browser for security reasons.
            // Distinguish likely causes by checking if socket never opened.
            const readyState = socket ? socket.readyState : -1;
            let msg;

            if (pageIsHttps && isPlainWs && isLocalhost) {
                msg = "ECR_SERVICE_UNREACHABLE: Could not connect to Geidea Web ECR service at " + url +
                    ". Ensure GeideaWebECR.exe is running on this machine (Task Manager → check process). " +
                    "If the process is running, check that port " + urlObj.port + " is not blocked by firewall. " +
                    "(Page is HTTPS + ws://localhost — browser should allow this, so the ECR service is likely not running.)";
            } else if (pageIsHttps && isPlainWs && !isLocalhost) {
                msg = "MIXED_CONTENT: Browser blocked ws:// connection from HTTPS page to non-localhost host. Use wss://.";
            } else {
                msg = "ECR_CONNECT_ERROR: WebSocket error connecting to " + url +
                    ". Ensure GeideaWebECR.exe is running and the URL is correct. ReadyState=" + readyState;
            }

            console.error("[Geidea ECR] " + msg, error);
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync("OnSocketError", msg);
            }
        };

        socket.onclose = (event) => {
            // Close code 1006 = abnormal closure (connection refused / mixed-content / network error).
            let closeDetail = "code=" + event.code + ", reason=" + (event.reason || "(none)") + ", wasClean=" + event.wasClean;
            if (event.code === 1006) {
                closeDetail += " | Code 1006 = abnormal close — ECR service not running, connection refused, or mixed-content block.";
            }
            console.log("[Geidea ECR] WebSocket closed: " + closeDetail);
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync("OnSocketClose", event.code, closeDetail);
            }
            socket = null;
        };

    } catch (ex) {
        const msg = "ECR_EXCEPTION: " + ex.message;
        console.error("[Geidea ECR] Failed to initialize WebSocket:", ex);
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync("OnSocketError", msg);
        }
    }
}

export function disconnect() {
    if (socket) {
        console.log("[Geidea ECR] Closing WebSocket connection explicitly...");
        socket.close();
        socket = null;
    }
}

export function send(payload) {
    if (socket && socket.readyState === WebSocket.OPEN) {
        console.log("[Geidea ECR] Sending payload:", payload);
        socket.send(payload);
        return true;
    } else {
        console.error("[Geidea ECR] Failed to send: WebSocket is not open. ReadyState=" + (socket ? socket.readyState : "null"));
        return false;
    }
}
