function signOut() {
    
}

function SearchDisplay() {
    if ($("#searchDiv").css("display") == "none") {
        $("#searchDiv").css("display", "block");
    } else {
        $("#searchDiv").css("display", "none");
    }
}

function Search() {
    var searchTerm = document.getElementById("searchInput").value;

    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {

            var res = JSON.parse(this.responseText);
            $("#resultDiv").empty();
            if (res.length > 0) {
                for (var i = 0; i < res.length; i++) {
                    var concertDiv = $("<div></div>");
                    concertDiv.css("display", "inline-block");
                    for (var prop in res[i]) {
                        var propP = $("<p></p>").text(res[i][prop]);
                        concertDiv.append(propP);
                    }
                    $("#resultDiv").append(concertDiv)
                }
            }
        }
    }

    xhr.open("POST", "http://127.0.0.10/api/Search/Get?searchParam=" + searchTerm, true);
    xhr.send();
}