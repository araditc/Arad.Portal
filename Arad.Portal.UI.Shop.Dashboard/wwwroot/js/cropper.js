var cropper = null;
var currentFileSize;
var currentFileName;
var aspectRatio;

function upRiseModal() {
    $("#send_pic").attr("disabled", true);
    $("#uploadModal").modal("show");
}

function initCropper() {
    
    var image = document.getElementById('blah');
    if (cropper !== null && cropper !== undefined) {
        cropper.destroy();
    }

    cropper = new Cropper(image,
        {
            background: true,
            aspectRatio: aspectRatio,
            viewMode: 2,
            responsive: true
        });


    document.getElementById('crop_button').addEventListener('click',
        function () {
            document.getElementById("resultCrop").innerHTML = "";
            var imgUrl = cropper.getCroppedCanvas().toDataURL('image/jpeg');
            var img = document.createElement("img");
            img.setAttribute("id", "pic");
            img.setAttribute("class", "img-fluid");
            var height = document.getElementById("container").getAttribute("width");
            var croppedContainer = document.createElement("div");
            croppedContainer.setAttribute("class","text-center");
            croppedContainer.setAttribute("id", "cropped_result");
            croppedContainer.setAttribute("style", "margin-bottom: 10px;padding: 10px;background: #f2f2f2;margin-right: 10px;max-width:100%; max-height:100%; width: 100%; height:" + height + "; border: none");
            document.getElementById("resultCrop").appendChild(croppedContainer);
            // document.getElementById("cropped_result").setAttribute("style", "max-width:100%;max-height:100%;width:400px; height:" + height + "; border: none");
            img.setAttribute("style", "max-width:100%;max-height:100%;width: 200px;height: " + height + ";position: relative;left: 0px;border: 1px solid #bbb8b8");
            img.src = imgUrl;
           
            document.getElementById("cropped_result").innerHTML = "";
           
            document.getElementById("cropped_result").appendChild(img);
            $("#send_pic").attr("disabled", false);
        });
}

function readURL(input, aspect) {
    aspectRatio = aspect;

    $('span[data-val-result = "Pic"]').html("");
    $("#cropped_result").remove();
    if (input.files && input.files[0]) {
        var file = input.files[0];
        currentFileName = file.name;
        currentFileSize = file.size;

        if (currentFileSize <= 500 * 1024) {
            var reader = new FileReader();
            reader.readAsDataURL(input.files[0]);
            reader.onload = function (e) {
                var content = e.target.result;
                
                //if file format is incorrect, then the 'content' won't have any src.
                if (content !== "data:") {
                    var reducedContent = content.slice(5);
                    
                    if (reducedContent.startsWith("image")) {
                        $('#blah').attr('src', content);
                        setTimeout(upRiseModal, 100);
                        setTimeout(initCropper, 500);

                    } else {

                        $('span[data-val-result = "Pic"]').html("فرمت فایل انتخاب شده نادرست است.");

                        document.getElementById("Image").value = "";
                        $("#cropped_result").remove();

                        setTimeout(function () {
                            $("#errorModal").modal("hide");
                        },
                            2000);
                    }
                } else {

                    $('span[data-val-result = "Pic"]').html("فرمت فایل انتخاب شده نادرست است.");


                    document.getElementById("Image").value = "";
                    $("#cropped_result").remove();

                    setTimeout(function () {
                        $("#errorModal").modal("hide");
                    },
                        2000);
                }
            };
        } else {
            $('span[data-val-result = "Pic"]').html("حجم فایل بیش از 500 کیلوبایت است.");

            setTimeout(function () {
                    document.getElementById("Image").value = "";
                $("#cropped_result").remove();
                $("#errorModal").modal("hide");
            },
                2000);
        }
    }
}