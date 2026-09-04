var FilesBox = {};
FilesBox.HtmlFormatFile = '';
// Pending uploads waiting for server confirmation (stores File objects).
FilesBox.ValidatedFile = [];
// Temporary identifiers used client-side only until the server returns the UUIDv7 id.
FilesBox.PendingUploadIds = [];
FilesBox.DisableFocusFileBox = false;

/**
 * Configuration object - can be overridden
 */
FilesBox.Config = {
    MaxFileSize: 10 * 1024 * 1024, // 10MB
    AllowedTypes: [
        'image/jpeg', 'image/png',
        'application/pdf',
        'application/msword',
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
        'application/vnd.ms-excel',
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        'audio/mpeg', 'audio/mp3',
        'video/mp4', 'video/avi', 'video/x-msvideo'
    ],
    Timeout: 300000, // 5 minutes
    RetryAttempts: 2
};
/**
 * Create a temporary key for client-side tracking.
 */
FilesBox.GenerateTempUploadId = function (existingIds) {
    var pool = existingIds || FilesBox.PendingUploadIds;
    var candidate;

    do {
        candidate = 'tmp-' + Date.now().toString(36) + '-' + Math.floor(Math.random() * 1e6);
    } while (pool.indexOf(candidate) !== -1);

    return candidate;
};

/**
 * Normalize the legacy "|New" suffix.
 */
FilesBox.NormalizePendingKey = function (key) {
    return (key || '').replace('|New', '');
};

FilesBox.EscapeForRegex = function (value) {
    return value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
};

FilesBox.ReplaceIdentifier = function (value, tempId, serverId) {
    if (!value) {
        return value;
    }

    var normalizedTemp = FilesBox.NormalizePendingKey(tempId);
    var escapedTemp = FilesBox.EscapeForRegex(normalizedTemp);
    var escapedTempWithSuffix = FilesBox.EscapeForRegex(normalizedTemp + '|New');

    return value
        .replace(new RegExp(escapedTempWithSuffix, 'g'), serverId)
        .replace(new RegExp(escapedTemp, 'g'), serverId);
};

FilesBox.FindItemByKey = function (key) {
    if (!key) {
        return $();
    }
    var selector = ".file-box.active .illustration-upload .item";
    var item = $(selector + "[data-ar='" + key + "']");

    if (item.length === 0) {
        var normalized = FilesBox.NormalizePendingKey(key);
        if (!normalized) {
            return item;
        }
        item = $(selector + "[data-temp-id='" + normalized + "']");
    }

    return item;
};
FilesBox.AddFile = function (isMultiple, type) {
    var ipfFile = $(".file-box.active .ipfFile");

    if (isMultiple)
        ipfFile.attr('multiple', 'multiple');
    //else
    //    ipfFile.attr('multiple', '');

    ipfFile.attr('data-type', type);

    ipfFile.click();
}
FilesBox.ReorderFile = function () {
    var totalItem = $(".file-box.active .illustration-upload .item").length;
    $(".file-box.active .illustration-upload .item").each(function (index, tag) {
        //$(this).find(".order").val(totalItem - index);
        $(this).find(".order").val(index);
    });
};
FilesBox.LayoutFilePopUp = function (el) {
    var fileName = $(el).attr("data-path");

    var isVideo = FilesBox.IsVideo(fileName);
    var isDoc = FilesBox.IsDoc(fileName);
    var isExcel = FilesBox.IsExcel(fileName);
    var isPDF = FilesBox.IsPDF(fileName);

    $(".body-preview").empty();
    if (isVideo) {
        const html = String.format("<video style='max-width: 100vw; max-height: 100vh;' controls>\
                    <source src=\"{0}\" type =\"video/mp4\" >\
        </video>", $(el).attr("data-path"));
        $(".body-preview").append(html);
        $('#file-box-viewer').show();
    }
    else if (isPDF) {
        var reviewUrl = String.format('https://docs.google.com/gview?url={0}&embedded=true'
            , CMSMasterJs.HostPath + $(el).attr("data-path"));

        var html = '<iframe style="height: 100vh; width: calc(100vw - 200px);" src=""></iframe>';
        $(".body-preview").append(html);
        $(".body-preview iframe").attr('src', reviewUrl);
        $('#file-box-viewer').show();
    }
    else if (isDoc || isExcel) {
        var reviewUrl = String.format('https://docs.google.com/gview?url={0}&embedded=true'
            , CMSMasterJs.HostPath + $(el).attr("data-path"));

        var html = '<iframe style="height: 100vh; width: calc(100vw - 200px);" src=""></iframe>';
        $(".body-preview").append(html);
        $(".body-preview iframe").attr('src', reviewUrl);
        $('#file-box-viewer').show();
    }

    else {
        var img = $(el).closest('.img-container').find('img');
        var imageTag = $('<img class="modal-content full-image" src="' + img.attr("src") + '">');
        $(".body-preview").append(imageTag);
        $('#file-box-viewer').show();
    }
}
FilesBox.ClosePopUp = function () {
    $('#file-pop').modal("hide");
}
FilesBox.ReplaceFile = function (t) {
    if (typeof (CMSMasterJs) == 'undefined')
        return;
    CMSMasterJs.OpenSelectImage(t);
}
FilesBox.SetFileUrl = function (txtid, url, path) {
    $('#' + txtid).val(url);

    var content = $('#' + txtid).closest('.item');
    content.find('.img-container img').attr('src', url);
};
FilesBox.LayoutFile = function () {
    $(".file-box.active .illustration-upload .progress-content").hide();
    var layout = $(".file-box.active:not(.file-box-single) .illustration-upload");
    if (typeof (layout) == 'undefined' || !layout.length)
        return;
    if (layout.hasClass('isotope-content'))
        layout.isotope('destroy');
    layout.isotope({
        itemSelector: '.item',
        layoutMode: 'masonry',
        filter: '.show'
    });
    layout.addClass('isotope-content');

    new Sortable(layout[0], {
        animation: 150,
        handle: ".sort-item",
        // Element dragging ended
        onEnd: function (/**Event*/evt) {
            //var itemEl = evt.item;  // dragged HTMLElement
            //evt.to;    // target list
            //evt.from;  // previous list
            //evt.oldIndex;  // element's old index within old parent
            //evt.newIndex;  // element's new index within new parent
            //evt.oldDraggableIndex; // element's old index within old parent, only counting draggable elements
            //evt.newDraggableIndex; // element's new index within new parent, only counting draggable elements
            //evt.clone // the clone element
            //evt.pullMode;  // when item is in another sortable: `"clone"` if cloning, `true` if moving
            FilesBox.ReorderFile();
        }
    });

    //Bind event click on the item to show large popup to view
    //$(".file-box.active .illustration-upload .item img").off("click", FilesBox.LayoutFilePopUp);
    //$(".file-box.active .illustration-upload .item img").on("click", FilesBox.LayoutFilePopUp);
}
FilesBox.CheckLayoutFile = function () {
    var layout = $(".file-box.active .illustration-upload");
    if (!layout.hasClass('first-isotope')) {
        //setTimeout(function () {
        FilesBox.LayoutFile();
        //}, 200)
    }
    layout.addClass('first-isotope')
}
FilesBox.IsVideo = function (filename) {
    var getExtension = function (filename) {
        var parts = filename.split('.');
        return parts[parts.length - 1];
    }

    var ext = getExtension(filename);
    switch (ext.toLowerCase()) {
        case 'm4v':
        case 'avi':
        case 'mpg':
        case 'mp4':
            // etc
            return true;
    }
    return false;
}
FilesBox.IsDoc = function (filename) {
    var getExtension = function (filename) {
        var parts = filename.split('.');
        return parts[parts.length - 1];
    }

    var ext = getExtension(filename);
    switch (ext.toLowerCase()) {
        case 'doc':
        case 'docx':
            // etc
            return true;
    }
    return false;
}
FilesBox.IsExcel = function (filename) {
    var getExtension = function (filename) {
        var parts = filename.split('.');
        return parts[parts.length - 1];
    }

    var ext = getExtension(filename);
    switch (ext.toLowerCase()) {
        case 'xls':
        case 'xlsx':
            // etc
            return true;
    }
    return false;
}
FilesBox.IsPDF = function (filename) {
    var getExtension = function (filename) {
        var parts = filename.split('.');
        return parts[parts.length - 1];
    }

    var ext = getExtension(filename);
    switch (ext.toLowerCase()) {
        case 'pdf':
            // etc
            return true;
    }
    return false;
}
FilesBox.ResizeImage = function (img, quality) {

    var canvas = document.createElement('canvas');

    var width = img.width;
    var height = img.height;

    // calculate the width and height, constraining the proportions
    //if (width > height) {
    //    if (width > max_width) {
    //        //height *= max_width / width;
    //        height = Math.round(height *= max_width / width);
    //        width = max_width;
    //    }
    //} else {
    //    if (height > max_height) {
    //        //width *= max_height / height;
    //        width = Math.round(width *= max_height / height);
    //        height = max_height;
    //    }
    //}

    // resize the canvas and draw the image data into it
    canvas.width = width;
    canvas.height = height;
    var ctx = canvas.getContext("2d");
    ctx.drawImage(img, 0, 0, width, height);

    return canvas.toDataURL("image/jpeg", quality); // get the data from canvas as 70% JPG (can be also PNG, etc.)
}
FilesBox.DataURLtoFile = function (dataurl, filename) {
    var arr = dataurl.split(','),
        mime = arr[0].match(/:(.*?);/)[1],
        bstr = atob(arr[1]),
        n = bstr.length,
        u8arr = new Uint8Array(n);

    while (n--) {
        u8arr[n] = bstr.charCodeAt(n);
    }

    return new File([u8arr], filename, { type: mime });
}
FilesBox.DefaultFileTitle = function () {

}
FilesBox.SelectedFile = function (elm) {
    FilesBox.DisableFocusFileBox = true;

    var loading = $(".file-box.active .loading");
    var ipfFile = $(".file-box.active .ipfFile");
    const allowedTypes = ipfFile.attr("accept").split(',').map(type => type.trim());
    loading.attr("style", "display: block!important;");
    if (!window.File || !window.FileReader || !window.FileList || !window.Blob) {
        loading.attr("style", "display: none!important;");
        ipfFile.val("");
        return;
    }

    var listFile = $(elm)[0].files;
    let isValid = true;
    let message = '';
    for (var i = 0; i < listFile.length; i++) {
        var file = listFile[i];
        const isImage = file.type.startsWith("image/") && allowedTypes.includes("image/*");
        const isExactType = allowedTypes.includes(file.type);
        if (!isImage && !isExactType) {
            isValid = false;
            message = 'File type not allowed: ' + file.name;
            break;
        }
    }
    if (!isValid) {
        alert(message);
        loading.attr("style", "display: none!important;");
        ipfFile.val("");
        return;
    }
    var maxCount = parseInt($(elm).attr("data-max-count"));
    if (listFile.length > maxCount) {
        alert("You can only upload a maximum of 10 files!");
        loading.attr("style", "display: none!important;");
        ipfFile.val("");
        return;
    }

    var maxSize = parseInt($(elm).attr("data-max-size"));
    var type = $(elm).attr("data-type");
    var invalidatedSizeFile = [];
    var countLoadedFile = 0;

    $('.file-box.active.file-box-single .remove-item').each(function (i, el) {
        FilesBox.RemoveFile(el, false);
    });

    $('#UpdateProgress1').show();
    $(listFile).each(function (index, file) {
        if (!file)
            return true;

        var isVideo = FilesBox.IsVideo(file.name);
        var isDoc = FilesBox.IsDoc(file.name);
        var isExcel = FilesBox.IsExcel(file.name);
        var isPDF = FilesBox.IsPDF(file.name);

        var fileName = file.name.split('.').slice(0, -1).join('.');

        var waitToResize = false;
        if (file.size > maxSize && !isVideo && !isDoc && !isPDF && !isExcel) {
            waitToResize = true;

            //compress the image
            // read the files
            var reader = new FileReader();
            reader.readAsArrayBuffer(file);

            reader.onload = function (event) {
                // blob stuff
                var blob = new Blob([event.target.result]); // create blob
                window.URL = window.URL || window.webkitURL;
                var blobURL = window.URL.createObjectURL(blob); // and get it's URL

                // helper Image object
                var image = new Image();
                image.src = blobURL;
                //preview.appendChild(image); // preview commented out, I am using the canvas instead
                image.onload = function () {
                    // have to wait till it's loaded
                    var quality = maxSize / file.size;
                    var resized = FilesBox.ResizeImage(image, quality); // send it to canvas
                    var newFile = FilesBox.DataURLtoFile(resized, file.name);
                    file = newFile;

                    waitToResize = false;
                }
            };


            //Save invalidated file name to alert
            //invalidatedSizeFile.push(fileName);
            //countLoadedFile++;
            //return true;
        }

        var afterResize = function () {
            setTimeout(function () {
                if (waitToResize) {
                    afterResize();
                    return;
                }

                // Create a temporary identifier and remember the file until the upload completes
                var tempUploadId = FilesBox.GenerateTempUploadId(FilesBox.PendingUploadIds);
                FilesBox.ValidatedFile.push(file);
                FilesBox.PendingUploadIds.push(tempUploadId);

                //Review and controls
                var readerUrl = new FileReader();
                readerUrl.onload = function (e) {
                    var reviewSrc;
                    if (isVideo)
                        reviewSrc = '/styles/images/video-thumbnail.jpg';
                    else if (isDoc)
                        reviewSrc = '/styles/images/doc-thumbnail.jpg';
                    else if (isExcel)
                        reviewSrc = '/styles/images/excel-thumbnail.jpg';
                    else if (isPDF)
                        reviewSrc = '/styles/images/pdf-thumbnail.png';
                    else
                        reviewSrc = e.target.result;
                    var clientId = $('.file-box.active').attr('data-clientid');
                    var $htmlFile = $(String.format(FilesBox.HtmlFormatFile
                        , reviewSrc
                        , fileName
                        , "opaction"
                        , clientId + 'fileTitle$' + tempUploadId + '|New'
                        , "-1"
                        , fileName
                        , clientId + 'fileOrder$' + tempUploadId + '|New'
                        , tempUploadId + '|New'
                        , clientId + 'filePath$' + tempUploadId + '|New'
                        , ''
                        , clientId + 'filePath_' + tempUploadId + '|New'
                        , ''
                        , ''));

                    // Keep the temporary id available so that progress updates can find the element
                    $htmlFile.attr('data-temp-id', tempUploadId);

                    $(".file-box.active .illustration-upload").prepend($htmlFile);

                    countLoadedFile++;
                    //End upload
                    if (countLoadedFile === listFile.length) {
                        setTimeout(function () {
                            $('.file-box.active .illustration-upload').removeClass('sorting');
                            $('.file-box.active .chkEnableSorting').prop('checked', false);
                            FilesBox.ReorderFile();
                            FilesBox.LayoutFile();
                            loading.attr("style", "display: none!important;");
                            $('#UpdateProgress1').hide();
                            ipfFile.val("");

                            if ($('.file-box.active.file-box-single [data-selector="btnApplyFile"]').length > 0)
                                $('.file-box.active.file-box-single [data-selector="btnApplyFile"]')[0].click();

                            FilesBox.DisableFocusFileBox = false;
                        }, 300)
                    }
                }
                readerUrl.readAsDataURL(file);
                //End review and controls
            }, 100);
        }
        afterResize();
    });

    //Alert invalidated file
    if (invalidatedSizeFile.length > 0) {
        if (/*listFileLength === 1 && */invalidatedSizeFile.length === 1)
            alert("Please choose a smaller file which is less than " + maxSize / (1024 * 1024) + "MB.");
        else {
            alert("Some files are reaching the size limitation. Please choose smaller file which is less than " + maxSize / (1024 * 1024) + "MB. (" + invalidatedSizeFile.join(", ") + ")");
        }
    }
    //End alert invalidated file
}
FilesBox.EnableSorting = function (el) {
    if ($(el).is(':checked')) {
        $('.file-box.active .illustration-upload').addClass('sorting');
        var layout = $(".file-box.active .illustration-upload");
        if (layout.hasClass('isotope-content')) {
            layout.isotope('destroy');
            layout.removeClass('isotope-content')
        }
    }
    else {
        $('.file-box.active .illustration-upload').removeClass('sorting');
        FilesBox.LayoutFile();
    }
}
FilesBox.RemoveFile = function (tag, isSinglePostback) {
    tag = $(tag);
    var box = tag.closest('.file-box');

    var ar = tag.closest(".item").attr("data-ar").replace('|New', '');
    var index = FilesBox.PendingUploadIds.indexOf(ar);

    //The newly uploaded file has not been saved yet
    if (index != -1) {
        FilesBox.PendingUploadIds.splice(index, 1);
        FilesBox.ValidatedFile.splice(index, 1);
        $(".file-box.active .illustration-upload").isotope('remove', tag.closest(".item")).isotope('layout');
        tag.closest(".item").remove();
    }
    else {
        //Saved files, then temporarily remember to txtArFileRemove when clicking Save will delete
        var arFileRemove = tag.closest(".item").removeClass("show").attr("data-ar");
        tag.closest(".item").remove();
        var currentFileRemove = $('.file-box.active [data-selector="txtArFileRemove"]').val();
        if (currentFileRemove === '')
            $('.file-box.active [data-selector="txtArFileRemove"]').val(arFileRemove);
        else
            $('.file-box.active [data-selector="txtArFileRemove"]').val(currentFileRemove + ',' + arFileRemove);
        //$(".file-box.active .illustration-upload").isotope({ filter: '.show' });
        FilesBox.LayoutFile();
    }

    if (isSinglePostback === undefined || isSinglePostback === true) {
        if (box.hasClass('file-box-single')
            && box.find('.uploaded-content .item').length === 0) {
            box.find('[data-selector="btnApplyFile"]')[0].click();
        }
    }
};
FilesBox.DiscardFile = function () {
    $('.file-box.active .illustration-upload').removeClass('sorting');
    FilesBox.LayoutFile();
    if ($('.file-box.active .chkEnableSorting').is(':checked')) {
        $('.file-box.active .illustration-upload').addClass('sorting');
        var layout = $(".file-box.active .illustration-upload");
        if (layout.hasClass('isotope-content')) {
            layout.isotope('destroy');
            layout.removeClass('isotope-content')
        }
    }
    if (typeof ($targetButton) != 'undefined' && $targetButton.length)
        $targetButton.addClass("d-none");
    $('.control-help').removeClass("text-danger");
    $('.file-box.active [data-selector="txtArFileRemove"]').val('');
}
FilesBox.FocusFileBox = function (el) {
    if (FilesBox.DisableFocusFileBox)
        return;

    $('.file-box.active').removeClass('active');
    $(el).addClass('active');
}
FilesBox.DOMSubtreeModified = function () {
    const $uploadedContent = document.getElementById("uploaded-content");
    if (typeof ($uploadedContent) == 'undefined' || $uploadedContent == null)
        return;
    const $targetButton = $('.btn-apply-file');

    // Function to enable the button
    const enableButton = () => {
        $targetButton.removeClass("d-none");
        $('.control-help').addClass("text-danger");
    };

    // MutationObserver to detect changes in .uploaded-content div
    const observer = new MutationObserver((mutationsList) => {
        for (let mutation of mutationsList) {
            if (mutation.type === 'childList' || mutation.type === 'attributes' || mutation.type === 'characterData') {
                enableButton();
                break;
            }
        }
    });


    // Configuration of the observer
    const config = { childList: true, subtree: true, attributes: true, characterData: true };

    // Start observing the .uploaded-content div
    observer.observe($uploadedContent, config);

    // Also listen for input changes within the .uploaded-content div
    $uploadedContent.addEventListener('input', enableButton);
};
FilesBox.DOMSubtreeModified();
FilesBox.GetPermission = function () {
    var filePers = [];
    $('.uploaded-content .file-actions').each(function () {
        filePers.push({
            Id: $(this).attr("data-fileid"),
            IsHost: $(this).find('.chk-host').prop('checked'),
            IsSecretary: $(this).find('.chk-secretary').prop('checked'),
            IsParticipant: $(this).find('.chk-participant').prop('checked')
        });
    });
    $('[data-selector="hdfFilePermission"]').val(JSON.stringify(filePers));
    console.log(filePers);
};

FilesBox.SaveFile = function (refType, refId) {
    if (FilesBox.ValidatedFile.length === 0)
        return true;

    if ($('.file-box.active.file-box-single').length === 0)
        $('#UpdateProgress1').show();

    countUploaddingFile = FilesBox.ValidatedFile.length;
    var timing = 0;
    const folder = $('[data-selector="hdfFolderFileBox"]').val();
    FilesBox.ValidatedFile.forEach(function (file, index) {
        var tempId = FilesBox.PendingUploadIds[index];
        var pendingKey = tempId + "|New";
        var tag = FilesBox.FindItemByKey(pendingKey);
        if (tag.length === 0) {
            console.error('Pending upload element not found for', pendingKey);
            return;
        }
        var title = tag.find("input.title").val();
        var order = tag.find("input.order").val();
        setTimeout(function () {
            FilesBox.SendFile(folder, file, refType, refId, title, order, pendingKey);
        }, timing);
        timing += 500;
    });
    var item = $(".file-box.active .illustration-upload .item").not(".show");
    if (item.length > 0)
        $(".file-box.active .illustration-upload").isotope('remove', item);
    FilesBox.ValidatedFile = [];
    FilesBox.PendingUploadIds = [];

    return false;
}
/**
 * Secure File Upload Client
 * Compatible with SecureFileUploadHandler.ashx
 * Enhanced error handling for AJAX requests
 */
FilesBox.SendFile = function (folder, file, refType, refId, title, order, ar) {
    // Validate input parameters
    if (!FilesBox.ValidateUploadParameters(file, refType, refId, title, ar)) {
        return;
    }

    // Show progress bar
    FilesBox.ShowProgressBar(ar);

    // Create form data
    var formData = new FormData();
    formData.append('file', file);

    // Configure AJAX request
    $.ajax({
        xhr: function () {
            return FilesBox.CreateXHRWithProgress(ar);
        },
        type: 'POST',
        url: FilesBox.BuildUploadUrl(folder, refType, refId, title, order),
        data: formData,
        timeout: 300000, // 5 minutes timeout
        success: function (response, textStatus, xhr) {
            console.log("AJAX Success - Status:", textStatus, "Response:", response);
            FilesBox.HandleUploadSuccess(response, ar);
        },
        error: function (xhr, status, error) {
            console.log("AJAX Error - Status:", status, "Error:", error);
            console.log("Response Text:", xhr.responseText);
            console.log("HTTP Status:", xhr.status);
            FilesBox.HandleUploadError(xhr, status, error, ar);
        },
        complete: function (xhr, status) {
            console.log("AJAX Complete - Status:", status);
            FilesBox.HandleUploadComplete(ar);
        },
        processData: false,
        contentType: false,
        cache: false
    });
};

/**
 * Validate upload parameters
 */
FilesBox.ValidateUploadParameters = function (file, refType, refId, title, ar) {
    console.log("Validating parameters:", {
        file: file ? file.name : 'null',
        refType: refType,
        refId: refId,
        title: title,
        ar: ar
    });

    // Check if file exists
    if (!file) {
        FilesBox.ShowError("No file selected", ar);
        return false;
    }

    // Check file size (client-side validation)
    var maxSize = FilesBox.Config.MaxFileSize || (10 * 1024 * 1024); // 10MB default
    if (file.size > maxSize) {
        var maxSizeMB = Math.round(maxSize / (1024 * 1024));
        FilesBox.ShowError("File size exceeds " + maxSizeMB + "MB limit", ar);
        return false;
    }

    // Check file type (client-side validation)
    var allowedTypes = FilesBox.Config.AllowedTypes || [
        'image/jpeg', 'image/png', 'application/pdf',
        'application/msword', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
        'application/vnd.ms-excel', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        'audio/mpeg', 'video/mp4', 'video/avi'
    ];

    if (allowedTypes.indexOf(file.type) === -1) {
        FilesBox.ShowError("File type not allowed: " + file.type, ar);
        return false;
    }

    // Validate required parameters
    if (!refType || !refId || !ar) {
        FilesBox.ShowError("Missing required parameters", ar);
        return false;
    }

    // Validate RefId
    var guidPattern = /^(?:urn:uuid:|uuid:)?[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
    if (!guidPattern.test(refId)) {
        FilesBox.ShowError("Invalid RefId format", refId);
        return false;
    }


    console.log("Parameter validation passed");
    return true;
};

/**
 * Build upload URL with proper encoding
 */
FilesBox.BuildUploadUrl = function (folder, refType, refId, title, order) {
    var baseUrl = '/fFilesBox/SecureFileUploadHandler.ashx';
    var params = [];

    // Add required parameters
    params.push('Folder=' + encodeURIComponent(folder));
    params.push('RefType=' + encodeURIComponent(refType));
    params.push('RefId=' + encodeURIComponent(refId));

    // Add optional parameters
    if (title) {
        params.push('FileTitle=' + encodeURIComponent(title));
    }
    if (order) {
        params.push('Order=' + encodeURIComponent(order));
    }
    return baseUrl + '?' + params.join('&');
};

/**
 * Create XMLHttpRequest with progress tracking
 */
FilesBox.CreateXHRWithProgress = function (ar) {
    var xhr = new window.XMLHttpRequest();

    // Upload progress event
    xhr.upload.addEventListener("progress", function (evt) {
        if (evt.lengthComputable) {
            var percentComplete = Math.round((evt.loaded / evt.total) * 100);
            FilesBox.UpdateProgress(ar, percentComplete, "Uploading...");
        }
    }, false);

    // Handle upload completion
    xhr.upload.addEventListener("load", function (evt) {
        FilesBox.UpdateProgress(ar, 100, "Processing...");
    }, false);

    // Handle upload errors
    xhr.upload.addEventListener("error", function (evt) {
        FilesBox.ShowError("Upload failed", ar);
    }, false);

    return xhr;
};

/**
 * Show progress bar
 */
FilesBox.ShowProgressBar = function (ar) {
    var item = FilesBox.FindItemByKey(ar);
    if (item.length === 0) {
        return;
    }
    item.find(".progress-content").show();
    item.find(".opaction img").closest(".img-container").addClass("uploading");
};

/**
 * Update progress bar
 */
FilesBox.UpdateProgress = function (ar, percent, message) {
    var item = FilesBox.FindItemByKey(ar);
    if (item.length === 0) {
        return;
    }
    var progress = item.find(".progress-content .progress");

    progress.find(".number").text(percent + "% " + (message || ""));
    progress.find(".bar").css("width", percent + "%");

    if (percent >= 100) {
        item.find(".opaction").removeClass("opaction");
    }
};

/**
 * Handle successful upload response
 */
FilesBox.HandleUploadSuccess = function (response, ar) {
    console.log("Raw response:", response); // Debug log

    try {
        var result;

        // Handle different response types
        if (typeof response === 'string') {
            // Clean up response string - remove any whitespace/newlines
            var cleanResponse = response.trim();

            // Check if it looks like JSON
            if (cleanResponse.startsWith('{') && cleanResponse.endsWith('}')) {
                try {
                    result = JSON.parse(cleanResponse);
                } catch (parseError) {
                    console.error("JSON Parse Error:", parseError);
                    console.error("Response that failed to parse:", cleanResponse);

                    // Try to fix common JSON issues
                    var fixedResponse = FilesBox.FixJsonResponse(cleanResponse);
                    if (fixedResponse) {
                        result = JSON.parse(fixedResponse);
                    } else {
                        throw parseError;
                    }
                }
            } else {
                throw new Error("Invalid response format: " + cleanResponse);
            }
        } else if (typeof response === 'object') {
            result = response;
        } else {
            throw new Error("Unexpected response type: " + typeof response);
        }

        // Handle parsed JSON result
        if (result.success === true && result.fileId) {
            var serverFileId = result.fileId;
            if (typeof serverFileId === 'string') {
                serverFileId = serverFileId.trim();
            }

            if (serverFileId && typeof serverFileId !== 'string') {
                serverFileId = serverFileId.toString();
            }

            if (!serverFileId) {
                FilesBox.ShowError("Server did not return a file identifier", ar);
                return;
            }

            FilesBox.OnUploadSuccess(serverFileId, ar, result.message);
        } else if (result.success === false) {
            // Handle API error response
            var errorMessage = result.message || "Upload failed";
            var errorCode = result.errorCode || "UNKNOWN_ERROR";
            FilesBox.ShowError(errorMessage + " (" + errorCode + ")", ar);
        } else {
            // Ambiguous result
            console.warn("Ambiguous result:", result);
            FilesBox.ShowError("Unexpected server response format", ar);
        }

    } catch (e) {
        console.error("Response handling error:", e);
        console.error("Original response:", response);

        FilesBox.ShowError("Failed to parse server response: " + e.message, ar);
    }
};

/**
 * Try to fix common JSON formatting issues
 */
FilesBox.FixJsonResponse = function (jsonString) {
    try {
        console.log("Attempting to fix JSON:", jsonString);

        // Fix common issues:
        // 1. Unquoted object values that should be strings
        var fixed = jsonString.replace(
            /"fileId":\s*([a-zA-Z0-9\-\s]+)(?=\s*[,}])/g,
            function (match, value) {
                // Remove spaces and wrap in quotes
                var cleanValue = value.trim().replace(/\s+/g, '');
                return '"fileId": "' + cleanValue + '"';
            }
        );

        // 2. Fix any other unquoted values
        fixed = fixed.replace(
            /"([^"]+)":\s*([^",}\[\]]+)(?=[,}])/g,
            function (match, key, value) {
                var trimmedValue = value.trim();
                // If it's a boolean or number, leave as is
                if (trimmedValue === 'true' || trimmedValue === 'false' || !isNaN(trimmedValue)) {
                    return '"' + key + '": ' + trimmedValue;
                }
                // Otherwise quote it
                return '"' + key + '": "' + trimmedValue + '"';
            }
        );

        console.log("Fixed JSON:", fixed);

        // Test if it parses now
        JSON.parse(fixed);
        return fixed;

    } catch (e) {
        console.error("Could not fix JSON:", e);
        return null;
    }
};
/**
 * Handle upload success
 */
FilesBox.OnUploadSuccess = function (fileId, ar, message) {
    var normalizedTemp = FilesBox.NormalizePendingKey(ar);
    //var guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

    //if (!fileId || typeof fileId !== 'string' || !guidPattern.test(fileId) || fileId.charAt(14).toLowerCase() !== '7') {
    //    FilesBox.ShowError("Invalid file ID returned from server (expected UUIDv7)", ar);
    //    return;
    //}

    console.log("Upload success - FileId:", fileId, "AR:", ar);

    var item = FilesBox.FindItemByKey(ar);

    if (item.length === 0) {
        console.error("Could not find item with key:", ar);
        FilesBox.ShowError("UI synchronization error", ar);
        return;
    }

    // Update progress before mutating the identifier
    FilesBox.UpdateProgress(ar, 100, "Complete!");

    // Persist the server-provided identifier throughout the markup
    item.attr("data-ar", fileId);
    item.attr("data-temp-id", normalizedTemp);

    item.add(item.find('*')).each(function () {
        var $el = $(this);
        ['id', 'name', 'data-selector', 'data-fileid', 'data-fileId', 'for', 'data-key', 'data-hdf'].forEach(function (attr) {
            var current = $el.attr(attr);
            var updated = FilesBox.ReplaceIdentifier(current, normalizedTemp, fileId);
            if (current !== updated) {
                $el.attr(attr, updated);
            }
        });
    });

    // Hide progress after delay
    setTimeout(function () {
        item.find(".progress-content").hide();
        item.find(".img-container").removeClass("uploading");
    }, 1000);

    // Update global counter
    if (typeof countUploaddingFile !== 'undefined') {
        countUploaddingFile--;
        console.log("Remaining uploads:", countUploaddingFile);

        if (countUploaddingFile <= 0) {
            FilesBox.OnAllUploadsComplete();
        }
    }

    // Show success message if provided
    if (message) {
        FilesBox.ShowNotification("success", message);
    }
};
/**
 * Handle upload error
 */
FilesBox.HandleUploadError = function (xhr, status, error, ar) {
    console.log("Handling upload error:", { xhr: xhr, status: status, error: error });

    var errorMessage = "Upload failed";
    var errorDetails = "";

    try {
        if (xhr.responseText) {
            console.log("Raw error response:", xhr.responseText);

            // Try to parse error response
            var response = JSON.parse(xhr.responseText);
            if (response.message) {
                errorMessage = response.message;
            }
            if (response.errorCode) {
                errorDetails = " (" + response.errorCode + ")";
            }
        }
    } catch (e) {
        console.log("Could not parse error response as JSON");

        // Use default error handling based on status
        if (xhr.status === 0) {
            errorMessage = "Network error or request timeout";
        } else if (xhr.status === 413) {
            errorMessage = "File too large";
        } else if (xhr.status === 415) {
            errorMessage = "File type not supported";
        } else if (xhr.status >= 500) {
            errorMessage = "Server error (HTTP " + xhr.status + ")";
        } else if (xhr.status >= 400) {
            errorMessage = "Client error (HTTP " + xhr.status + ")";
        } else if (status === 'timeout') {
            errorMessage = "Upload timeout";
        } else if (status === 'parsererror') {
            errorMessage = "Invalid server response";
        } else if (status === 'abort') {
            errorMessage = "Upload cancelled";
        }

        // Include raw response if available and not too long
        if (xhr.responseText && xhr.responseText.length < 200) {
            errorDetails += " (Response: " + xhr.responseText + ")";
        }
    }

    FilesBox.ShowError(errorMessage + errorDetails, ar);
};

/**
 * Handle upload completion (success or failure)
 */
FilesBox.HandleUploadComplete = function (ar) {
    // Clean up any temporary states
    var item = FilesBox.FindItemByKey(ar);
    if (item.length === 0) {
        return;
    }
    item.find(".img-container").removeClass("uploading");

    // The server id is now persisted, so the temporary reference can be removed
    var normalizedTemp = FilesBox.NormalizePendingKey(ar);
    if (item.attr('data-temp-id') === normalizedTemp) {
        item.removeAttr('data-temp-id');
    }
};

/**
 * Show error message and update UI
 */
FilesBox.ShowError = function (message, ar) {
    // Show user-friendly error message
    if (typeof toastr !== 'undefined') {
        toastr.error(message, "Upload Error");
    } else {
        alert("Upload Error: " + message);
    }

    // Update progress bar to show error
    if (ar) {
        var item = FilesBox.FindItemByKey(ar);
        if (item.length === 0) {
            return;
        }
        var progress = item.find(".progress-content .progress");
        progress.find(".number").text("Error!");
        progress.find(".bar").css("width", "100%").addClass("error");

        // Hide error after delay
        setTimeout(function () {
            item.find(".progress-content").hide();
            progress.find(".bar").removeClass("error");
        }, 3000);
    }

    // Log error for debugging
    console.error("Upload error:", message);
};

/**
 * Show notification message
 */
FilesBox.ShowNotification = function (type, message) {
    return;
    if (typeof toastr !== 'undefined') {
        toastr[type](message);
    } else if (type === 'success') {
        console.log("Success:", message);
    }
};

/**
 * Handle completion of all uploads
 */
FilesBox.OnAllUploadsComplete = function () {
    // Hide global progress indicator
    $('#UpdateProgress1').hide();

    // Trigger save action
    var saveButton = $('.file-box.active [data-selector="btnApplyFile"]')[0];
    if (saveButton) {
        saveButton.click();
    }
    if (FilesBox.ValidatedFile.length > 1)
        FilesBox.ShowNotification("success", "Tất cả các tập tin đã được tải lên thành công");
};


/**
 * Initialize FilesBox if not already initialized
 */
if (typeof FilesBox === 'undefined') {
    window.FilesBox = {};
}
CMSMasterJs.AddEndRequest(FilesBox.DOMSubtreeModified);
//if (typeof __doPostBack !== 'undefined') {
//    FilesBox.OriginalDoPostback = __doPostBack;
//    __doPostBack = function (p1, p2) {
//        FilesBox.GetPermission();
//        FilesBox.OriginalDoPostback(p1, p2);
//    };
//}
FilesBox.DisabledDropdownMenuHidden = function () {
    document.querySelectorAll('.file-actions .dropdown-menu .form-check-label').forEach(label => {
        label.addEventListener('click', function (e) {
            e.stopPropagation(); // Ngăn Bootstrap đóng dropdown
        });
    });
}
FilesBox.DisabledDropdownMenuHidden();
CMSMasterJs.AddEndRequest(FilesBox.DisabledDropdownMenuHidden);
