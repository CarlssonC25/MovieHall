$(document).ready(function () {
    console.log("itemView.js loaded");


    // ------------------------------------- Anime -------------------------------------
    // --- add Child Anime ---
    $("#openCreateAnimeModal").click(function () {
        var parentId = $(this).data("parent-id");

        $.get("/ItemView/CreateAnimeChildPartial", { parentId: parentId }, function (data) {
            $("#animeModalContent").html(data);

            var modal = new bootstrap.Modal(document.getElementById('animeModal'));
            modal.show();

            bindCreateAnimeChildForm(modal, parentId); // ✅ Handler binden
        });
    });
    function bindCreateAnimeChildForm(modal, parentId) {
        $("#createAnimeForm").on("submit", function (e) {
            e.preventDefault();

            var formData = new FormData(this);

            $.ajax({
                type: "POST",
                url: "/ItemView/CreateAnimeChildPartial",
                data: formData,
                processData: false,
                contentType: false,
                success: function (res, status, xhr) {
                    const contentType = xhr.getResponseHeader("content-type");

                    if (contentType && contentType.indexOf("application/json") !== -1) {
                        // ✅ Erfolg → Modal schließen + Seite neu laden
                        if (res.success) {
                            modal.hide();
                            location.reload();
                        }
                    } else {
                        // ❌ Validation Errors → PartialView neu laden
                        console.log("Validation errors, rendering PartialView");
                        $("#animeModalContent").html(res);

                        // 🔁 Neuen Handler wieder binden
                        bindCreateAnimeChildForm(modal, parentId);
                    }
                }
            });
        });
    }

    // --- Edit Anime ---
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

        $.post("/Anime/DeleteAnimeConfirmed/" + id, function () {
            if (parentId) {
                // Redirect zur ParentId
                const currentUrl = new URL(window.location.href);
                currentUrl.pathname = "/ItemView/Index/" + parentId;
                currentUrl.searchParams.set("type", "Anime");
                window.location.href = currentUrl.toString();
            } else {
                // Fallback-Redirect, z.B. auf Home
                window.location.href = "/Anime/Index";
            }
        });
    });


    // --- WhatTimes EDIT ---
    $(".rewatch").click(function () {
        var id = $(this).data("id");
        var rewatch = $(this).data("rewatch");

        $.ajax({
            url: "/ItemView/UpdateRewatch",
            type: "POST",
            data: { id: id, change: rewatch },
            success: function (response) {
                if (response.success) {
                    $("#rewatch-count-" + id).text(response.newValue);
                }
            }
        });
    });


    // --- add Anime Note ---
    $("#addAnimeNote").click(function () {
        var animeId = $(this).data("id");
        var img = $(this).data("img");

        $.get("/Note/CreateAnimeNotePartial", { animeId: animeId, img: img}, function (data) {
            $("#popup").html(data);

            var modal = new bootstrap.Modal(document.getElementById('popupModal'));
            modal.show();

            $("#createAnimeNoteForm").on("submit", function (e) {
                e.preventDefault();
                var formData = new FormData(this);

                $.ajax({
                    type: "POST",
                    url: "/Note/CreateAnimeNotePartial",
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
                            $("#createAnimeNoteForm").html(res);
                        }
                    }
                });
            });
        });
    });

    // --- Delete Anime Note ---
    $(".delete-a-note").click(function () {
        var id = $(this).data("id");
        var img = $(this).data("img");
        var comment = $(this).data("comment");
        var animeId = $(this).data("animeid");

        $("#deleteAnimeNoteId").val(id);
        $("#deleteAnimeNoteImg").attr("src", "/img/Anime_imgs/"+img);
        $("#deleteAnimeNoteText").text(comment);
        $("#deleteAnimeNoteForm").data("animeid", animeId);
        var modal = new bootstrap.Modal(document.getElementById('deleteAnimeNoteModal'));
        modal.show();
    });

    $("#deleteAnimeNoteForm").on("submit", function (e) {
        e.preventDefault();
        var id = $("#deleteAnimeNoteId").val();
        var animeId = $(this).data("animeid");

        $.post("/Note/DeleteAnimeNoteConfirmed/" + id, function () {
            if (id) {
                // Redirect zur Anime
                const currentUrl = new URL(window.location.href);
                currentUrl.pathname = "/ItemView/Index/" + animeId;
                currentUrl.searchParams.set("type", "Anime");
                window.location.href = currentUrl.toString();
            } else {
                // Fallback
                window.location.href = "/Anime/Index";
            }
        });
    });


    // ------------------------------------- Movie -------------------------------------
    // --- add Child Movie ---
    $("#openCreateMovieModal").click(function () {
        var parentId = $(this).data("parent-id");

        $.get("/ItemView/CreateMovieChildPartial", { parentId: parentId }, function (data) {
            $("#movieModalContent").html(data);

            var modal = new bootstrap.Modal(document.getElementById('movieModal'));
            modal.show();

            bindCreateMovieChildForm(modal, parentId); // ✅ Submit-Handler binden
        });
    });
    function bindCreateMovieChildForm(modal, parentId) {
        $("#createMovieForm").on("submit", function (e) {
            e.preventDefault();

            var formData = new FormData(this);

            $.ajax({
                type: "POST",
                url: "/ItemView/CreateMovieChildPartial",
                data: formData,
                processData: false,
                contentType: false,
                success: function (res, status, xhr) {
                    const contentType = xhr.getResponseHeader("content-type");

                    if (contentType && contentType.indexOf("application/json") !== -1) {
                        // ✅ Erfolg → Modal schließen + Seite neu laden
                        if (res.success) {
                            modal.hide();
                            location.reload();
                        }
                    } else {
                        // ❌ Validation Errors → PartialView neu laden
                        console.log("Validation errors, rendering PartialView");
                        $("#movieModalContent").html(res);

                        // 🔁 Handler neu binden, da das Form neu gerendert wurde
                        bindCreateMovieChildForm(modal, parentId);
                    }
                }
            });
        });
    }

    // --- Edit Movie ---
    $(".edit-movie-link").click(function () {
        var id = $(this).data("id");
        $.get("/Movie/EditMoviePartial?id=" + id, function (data) {
            $("#movieModalContent").html(data); // gleiche ID wie beim Create
            var modal = new bootstrap.Modal(document.getElementById('movieModal')); // gleiche Modal-ID
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

        $.post("/Movie/DeleteMovieConfirmed/" + id, function () {
            if (parentId) {
                // Redirect zur ParentId
                const currentUrl = new URL(window.location.href);
                currentUrl.pathname = "/ItemView/Index/" + parentId;
                currentUrl.searchParams.set("type", "Movie");
                window.location.href = currentUrl.toString();
            } else {
                // Fallback-Redirect, z.B. auf Home
                window.location.href = "/Movie/Index";
            }
        });
    });


    // --- add Movie Note ---
    $("#addMovieNote").click(function () {
        var movieId = $(this).data("id");
        var img = $(this).data("img");

        $.get("/Note/CreateMovieNotePartial", { movieId: movieId, img: img }, function (data) {
            $("#popup").html(data);

            var modal = new bootstrap.Modal(document.getElementById('popupModal'));
            modal.show();

            $("#createMovieNoteForm").on("submit", function (e) {
                e.preventDefault();
                var formData = new FormData(this);

                $.ajax({
                    type: "POST",
                    url: "/Note/CreateMovieNotePartial",
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
                            $("#createMovieNoteForm").html(res);
                        }
                    }
                });
            });
        });
    });

    // --- Delete Movie Note ---
    $(".delete-m-note").click(function () {
        var id = $(this).data("id");
        var img = $(this).data("img");
        var comment = $(this).data("comment");
        var movieId = $(this).data("movieid");

        $("#deleteMovieNoteId").val(id);
        $("#deleteMovieNoteImg").attr("src", "/img/Movie_imgs/" + img);
        $("#deleteMovieNoteText").text(comment);
        $("#deleteMovieNoteForm").data("movieid", movieId);
        var modal = new bootstrap.Modal(document.getElementById('deleteMovieNoteModal'));
        modal.show();
    });

    $("#deleteMovieNoteForm").on("submit", function (e) {
        e.preventDefault();
        var id = $("#deleteMovieNoteId").val();
        var movieId = $(this).data("movieid");

        $.post("/Note/DeleteMovieNoteConfirmed/" + id, function () {
            if (id) {
                // Redirect zur Movie
                const currentUrl = new URL(window.location.href);
                currentUrl.pathname = "/ItemView/Index/" + movieId;
                currentUrl.searchParams.set("type", "Movie");
                window.location.href = currentUrl.toString();
            } else {
                // Fallback
                window.location.href = "/Movie/Index";
            }
        });
    });
});

// ------------------------------------- CopyTitle -------------------------------------
$(document).ready(function () {
    $("#copyTitle").on("click", function () {
        var textToCopy = $(this).text().trim();

        navigator.clipboard.writeText(textToCopy)
            .then(function () {
                // Optional: Kurze visuelle Rückmeldung
                const originalText = $("#copyTitle").text();
                $("#copyTitle").text(originalText + " ©");
                setTimeout(function () {
                    $("#copyTitle").text(originalText);
                }, 400);
            })
            .catch(function (err) {
                console.error("Fehler beim Kopieren:", err);
                alert("Konnte nicht in die Zwischenablage kopieren.");
            });
    });
});