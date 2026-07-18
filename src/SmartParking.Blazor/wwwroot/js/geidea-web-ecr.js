// Native WebSocket ES6 wrapper module for Blazor JSInterop.
// Interacts with Geidea Web ECR Service.

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

    try {
        socket = new WebSocket(url);

        socket.onopen = () => {
            console.log("WebSocket connection established to " + url);
            // WebSocket state is ready. Blazor will send the CONNECT request.
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync("OnSocketOpen");
            }
        };

        socket.onmessage = (event) => {
            console.log("WebSocket message received:", event.data);
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync("OnMessageReceived", event.data);
            }
        };

        socket.onerror = (error) => {
            console.error("WebSocket error:", error);
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync("OnSocketError", "WebSocket connection error");
            }
        };

        socket.onclose = (event) => {
            console.log(`WebSocket closed: code=${event.code}, reason=${event.reason}`);
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync("OnSocketClose", event.code, event.reason);
            }
            socket = null;
        };

    } catch (ex) {
        console.error("Failed to initialize WebSocket:", ex);
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync("OnSocketError", ex.message);
        }
    }
}

export function disconnect() {
    if (socket) {
        console.log("Closing WebSocket connection explicitly...");
        socket.close();
        socket = null;
    }
}

export function send(payload) {
    if (socket && socket.readyState === WebSocket.OPEN) {
        console.log("Sending payload to WebSocket:", payload);
        socket.send(payload);
        return true;
    } else {
        console.error("Failed to send: WebSocket is not open.");
        return false;
    }
}
