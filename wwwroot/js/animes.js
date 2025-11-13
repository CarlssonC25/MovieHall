// Filter Hilfsfunktion
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


function bindCreateAnimeForm(modal) {
    $("#createAnimeForm").on("submit", function (e) {
        e.preventDefault();
        var formData = new FormData(this);

        $.ajax({
            type: "POST",
            url: "/Anime/CreateAnimePartial",
            data: formData,
            processData: false,
            contentType: false,
            success: function (res, status, xhr) {
                const contentType = xhr.getResponseHeader("content-type");

                if (contentType && contentType.indexOf("application/json") !== -1) {
                    // Erfolg → schließen + reload
                    if (res.success) {
                        modal.hide();
                        location.reload();
                    }
                } else {
                    // Validation-Errors → PartialView neu rendern
                    $("#animeModalContent").html(res);
                    bindCreateAnimeForm(modal); // 🔁 WICHTIG: neu binden!
                }
            }
        });
    });
}

$(document).ready(function () {
    console.log("animes.js loaded");

    // Create Modal öffnen
    $(document).on("click", "#openCreateAnimeModal", function () {
        $.get("/Anime/CreateAnimePartial", function (data) {
            $("#animeModalContent").html(data);
            var modal = new bootstrap.Modal(document.getElementById('animeModal'));
            modal.show();

            bindCreateAnimeForm(modal);
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
                            $("#animeModalContent").html(res);
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


    // --------------------- Notes scroll ---------------------

    const $track = $('.gallery-track');
    const $scrollLeft = $('#scrollLeft');
    const $scrollRight = $('#scrollRight');

    if ($track.length === 0) return;

    const $firstItem = $track.find('.note-elem').first();
    const itemWidth = $firstItem.outerWidth(true) || 250;

    let position = 0;

    function updateButtons() {
        const maxScroll = -($track[0].scrollWidth - $track.parent().outerWidth());

        // Links
        if (position >= 0) {
            $scrollLeft.find('img').hide(); // Pfeil ausblenden
            $scrollLeft.css('cursor', 'default');
        } else {
            $scrollLeft.find('img').show();
            $scrollLeft.css('cursor', 'pointer');
        }

        // Rechts
        if (position <= maxScroll) {
            $scrollRight.find('img').hide(); // Pfeil ausblenden
            $scrollRight.css('cursor', 'default');
        } else {
            $scrollRight.find('img').show();
            $scrollRight.css('cursor', 'pointer');
        }
    }

    // Initiales Update
    updateButtons();

    $scrollLeft.on('click', function () {
        if (position >= 0) return; // Klick ignorieren, wenn nicht scrollbar
        position += itemWidth * 4;
        if (position > 0) position = 0;
        $track.css('transform', `translateX(${position}px)`);
        updateButtons();
    });

    $scrollRight.on('click', function () {
        const maxScroll = -($track[0].scrollWidth - $track.parent().outerWidth());
        if (position <= maxScroll) return; // Klick ignorieren, wenn nicht scrollbar
        position -= itemWidth * 4;
        if (position < maxScroll) position = maxScroll;
        $track.css('transform', `translateX(${position}px)`);
        updateButtons();
    });

    // --------------------- Filter ---------------------

    // Language-Filter
    $(document).on("click", ".language", function () {
        $(".language").removeClass("aktiv-btn");
        $(this).addClass("aktiv-btn");
        loadAnimes(1);
    });

    // Country-Filter
    $(document).on("click", ".country", function () {
        $(".country").removeClass("aktiv-btn");
        $(this).addClass("aktiv-btn");
        loadAnimes(1);
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
            loadAnimes(1);
        } else {
            $(".rank").removeClass("aktiv-btn");
            $(this).addClass("aktiv-btn");
            loadAnimes(1);
        }
    });

    // Pagination-Click
    $(document).on("click", ".pagination-link", function (e) {
        e.preventDefault();

        let page = $(this).data("page");

        if (!page || $(this).hasClass("disabled")) {
            return;
        }

        loadAnimes(page);
    });

});

