(function () {
    window.addEventListener("load", function () {
        const ui = window.ui;
        if (!ui) return;

        const originalAuthorize = ui.authActions.authorize;
        ui.authActions.authorize = function (payload) {
            const key = Object.keys(payload)[0];
            let token = payload[key].value;

            // Tự động thêm "Bearer " nếu chưa có
            if (token && !token.toLowerCase().startsWith("bearer ")) {
                token = "Bearer " + token;
            }

            const newPayload = {
                [key]: {
                    value: token
                }
            };

            return originalAuthorize(newPayload);
        };
    });
})();
