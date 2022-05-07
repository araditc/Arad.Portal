function isUnique(langId, tableId, languageColumnOrder, messageToShow) {
    var isunique = true;
    $(`#${tableId} tr`).each(function () {
        if ($(this).children.length != 1) {
            if ($(this).find(`td:eq(${languageColumnOrder})`).text() == langId) {
                isunique = false;
            }
        }
    });
    if (!isunique) {
        $('#mainToastBody').html(`<i class="fas fa-exclamation-triangle"></i>${messageToShow}`);
        $('#mainToastBody').addClass('alert-danger');
        var toastDiv = $("#mainToast");
        $("#toastPanel").show();
        var toast = new bootstrap.Toast(toastDiv);
        toast.show();
        //setTimeout(function () {
        //    CKEDITOR.instances.editorId.setData(""), 3000
        //});
    }
    return isunique;
}