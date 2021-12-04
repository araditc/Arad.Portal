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
            DivId = $(sender).parent().parent();
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
                        $(DivId).removeClass("show");
                        var html = '<div class="media mt-4"><div class="media-body"><div class="row"><div class="col-md-8 d-flex"><h5>' +
                            result.username + '</h5><span>' + result.date + '</span></div><div class="col-md-1"><div class="pull-right reply">' +
                            '<a data-toggle="collapse" href="#bx_' + result.commentid + '" role="button" aria-expanded="false" aria-controls="bx_'
                            + result.commentid + '"><i class="fa fa-reply"></i></a></div></div><div class="col-md-1"><div> <a href="#"' +
                            'role="button" aria-expanded="false" onclick="likeDisLike(this,' + result.commentid + ', true);"><i class="far fa-thumbs-up"></i><i class="d-none fas fa-thumbs-up"></i></a >' +
                            '<span class="statistics">0</span></div></div> <div class="col-md-1"><div> <a href="#"' +
                            'role="button" aria-expanded="false" onclick ="likeDisLike(this, ' + result.commentid + ', false);" ><i class="far fa-thumbs-down"></i><i class="d-none fas fa-thumbs-down"></i></a>' +
                            '<span class="statistics">0</span></div></div></div>' + result.content + ' <div class="row collapse" id="bx_' + result.commentid + '"><div class="col-md-10"><input type="text" class="cmt form-control" value="" />' +
                            '</div><div class="col-md-2"> <a href="#" role="button" aria-expanded="false" onclick="submitComment(this, ' + result.refid + ',true);"><i class="fas fa-check"></i></a></div></div>';
                        $(DivId).parent().append(html);
                    }
                    DivId = null;
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

