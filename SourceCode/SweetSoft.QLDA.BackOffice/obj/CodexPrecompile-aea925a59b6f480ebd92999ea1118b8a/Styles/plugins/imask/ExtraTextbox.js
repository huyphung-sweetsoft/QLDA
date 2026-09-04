const ExtraTextBoxJs = {};
$(function () {
    ExtraTextBoxJs.Init = () => {
        const $elements = $("*[data-inputmask]");
        if ($elements == null || $elements.length <= 0)
            return;
        const $lang = $('html').attr("lang");
        if ($lang == 'en') {
            $elements.each(function (el) {
                const $id = $(this).attr("id");
                const $val = $(this).val() || '';
                if (typeof ($val) == 'undefined' || $val == null)
                    $val = '';
                const mask = IMask(document.getElementById($id), {
                    mask: Number,
                    min: -999999999999,
                    max: 999999999999,
                    scale: 2,  // digits after point, 0 for integers
                    signed: false,  // disallow negative
                    thousandsSeparator: ',',  // any single char
                    padFractionalZeros: false,  // if true, then pads zeros at end to the length of scale
                    normalizeZeros: true,  // appends or removes zeros at ends
                    radix: '.',  // fractional delimiter
                    mapToRadix: ['.'],  // symbols to process as radix
                });
                mask.value = $val;
            });
        }
        else {
            $elements.each(function (el) {
                const $id = $(this).attr("id");
                const $val = $(this).val() || '';
                if (typeof ($val) == 'undefined' || $val == null)
                    $val = '';
                const mask = IMask(document.getElementById($id), {
                    mask: Number,
                    min: -999999999999,
                    max: 999999999999,
                    scale: 2,  // digits after point, 0 for integers
                    signed: false,  // disallow negative
                    thousandsSeparator: '.',  // any single char
                    padFractionalZeros: false,  // if true, then pads zeros at end to the length of scale
                    normalizeZeros: true,  // appends or removes zeros at ends
                    radix: ',',  // fractional delimiter
                    mapToRadix: [','],  // symbols to process as radix
                });
                mask.value = $val;
            });
        }
    }

    ExtraTextBoxJs.Init();
    if (typeof Sys !== 'undefined' && typeof Sys.WebForms !== 'undefined')
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(ExtraTextBoxJs.Init);
});

window.ExtraTextBoxChange = function (postBackEvent) {
    if (window.ExtraTextBoxChangeTimeout != undefined)
        clearTimeout(window.ExtraTextBoxChangeTimeout);

    window.ExtraTextBoxChangeTimeout = setTimeout(function () {
        eval(postBackEvent);
    }, 500);
}