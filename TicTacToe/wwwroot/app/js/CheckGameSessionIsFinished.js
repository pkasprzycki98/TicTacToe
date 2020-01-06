function CheckGameSessionIsFinished() {
    var port = document.location.port ? (":" + document.location.port) : "";
    var url = document.location.protocol + "//" + document.location.hostname + port + "/restapi/v1/CheckGameSessionIsFinished/" + window.GameSessionId;

    $.get(url, function (data) {
        debugger;
        if (data.indexOf("wygrał") > 0 || data == "Gra zakończyła się remisem.") {
            alert(data);
            window.location.href = document.location.protocol + "//" + document.location.hostname + port;
        }
    });
}