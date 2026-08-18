// Custom setting CKEditor
CKEDITOR.editorConfig = function (config) {
    config.toolbar = [
               { name: 'document', groups: ['mode', 'document', 'doctools'], items: ['Source', '-', 'Maximize' ] },
               { name: 'clipboard', groups: ['clipboard', 'undo'], items: ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo'] },
               { name: 'basicstyles', groups: ['basicstyles', 'cleanup'], items: ['Bold', 'Italic', 'Underline', '-', 'CopyFormatting', 'RemoveFormat'] },
               { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align', 'bidi'], items: ['NumberedList', 'BulletedList', '-', 'Blockquote', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', '-'] },
               { name: 'editing', groups: ['find', 'selection', 'spellchecker'], items: ['Replace', '-', 'SelectAll', '-'] },
               { name: 'insert', items: ['Image', 'Video', 'Table', 'Iframe'] },
               { name: 'links', items: ['Link', 'Unlink', 'Anchor', 'PageBreak'] },
               { name: 'styles', items: ['Styles', 'Format', 'Font', 'FontSize'] },
               { name: 'colors', items: ['TextColor', 'BGColor'] }
    ];
    config.extraPlugins = 'wordcount,undo,htmlwriter,notification,toolbar,button,widget,lineutils,widgetselection,image2,video';
    config.extraAllowedContent = 'span;ul;li;table;td;style;*[id];*(*);*{*}';
    config.image2_alignClasses = ['align-left', 'align-center', 'align-right'];
    config.image2_captionedClass = 'image-captioned';
    config.allowedContent = true;
    config.fillEmptyBlocks = false;
    config.entities = false;
    config.tabSpaces = 0;
    config.basicEntities = false;
    config.removePlugins = 'blockquote';
    config.entities_greek = false;
    config.entities_latin = false;
    config.entities_additional = '';
    config.filebrowserBrowseUrl = '/file-management.html';
    config.filebrowserImageBrowseUrl = '/file-management.html';
    config.filebrowserFlashBrowseUrl = null;
    config.filebrowserUploadUrl = null;
    config.filebrowserImageUploadUrl = null;
    config.filebrowserFlashUploadUrl = null;
    config.filebrowserWindowWidth = '100%';
    config.filebrowserWindowHeight = '650';
    //config.skin = document.getElementsByTagName("body")[0].getAttribute("data-layout-mode") == 'dark' ? 'moono-dark' : 'moono';
};
CKEDITOR.on('instanceReady', function (evt) {
    var editor = evt.editor;
    editor.on('focus', function (e) {
        if ($.fn.select2 != undefined && typeof ($.fn.select2) === 'function')
            $('select.select2').select2('close');
    });
});
//CKEDITOR.on('customImage', function (evt) {
//    // Prevent the default response handler.
//    evt.stop();

//    // Get XHR response.
//    var data = evt.data,
//        xhr = data.fileLoader.xhr,
//        response = xhr.responseText.split('\n');

//    for (var i = 0; i < response.length; i++) {
//        var img = CKEDITOR.dom.element.createFromHtml('<img src="' + response[i] + '"/>');
//        CKEDITOR.instances.editor1.insertElement(img);
//    }

//    // If the response status is OK, let's simply insert the file URL as a text.
//    if (data.fileLoader.status == 200) {
//        var url = data.fileLoader.uploadUrl;

//        evt.editor.insertHtml('<img src="' + url + '" alt="" />');
//    }
//});