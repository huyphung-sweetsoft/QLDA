const toastOptions = {
    closeButton: true,
    debug: false,
    newestOnTop: false,
    progressBar: false,
    positionClass: 'toast-bottom-right',
    preventDuplicates: false,
    showDuration: '1000',
    hideDuration: '1000',
    timeOut: '5000',
    extendedTimeOut: '1000',
    showEasing: 'swing',
    hideEasing: 'linear',
    showMethod: 'fadeIn',
    hideMethod: 'fadeOut'
};
(function ($) {
    'use strict'
    CMSMasterJs.Data = {
        TimeoutCloseModal: undefined
    };
    CMSMasterJs.InitNotify = function () {
        if (typeof toastr !== 'undefined') {
            toastr.options = toastOptions;
        } else {
            setTimeout(CMSMasterJs.InitNotify, 100);
        }
    };
    CMSMasterJs.ShowNotify = function (message, type) {
        if (!message || typeof toastr === 'undefined') return;

        const title = 'Thông báo';
        switch ((type || '').toLowerCase()) {
            case 'danger':
            case 'error':
                toastr.error(message, title);
                break;
            case 'warning':
                toastr.warning(message, title);
                break;
            case 'success':
                toastr.success(message, title);
                break;
            default:
                toastr.info(message, title);
                break;
        }
    };
    CMSMasterJs.SearchOffcanvasId = "search-offcanvas";
    CMSMasterJs.SearchOffcanvas = undefined;
    CMSMasterJs.HEIGHT_HEADER = 55;
    CMSMasterJs.ValidateBeforePostback = false;
    CMSMasterJs.DisableValidateBeforePostback = () => {
        CMSMasterJs.ValidateBeforePostback = false;
    };
    CMSMasterJs.EnableValidateBeforePostback = () => {
        CMSMasterJs.ValidateBeforePostback = false;
    };
    CMSMasterJs.HideAllValidatorPrompts = () => {
        setTimeout(function () {
            $("#aspnet").validationEngine("hideAll");
        }, 3000);
    };
    CMSMasterJs.HideModalMessage = () => {
        $("#modal-notify").modal('hide');
        CMSMasterJs.PaceRestart();
        return true;
    };
    CMSMasterJs.PaceRestart = () => {
        if (typeof (Pace) != undefined)
            Pace.restart();
    };
    CMSMasterJs.OpenLoading = () => {
        if (typeof (Pace) != undefined)
            Pace.restart();
    };
    CMSMasterJs.CloseLoading = () => {

    };
    CMSMasterJs.CloseSearchPopup = (panelId) => {

    };
    CMSMasterJs.SetMinHeightBody = () => {
        const height = window.outerHeight;
        $('.box-shadow-body').css('min-height', height - CMSMasterJs.HEIGHT_HEADER);
    };
    CMSMasterJs.CollapseEvent = () => {
        $('[role="button"][data-bs-toggle="collapse"]').click(function () {
            const $this = $(this);
            //console.log($this.attr("aria-expanded"));
            if ($this.attr("aria-expanded") == "true") {
                $this.find(".title-button").text($this.attr("data-text-collapse"));
                $this.find("i").attr("class", String.format("icon {0}", $this.attr("data-icon-collapse")));
            }
            else {
                $this.find(".title-button").text($this.attr("data-title"));
                $this.find("i").attr("class", String.format("icon {0}", $this.attr("data-icon-default")));
                if ($this.attr("data-bs-open-button") != '') {
                    const $button = $(String.format('[data-selector="{0}"]', $this.attr("data-bs-open-button")));
                    if ($button != undefined) {
                        $button.find(".title-button").text($this.attr("data-text-collapse"));
                        $button.find("i").attr("class", String.format("icon {0}", $this.attr("data-icon-collapse")));
                    }
                }
            }
        });
    };
    CMSMasterJs.EnableContentChanged = () => {
        window.onbeforeunload = function (e) {
            var message = "",
                e = e || window.event;
            if (e) {
                e.returnValue = message;
            }
            return message;
        };
    };
    CMSMasterJs.DisableContentChanged = () => {
        window.onbeforeunload = null;
    };
    CMSMasterJs.CheckPageIsValid = () => {
        return CMSMasterJs.ValidateBeforePostback ? (Page_IsValid = $("#aspnetForm").validationEngine("validate",
            {
                promptPosition: "topLeft",
                autoHidePrompt: !1,
                scroll: !1
            }), Page_IsValid) : ($("#aspnetForm").validationEngine("detach"), !0)
    };
    CMSMasterJs.CheckValid = () => {
        const $isValid = $('.js-validation.validationEngineContainer').validationEngine('validate', { maxErrorsPerField: 1 });
        if (!$isValid) {
            if (typeof (ExtraButtonJs) != 'undefined')
                ExtraButtonJs.ClearLoading();
            setTimeout(function () {
                $('.js-validation.validationEngineContainer').validationEngine('hideAll');
            }, 5000);
        }
        else
            CMSMasterJs.DisableContentChanged();
        return $isValid;
    };
    CMSMasterJs.ValidElement = ($t) => {
        const $isValid = $($t).validationEngine('validate', { maxErrorsPerField: 1 });
        if (!$isValid) {
            if (typeof (ExtraButtonJs) != 'undefined')
                ExtraButtonJs.ClearLoading();
            setTimeout(function () {
                $($t).validationEngine('hideAll');
            }, 5000);
        }
        else
            CMSMasterJs.DisableContentChanged();
        return $isValid;
    };
    CMSMasterJs.ValidForm = ($className) => {
        const $isValid = $($className).validationEngine('validate');
        if (!$isValid) {
            if (typeof (ExtraButtonJs) != 'undefined')
                ExtraButtonJs.ClearLoading();
            setTimeout(function () {
                $($className).validationEngine('hideAll');
            }, 5000);
        }
        else
            CMSMasterJs.DisableContentChanged();
        return $isValid;
    };
    CMSMasterJs.ShowPrompt = ($clientId, $message) => {
        $('#' + $clientId).validationEngine('showPrompt', $message, 'error', 'topLeft', true);
    };
    CMSMasterJs.HidePrompt = ($clientId) => {
        const $parent = $('#' + $clientId).parent();
        if ($parent != undefined) {
            $parent.removeClass("ext-error");
            $parent.next('.el-error').remove();
        }
        else {
            $('#' + $clientId).removeClass("ext-error");
            $('#' + $clientId).next('.el-error').remove();
        }
    };
    CMSMasterJs.ShowMessage = ($selector, $mess) => {
        $($selector).text($mess);
        $($selector).removeClass('d-none');
        $('input[type="password"]').val('');
    };
    CMSMasterJs.SetInputShowText = ($input, $elem) => {
        var _text = $($input).val();
        var _elm = $($elem);
        if (_elm.length) {
            _elm.empty();
            _elm.append(_text);
        }
    };
    CMSMasterJs.OpenDialog = ($selector, $title) => {
        if ($title !== '')
            $($selector).find('.modal-title').html(String.format("{0}", $title));
        $($selector).modal("show");
    };
    CMSMasterJs.CloseDialog = ($selector) => {
        $($selector).modal("hide");
        $('.modal-backdrop').remove();
    };
    CMSMasterJs.OpenMessageBox = ($selector, $timeOut) => {
        if (typeof ($timeOut) == undefined)
            $timeOut = 20000;
        setTimeout(function () {
            if ($selector != undefined && $selector != '') {
                $($selector).modal('show');
                if (typeof CMSMasterJs.Data.TimeoutCloseModal !== 'undefined') {
                    window.clearTimeout(CMSMasterJs.Data.TimeoutCloseModal);
                    CMSMasterJs.Data.TimeoutCloseModal = undefined;
                }
                CMSMasterJs.Data.TimeoutCloseModal = setTimeout(() => {
                    $($selector).modal('hide');
                }, $timeOut);
            }
        }, 500)
    };
    CMSMasterJs.CloseMessageBox = ($selector) => {
        if ($selector != undefined && $selector != '') {
            $($selector).modal('hide');
            //$('body').removeClass('modal-open');
            //$('.modal-backdrop').remove();
        }
    };
    CMSMasterJs.DefaultSubmit = () => {
        $('input[data-input-enter="true"], div[data-input-enter="true"]').keypress(function (event) {
            if (event.keyCode === 13) {
                CMSMasterJs.DisableContentChanged();
                const $button = $('#' + $(this).attr('data-enter-id'));
                if ($button != undefined && $button.length > 0) {
                    $button[0].click();
                }
            }
        });
    };
    CMSMasterJs.ShowOffcanvasSearch = ($elementId) => {
        if (typeof (bootstrap) == 'undefined')
            return;
        if (typeof ($elementId) != 'undefined' && $elementId != '')
            CMSMasterJs.SearchOffcanvas = new bootstrap.Offcanvas(document.getElementById($elementId));
        else
            CMSMasterJs.SearchOffcanvas = new bootstrap.Offcanvas(document.getElementById(CMSMasterJs.SearchOffcanvasId));
        CMSMasterJs.SearchOffcanvas.show();
    }
    CMSMasterJs.HideOffcanvasSearch = () => {
        if (typeof (CMSMasterJs.SearchOffcanvas) == 'undefined' || CMSMasterJs.SearchOffcanvas == null)
            return;
        CMSMasterJs.SearchOffcanvas.hide();
    }
    CMSMasterJs.EnterSubmit = (event, t) => {
        if (event.keyCode === 13 || event.which == 13) {
            CMSMasterJs.DisableContentChanged();
            let $button = $('#' + $(t).attr('data-enter-id'));
            if (typeof ($button) == 'undefined' || $button == null || $button.length == 0)
                $button = $('[data-id=' + $(t).attr('data-enter-id') + ']');
            if ($button != undefined && $button.length > 0) {
                $button[0].click();
            }
        }
    };
    CMSMasterJs.ShowButtonTab = ($lstShow, $lstHide) => {
        var $showItems = $lstShow.split(',');
        var $hideItems = $lstHide.split(',');
        if ($showItems.length > 0) {
            Array.from($showItems, function (element, index) {
                $(element).show();
            });
        }
        if ($hideItems.length > 0) {
            Array.from($hideItems, function (element, index) {
                $(element).hide();
            });
        }
    };
    CMSMasterJs.SubmitForm = () => {
        $('[data-submit-form="true"] input').keypress(function (event) {
            // Check if the key pressed is the "Enter" key (keycode 13)
            console.log(13);
            if (event.which == 13) {
                console.log(1);
                const $form = $(this).closest('[data-submit-form="true"]');
                if (typeof ($form) == 'undefined' || $form == null)
                    return;
                CMSMasterJs.DisableContentChanged();
                let $button = $('#' + $($form).attr('data-enter-id'));
                if (typeof ($button) == 'undefined' || $button == null || $button.length == 0)
                    $button = $('[data-id=' + $($form).attr('data-enter-id') + ']');
                if ($button != undefined && $button.length > 0) {
                    $button[0].click();
                }
                event.preventDefault();
            }
        });
    };
    CMSMasterJs.SetContentCol = () => {
        var colItem = $('.rowSplit .colItem');
        $('.rowSplit').parents('.offcanvas').addClass('fullCanvas');
        if (colItem.length) {
            colItem.each(function () {
                if ($(this)[0].children.length == 0) {
                    $(this).addClass('d-none');

                    $('.rowSplit').parents('.offcanvas').removeClass('fullCanvas');
                } else {
                    $(this).removeClass('d-none');
                }
            });
        }
    };
    CMSMasterJs.FindContentTop = () => {
        if (!$('.wrapTitleAndBtnCtrl .wrapBtnRight ').length) $('body').addClass('noBtnCtrRight');
    };
    CMSMasterJs.SetCookie = (cName, cValue, expDays) => {
        let date = new Date();
        date.setTime(date.getTime() + (expDays * 24 * 60 * 60 * 1000));
        const expires = "expires=" + date.toUTCString();
        document.cookie = cName + "=" + cValue + "; " + expires + "; path=/";
    };
    CMSMasterJs.GetCookie = (cName) => {
        const name = cName + "=";
        const cDecoded = decodeURIComponent(document.cookie); //to be careful
        const cArr = cDecoded.split('; ');
        let res;
        cArr.forEach(val => {
            if (val.indexOf(name) === 0) res = val.substring(name.length);
        })
        return res;
    };
    CMSMasterJs.SetTheme = (theme) => {
        document.body.setAttribute("data-layout-mode", theme),
            document.body.setAttribute("data-topbar", theme),
            document.body.setAttribute("data-sidebar", theme)
    };
    CMSMasterJs.GenerateSlugUrl = ($element) => {
        var $str = $($element).val().latinise();
        if ($str != undefined && $str != '') {
            $str = $str.replace(/[^\w\s]+/g, '').replace(/\s+/g, "-").toLowerCase();
            if ($str != null && $str != '')
                $str = $str.trimLeft('-').trimRight('-');
            $('[data-selector="txtSlugUrl"]').val($str);
        }
        else
            $('[data-selector="txtSlugUrl"]').val('');
    };
    CMSMasterJs.GetSEOUrlForLink = ($slug) => {
        $('[data-selector="txtSlugUrl"]').val($slug);
    };
    CMSMasterJs.DetectingDarkMode = () => {
        let matched = window.matchMedia('(prefers-color-scheme: dark)').matches;
        const theme = CMSMasterJs.GetCookie("data-layout-mode");
        if (typeof (theme) == 'undefined' || theme == '') {
            if (matched)
                CMSMasterJs.SetCookie("data-layout-mode", "dark", 7);
            else
                CMSMasterJs.SetCookie("data-layout-mode", "light", 7);
        }
    };
    CMSMasterJs.TrackingOnChangeInput = () => {
        $('input, textarea, select').filter(function () {
            return !$(this).hasClass('ignore') && $(this).attr('type') !== 'hidden';
        }).on('change', function () {
            $(this).removeClass("ext-error");
            CMSMasterJs.EnableContentChanged();
        });
    };
    CMSMasterJs.AddEndRequest = ($functionName) => {
        if (typeof Sys !== 'undefined' && typeof Sys.WebForms !== 'undefined')
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest($functionName);
    };
    CMSMasterJs.OpenSelectImage = (t) => {
        const $this = $(t);
        var ws = getWindowSize();
        const uploadKey = $('[data-selector="hdfFolderKey"]').val();
        if (typeof (uploadKey) == 'undefined' || uploadKey == null || uploadKey == '')
            return;
        $.lightbox('/Administration/RichFilemanager/default.aspx?field_name=' + $this.attr('id')
            + '&key=' + uploadKey
            + '&selectFun=CMSMasterJs.setImageUrl',
            {
                iframe: true,
                width: ws.width - 60,
                height: ws.height - 40,
            });
    };
    CMSMasterJs.setImageUrl = ($clientId, $url) => {
        const $img = $('#' + $clientId);
        if ($img == undefined || !$img)
            return;
        $img.attr('src', $url);
        var $hdf = $(String.format('[data-selector="{0}"]', $img.attr("data-hdf")));
        if ($hdf)
            $hdf.val($url);
    };
    CMSMasterJs.RemoveThumbnail = (t) => {
        const $this = $(t);
        const $img = $(String.format('[data-selector="{0}"]', $this.attr("data-img")));
        if ($img == undefined || !$img)
            return;
        $img.attr("src", "/uploads/no-image.jpg");
        var $hdf = $(String.format('[data-selector="{0}"]', $img.attr("data-hdf")));
        if ($hdf)
            $hdf.val("/uploads/no-image.jpg");
    };
    CMSMasterJs.OpenFile = ($fileUrl) => {
        var ws = getWindowSize();
        if ($fileUrl) {
            $.lightbox($fileUrl,
                {
                    iframe: true,
                    width: ws.width - 60,
                    height: ws.height - 40,
                });
        }
    };
    CMSMasterJs.GetRandomInt = (max) => {
        return Math.floor(Math.random() * Math.floor(max));
    };
    CMSMasterJs.FormatCurrency = (number) => {
        //return number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",") + 'đ';
        //return number.toLocaleString('vi-VN', { style: 'currency', currency: 'VND', });
        return number.toLocaleString('vi-VN');
    };
    CMSMasterJs.FormatNumber = (val) => {
        if (val == "")
            return 0;
        const number = parseFloat(val);
        //return number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        if (Number.isInteger(number)) {
            return number.toLocaleString('en-US'); // Format integers with commas for thousands
        } else {
            return number.toFixed(2); // For decimal numbers, keep only 2 decimal places
        }
    };
    CMSMasterJs.GetValueNumber = (val) => {
        if (val == '')
            return 0;
        return parseFloat(val.replace(",", ""));
    };
    CMSMasterJs.CreateSlugUrl = ($tag, $selector) => {
        var $str = $($tag).val().latinise();
        if ($str != undefined && $str != '') {
            $str = $str.replace(/[^\w\s]+/g, '').replace(/\s+/g, "-").toLowerCase();
            if ($str != null && $str != '')
                $str = $str.trimLeft('-').trimRight('-');
            const $parentURL = $($tag).prev('[data-selector="hdfParentSlugURL"]').val();
            if (typeof ($parentURL) != 'undefined' && $parentURL != '')
                $str = $parentURL + '/' + $str;
            if (typeof ($selector) != 'undefined')
                $('[data-selector="' + $selector + '"]').val($str);
            else
                $('[data-selector="txtSlugUrl"]').val($str);
        }
        else
            $('[data-selector="txtSlugUrl"]').val('');
    };
    CMSMasterJs.UploadAvatar = () => {
        $('[data-selector="fileAvatar"]').click();
        $('[data-selector="fileAvatar"]').on('change', function (event) {
            event.preventDefault();
            event.stopPropagation();
            const files = event.target.files;
            if (files == undefined || files.length <= 0)
                return;
            var formData = new FormData();
            // add assoc key values, this will be posts values
            formData.append("file", files[0]);
            formData.append("csrf", $('[data-selector="hdfCSRF"]').val());
            $.ajax({
                type: "POST",
                url: "/api/v1/User/UploadAvatar",
                headers: {
                    'Accept-Language': 'vi'
                },
                success: function (data) {
                    //console.log(data);
                    if (data != undefined && data.StatusResult.Code == 1) {
                        $('[data-selector="useravatar"]').attr('src', data.Data.Path);
                        $('[data-selector="btnRefreshUser"]')[0].click();
                        CMSMasterJs.DisableContentChanged();
                    }
                },
                error: function (error) {
                    //console.log(error)
                },
                async: true,
                data: formData,
                cache: false,
                contentType: false,
                processData: false,
                timeout: 60000
            });
        });
    };
    CMSMasterJs.GetParameterByName = ($name) => {
        if ($name == undefined || $name == '')
            return '';
        $name = $name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
        var $results = null;
        var $regex = new RegExp("[\\?&]" + $name + "=([^&#]*)"),
            $results = $regex.exec(location.search);
        return $results === null ? "" : decodeURIComponent($results[1].replace(/\+/g, " "));
    };
    CMSMasterJs.RemoveParamsFromURL = ($paramName) => {
        const $url = window.location.href;
        let urlParts = $url.split('?');
        if (urlParts.length >= 2) {
            let baseUrl = urlParts[0];
            let params = urlParts[1].split('&');
            let updatedParams = params.filter(param => {
                return !param.startsWith($paramName); // Replace 'paramToRemove' with the parameter you want to remove
            });
            return baseUrl + (updatedParams.length > 0 ? '?' + updatedParams.join('&') : '');
        }
        return url;
    };
    CMSMasterJs.ClipboardJS = () => {
        if ($('.btn-copy').length > 0) {
            $('.btn-copy').click(function () {
                const $el = $($(this).attr("data-clipboard-target"));
                if ($el != undefined) {
                    const $hostPath = window.location.origin;
                    const $prefix = $('html').attr("lang");
                    CMSMasterJs.FallbackCopyTextToClipboard(String.format("{0}/{1}/{2}", $hostPath, $prefix, $el.val()));
                }
            })
        }
    };
    CMSMasterJs.CopyTextToClipboardJS = () => {
        if (typeof (ClipboardJS) == 'undefined')
            return;
        const $btnCopy = new ClipboardJS('.btn-copy-text');
        $btnCopy.on('success', function (e) {
            e.clearSelection();
        });

        $btnCopy.on('error', function (e) {
            console.error('Action:', e.action);
            console.error('Trigger:', e.trigger);
        });
    };
    CMSMasterJs.FallbackCopyTextToClipboard = (text) => {
        var textArea = document.createElement("textarea");
        textArea.value = text;

        // Avoid scrolling to bottom
        textArea.style.top = "0";
        textArea.style.left = "0";
        textArea.style.position = "fixed";

        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();

        try {
            var successful = document.execCommand('copy');
            if (successful)
                CMSMasterJs.ShowNotify('Sao chép thành công!', 'success');
            else
                CMSMasterJs.ShowNotify('Sao chép thất bại!', 'error');

        } catch (err) {
            CMSMasterJs.ShowNotify('Không thể sao chép!', 'error');
            console.error('Fallback: Oops, unable to copy', err);
        }
        document.body.removeChild(textArea);
    };
    CMSMasterJs.uuidv4 = () => {
        return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    };
    CMSMasterJs.LoadTab = (t, key) => {
        if ($(t).hasClass("loaded"))
            return;
        $(t).addClass("loaded");
        $('[data-selector="hdfTabKey"]').val(key);
        $('[data-selector="btnLoadTab"]')[0].click();
    };
    CMSMasterJs.FormatInput = () => {
        $(document).on('input', 'input[data-format-number]', function () {
            const $this = $(this);
            const $val = $this.val().replaceAll(",", "");
            if ($val == '')
                return;
            const format = CMSMasterJs.FormatNumber($val);
            //console.log({ format });
            $this.val(format);
        })
        $(document).on('input', 'input[data-format-currency]', function () {
            const $this = $(this);
            const $val = $this.val().replaceAll(",", "").replace("đ", "");
            if ($val == '')
                return;
            const format = CMSMasterJs.FormatCurrency($val);
            //console.log({ format });
            $this.val(format);
        })
    };
    CMSMasterJs.Tooltips = () => {
        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
        var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl)
        })
    };
    CMSMasterJs.InactivityTime = () => {
        return;
        var time;
        window.onload = resetTimer;
        document.onmousemove = resetTimer;
        document.onkeypress = resetTimer;

        function logout() {
            const $button = $('[data-selector="lbtLockScreen"]');
            if (typeof ($button) != 'undefined') {
                CMSMasterJs.DisableContentChanged();
                $button[0].click();
            }
        }

        function resetTimer() {
            clearTimeout(time);
            time = setTimeout(logout, 10 * 60000);  // 10 minutes in milliseconds
        }
    }
    CMSMasterJs.RemoveLoading = () => {
        $('body').removeClass('loadingPage');
    };
    CMSMasterJs.MakeGuid = () => {
        return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    };
    CMSMasterJs.CollapButtons = () => {
        if ($('.action-button')) {
            var buttons = $('.list-btn-action').find('.btn');
            if (buttons.length > 2) {
                $('.dropdown-toggle').not('.ignore').show();
                $('.list-action-dropdown').addClass('dropdown-menu');
                $('.list-action-dropdown').removeClass('list-btn-action')

            }
            else {
                $('.dropdown-toggle').not('.ignore').hide();
            }
        }
    };
    CMSMasterJs.replaceUrl = (newUrl) => {
        console.log({ newUrl });
        window.history.replaceState(null, '', newUrl);
        setTimeout(() => {
            const t = newUrl.replace(window.location.origin, '');
            console.log({ t });
            $("#aspnet").attr("action", t);
        }, 500);
    };
    CMSMasterJs.Init = () => {
        const url = location.pathname;
        if (url.indexOf('/Administration/lock-screen') < 0)
            CMSMasterJs.InactivityTime();
        CMSMasterJs.SetMinHeightBody();
        CMSMasterJs.TrackingOnChangeInput();
        const theme = CMSMasterJs.GetCookie("data-layout-mode");
        if (typeof (theme) != 'undefined' && theme != '')
            CMSMasterJs.SetTheme(theme);
        CMSMasterJs.SubmitForm();
        CMSMasterJs.SetContentCol();
        CMSMasterJs.FindContentTop();
        CMSMasterJs.CollapseEvent();
        CMSMasterJs.ClipboardJS();
        CMSMasterJs.CopyTextToClipboardJS();
        CMSMasterJs.Tooltips();
        CMSMasterJs.CollapButtons();
        //------------------------------------------------
        setTimeout(() => {
            CMSMasterJs.DefaultSubmit();
        }, 200);
    };
    CMSMasterJs.Init();
    CMSMasterJs.AddEndRequest(CMSMasterJs.SubmitForm);
    CMSMasterJs.AddEndRequest(CMSMasterJs.DefaultSubmit);
    CMSMasterJs.AddEndRequest(CMSMasterJs.Tooltips);
    CMSMasterJs.AddEndRequest(CMSMasterJs.RemoveLoading);
    CMSMasterJs.AddEndRequest(CMSMasterJs.CollapButtons);
    CMSMasterJs.AddEndRequest(CMSMasterJs.TrackingOnChangeInput);
    CMSMasterJs.AddEndRequest(CMSMasterJs.CopyTextToClipboardJS);
    $(CMSMasterJs.InitNotify);
})(jQuery);
$(window).on('scroll', function () {
    //CMSMasterJs.SubmitForm();
    CMSMasterJs.SetContentCol();
    CMSMasterJs.FindContentTop();
});
$(window).on('resize', function () {

    CMSMasterJs.SetMinHeightBody();
    if ($(window).width() > 1199 && $('.wrapTitleAndBtnCtrl').length) {
        $('.wrapTitleAndBtnCtrl').attr('style', '');
    }
});
var getWindowSize = function () {
    var w = 0; var h = 0;
    //IE
    if (!window.innerWidth) {
        if (!(document.documentElement.clientWidth === 0)) {
            //strict mode
            w = document.documentElement.clientWidth;
            h = document.documentElement.clientHeight;
        } else {
            //quirks mode
            w = document.body.clientWidth; h = document.body.clientHeight;
        }
    } else {
        //w3c
        w = window.innerWidth; h = window.innerHeight;
    }
    return {
        width: w, height: h
    };
};
(function ($) {
    const originalVal = $.fn.val;

    $.fn.val = function () {
        if (arguments.length) {
            const value = arguments[0];
            const result = originalVal.apply(this, arguments);

            this.each(function () {
                const $el = $(this);
                if (value !== "") {
                    $el.addClass("valid-success");
                } else {
                    $el.removeClass("valid-success");
                }
            });

            return result;
        }

        return originalVal.apply(this);
    };
})(jQuery);

