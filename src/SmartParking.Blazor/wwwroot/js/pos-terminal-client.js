window.posTerminalClient = {
    sendCommand: async function (serviceBaseUrl, organizationId, apiKey, commandPayload) {
        const baseUrl = (serviceBaseUrl || "http://127.0.0.1:5005").replace(/\/+$/, '');
        const targetUrl = `${baseUrl}/api/pos/commands`;
        const reqId = commandPayload.requestId || commandPayload.RequestId || "req-" + Date.now();

        console.log(`[posTerminalClient] Dispatching HTTP POST to ${targetUrl}`, { organizationId, command: commandPayload.command });

        try {
            const response = await fetch(targetUrl, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "X-Organization-Id": String(organizationId),
                    "X-API-Key": apiKey
                },
                body: JSON.stringify(commandPayload)
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.warn(`[posTerminalClient] HTTP ${response.status} Error:`, errorText);
                return {
                    requestId: reqId,
                    success: false,
                    status: `HTTP_${response.status}`,
                    error: { code: `HTTP_${response.status}`, message: errorText || `HTTP status ${response.status}` }
                };
            }

            const data = await response.json();
            console.log(`[posTerminalClient] Command correlated response received:`, data);
            return data;
        } catch (err) {
            console.error(`[posTerminalClient] Local POS Service fetch failed:`, err);
            return {
                requestId: reqId,
                success: false,
                status: "CONNECTION_ERROR",
                error: { code: "FETCH_ERROR", message: err.message || "Failed to communicate with local POS service" }
            };
        }
    }
};
