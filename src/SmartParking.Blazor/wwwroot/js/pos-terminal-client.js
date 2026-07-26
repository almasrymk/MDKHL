/**
 * SmartParking Client POS Terminal JS Client
 * Direct browser WebSocket communication with Geidea Web ECR (ws://localhost:5000/messages).
 */
(function () {
    const DEFAULT_WS_URL = "ws://localhost:5000/messages";
    let wsUrl = DEFAULT_WS_URL;
    let socket = null;
    let reconnectAttempts = 0;
    const MAX_RECONNECT_ATTEMPTS = 10;
    const RECONNECT_DELAY_MS = 2000;
    let reconnectTimer = null;
    let isExplicitDisconnect = false;

    // Active transaction pending promise correlation
    let activePendingTxn = null;

    // Optional event subscriber
    let eventCallback = null;

    function getWsUrl(url) {
        if (!url || typeof url !== "string" || url.trim() === "") {
            return DEFAULT_WS_URL;
        }
        let clean = url.trim();
        // Convert legacy http:// or https:// to ws://
        if (clean.startsWith("http://") || clean.startsWith("https://")) {
            clean = clean.replace(/^https?:\/\//i, "ws://");
        }
        if (!clean.includes("/messages")) {
            clean = clean.replace(/\/+$/, "") + "/messages";
        }
        return clean;
    }

    function connect(url) {
        isExplicitDisconnect = false;
        if (url) {
            wsUrl = getWsUrl(url);
        }

        if (socket && (socket.readyState === WebSocket.CONNECTING || socket.readyState === WebSocket.OPEN)) {
            console.log("[posTerminalClient] WebSocket already open or connecting to " + wsUrl);
            return Promise.resolve(true);
        }

        return new Promise((resolve) => {
            try {
                console.log("[posTerminalClient] Connecting to WebSocket: " + wsUrl);
                socket = new WebSocket(wsUrl);

                socket.onopen = () => {
                    console.log("[posTerminalClient] WebSocket connection established to " + wsUrl);
                    reconnectAttempts = 0;
                    if (reconnectTimer) {
                        clearTimeout(reconnectTimer);
                        reconnectTimer = null;
                    }
                    resolve(true);
                };

                socket.onmessage = (event) => {
                    handleMessage(event.data);
                };

                socket.onerror = (error) => {
                    console.error("[posTerminalClient] WebSocket error on " + wsUrl, error);
                };

                socket.onclose = (event) => {
                    console.warn(`[posTerminalClient] WebSocket closed (code=${event.code}, reason=${event.reason || "none"})`);
                    socket = null;

                    if (activePendingTxn) {
                        const pending = activePendingTxn;
                        activePendingTxn = null;
                        if (pending.timeoutTimer) clearTimeout(pending.timeoutTimer);
                        pending.resolve({
                            success: false,
                            status: "DISCONNECTED",
                            responseCode: "DISCONNECTED",
                            responseMessage: "WebSocket disconnected during transaction",
                            error: { code: "WEBSOCKET_CLOSED", message: "WebSocket connection closed" }
                        });
                    }

                    if (!isExplicitDisconnect && reconnectAttempts < MAX_RECONNECT_ATTEMPTS) {
                        reconnectAttempts++;
                        console.log(`[posTerminalClient] Scheduling reconnect attempt ${reconnectAttempts}/${MAX_RECONNECT_ATTEMPTS} in ${RECONNECT_DELAY_MS}ms...`);
                        reconnectTimer = setTimeout(() => {
                            connect(wsUrl);
                        }, RECONNECT_DELAY_MS);
                    }
                    resolve(false);
                };
            } catch (ex) {
                console.error("[posTerminalClient] Connection exception:", ex);
                resolve(false);
            }
        });
    }

    function disconnect() {
        isExplicitDisconnect = true;
        if (reconnectTimer) {
            clearTimeout(reconnectTimer);
            reconnectTimer = null;
        }
        if (socket) {
            console.log("[posTerminalClient] Disconnecting WebSocket...");
            socket.close();
            socket = null;
        }
    }

    function handleMessage(dataStr) {
        console.log("[posTerminalClient] Message received:", dataStr);
        let parsed = null;
        try {
            parsed = JSON.parse(dataStr);
        } catch (err) {
            console.warn("[posTerminalClient] Non-JSON message received:", dataStr);
            return;
        }

        if (eventCallback) {
            try {
                eventCallback(parsed, dataStr);
            } catch (e) {
                console.error("[posTerminalClient] Event callback error:", e);
            }
        }

        // Correlate TERMINAL_RESPONSE
        const eventName = (parsed && (parsed.EventName || parsed.Event || "")).toUpperCase();
        if (parsed && (eventName === "TERMINAL_RESPONSE" || eventName === "ONMESSAGE")) {
            let jsonResult = null;
            if (parsed.JsonResult) {
                try {
                    jsonResult = typeof parsed.JsonResult === "string" ? JSON.parse(parsed.JsonResult) : parsed.JsonResult;
                } catch (e) {
                    console.error("[posTerminalClient] Failed to parse JsonResult:", e);
                    jsonResult = parsed.JsonResult;
                }
            } else if (parsed.TransactionResponse || parsed.PrimaryAccountNumber) {
                jsonResult = parsed;
            }

            if (activePendingTxn && jsonResult) {
                const pending = activePendingTxn;
                activePendingTxn = null;
                if (pending.timeoutTimer) clearTimeout(pending.timeoutTimer);

                const responseCode = jsonResult.TransactionResponse || jsonResult.ResponseCode || jsonResult.responseCode || "";
                const responseMessage = jsonResult.TransactionResponseEnglish || jsonResult.ResponseMessage || jsonResult.ResponseCodeDescription || "";
                const isApproved = responseCode === "000" || (responseMessage && responseMessage.toUpperCase().includes("APPROVED"));

                const maskedPan = jsonResult.PrimaryAccountNumber || jsonResult.CardNumber || "";
                const authCode = jsonResult.TransactionAuthCode || jsonResult.AuthCode || "";
                const rrn = jsonResult.RetrievalReferenceNumber || jsonResult.RRNNumber || "";
                const cardScheme = jsonResult.CardScheme || jsonResult.CardNameEnglish || "";

                pending.resolve({
                    success: isApproved,
                    status: isApproved ? "APPROVED" : (responseCode ? `DECLINED_${responseCode}` : "DECLINED"),
                    responseCode: responseCode,
                    responseMessage: responseMessage || (isApproved ? "Transaction Approved" : "Transaction Declined"),
                    maskedPan: maskedPan,
                    authCode: authCode,
                    rrn: rrn,
                    cardScheme: cardScheme,
                    raw: parsed,
                    jsonResult: jsonResult
                });
            }
        }
    }

    async function ensureConnected(targetUrl) {
        if (!socket || socket.readyState !== WebSocket.OPEN) {
            console.log("[posTerminalClient] WebSocket not connected. Initiating connection...");
            await connect(targetUrl || wsUrl);
            let waited = 0;
            while (socket && socket.readyState === WebSocket.CONNECTING && waited < 3000) {
                await new Promise(r => setTimeout(r, 200));
                waited += 200;
            }
        }
        return socket && socket.readyState === WebSocket.OPEN;
    }

    async function sendPurchase(amount, ecrNumber, options) {
        const targetUrl = (options && options.url) ? options.url : wsUrl;
        const isReady = await ensureConnected(targetUrl);

        if (!isReady) {
            return {
                success: false,
                status: "CONNECTION_ERROR",
                responseCode: "CONNECTION_ERROR",
                responseMessage: "Could not connect to Geidea Web ECR service at " + wsUrl,
                error: { code: "ECR_UNREACHABLE", message: "Could not connect to Geidea Web ECR service at " + wsUrl }
            };
        }

        const timeoutMs = (options && options.timeoutMs) ? options.timeoutMs : 90000;
        let formattedAmount = "0.00";
        if (typeof amount === "number") {
            formattedAmount = amount.toFixed(2);
        } else if (typeof amount === "string") {
            const num = parseFloat(amount);
            formattedAmount = isNaN(num) ? amount : num.toFixed(2);
        }

        const purchasePayload = {
            Event: "TRANSACTION",
            Operation: "PURCHASE",
            Amount: formattedAmount,
            PrintSettings: (options && options.printSettings) ? String(options.printSettings) : "1",
            AppId: (options && options.appId) ? String(options.appId) : "11",
            ECRNumber: String(ecrNumber || "")
        };

        return sendCommand(purchasePayload, timeoutMs);
    }

    function sendCommand(payloadObj, timeoutMs = 90000) {
        return new Promise((resolve) => {
            if (!socket || socket.readyState !== WebSocket.OPEN) {
                resolve({
                    success: false,
                    status: "CONNECTION_ERROR",
                    responseCode: "NOT_CONNECTED",
                    responseMessage: "WebSocket is not open",
                    error: { code: "NOT_CONNECTED", message: "WebSocket is not open" }
                });
                return;
            }

            if (activePendingTxn) {
                if (activePendingTxn.timeoutTimer) clearTimeout(activePendingTxn.timeoutTimer);
                activePendingTxn.resolve({
                    success: false,
                    status: "SUPERSEDED",
                    responseCode: "SUPERSEDED",
                    responseMessage: "Transaction superseded by new command"
                });
                activePendingTxn = null;
            }

            const timeoutTimer = setTimeout(() => {
                if (activePendingTxn && activePendingTxn.timeoutTimer === timeoutTimer) {
                    activePendingTxn = null;
                    resolve({
                        success: false,
                        status: "TIMEOUT",
                        responseCode: "TIMEOUT",
                        responseMessage: `Transaction timed out after ${timeoutMs}ms`,
                        error: { code: "TRANSACTION_TIMEOUT", message: `Transaction timed out after ${timeoutMs}ms` }
                    });
                }
            }, timeoutMs);

            activePendingTxn = { resolve, timeoutTimer };

            const jsonStr = typeof payloadObj === "string" ? payloadObj : JSON.stringify(payloadObj);
            console.log("[posTerminalClient] Dispatching Web ECR payload:", jsonStr);
            try {
                socket.send(jsonStr);
            } catch (err) {
                clearTimeout(timeoutTimer);
                activePendingTxn = null;
                resolve({
                    success: false,
                    status: "SEND_ERROR",
                    responseCode: "SEND_ERROR",
                    responseMessage: err.message,
                    error: { code: "SOCKET_SEND_FAILED", message: err.message }
                });
            }
        });
    }

    function getStatus() {
        if (!socket) return "DISCONNECTED";
        switch (socket.readyState) {
            case WebSocket.CONNECTING: return "CONNECTING";
            case WebSocket.OPEN: return "CONNECTED";
            case WebSocket.CLOSING: return "CLOSING";
            case WebSocket.CLOSED: return "DISCONNECTED";
            default: return "UNKNOWN";
        }
    }

    window.posTerminalClient = {
        connect,
        disconnect,
        sendPurchase,
        purchase: sendPurchase,
        sendCommand,
        getStatus,
        setEventCallback: (cb) => { eventCallback = cb; }
    };
})();
