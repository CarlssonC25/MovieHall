﻿// Genre
$(document).ready(function () {
    $("#openCreateModal").click(function () {
        $.get("/Settings/CreatePartial", function (data) {
            $("#createModalContent").html(data);
            new bootstrap.Modal('#createModal').show();

            $("#createGenreForm").on("submit", function (e) {
                e.preventDefault();
                $.post("/Settings/CreatePartial", $(this).serialize(), function (res) {
                    if (res.success) location.reload();
                    else $("#createModalContent").html(res);
                });
            });
        });
    });

    $(".edit-link").click(function () {
        let id = $(this).data("id");
        $.get("/Settings/EditPartial/" + id, function (data) {
            $("#editModalContent").html(data);
            new bootstrap.Modal('#editModal').show();

            $("#editGenreForm").on("submit", function (e) {
                e.preventDefault();
                $.post("/Settings/EditPartial", $(this).serialize(), function (res) {
                    if (res.success) location.reload();
                    else $("#editModalContent").html(res);
                });
            });
        });
    });

    $(".delete-link").click(function () {
        let id = $(this).data("id");
        let name = $(this).data("name");
        $("#deleteGenreId").val(id);
        $("#deleteGenreName").text(name);
        new bootstrap.Modal('#deleteModal').show();
    });

    $("#deleteGenreForm").on("submit", function (e) {
        e.preventDefault();
        let id = $("#deleteGenreId").val();
        $.post("/Settings/DeleteConfirmed", { id: id }, function () {
            location.reload();
        });
    });
});


// Settings
$(document).ready(function () {    
    $("#openCreateSettingModal").click(function () {
        $.get("/Settings/CreateSettingPartial", function (data) {
            $("#createSettingModalContent").html(data);
            var modal = new bootstrap.Modal(document.getElementById('createSettingModal'));
            modal.show();

            $("#createSettingForm").on("submit", function (e) {
                e.preventDefault();
                $.ajax({
                    type: "POST",
                    url: "/Settings/CreateSettingPartial",
                    data: $(this).serialize(),
                    success: function (res) {
                        if (res.success) location.reload();
                        else $("#createSettingModalContent").html(res);
                    }
                });
            });
        });
    });

    $(".edit-setting-link").click(function () {
        var id = $(this).data("id");
        $.get("/Settings/EditSettingPartial/" + id, function (data) {
            $("#editSettingModalContent").html(data);
            var modal = new bootstrap.Modal(document.getElementById('editSettingModal'));
            modal.show();

            $("#editSettingForm").on("submit", function (e) {
                e.preventDefault();
                $.ajax({
                    type: "POST",
                    url: "/Settings/EditSettingPartial",
                    data: $(this).serialize(),
                    success: function (res) {
                        if (res.success) location.reload();
                        else $("#editSettingModalContent").html(res);
                    }
                });
            });
        });
    });

    $(".delete-setting-link").click(function () {
        var id = $(this).data("id");
        var name = $(this).data("name");
        $("#deleteSettingId").val(id);
        $("#deleteSettingName").text(name);
        var modal = new bootstrap.Modal(document.getElementById('deleteSettingModal'));
        modal.show();
    });

    $("#deleteSettingForm").on("submit", function (e) {
        e.preventDefault();
        var id = $("#deleteSettingId").val();
        $.post("/Settings/DeleteSettingConfirmed/" + id, function () {
            location.reload();
        });
    });

});


// WatchedWith
$(document).ready(function () {    
    $("#openCreateWatchedWithModal").click(function () {
        $.get("/Settings/CreateWatchedWithPartial", function (data) {
            $("#createWatchedWithModalContent").html(data);
            var modal = new bootstrap.Modal(document.getElementById('createWatchedWithModal'));
            modal.show();

            $("#createWatchedWithForm").on("submit", function (e) {
                e.preventDefault();
                $.ajax({
                    type: "POST",
                    url: "/Settings/CreateWatchedWithPartial",
                    data: $(this).serialize(),
                    success: function (res) {
                        if (res.success) location.reload();
                        else $("#createWatchedWithModalContent").html(res);
                    }
                });
            });
        });
    });

    $(".edit-watchedWith-link").click(function () {
        var id = $(this).data("id");
        $.get("/Settings/EditWatchedWithPartial/" + id, function (data) {
            $("#editWatchedWithModalContent").html(data);
            var modal = new bootstrap.Modal(document.getElementById('editWatchedWithModal'));
            modal.show();

            $("#editWatchedWithForm").on("submit", function (e) {
                e.preventDefault();
                $.ajax({
                    type: "POST",
                    url: "/Settings/EditWatchedWithPartial",
                    data: $(this).serialize(),
                    success: function (res) {
                        if (res.success) location.reload();
                        else $("#editWatchedWithModalContent").html(res);
                    }
                });
            });
        });
    });

    $(".delete-watchedWith-link").click(function () {
        var id = $(this).data("id");
        var name = $(this).data("name");
        $("#deleteWatchedWithId").val(id);
        $("#deleteWatchedWithName").text(name);
        var modal = new bootstrap.Modal(document.getElementById('deleteWatchedWithModal'));
        modal.show();
    });

    $("#deleteWatchedWithForm").on("submit", function (e) {
        e.preventDefault();
        var id = $("#deleteWatchedWithId").val();
        $.post("/Settings/DeleteWatchedWithConfirmed/" + id, function () {
            location.reload();
        });
    });

});



// Funktionen außerhalb von $(document).ready)
function previewImage(input, previewId, settingName) {
    const file = input.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            document.getElementById(previewId).src = e.target.result;
        };
        reader.readAsDataURL(file);
    }
}

function uploadImage(inputId, settingName) {
    const input = document.getElementById(inputId);
    const file = input.files[0];
    if (!file) {
        alert("Bitte wählen Sie eine Datei aus.");
        return;
    }

    const formData = new FormData();
    formData.append("file", file);
    formData.append("settingName", settingName);

    fetch("/Settings/UploadSettingImage", {
        method: "POST",
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert("Bild gespeichert: " + data.fileName);
            } else {
                alert("Fehler: " + data.error);
            }
        });
}


// DB import
document.getElementById('zipFile').addEventListener('change', function () {
    let fileName = this.files.length > 0 ? this.files[0].name : "Keine Datei gewählt";
    document.getElementById('fileName').textContent = fileName;
});