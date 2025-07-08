$(document).ready(function () {
    console.log("animes.js geladen");
    // Create Modal öffnen
    $("#openCreateAnimeModal").click(function () {
        $.get("/Home/CreateAnimePartial", function (data) {
            $("#animeModalContent").html(data);
            var modal = new bootstrap.Modal(document.getElementById('animeModal'));
            modal.show();

            $("#createAnimeForm").on("submit", function (e) {
                e.preventDefault();
                var formData = new FormData(this);


                $.ajax(
                    {
                    type: "POST",
                    url: "/Home/CreateAnimePartial",
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
        $.post("/Home/DeleteAnimeConfirmed/" + id, function () {
            location.reload();
        });
    });

});