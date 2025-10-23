$(document).ready(function () {
    let $input = $("#searchInput");
    let $results = $("#searchResults");

    let controller = $("body").data("controller"); // controller prüfen "Anime", "Movie", "ItemView"
    let type = $("body").data("type");            // "Anime" oder "Movie" bei ItemView


    // Autocomplete
    $input.on("keyup", function () {
        let query = $input.val().trim();
        if (query.length < 2) {
            $results.hide();
            return;
        }

        let url = "";
        // Normal auf Controller prüfen
        if (controller === "Anime" || type === "Anime") {
            url = "/Search/AutoCompleteAnime";
        } else if (controller === "Movie" || type === "Movie") {
            url = "/Search/AutoCompleteMovie";
        } else {
            $results.hide();
            return;
        }

        $.getJSON(url, { query: query }, function (data) {
            $results.empty();

            if (!data.results || data.results.length === 0) {
                $results.hide();
                return;
            }

            data.results.forEach(r => {
                $results.append(`
                    <a class="d-flex align-items-center m-1" href="/ItemView/Index/${r.id}?type=${(type || controller)}">
                        <img src="/img/${(type || controller)}_imgs/${r.img}" alt="${r.name}"/>
                        <span>${r.name}</sapn>
                    </a>
                `);
            });

            $results.show();
        });
    });

    // Such-Button
    $("#searchButton").on("click", function (e) {
        e.preventDefault();

        let query = encodeURIComponent($input.val().trim());
        let dataType = $("body").data("type"); 

        // Weiterleitung zur Suchseite
        window.location.href = `/Search/Index?query=${query}&type=${dataType}`;
    });

    $input.on("keypress", function (e) {
        if (e.which === 13) { // Enter-Taste
            e.preventDefault();
            $("#searchButton").click();
        }
    });
});
