$(document).ready(function () {
    console.log("itemView.js geladen");

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
});