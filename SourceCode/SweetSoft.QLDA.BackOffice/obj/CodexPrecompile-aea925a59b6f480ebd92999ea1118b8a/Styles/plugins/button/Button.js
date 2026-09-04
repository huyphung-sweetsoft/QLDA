const ExtraButtonJs = {};
$(function () {
    ExtraButtonJs.ClassLoading = "bx bx-loader bx-spin font-size-16 align-middle";
    ExtraButtonJs.TimeOutButton = undefined;
    ExtraButtonJs.Button = undefined;
    ExtraButtonJs.ClearLoading = function () {
        if (ExtraButtonJs.Button != undefined) {
            const $class = ExtraButtonJs.Button.attr("data-icon-default");
            ExtraButtonJs.Button.find("i").attr("class", $class);
        }
    };
    ExtraButtonJs.RenderAction = function () {
        $('.btn-spiner:not([data-submit="false"]):not(.btn-block)').click(function () {
            ExtraButtonJs.Button = $(this);
            ExtraButtonJs.Button.find('i').attr("class", ExtraButtonJs.ClassLoading);
            if (typeof (ExtraButtonJs.TimeOutButton) != 'undefined')
                clearTimeout(ExtraButtonJs.TimeOutButton);
            ExtraButtonJs.TimeOutButton = setTimeout(function () {
                ExtraButtonJs.ClearLoading();
            }, 2000);
        });
        $('.btn[data-loading="true"]').click(function () {
            if (typeof (isEnabledLoading) == 'undefined' || isEnabledLoading == null)
                return;
            isEnabledLoading = true;
            setTimeout(() => {
                isEnabledLoading = false;
            },3000);
        });
    }
    ExtraButtonJs.Init = function () {
        ExtraButtonJs.RenderAction();
    };
    ExtraButtonJs.Init();
    if (typeof Sys !== 'undefined' && typeof Sys.WebForms !== 'undefined')
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(ExtraButtonJs.RenderAction);
});