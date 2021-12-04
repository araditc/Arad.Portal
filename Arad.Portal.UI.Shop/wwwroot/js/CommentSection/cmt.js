var DivId;
var Sender;
function submitComment(sender, refId, isLogged) {
    debugger;
    var obj = {};
    if (isLogged == 'True') {
        var parent = $(sender).parent().parent();
        
        if ($(parent).attr("id").startsWith("bx_")) {
            obj.ParentId = "p*" + $(sender).parent().parent().attr("id").replace("bx_", "");
        }
        if (parent.find(".cmt").val() != "") {
            obj.ReferenceId = refId;
            obj.Content = parent.find(".cmt").val()
            DivId = $(sender).parent().parent().attr("id");
           /* obj.ParentId = "p*" + $(sender).parent().parent().attr("id").replace("bx_", "");*/
           
            $.ajax({
                url: "/Comment/SubmitComment",
                contentType: 'application/json',
                data: JSON.stringify(dto),
                type: 'Post',
                dataType: 'json',
                beforeSend: function () {
                },
                success: function (result) {

                    if (result.status === "Success") {
                        $("#" + DivId).removeClass("show");
                    }
                }
            });
        }
    }
}

function likeDisLike(sender, cmtId, isLike) {
    debugger;
    if ($(sender).siblings('.fas.fa-thumbs-up').hasClass('d-none')
        && $(sender).siblings('.fas.fa-thumbs-down').hasClass('d-none'))
    {
        Sender = sender;
        $.ajax({
            url: "/Comment/AddLikeDisLike?commentId=" + cmtId + "&isLike=" + isLike,
            contentType: 'application/json',
            type: 'Post',
            dataType: 'json',
            beforeSend: function () {
            },
            success: function (result) {
                if (result.status == "Success")
                {
                    $(Sender).attr("disabled", "disabled");
                    if (result.isLike)
                    {
                        $(Sender).siblings('.far.fa-thumbs-up').addClass('d-none');
                        $(Sender).siblings('.fas.fa-thumbs-up').removeClass('d-none');
                    }
                    else
                    {
                        $(Sender).siblings('.far.fa-thumbs-down').addClass('d-none');
                        $(Sender).siblings('.fas.fa-thumbs-down').removeClass('d-none');
                    }
                    var span = $(Sender).parent().find("span.statistics").text();
                    var newVal = span != undefined ? parseInt(span) + 1 : 1;
                    $(Sender).parent().find("span.statistics").text(newVal);
                }
                Sender = null;
            }
        });
    }
    //otherwise its like or dislike by this user then dont change status
}

