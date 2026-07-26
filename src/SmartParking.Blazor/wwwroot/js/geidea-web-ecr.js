// Native WebSocket ES6 wrapper module for Blazor JSInterop.
// Delegates to authoritative window.posTerminalClient to maintain a single Web ECR WebSocket connection.

let dotNetHelper = null;

export function initialize(helper) {
    dotNetHelper = helper;
    if (window.posTerminalClient) {
        window.posTerminalClient.setEventCallback((parsedObj, rawMsg) => {
            if (dotNetHelper) {
                try {
                    dotNetHelper.invokeMethodAsync("OnMessageReceived", typeof rawMsg === "string" ? rawMsg : JSON.stringify(parsedObj));
                } catch (e) {
                    console.error("[Geidea ECR Interop] Error invoking OnMessageReceived:", e);
                }
            }
        });
    }
}

export function connect(url) {
    if (window.posTerminalClient) {
        return window.posTerminalClient.connect(url).then(success => {
            if (success && dotNetHelper) {
                dotNetHelper.invokeMethodAsync("OnSocketOpen");
            } else if (!success && dotNetHelper) {
                dotNetHelper.invokeMethodAsync("OnSocketError", "Failed to connect to " + url);
            }
            return success;
        });
    }
    return Promise.resolve(false);
}

export function disconnect() {
    if (window.posTerminalClient) {
        window.posTerminalClient.disconnect();
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync("OnSocketClose", 1000, "Clean disconnect");
        }
    }
}

export function send(payload) {
    if (window.posTerminalClient) {
        let payloadObj = payload;
        if (typeof payload === "string") {
            try { payloadObj = JSON.parse(payload); } catch (e) { }
        }
        window.posTerminalClient.sendCommand(payloadObj);
        return true;
    }
    return false;
}
