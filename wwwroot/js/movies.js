function openCreateModal() {
    $.get('/Home/CreateMoviePartial', function (data) {
        showModal('Neuen Film hinzufügen', data, function () {
            const form = $('#modalForm');
            const formData = new FormData(form[0]);

            $.ajax({
                url: '/Home/CreateMoviePartial',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (res) {
                    if (res.success) {
                        $('#movieModal').modal('hide');
                        location.reload();
                    }
                },
                error: function (err) {
                    alert("Fehler beim Speichern.");
                }
            });
        });
    });
}

function openEditModal(id) {
    $.get('/Home/EditMoviePartial?id=' + id, function (data) {
        showModal('Film bearbeiten', data, function () {
            const form = $('#modalForm');
            const formData = new FormData(form[0]);

            $.ajax({
                url: '/Home/Movie',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (res) {
                    if (res.success) {
                        $('#movieModal').modal('hide');
                        location.reload();
                    }
                },
                error: function () {
                    alert("Fehler beim Bearbeiten.");
                }
            });
        });
    });
}

function deleteMovie(id) {
    if (!confirm("Möchten Sie diesen Film wirklich löschen?")) return;

    $.post('/Home/DeleteMovieConfirmed', { id: id }, function (res) {
        if (res.success) {
            location.reload();
        } else {
            alert("Fehler beim Löschen.");
        }
    });
}

function showModal(title, content, onSaveCallback) {
    $('#movieModalLabel').text(title);
    $('#movieModalBody').html(content);
    $('#movieModalSaveBtn').off('click').on('click', onSaveCallback);
    $('#movieModal').modal('show');
}
