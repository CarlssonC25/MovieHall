$(document).ready(function () {
    console.log("animes.js geladen");
    // Create Modal öffnen
    $("#openCreateAnimeModal").click(function () {
        $.get("/Anime/CreateAnimePartial", function (data) {
            $("#animeModalContent").html(data);
            var modal = new bootstrap.Modal(document.getElementById('animeModal'));
            modal.show();

            $("#createAnimeForm").on("submit", function (e) {
                e.preventDefault();
                var formData = new FormData(this);


                $.ajax(
                    {
                    type: "POST",
                    url: "/Anime/CreateAnimePartial",
                    data: formData,
                    processData: false,
                    contentType: false,
                    success: function (res, status, xhr) {
                        const contentType = xhr.getResponseHeader("content-type");

                        if (contentType && contentType.indexOf("application/json") !== -1) {
                            if (res.success) {
                                modal.hide();
                                location.reload();
                            }
                        } else { // HTML → Validation Errors
                            console.log("Validation errors, rendering PartialView");
                            $("#animeModalContent").html(res);
                        }
                    }
                });
            });
        });
    });

    // Edit Modal öffnen
    $(".edit-anime-link").click(function () {
        var id = $(this).data("id");
        $.get("/Anime/EditAnimePartial?id=" + id, function (data) {
            $("#animeModalContent").html(data); // gleiche ID wie beim Create
            var modal = new bootstrap.Modal(document.getElementById('animeModal')); // gleiche Modal-ID
            modal.show();

            $("#editAnimeForm").on("submit", function (e) {
                e.preventDefault();
                var formData = new FormData(this);

                $.ajax({
                    type: "POST",
                    url: "/Anime/EditAnimePartial",
                    data: formData,
                    processData: false,
                    contentType: false,
                    success: function (res, status, xhr) {
                        const contentType = xhr.getResponseHeader("content-type");

                        if (contentType && contentType.indexOf("application/json") !== -1) {
                            if (res.success) {
                                modal.hide();
                                location.reload();
                            }
                        } else {
                            console.log("Validation errors, rendering Edit PartialView");
                            $("#animeModalContent").html(res); // auch hier anpassen
                        }
                    }
                });
            });
        });
    });

    // Delete
    $(".delete-anime-link").click(function () {
        var id = $(this).data("id");
        var name = $(this).data("name");
        $("#deleteAnimeId").val(id);
        $("#deleteAnimeName").text(name);
        var modal = new bootstrap.Modal(document.getElementById('deleteAnimeModal'));
        modal.show();
    });

    $("#deleteAnimeForm").on("submit", function (e) {
        e.preventDefault();
        var id = $("#deleteAnimeId").val();
        $.post("/Anime/DeleteAnimeConfirmed/" + id, function () {
            location.reload();
        });
    });


    // --------------------- Filter ---------------------

    // Language-Filter
    $(document).on("click", ".language", function () {
        $(".language").removeClass("aktiv-btn");
        $(this).addClass("aktiv-btn");
        loadAnime(1);
    });

    // Country-Filter
    $(document).on("click", ".country", function () {
        $(".country").removeClass("aktiv-btn");
        $(this).addClass("aktiv-btn");
        loadAnime(1);
    });

    // Rank-Filter
    $(document).on("click", ".rank", function () {
        if ($(this).hasClass("aktiv-btn")) {
            $(".rank").removeClass("aktiv-btn");
            $(".rank").each(function () {
                if ($(this).text() === "ALL") {
                    $(this).addClass("aktiv-btn");
                }
            });
            loadAnime(1);
        } else {
            $(".rank").removeClass("aktiv-btn");
            $(this).addClass("aktiv-btn");
            loadAnime(1);
        }
    });

    // Pagination-Click
    $(document).on("click", ".pagination-link", function (e) {
        e.preventDefault();

        let page = $(this).data("page");

        if (!page || $(this).hasClass("disabled")) {
            return;
        }

        loadAnimes(page); // eigene Funktion zum Laden
    });

});

function loadAnimes(page) {
    let language = $(".language.aktiv-btn").data("filter") || "ALL";
    let country = $(".country.aktiv-btn").data("filter") || "ALL";
    let rank = $(".rank.aktiv-btn").data("filter") || "ALL";

    $.ajax({
        url: "/Anime/Index?page=" + page +
             "&Cfilter=" + encodeURIComponent(country) +
             "&Lfilter=" + encodeURIComponent(language) +
             "&Rfilter=" + encodeURIComponent(rank),
        type: "GET",
        headers: { "X-Requested-With": "XMLHttpRequest" },
        success: function (data) {
            $("#animeContainer").html(data);
            $("html, body").animate({ scrollTop: $("#animeContainer").offset().top }, 300);
        },
        error: function () {
            alert("Fehler beim Laden der Anime-Liste.");
        }
    });
}