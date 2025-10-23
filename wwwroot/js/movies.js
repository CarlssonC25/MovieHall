// Filter Hilfsfunktion für AJAX-Load
function loadMovies(page) {
    let filter = $("#FSK-search").val();

    $.ajax({
        url: "/Movie/Index?page=" + page + "&filter=" + encodeURIComponent(filter),
        type: "GET",
        headers: { "X-Requested-With": "XMLHttpRequest" },
        success: function (data) {
            $("#movieContainer").html(data);
            $("html, body").animate({ scrollTop: $("#movieContainer").offset().top }, 300);
        },
        error: function () {
            alert("Fehler beim Laden der Filme.");
        }
    });
}

$(document).ready(function () {
    console.log("movies.js loaded");
    // Create Modal öffnen
    $("#openCreateMovieModal").click(function () {
        $.get("/Movie/CreateMoviePartial", function (data) {
            $("#movieModalContent").html(data);
            var modal = new bootstrap.Modal(document.getElementById('movieModal'));
            modal.show();

            $("#createMovieForm").on("submit", function (e) {
                e.preventDefault();
                var formData = new FormData(this);


                $.ajax(
                    {
                    type: "POST",
                    url: "/Movie/CreateMoviePartial",
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
                            $("#movieModalContent").html(res);
                        }
                    }
                });
            });
        });
    });

    // Edit Modal öffnen
    $(".edit-movie-link").click(function () {
        var id = $(this).data("id");
        $.get("/Movie/EditMoviePartial?id=" + id, function (data) {
            $("#movieModalContent").html(data);
            var modal = new bootstrap.Modal(document.getElementById('movieModal'));
            modal.show();

            $("#editMovieForm").on("submit", function (e) {
                e.preventDefault();
                var formData = new FormData(this);

                $.ajax({
                    type: "POST",
                    url: "/Movie/EditMoviePartial",
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
                            $("#movieModalContent").html(res);
                        }
                    }
                });
            });
        });
    });

    // Delete
    $(".delete-movie-link").click(function () {
        var id = $(this).data("id");
        var name = $(this).data("name");
        $("#deleteMovieId").val(id);
        $("#deleteMovieName").text(name);
        var modal = new bootstrap.Modal(document.getElementById('deleteMovieModal'));
        modal.show();
    });

    $("#deleteMovieForm").on("submit", function (e) {
        e.preventDefault();
        var id = $("#deleteMovieId").val();
        $.post("/Movie/DeleteMovieConfirmed/" + id, function () {
            location.reload();
        });
    });

    // Pagination-Click
    $(document).on("click", ".pagination-link", function (e) {
        e.preventDefault();

        let page = $(this).data("page");

        if (!page || $(this).parent().hasClass("disabled") || $(this).parent().hasClass("active")) {
            return;
        }

        // über die Hilfsfunktion laden → Filter wird automatisch mitgeschickt
        loadMovies(page);
    });

    // Filter-Select
    $(document).on("change", "#FSK-search", function () {
        console.log("Filter geändert:", $(this).val());
        loadMovies(1); // bei Filteränderung immer auf Seite 1 springen
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
});
