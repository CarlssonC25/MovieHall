$(document).ready(function () {
    console.log("itemView.js geladen");


    // ------------------------------------- Movie -------------------------------------
    // --- add Child Movie ---
    $("#openCreateMovieModal").click(function () {
        var parentId = $(this).data("parent-id");

        $.get("/Home/CreateMovieChildPartial", { parentId: parentId }, function (data) {
            $("#movieModalContent").html(data);

            var modal = new bootstrap.Modal(document.getElementById('movieModal'));
            modal.show();

            $("#createMovieForm").on("submit", function (e) {
                e.preventDefault();
                var formData = new FormData(this);

                $.ajax({
                    type: "POST",
                    url: "/Home/CreateMovieChildPartial",
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
                            $("#movieModalContent").html(res);
                        }
                    }
                });
            });
        });
    });

    // --- Edit Movie ---
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

    // --- Delete Movie ---
    $(".delete-movie-link").click(function () {
        var id = $(this).data("id");
        var name = $(this).data("name");
        var parentId = $(this).data("parent-id");

        $("#deleteMovieId").val(id);
        $("#deleteMovieName").text(name);
        $("#deleteMovieForm").data("parent-id", parentId);
        var modal = new bootstrap.Modal(document.getElementById('deleteMovieModal'));
        modal.show();
    });

    $("#deleteMovieForm").on("submit", function (e) {
        e.preventDefault();
        var id = $("#deleteMovieId").val();
        var parentId = $(this).data("parent-id");

        $.post("/Home/DeleteMovieConfirmed/" + id, function () {
            if (parentId) {
                // Redirect zur ParentId
                const currentUrl = new URL(window.location.href);
                currentUrl.pathname = "/Home/ItemView/" + parentId;
                currentUrl.searchParams.set("type", "Movie");
                window.location.href = currentUrl.toString();
            } else {
                // Fallback-Redirect, z.B. auf Home
                window.location.href = "/Home/Movie";
            }
        });
    });

    // ------------------------------------- Anime -------------------------------------
    // --- add Child Anime ---
    $("#openCreateAnimeModal").click(function () {
        var parentId = $(this).data("parent-id");

        $.get("/Home/CreateAnimeChildPartial", { parentId: parentId }, function (data) {
            $("#animeModalContent").html(data);

            var modal = new bootstrap.Modal(document.getElementById('animeModal'));
            modal.show();

            $("#createAnimeForm").on("submit", function (e) {
                e.preventDefault();
                var formData = new FormData(this);

                $.ajax({
                    type: "POST",
                    url: "/Home/CreateAnimeChildPartial",
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
                            $("#animeModalContent").html(res);
                        }
                    }
                });
            });
        });
    });

    // --- Edit Anime ---
    $(".edit-anime-link").click(function () {
        var id = $(this).data("id");
        $.get("/Home/EditAnimePartial?id=" + id, function (data) {
            $("#animeModalContent").html(data); // gleiche ID wie beim Create
            var modal = new bootstrap.Modal(document.getElementById('animeModal')); // gleiche Modal-ID
            modal.show();

            $("#editAnimeForm").on("submit", function (e) {
                e.preventDefault();
                var formData = new FormData(this);

                $.ajax({
                    type: "POST",
                    url: "/Home/EditAnimePartial",
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

    // --- Delete Anime ---
    $(".delete-anime-link").click(function () {
        var id = $(this).data("id");
        var name = $(this).data("name");
        var parentId = $(this).data("parent-id");

        $("#deleteAnimeId").val(id);
        $("#deleteAnimeName").text(name);
        $("#deleteAnimeForm").data("parent-id", parentId);
        var modal = new bootstrap.Modal(document.getElementById('deleteAnimeModal'));
        modal.show();
    });

    $("#deleteAnimeForm").on("submit", function (e) {
        e.preventDefault();
        var id = $("#deleteAnimeId").val();
        var parentId = $(this).data("parent-id");

        $.post("/Home/DeleteAnimeConfirmed/" + id, function () {
            if (parentId) {
                // Redirect zur ParentId
                const currentUrl = new URL(window.location.href);
                currentUrl.pathname = "/Home/ItemView/" + parentId;
                currentUrl.searchParams.set("type", "Anime");
                window.location.href = currentUrl.toString();
            } else {
                // Fallback-Redirect, z.B. auf Home
                window.location.href = "/Home/Anime";
            }
        });
    });
});