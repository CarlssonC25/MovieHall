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
            $("#movieModalContent").html(data); // gleiche ID wie beim Create
            var modal = new bootstrap.Modal(document.getElementById('movieModal')); // gleiche Modal-ID
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
                            $("#movieModalContent").html(res); // auch hier anpassen
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

});