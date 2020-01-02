var interval;
function EmailConfirmation(email) {
    if (window.WebSocket) {
        alert("Gniazda WebSockets są aktywne");
        openSocket(email, "Email");
    }
    else {
        alert("Gniazda WebSockets nie są aktywne");
        interval = setInterval(() => {
            CheckEmailConfirmationStatus(email);
        }, 5000);
    }
}

function GameInvitationConfirmation(id) {
    if (window.WebSocket) {
        alert("Gniazda WebSockets są aktywne");
        openSocket(id, "GameInvitation");
    }
    else {
        alert("Gniazda WebSockets nie są aktywne");
        interval = setInterval(() => {
            CheckGameInvitationConfirmationStatus(id);
        }, 5000);
    }
}