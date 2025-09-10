// Filter Hilfsfunktion für AJAX-Load
function loadMovies(page) {
    let filter = $("#FSK-search").val();

    $.ajax({
        url: "/Home/Movie?page=" + page + "&filter=" + encodeURIComponent(filter),
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
    console.log("movies.js geladen");
    // Create Modal öffnen
    $("#openCreateMovieModal").click(function () {
        $.get("/Home/CreateMoviePartial", function (data) {
            $("#movieModalContent").html(data);
            var modal = new bootstrap.Modal(document.getElementById('movieModal'));
            modal.show();

            $("#createMovieForm").on("submit", function (e) {
                e.preventDefault();
                var formData = new FormData(this);


                $.ajax(
                    {
                    type: "POST",
                    url: "/Home/CreateMoviePartial",
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
        $.get("/Home/EditMoviePartial?id=" + id, function (data) {
            $("#movieModalContent").html(data);
            var modal = new bootstrap.Modal(document.getElementById('movieModal'));
            modal.show();

            $("#editMovieForm").on("submit", function (e) {
                e.preventDefault();
                var formData = new FormData(this);

                $.ajax({
                    type: "POST",
                    url: "/Home/EditMoviePartial",
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
        $.post("/Home/DeleteMovieConfirmed/" + id, function () {
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
});
