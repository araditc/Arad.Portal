var DivId;
var Sender;

function submitComment(sender, refId, isLogged,lanIcon) {
    debugger;
    var obj = {};
    if (isLogged == 'True') {
        var parent = $(sender).parent().parent();
        
        if ($(parent).attr("id").startsWith("bx_")) {
            obj.ParentId =  $(sender).parent().parent().attr("id").replace("bx_", "");
        }
        if (parent.find(".cmt").val() != "") {
            obj.ReferenceId = refId;
           
            obj.Content = parent.find(".cmt").val()
            DivId = $(sender).parent().parent();
         
           
            $.ajax({
                url: `/${lanIcon}/Comment/SubmitComment`,
                contentType: "application/json",
                data: JSON.stringify(obj),
                type: "Post",
                beforeSend: function () {
                },
                success: function (result) {
                    debugger;
                    if (result.status === "Success") {
                        $(DivId).find(".cmt").val(result.message);
                        setTimeout(function () {
                            $(DivId).find(".cmt").val("");
                            $(DivId).removeClass("show"); DivId = null;
                        },
                            3000);
                       
                    }
                    else
                    {
                        $(DivId).find(".cmt").val(result.message);
                        setTimeout(function () {
                            $(DivId).find(".cmt").val("");
                            $(DivId).removeClass("show");
                            DivId = null;
                        },
                            3000);
                    }
                    
                }
            });
        }
    }
}

function likeDisLike(sender, cmtId, isLike, lanIcon) {
    debugger;
    if ((isLike && $(sender).find('.fas.fa-thumbs-up').hasClass('d-none')
        && $(sender).parent().parent().siblings().find('.fas.fa-thumbs-down').hasClass('d-none'))
        || (!isLike && $(sender).parent().parent().siblings().find('.fas.fa-thumbs-up').hasClass('d-none')
            && $(sender).find('.fas.fa-thumbs-down').hasClass('d-none')))
    {
        debugger;
        Sender = sender;
        $.ajax({
            url: `/${lanIcon}/Comment/AddLikeDisLike?commentId=` + cmtId + "&isLike=" + isLike,
            contentType: 'application/json',
            type: 'Post',
            dataType: 'json',
            beforeSend: function () {
            },
            success: function (result) {
                debugger;
                if (result.status == "Success")
                {
                    $(Sender).attr("disabled", "disabled");
                    if (result.isLike)
                    {
                        $(Sender).find('.far.fa-thumbs-up').addClass('d-none');
                        $(Sender).find('.fas.fa-thumbs-up').removeClass('d-none');
                    }
                    else
                    {
                        $(Sender).find('.far.fa-thumbs-down').addClass('d-none');
                        $(Sender).find('.fas.fa-thumbs-down').removeClass('d-none');
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

